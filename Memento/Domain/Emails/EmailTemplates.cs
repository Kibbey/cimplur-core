

using Domain.Models;
using System;

namespace Domain.Emails
{
    public class EmailTemplates
    {
       
        public static string GetTemplateByName(EmailTypes name) {
            return WrapInTemplate(GetBody(name), GetSubjectByName(name), name);
        }

        private static string GetBody(EmailTypes name) {
            switch (name)
            {
                case EmailTypes.Receipt:
                    return @"<p>Thank you for your purchase of a Fyli Family Plan!</p> <p>You will see a charge from Fyli.com of "+ Constants.PremiumPlanCost.ToString("C") +" on your statement.</p>";
                case EmailTypes.ForgotPassword:
                    return @"<p>We have recently recieved a request to recover your user name or password.  Your user name is @Model.UserName.  You can reset your password <a href='" + Constants.BaseUrl + "/#/resetPassword?token=@Model.Token'>here</a>.</p>";
                case EmailTypes.ConnectionRequest:
                    return @"<p>@Model.User wants to share memories with you on Fyli.   See what they shared <a href='" + Constants.BaseUrl + "/#/sharingRequest?request=@Model.Token&link=@Model.Link'>here</a>.</p>";
                case EmailTypes.ConnectionRequestNewUser:
                    return @"<p>@Model.User wants to preserve and share memories with you on Fyli.   See what they shared <a href='" + Constants.BaseUrl + "/#/sharingRequest?request=@Model.Token'>here</a>.</p>" + aboutFyli;
                case EmailTypes.EmailNotification:
                    return @"<p>@Model.User has shared with you on <a href='" + Constants.BaseUrl + "/#/memory?dropId=@Model.DropId&link=@Model.Link'>Fyli</a>.</p>";
                case EmailTypes.ClaimEmail:
                    return @"<p>@Model.User wants Fyli requests for this email to be sent to @Model.User.  Click <a href='" + Constants.BaseUrl + "/Connection/ClaimEmail?token=@Model.Token'>here</a> to enable or ingore to do nothing.</p>";
                case EmailTypes.CommentEmail:
                    return @"<p>@Model.User has commented on a memory you are following on <a href='" + Constants.BaseUrl + "/#/memory?dropId=@Model.DropId&link=@Model.Link'>Fyli</a>.</p>";
                case EmailTypes.ThankEmail:
                    return @"<p>@Model.User has thanked you for sharing a memory <a href='" + Constants.BaseUrl + "/#/memory?dropId=@Model.DropId&link=@Model.Link'>Fyli</a>.</p>";
                case EmailTypes.ConnecionSuccess:
                    return@"<p>@Model.User has accepted your sharing request.</p>";
                case EmailTypes.Contact:
                    return @"<p>@Model.Name has submitted the following:</p><p>@Model.Content</p>";
                case EmailTypes.SignUp:
                    return @"New sign up from @Model.Name.";
                case EmailTypes.PaymentNotice:
                    return @"New payment from @Model.Email.";
                case EmailTypes.InviteNotice:
                    return @"New invite from @Model.Name.";
                case EmailTypes.Feedback:
                    return @"New feedback request from @Model.Name.";
                case EmailTypes.Question:
                    return @"<p>@Model.User asked you a question on Fyli-</p><p><i><b>@Model.Question</b></i></p><p>Answer the question they asked you <a href='" + Constants.BaseUrl + "/#/memory/add?questionId=@Model.Id&link=@Model.Link'>here</a>.</p>";
                case EmailTypes.ConnectionRequestQuestion:
                    return @"<p>@Model.User asked you a question on Fyli-</p><p><i><b>@Model.Question</b></i></p><p>Answer the question they asked you <a href='" + Constants.BaseUrl + "/#/sharingRequest?request=@Model.Token'>here</a>.</p>";
                case EmailTypes.ConnectionRequestNewUserQuestion:
                    return @"<p>@Model.User asked you a question on Fyli-</p><p><i><b>@Model.Question</b></i></p><p>Answer the question they asked you <a href='" + Constants.BaseUrl + "/#/sharingRequest?request=@Model.Token'>here</a>.</p>" + aboutFyli;
                case EmailTypes.Login:
                    return @"<p>@Model.Name, click this <a href='" + Constants.BaseUrl + "/#/?link=@Model.Token'>link</a> to log in to your Fyli account.</p>";
                case EmailTypes.Welcome:
                    return @"<p>Welcome to Fyli @Model.Name!</p><p>We are excited that you have started this journey to Preserve, Discover and Share memories with those that matter most to you. We are dedicated to creating an amazing experience for you.</p><p>Would you like to be able to tell us directly the things you love, the things you want to change, and any ideas you have for making Fyli better?</p><p>If you would be open to sharing your thoughts to help us make Fyli better for everyone (Awesome!) click 'YES' below, and we will reach out to you soon.</p><p>Team Fyli</p>" +
                         "<p style=\"text-align: center\"><a href='" + Constants.BaseUrl + "/#/welcome?link=@Model.Token' style=\"font-family: Arial,Helvetica,sans-serif; font-size: 17px; background-color: rgb(48, 165, 56); border: 2px solid rgb(0, 128, 0); border-radius: 10px; font-weight: 700; line-height: 44px; padding: 10px 24px; text-align: center; text-decoration-line: none; vertical-align: middle; color: rgb(255, 255, 255);\">Yes</a></p>";
                case EmailTypes.Fyli:
                    return @"<p>@Model.Name - We are pleased to announce that cimplur is now Fyli.  You can still log in the same way and nothing else will have changed.</p><p>We value your feedback.  If you have any ideas on how to make Fyli (cimplur) better, please email us at information@fyli.com</p></p>";
                case EmailTypes.TimelineInviteExisting:
                    return @"<p>@Model.User wants help preserving stories about @Model.TimelineName on Fyli.</p><p>What is your favorite memory of <a href='" + Constants.BaseUrl + "/#/sharingRequest?request=@Model.Token&link=@Model.Link'>@Model.TimelineName</a>?</p>";
                case EmailTypes.TimelineInviteNew:
                    return @"<p>@Model.User wants help preserving stories about @Model.TimelineName on Fyli.</p><p>What is your favorite memory of <a href='" + Constants.BaseUrl + "/#/sharingRequest?request=@Model.Token'>@Model.TimelineName</a>?</p>" + aboutFyli;
                case EmailTypes.TimelineShare:
                    return @"<p>@Model.User wants help preserving stories about @Model.TimelineName on Fyli.</p><p>What is your favorite memory of <a href='" + Constants.BaseUrl + "/#/timelines/@Model.TimelineId?link=@Model.Link'>@Model.TimelineName</a>?</p>";
                case EmailTypes.Suggestions:
                    return @"<p>Connect with <span style='text-transform: capitalize'>@Model.Name</span> and others on <a href='" + Constants.BaseUrl + "/#/sharing?link=@Model.Link'>Fyli</a>.</p>";
                case EmailTypes.Requests:
                    return @"<p>You have connection requests waiting for you on <a href='" + Constants.BaseUrl + "/#/sharing?link=@Model.Link'>Fyli</a>.</p>";
                case EmailTypes.QuestionReminders:
                    return @"<p><span style='text-transform: capitalize'>@Model.Name</span> asked you:</p> <p><i><b>@Model.Question</b></i></p> <p>Help preserve your family's history by <a href='" + Constants.BaseUrl + "/#/questions?link=@Model.Link'>sharing</a> your memories about this and other questions you have been asked.</p>";

                default:
                    throw new Exception("Email template not found:" + name.ToString());
            }
        }

