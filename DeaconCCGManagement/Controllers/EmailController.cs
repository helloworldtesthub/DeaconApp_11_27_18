using DeaconCCGManagement.SendEmailService;
using DeaconCCGManagement.Services;
using DeaconCCGManagement.Utilities;
using DeaconCCGManagement.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security.AntiXss;

namespace DeaconCCGManagement.Controllers
{
    [Authorize]
    public class EmailController : ControllerBase
    {
        private EmailService _service;

        public EmailController()
        {
            _service = new EmailService(unitOfWork);
        } 

        public ActionResult SendEmail(int memberId)
        {           

            var emailContact = new EmailContact();

            var user = unitOfWork.AppUserRepository.FindUserById(User.Identity.GetUserId());
            var member = unitOfWork.MemberRepository.FindMemberById(memberId);

            emailContact.FromEmailAddress = user.Email;
            emailContact.ToEmailAddress = member.EmailAddress;
            emailContact.MemberId = memberId;
            emailContact.SenderName = user.FullName;
            emailContact.DateSent = DateTime.Now;
            emailContact.PlainTextBody = string.Empty;
            emailContact.ReceiverName = $"{member.FirstName} {member.LastName}";


            var emailViewModel = new EmailViewModel
            {
                EmailContact = emailContact,
                Bulk = false,
                IsTesting = bool.Parse(ConfigurationManager.AppSettings["TestSendGrid"])
            };

            emailViewModel.StatusNotification = new NotificationViewModel();

            return View(emailViewModel);
        }

        public ActionResult SendBulkEmail()
        {
            var emailViewModel = TempData["EmailViewModel"];

            if (emailViewModel == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View("SendEmail", emailViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> SendEmail(EmailViewModel emailViewModel)
        {
            if (!ModelState.IsValid) return View(emailViewModel);

            emailViewModel.UserEmail = User.Identity.Name;

            // sanitize the html from user
            emailViewModel.EmailContact.HtmlBody =
                AntiXssEncoder.HtmlEncode(emailViewModel.EmailContact.HtmlBody, false);
            emailViewModel.EmailContact.HtmlBody = 
                WebUtility.HtmlDecode(emailViewModel.EmailContact.HtmlBody);



            if (emailViewModel == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var client = new EmailWrapperClient();

            if (emailViewModel.IsTesting && !string.IsNullOrEmpty(emailViewModel.TestToEmail))
            {
                emailViewModel.EmailContact.ToEmailAddress = emailViewModel.TestToEmail;
                emailViewModel.EmailContact.ReceiverName = "Deacon App Test SendGrid";
            }

            emailViewModel.EmailContact.PlainTextBody =
                HtmlRemoval.ConvertHtmlToPlainText(emailViewModel.EmailContact.HtmlBody);

            if (emailViewModel.Bulk)
                emailViewModel = await _service.SendBulkEmailsAsync(emailViewModel, client, User.Identity.Name);

            else // send email to one member 
                emailViewModel = await _service.SendSingleEmailAsync(emailViewModel, client, User.Identity.Name);

            return View("EmailComplete", emailViewModel);
        }

 
    }

    
}