using DeaconCCGManagement.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security.AntiXss;
using DeaconCCGManagement.Models;
using DeaconCCGManagement.ViewModels;
using DeaconCCGManagement.enums;
using DeaconCCGManagement.Helpers;
using System.Configuration;

namespace DeaconCCGManagement.Services
{
    public class PrayerRequestService : ContactRecordsService
    {
        public PrayerRequestService(UnitOfWork unitOfWork) : base(unitOfWork)
        {
            
        }

        public List<ContactRecord> PrayerRequests(int? page, int? itemsPerPage, out int totalItemsCount, int? memberId, int? ccgId,
            DateRangeFilter dateRange, ContactsSort contactsSort,
            bool getAll = false, string query = null, CCGAppUser appuser=null)
        {
            totalItemsCount = 0;

            // Get principal user obj
            var user = unitOfWork.AppUserRepository.FindUserByEmail(appuser.UserName);

            DateTime dateTimeOffset;
            GetOffsetDate(dateRange, out dateTimeOffset);

            List<ContactRecord> prayerRequests;

            // get all if user is admin, leadership, or pastor
            getAll = base.CanViewAllRecords(user.Email);

            // not leadership so only get prayer requests within ccg
            if (!getAll && ccgId == null) ccgId = user.CcgId;

            prayerRequests = GetPrayerRequests(page, itemsPerPage, out totalItemsCount, memberId,
                dateTimeOffset, ccgId, getAll, user, query).ToList();

            // Sort prayer requests
            return SortContactRecords(contactsSort, prayerRequests).ToList();
        }


        public IEnumerable<ContactRecord> GetPrayerRequests(int? page, int? itemsPerPage, 
            out int totalItemsCount, int? memberId, 
            DateTime dateTimeOffset, int? ccgId, bool getAll, CCGAppUser user, string query)
        {
            // Prevents having one query for search and one for no search.
            if (query == null) query = "";

            var contactType = GetPrayerRequestContactType();          

            if (memberId != null)
            {
                // Get all contact requests that are prayer requests for a member
                return unitOfWork.ContactRecordRepository
                    .GetContactRecords(out totalItemsCount, cr => cr.ContactTypeId == contactType.Id
                                && cr.CCGMemberId == memberId && cr.Timestamp > dateTimeOffset
                                && (cr.Subject.Contains(query) || cr.Comments.Contains(query)), page, itemsPerPage).ToList();
            }
            if (ccgId != null && ccgId != -1)
            {
                // Get all contact requests that are prayer requests for a CCG
                return unitOfWork.ContactRecordRepository
                    .GetContactRecords(out totalItemsCount, cr => cr.ContactTypeId == contactType.Id
                                && cr.CCGMember.CcgId == ccgId && cr.Timestamp > dateTimeOffset
                                && (cr.Subject.Contains(query) || cr.Comments.Contains(query)), page, itemsPerPage).ToList();
            }
            if (!getAll)
            {
                // Get prayer request for deacon's ccg only
                return unitOfWork.ContactRecordRepository
                    .GetContactRecords(out totalItemsCount, cr => cr.ContactTypeId == contactType.Id
                    && cr.AppUser.CcgId == user.CcgId && cr.Timestamp > dateTimeOffset
                    && (cr.Subject.Contains(query) || cr.Comments.Contains(query)), page, itemsPerPage).ToList();
            }

            // getAll is true
            // Get all contact requests that are prayer requests
            return unitOfWork.ContactRecordRepository
                .GetContactRecords(out totalItemsCount, cr => cr.ContactTypeId == contactType.Id
                && cr.Timestamp > dateTimeOffset
                && (cr.Subject.Contains(query) || cr.Comments.Contains(query)), page, itemsPerPage).ToList();
            
        } 

        public ContactType GetPrayerRequestContactType()
        {
            // Used to find contact type object
            string contactTypePR = "Prayer Request";

            // Get contact type object that matches 'Prayer Request'
            var contactType = unitOfWork.ContactTypeRepository
                .Find(t => t.Name.Equals(contactTypePR, StringComparison.CurrentCultureIgnoreCase));
            return contactType;
        }
     

        public PrayerRequestViewModel SanitizePrayerRequestViewModel(PrayerRequestViewModel viewModel)
        {
            bool sanitize = bool.Parse(ConfigurationManager.AppSettings["SanitizeContactRecords"]);

            if (sanitize)
            {
                viewModel.Comments = AntiXssEncoder.HtmlEncode(viewModel.Comments, false);
                viewModel.PassAlongComments = AntiXssEncoder.HtmlEncode(viewModel.PassAlongComments, false);
                viewModel.Subject = AntiXssEncoder.HtmlEncode(viewModel.Subject, false);
                viewModel.PassAlongComments = AntiXssEncoder.HtmlEncode(viewModel.PassAlongFollowUpComments, false);
            }
            return viewModel;
        }
    }
}