        private static string aboutFyli = "<p>Fyil is the private place for family to Preserve and Share the memories that matter most.</p>";

        public static string GetSubjectByName(EmailTypes name)
        {
            switch (name)
            {
                case EmailTypes.Receipt:
                    return @"Thank you for purchase of a Fyli Family Plan";
                case EmailTypes.ForgotPassword:
                    return @"Password reset request";
                case EmailTypes.ConnectionRequest:
                    return @"@Model.User wants to share memories with you";
                case EmailTypes.ConnectionRequestNewUser:
                    return @"@Model.User wants to share memories with you";
                case EmailTypes.EmailNotification:
                    return @"Memory shared with you on Fyli";
                case EmailTypes.ClaimEmail:
                    return @"Claim Fyli email";
                case EmailTypes.CommentEmail:
                    return @"Comment on Fyli";
                case EmailTypes.ThankEmail:
                    return @"@Model.User thanked you for sharing on Fyli";
                case EmailTypes.ConnecionSuccess:
                    return @"@Model.User has accepted your sharing request";
                case EmailTypes.Contact:
                    return @"Contact from website form";
                case EmailTypes.SignUp:
                    return @"New sign up";
                case EmailTypes.PaymentNotice:
                    return @"New payment";
                case EmailTypes.InviteNotice:
                    return @"New invite from @Model.Name.";
                case EmailTypes.Login:
                    return @"@Model.Name's login link";
                case EmailTypes.Welcome:
                    return @"Welcome to Fyli @Model.Name!";
                case EmailTypes.Fyli:
                    return @"Cimplur is now Fyli";
                case EmailTypes.Feedback:
                    return @"Feeback Notice";
                case EmailTypes.ConnectionRequestQuestion:
                    return @"@Model.User wants to share memories with you";
                case EmailTypes.ConnectionRequestNewUserQuestion:
                    return @"@Model.User wants to share a memory with you";
                case EmailTypes.Question:
                    return @"@Model.User asked you a question on Fyli";
                case EmailTypes.TimelineInviteExisting:
                    return @"@Model.User wants help preserving stories about @Model.TimelineName";
                case EmailTypes.TimelineInviteNew:
                    return @"@Model.User wants help preserving stories about @Model.TimelineName";
                case EmailTypes.TimelineShare:
                    return @"@Model.User wants help preserving stories about @Model.TimelineName";
                case EmailTypes.Suggestions:
                    return @"You have connection suggestions on Fyli";
                case EmailTypes.Requests:
                    return @"You have connection requests on Fyli";
                case EmailTypes.QuestionReminders:
                    return @"Reminder - questions are waiting for you on Fyli";

                default:
                    throw new Exception("Email template not found:" + name.ToString());
            }
        }

