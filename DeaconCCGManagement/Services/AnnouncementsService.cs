using DeaconCCGManagement.DAL;
using DeaconCCGManagement.Models;
using DeaconCCGManagement.SendEmailService;
using DeaconCCGManagement.SmsService;
using DeaconCCGManagement.Utilities;
using DeaconCCGManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace DeaconCCGManagement.Services
{
    public class AnnouncementsService
    {
        private readonly UnitOfWork _unitOfWork;

        public AnnouncementsService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task SendBulkEmailToAllDeacons(AnnouncementViewModel viewModel, string username)
        {
            var user = _unitOfWork.AppUserRepository.FindUserByEmail(username);
            var deacons = _unitOfWork.AppUserRepository.FindUsersByRole(enums.AppUserRole.Deacon);
            var emailContacts = new List<EmailContact>();

            var emailClient = new EmailWrapperClient();

            var email = new EmailContact
            {     
                FromEmailAddress = user.EmailAddress,
                Subject = viewModel.Title,
                PlainTextBody = HtmlRemoval.ConvertHtmlToPlainText(viewModel.AnnouncementHtml),
                HtmlBody = viewModel.AnnouncementHtml
            };
            
            foreach (var deacon in deacons)
            {
                emailContacts.Add(new EmailContact
                {
                    ToEmailAddress = deacon.EmailAddress,
                    ReceiverName = deacon.FullName
                });
            }           

            var response = await emailClient.SendMultipleEmailsAsync(emailContacts, email);
        }

        public async Task SendBulkSMSToAllDeacons(string message)
        {
            string fromNumber = GetFromNumber();
            var deacons = _unitOfWork.AppUserRepository.FindUsersByRole(enums.AppUserRole.Deacon);

            var smsClient = new TwillioWrapperClient();
            smsClient.Init();

            foreach (var deacon in deacons)
            {
                var smsMessage = new SmsMessage
                {
                    ToNumber = deacon.PhoneNumber,
                    FromNumber = fromNumber,
                    Message = message
                };

                // Send SMS message
                var response = await smsClient.SendSmsAsync(smsMessage);
            }          

        }


        private string GetFromNumber()
        {
            return ConfigurationManager.AppSettings["TwillioFromNumber"];  
        }

    }
}