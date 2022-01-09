using System;
using static Domain.Emails.EmailTemplates;
using PostmarkDotNet;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Domain.Repository;
using System.Dynamic;
using System.Collections.Generic;
using Domain.Utilities;

namespace Domain.Emails
{
    public static class SendEmailService
    {
        //public static void Send(string email, EmailTypes template, object model)
        //{
        //    SendAsync(email, template, model).ConfigureAwait(false);
        //}
        /* Keep for a quick flip in case of issues
         * 
         * - this goes in the caller to the below 
            var emailer = new Domain.Email.SendEmailService();
            var mailMessage = new MailMessage();
            // so we have some accounts - Kibbey kids - without email addresses
            if (!string.IsNullOrWhiteSpace(email)) {
                mailMessage.To.Add(email);
                emailer.Send(mailMessage, template, model);
            }
        private static void Send(MailMessage mailMessage, EmailTypes TemplateName, object Model)
        {
            if (mailMessage.From == null || string.IsNullOrEmpty(mailMessage.From.Address))
            {
                string from = "Fyli";
                if (TemplateName == EmailTypes.ConnectionRequest || TemplateName == EmailTypes.ConnectionRequestNewUser) {
                    from = $"{Model.GetType().GetProperty("User").GetValue(Model, null)} via Fyli";
                }  
                mailMessage.From = new MailAddress(EmailAddress, from);
            }

            mailMessage.Body = Email.EmailRender.GetStringFromView(EmailTemplates.GetTemplateByName(TemplateName), Model);
            mailMessage.Subject = Email.EmailRender.GetStringFromView(EmailTemplates.GetSubjectByName(TemplateName), Model);
            mailMessage.IsBodyHtml = true;
            mailMessage.BodyEncoding = Encoding.UTF8;
           
            if (InProduction)
            {
                var smtpClient = new SmtpClient();
                smtpClient.Host = "smtp.gmail.com";
                smtpClient.EnableSsl = true;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Port = 587;
                smtpClient.Credentials = new System.Net.NetworkCredential(EmailAddress, EmailPW);
                smtpClient.Send(mailMessage);
            }
        }*/

        public static async Task SendAsync(string email, EmailTypes template, object model)
        {
            var extendedModel = await AddTokenToModel(email, template, model).ConfigureAwait(false);
            var from = GetFrom(template, model);
            var body = EmailRender.GetStringFromView(EmailTemplates.GetTemplateByName(template), extendedModel);
            // Outlook does NOT like /#/ in a Url - this is a substitute with re-write url
            body = EmailSafeLinkCreator.FindAndReplaceLinks(body);
            var subject = EmailRender.GetStringFromView(EmailTemplates.GetSubjectByName(template), model);
            var text = GetPlainTextFromHtml(body);
            await SendPostmarkEmail(email, from, subject, text, body, template.ToString(), subject).ConfigureAwait(false);
        }

        private static async Task<ExpandoObject> AddTokenToModel(string email, EmailTypes template, object model) {
            var expando = new ExpandoObject();
            IDictionary<string, object> dictionary;
            if (model is ExpandoObject)
            {
                expando = (ExpandoObject)model;
                dictionary = (IDictionary<string, object>)expando;
            }
            else {
                dictionary = (IDictionary<string, object>)expando;
                foreach (var property in model.GetType().GetProperties())
                    dictionary.Add(property.Name, property.GetValue(model));
            }

            if (AddToken(template))
            {
                dictionary.Add("Link", await CreateLinkToken(email).ConfigureAwait(false));
            }

            return expando;
        }

        private static bool AddToken(EmailTypes emailType)
        {
            return TokenAddedEmails.Contains(emailType);
        }

        private static HashSet<EmailTypes> TokenAddedEmails = new HashSet<EmailTypes>{
            EmailTypes.ConnectionRequest,
            EmailTypes.ConnectionRequestQuestion,
            EmailTypes.Question,
            EmailTypes.CommentEmail,
            EmailTypes.ThankEmail,
            EmailTypes.EmailNotification,
            EmailTypes.TimelineShare,
            EmailTypes.TimelineInviteExisting,
            EmailTypes.Suggestions,
            EmailTypes.Requests,
            EmailTypes.QuestionReminders
        };

        private static async Task<string> CreateLinkToken(string email) {
            using (var userService = new UserService()) {
                var token = await userService.CreateLinkToken(email).ConfigureAwait(false);
                return token.Success ? token.Token : string.Empty;
            }
        }

        public static string GetFrom(EmailTypes emailTypes, object Model) {
            string from = "Fyli";
            if (emailTypes == EmailTypes.ConnectionRequest || 
                emailTypes == EmailTypes.ConnectionRequestNewUser ||
                emailTypes == EmailTypes.ConnectionRequestNewUserQuestion ||
                emailTypes == EmailTypes.ConnectionRequestQuestion ||
                emailTypes == EmailTypes.TimelineShare ||
                emailTypes == EmailTypes.TimelineInviteNew ||
                emailTypes == EmailTypes.TimelineInviteExisting)
            {
                if (Model is ExpandoObject) {
                    var dictionary = (IDictionary<string, object>)Model;
                    from = $"{dictionary["User"]} via Fyli";
                } else {
                    from = $"{Model.GetType().GetProperty("User").GetValue(Model, null)} via Fyli";
                }
            }
            return from;
        }

        public static async Task SendPostmarkEmail(string email, string from, string subject, string text, string body, string tag, string preview) {
            var to = InProduction ? email : "information@fyli.com";
            // comment out to test - only 100 test emails per month
            if (!InProduction) return;

            var message = new PostmarkMessage()
            {
                To = to,
                From = $"{from} <information@fyli.com>",
                TrackOpens = true,
                TrackLinks = LinkTrackingOptions.HtmlOnly,
                Subject = subject,
                TextBody = text,
                HtmlBody = body,
                Tag = tag,
                /*
                Headers = new HeaderCollection{
                    {"X-CUSTOM-HEADER", preview}
                  } */
            };

            var client = new PostmarkClient(PostMarkToken);
            var sendResult = await client.SendMessageAsync(message).ConfigureAwait(false);

            if (sendResult.Status == PostmarkStatus.Success) { /* Handle success */ }
            else { /* Resolve issue.*/ }
        }

        private static string GetPlainTextFromHtml(string htmlString)
        {
            try {
                htmlString = Regex.Replace(htmlString, htmlTagPattern, string.Empty);
                htmlString = Regex.Replace(htmlString, @"^\s+$[\r\n]*", Environment.NewLine, RegexOptions.Multiline);

                return htmlString;
            } catch (Exception e) {
                return string.Empty;
            }
        }

        private static string htmlTagPattern = "<.*?>";
        private static bool InProduction = Convert.ToBoolean(ConfigurationFactory.GetConfigurationValue("Production") ?? "false");
        private static string EmailAddress = ConfigurationFactory.GetConfigurationValue("Owner") ?? "";
        private static string EmailPW = ConfigurationFactory.GetConfigurationValue("EmailCode") ?? "";
        private static string PostMarkToken = ConfigurationFactory.GetConfigurationValue("EmailToken") ?? "";
    }
}

       