        public static string Receipt = "Receipt";
        public static string ForgotPassword = "ForgotPassword";
        public static string ConnectionRequest = "ConnectionRequest";
        private static string messagePlaceHolder = "{{message}}";
        private static string titlePlaceHolder = "{{title}}";
        public enum EmailTypes
        {
            Receipt = 1,
            ForgotPassword = 2,
            ConnectionRequest = 3,
            ConnectionRequestNewUser = 4,
            EmailNotification = 5,
            ClaimEmail = 6,
            CommentEmail = 7,
            ConnecionSuccess = 8,
            Contact = 9,
            SignUp = 10,
            Login = 11,
            Fyli =12,
            Welcome = 13,
            Feedback = 14,
            ConnectionRequestQuestion = 15,
            ConnectionRequestNewUserQuestion = 16,
            Question = 17,
            InviteNotice = 18,
            TimelineInviteExisting = 19,
            TimelineInviteNew = 20,
            TimelineShare = 21,
            PaymentNotice = 22,
            Suggestions = 23,
            Requests = 24,
            QuestionReminders = 25,
            ThankEmail = 26,
        }

        private static string WrapInTemplate(string message, string subject, EmailTypes emailtype) {
            return GetTemplate(emailtype).Replace(messagePlaceHolder, message).Replace(titlePlaceHolder,subject);
        }

        private static string GetTemplate(EmailTypes emailTypes ) {
            return @"<!DOCTYPE html><html> " +
                "<head>" +
                    "<title>" +
                        titlePlaceHolder +
                    "</title>" +
               "</head>" +
                "<body> " +
                    "<div style = \"background-color: #F7F7F7; padding-top: 30px; padding-bottom: 30px; height: 100%;\"> " +
                        "<table style = \"width: 400px; background-color: #FFF; margin: auto; font-family: worksans-extralight, 'Work Sans', sans-serif;\"> " +
                            "<tbody> " +
                                "<tr> " +
                                    "<th style = \"color: #40b3e7; font-weight:400; text-align: center; font-size: 2.3em; padding-top: 30px\"> " +
                                       "<img style=\"height: 34px; width: 84px;\" src='https://fyli.com/images/fyli-line-dark-text.png'/>" +
                                    "</th> " +
                               "</tr> " +
                               "<tr> " +
                                    "<td style = \"font-size: 1.3em; text-align: left; padding: 20px 30px 20px 30px;\"> " +
                                        messagePlaceHolder +
                                    "</td>" +
                                "</tr>" +
                                "<tr>" +
                                    "<td style=\"font-size:x-small; text-align: center; padding: 10px 0 0 0;\" >" +
                                         "<p> Fyli &#169; - Half Moon Bay, California 94019 </p>" +
                                    "</td>" +
                                "</tr>" +
                                "<tr>" +
                                    "<td style=\"font-size:x-small; text-align: center; padding: 10px 0 10px 0;\"> " +
                                         "<a data-pm-no-track style=\"text-decoration: none;\" href='" + Constants.BaseUrl +"/#/emailSubscription'>Manage email subscription</a>." +
                                        "</td> " +
                                "</tr>" +
                            "</tbody>" +
                        "</table>" +
                    "</div>" +
                "</body>" +
            "</html>";
        }


    }
}
