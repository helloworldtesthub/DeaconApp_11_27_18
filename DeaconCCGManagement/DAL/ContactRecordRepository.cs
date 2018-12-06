using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using DeaconCCGManagement.enums;
using DeaconCCGManagement.Models;

namespace DeaconCCGManagement.DAL
{
    public class ContactRecordRepository : GenericRepository<ContactRecord>, IContactRecordRepository<ContactRecord>
    {
        public ContactRecordRepository(CcgDbContext context) : base(context)
        {

        }

        public override ContactRecord FindById(int? id)
        {            
            var dbSet = context.Set<ContactRecord>();
            return dbSet.Include("CCGMember")
                .Include("AppUser")
                .Include("ContactType")
                .Include("PassAlongContact")
                .SingleOrDefault(c => c.Id == id);            
        }

        /// <summary>
        /// Get contact records using lazy loading.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="pageIndex">The current page number.</param>
        /// <param name="pageSize">Records per page.</param>
        /// <returns></returns>
        public IEnumerable<ContactRecord> GetContactRecords(out int totalItemsCount, Expression<Func<ContactRecord, bool>> predicate = null,
            int? pageIndex = null, int? pageSize = null, ContactsSort contactsSort = ContactsSort.DateDescending)
        {

            // log sql to console for debug
            context.Database.Log = generatedSQL =>
            {
                Debug.WriteLine(generatedSQL);
            };

            var dbSet = context.Set<ContactRecord>();
            totalItemsCount = 0;

            string contactTypeName = "ContactType";
            if (predicate != null)
            {   
                // Get total item count for pagination
                totalItemsCount = dbSet.Include(contactTypeName).AsNoTracking().Where(predicate).Count();

                // No page index or size given so get all records that satisfy predicate.
                if (pageIndex == null || pageSize == null)
                    return dbSet.AsNoTracking().Include(contactTypeName).Where(predicate).ToList();

                // Pull only records needed for page view.
                return GetContactRecordsHelper(contactsSort, contactTypeName, 
                    pageIndex, pageSize, predicate);
            }

            // Get total item count for pagination
            totalItemsCount = dbSet.Include(contactTypeName).AsNoTracking().Count();

            // No page index or size given so get all records.
            if (pageIndex == null && pageSize == null)            
                return dbSet.AsNoTracking().Include(contactTypeName).ToList();

            // Pull only records needed for page view.
            return GetContactRecordsHelper(contactsSort, contactTypeName,
                  pageIndex, pageSize);

        }

        public IEnumerable<ContactRecord> GetContactRecordsHelper(ContactsSort contactsSort,
             string contactTypeName, int? pageIndex, int? pageSize,
            Expression<Func<ContactRecord, bool>> predicate = null)
        {
        
            if (predicate == null)
            {
                predicate = c => c.Archive == false;
            }

            // Pull only records needed for page view.
            // switch statement for filtering options
            switch (contactsSort)
            {
                case ContactsSort.GroupByMember:
                    return dbSet.Include(contactTypeName)
                    .AsNoTracking()
                    .Where(predicate)
                    .OrderBy(c => c.CCGMember.LastName)
                    .Skip(((int)pageIndex - 1) * (int)pageSize)
                    .Take((int)pageSize).ToList();
                case ContactsSort.GroupByDeacon:
                    return dbSet.Include(contactTypeName)
                     .AsNoTracking()
                     .Where(predicate)
                     .OrderBy(c => c.AppUser.LastName)
                     .Skip(((int)pageIndex - 1) * (int)pageSize)
                     .Take((int)pageSize).ToList();
                case ContactsSort.GroupByContactType:
                    return dbSet.Include(contactTypeName)
                     .AsNoTracking()
                     .Where(predicate)
                     .OrderBy(c => c.ContactType.Name)
                     .Skip(((int)pageIndex - 1) * (int)pageSize)
                     .Take((int)pageSize).ToList();
                case ContactsSort.DateAscending:
                    return dbSet.Include(contactTypeName)
                        .AsNoTracking()
                        .Where(predicate)
                        .OrderBy(c => c.ContactDate)
                        .Skip(((int)pageIndex - 1) * (int)pageSize)
                        .Take((int)pageSize).ToList();
                case ContactsSort.DateDescending:
                case ContactsSort.None:
                default:
                    // DateDescending 
                    return dbSet.Include(contactTypeName)
                        .AsNoTracking()
                        .Where(predicate)
                        .OrderByDescending(c => c.ContactDate)
                        .Skip(((int)pageIndex - 1) * (int)pageSize)
                        .Take((int)pageSize).ToList();
            }
        }

        public override void Add(ContactRecord entity)
        {
            base.Add(entity);

            UpdateLastContactDate(entity);
            AddToPassAlongRecords(entity);
        }

        public override void AddOrUpdate(ContactRecord entity)
        {
            base.AddOrUpdate(entity);

            UpdateLastContactDate(entity);
            AddToPassAlongRecords(entity);
        }

        public void AddToPassAlongRecords(ContactRecord entity)
        {
            // If 'pass along' is true, add pass along record
            if (entity.PassAlong)
            {
                // So the db context will get the primary key Id
                context.SaveChanges();

                // Adds to the PassAlongContact table
                AddPassAlongContact(entity);
            }
        }

        public void UpdateLastContactDate(ContactRecord entity)
        {            
            // update last contacted date for member           
            var member = context.Members.SingleOrDefault(m => m.Id == entity.CCGMemberId);
            if (member != null)
            {
                member.LastDateContacted = entity.ContactDate;
                context.Entry(member).State = EntityState.Modified;
                context.SaveChanges();
            }            
        }

        public override void Update(ContactRecord entity)
        {
            base.Update(entity);

            //
            // Update PassAlongContact records
            // This model keeps track of the records that 
            // should get passed along.
            //

            PassAlongContact passAlongContact;
           
            passAlongContact = context.PassAlongContacts
                .SingleOrDefault(r => r.Id == entity.Id);
            

            // If not marked to 'pass along' and doesn't exist in pass along records
            if (!entity.PassAlong && passAlongContact == null) return;

            // If marked to 'pass along' and doesn't have existing 'pass along' record
            if (entity.PassAlong && passAlongContact == null)
            {
                // Adds to the PassAlongContact table
                AddPassAlongContact(entity);

                return;
            }
            // Pass along is false and pass along record exists, remove
            // pass along record. This could occur if a deacon changes
            // his mind and unchecks 'pass along' in edit view 
            if (!entity.PassAlong && passAlongContact != null)
            {               
                context.PassAlongContacts.Remove(passAlongContact);
                context.SaveChanges();
                
            }
        }

        public override void Delete(ContactRecord entity)
        {
            // Delete related PassAlongContact record if it exists
          
             // Find pass along record 
            var passAlongContact = context.PassAlongContacts
                .SingleOrDefault(c => c.Id == entity.Id);

            // Remove existing pass along record
            if (passAlongContact != null)
            {
                context.PassAlongContacts.Remove(passAlongContact);
            }

            context.SaveChanges();
            

            base.Delete(entity);
        }

        public void AddPassAlongContact(ContactRecord entity)
        {
           
            context.PassAlongContacts.Add(new PassAlongContact
            {
                Archive = false,
                Timestamp = DateTime.Now,
                ContactRecord = entity,
                PassAlongEmailSent = entity.PassAlong
            });

            context.SaveChanges();
            
        }       
    }
}