using DeaconCCGManagement.DAL;
using DeaconCCGManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DeaconCCGManagement.Models;
using DeaconCCGManagement.SendEmailService;
using SendGrid;
using Elmah;
using System.Net;
using DeaconCCGManagement.PushNotifications;
using System.Threading.Tasks;
using DeaconCCGManagement.Elmah;

namespace DeaconCCGManagement.Services
{
    public class EmailService
    {
        private readonly UnitOfWork _unitOfWork;

        public EmailService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;           
        }

        public async Task<EmailViewModel> SendSingleEmailAsync(EmailViewModel emailViewModel, EmailWrapperClient client, string username)
        {

            Response response;
            try
            {
                response = await client.SendSingleEmailAsync(emailViewModel.EmailContact);

                if (IsResponseOk(response))
                {
                    emailViewModel.StatusNotification = GetStatusNotification(EmailStatus.EmailDelivered, emailViewModel.EmailContact.ReceiverName);
                    emailViewModel.EmailContact.DateSent = DateTime.Now;
                    StoreEmailContact(emailViewModel.EmailContact, username);
                }
                else
                    emailViewModel.StatusNotification = GetStatusNotification(EmailStatus.EmailNotDelivered, emailViewModel.EmailContact.ReceiverName);

            }
            catch (Exception ex)
            {
                // log caught exception with Elmah
                ElmahService.LogException(ex);

                emailViewModel.StatusNotification = GetStatusNotification(EmailStatus.EmailNotDelivered, emailViewModel.EmailContact.ReceiverName);
            }

            if (emailViewModel.StatusNotification != null)
                emailViewModel.HasStatusNotification = true;
            return emailViewModel;


        }

        public async Task<EmailViewModel> SendBulkEmailsAsync(EmailViewModel emailViewModel, EmailWrapperClient client, string username)
        {
            Response response;
            var bulkContacts = GetBulkEmailContacts(emailViewModel);

            try
            {
                response = await client.SendMultipleEmailsAsync(bulkContacts, emailViewModel.EmailContact);
                if (IsResponseOk(response))
                {
                    //GetStatusNotification(EmailStatus.BulkEmailsDelivered);
                    emailViewModel.StatusNotification = GetStatusNotification(EmailStatus.BulkEmailsDelivered);
                    emailViewModel.EmailContact.DateSent = DateTime.Now;                                       
                    StoreBulkEmailContacts(bulkContacts, username);                    
                }
                else
                {
                    //GetStatusNotification(EmailStatus.BulkEmailsNotDelivered);
                    emailViewModel.StatusNotification = GetStatusNotification(EmailStatus.BulkEmailsNotDelivered);
                }
            }
            catch (Exception ex)
            {
                // log caught exception with Elmah
                ElmahService.LogException(ex);

                emailViewModel.StatusNotification = GetStatusNotification(EmailStatus.BulkEmailsNotDelivered);
            }

            if (emailViewModel.StatusNotification != null)
                emailViewModel.HasStatusNotification = true;
            return emailViewModel;
        }   

        private void StoreEmailContact(EmailContact emailContact, string username)
        {
            var member = _unitOfWork.MemberRepository.FindMemberById(emailContact.MemberId);

            // Get the contact type id for 'email'
            var contactType = _unitOfWork.ContactTypeRepository
                .Find(ct => ct.Name.Equals("Email", StringComparison.CurrentCultureIgnoreCase) ||
                            ct.Name.Equals("E-mail", StringComparison.CurrentCultureIgnoreCase));

            // To assign user id to contact record property
            var user = _unitOfWork.AppUserRepository.FindUserByEmail(username);

            var contact = new ContactRecord
            {
                CCGMemberId = member.Id,
                AppUserId = user.Id,
                ContactTypeId = contactType.Id,
                ContactDate = emailContact.DateSent,
                Subject = emailContact.Subject,
                Timestamp = DateTime.Now
            };
            _unitOfWork.ContactRecordRepository.Add(contact);
        }

        private void StoreBulkEmailContacts(List<EmailContact> emailContacts, string username)
        {
            foreach (var email in emailContacts)
            {
                StoreEmailContact(email, username);
            }
        }

        private bool IsResponseOk(Response response)
        {
            if (response.StatusCode.Equals(HttpStatusCode.Accepted)
                   || response.StatusCode.Equals(HttpStatusCode.OK))
            {
                return true;
            }
            return false;
        }

        private NotificationViewModel GetStatusNotification(EmailStatus status, string who = "")
        {
            string title = string.Empty;
            string msg = string.Empty;
            switch (status)
            {
                case EmailStatus.EmailDelivered:
                    title = "Email Sent";
                    msg = $"Your email to {who} has been delivered.";
                    return new NotificationViewModel
                    {
                        Title = title,
                        Message = msg,
                        NotifyType = NotificationType.Success
                    };
                case EmailStatus.EmailNotDelivered:
                    title = "Email Not Sent";
                    msg = $"Your email to {who} was not delivered.";
                    return new NotificationViewModel
                    {
                        Title = title,
                        Message = msg,
                        NotifyType = NotificationType.Failure
                    };
                case EmailStatus.BulkEmailsDelivered:
                    title = "Bulk Emails Sent";
                    msg = $"Your bulk emails were delivered.";
                    return new NotificationViewModel
                    {
                        Title = title,
                        Message = msg,
                        NotifyType = NotificationType.Success
                    };
                case EmailStatus.BulkEmailsNotDelivered:
                    title = "Bulk Emails Not Sent";
                    msg = $"Your bulk emails were not delivered.";
                    return new NotificationViewModel
                    {
                        Title = title,
                        Message = msg,
                        NotifyType = NotificationType.Failure
                    };
            }
            return null;
        }

        private List<EmailContact> GetBulkEmailContacts(EmailViewModel emailViewModel)
        {
            var toAddresses = new List<EmailContact>();
            foreach (var member in emailViewModel.Members)
            {
                var ccgMember = _unitOfWork.MemberRepository.FindMemberById(member.MemberId);
                toAddresses.Add(new EmailContact
                {
                    MemberId = member.MemberId,
                    ToEmailAddress = ccgMember.EmailAddress,
                    ReceiverName = ccgMember.FirstName + " " + ccgMember.LastName,
                    DateSent = emailViewModel.EmailContact.DateSent,
                    Subject = emailViewModel.EmailContact.Subject,
                    PlainTextBody = emailViewModel.EmailContact.PlainTextBody
                });
            }
            return toAddresses;
        }
    }

    public enum EmailStatus
    {
        EmailDelivered,
        EmailNotDelivered,
        BulkEmailsDelivered,
        BulkEmailsNotDelivered
    }
}