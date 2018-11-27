using DeaconCCGManagement.DeaconAnnouncements;
using System.Web.Mvc;


namespace DeaconCCGManagement.Controllers
{
    public class HomeController : ControllerBase
    {

        public ActionResult Index()
        {
            //bool useLocalEmulator =
            //    bool.Parse(ConfigurationManager.AppSettings["UseLocalStorageEmulator"]);

            // only call this if using local Azure storage emulator
            // otherwise an Azure function will be used
            //if (useLocalEmulator)
            //    AnnouncementHelper.RemoveOldAnnouncements();
            var viewModel = AnnouncementHelper.GetAllAnnouncements();

            return View(viewModel);         
        }

        public ActionResult About()
        {
            ViewBag.Message = "About this ZMBC Deacons CCG Management App.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your ZMBC Contacts.";

            return View();
        }
        
    }
}