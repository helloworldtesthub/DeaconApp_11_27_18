using DeaconCCGManagement.enums;
using DeaconCCGManagement.Services;
using System.Linq;
using System.Web.Mvc;

namespace DeaconCCGManagement.Controllers
{
    public class ExportDataController : ControllerBase
    {
        private ExportDataService _service;
        private CcgMembersService _membersService;
        private ContactRecordsService _contactRecordsService;

        public ExportDataController()
        {
            _service = new ExportDataService(unitOfWork);
            _membersService = new CcgMembersService(unitOfWork);
            _contactRecordsService = new ContactRecordsService(unitOfWork);
        }

        public FileResult CCGMembers(int? ccgId, bool getAll = false,
            string query = null, bool allAccess = false,
            int? memberFilter = (int)MemberFilter.None,
            int? dataDownloadFileType = (int)ExportDataFileType.Xls)
        {
            var fileType = dataDownloadFileType != null ? (ExportDataFileType)dataDownloadFileType : ExportDataFileType.Xls;

            // Get members collection from db
            // TODO  action result not needed here
            ActionResult actionResult;
            var members = _membersService.GetMembersList(ccgId, getAll, query, allAccess,
                User.Identity.Name, out actionResult).ToList();

            // Filter members
            var filter = memberFilter != null ? (MemberFilter)memberFilter : MemberFilter.None;
            _membersService.FilterMembers(ref members, filter, unitOfWork.AppUserRepository.FindUserByEmail(User.Identity.Name));

            switch (fileType)
            {
                case ExportDataFileType.Xls:
                    // Export excel file to user's browser
                    return _service.ExportCCGMembersAsXls(members);
                case ExportDataFileType.Csv:
                    break;
            }
            //TODO temporary
            return new FileContentResult(new byte[] { }, "");
        }

        public FileResult ContactRecords(int? memberId, int? ccgId,
            bool getAll = false, bool archive = false, string query = null,
            int? dateRangeFilter = (int)DateRangeFilter.None,
            int? dataDownloadFileType = (int)ExportDataFileType.Xls)
        {

            var fileType = dataDownloadFileType != null ? (ExportDataFileType)dataDownloadFileType
                : ExportDataFileType.Xls;
            var dateFilter = dateRangeFilter == null ? DateRangeFilter.ThreeMonths
                : (DateRangeFilter)dateRangeFilter;

            switch (fileType)
            {
                case ExportDataFileType.Xls:

                    // Get contact records
                    ActionResult actionResult;
                    var user = unitOfWork.AppUserRepository.FindUserByEmail(User.Identity.Name);

                    int totalItemsCount = 0;

                    var contactRecords = _contactRecordsService.GetContactRecords(memberId, ccgId, getAll,
                        archive, user, dateFilter, query, out actionResult, out totalItemsCount);

                    contactRecords = _contactRecordsService.SearchContactRecords(query, contactRecords);
                    contactRecords = _contactRecordsService.RemovePrayerRequests(contactRecords);

                    // Filter contact records by date range (eg. last 6 months)
                    //_contactRecordsService.FilterContactsByDateRange(contactRecords.ToList(), dateFilter);

                    // Export contact records to browser as xls file
                    return _service.ExportContactRecordsAsXls(contactRecords);


                case ExportDataFileType.Csv:
                    break;
            }
           
            return new FileContentResult(new byte[] { }, "");
        }

        public FileResult PrayerRequests(int? memberId, int? ccgId,
            int? dateRangeFilter = (int)DateRangeFilter.TwoWeeks,
            int? sortOption = (int)ContactsSort.DateDescending,
            bool getAll = false, string query = null)
        {

            var prayerReqService = new PrayerRequestService(unitOfWork);

            // Cast int passed by route to an enum
            var dateFilter = dateRangeFilter != null ? (DateRangeFilter)dateRangeFilter : DateRangeFilter.TwoWeeks;
            var contactsSort = sortOption == null ? ContactsSort.DateDescending : (ContactsSort)sortOption;

            // Get principal user obj
            var user = unitOfWork.AppUserRepository.FindUserByEmail(User.Identity.Name);

            int totalItemsCount = 0;

            var prayerRequests = prayerReqService.PrayerRequests(null, null, out totalItemsCount, memberId, ccgId, dateFilter, contactsSort, getAll, query, user);

            // Export prayer requests to browser as xls file
            return _service.ExportPrayerRequestsAsXls(prayerRequests);
        }

        public FileResult AppUsers()
        {
            return new FileContentResult(new byte[] { }, "");
        }
    }
}