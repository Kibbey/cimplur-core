using Domain.Emails;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using static Domain.Emails.EmailTemplates;

namespace Domain.Repository
{
    public class ContactService : BaseService
    {
        private SendEmailService sendEmailService;

        public ContactService(SendEmailService sendEmailService) {
            this.sendEmailService = sendEmailService;
        }
        public async Task SendMessage(string email, string name, string content, int userId) {
            content += "- email - " + email;
            await sendEmailService.SendAsync(Domain.Models.Constants.Email, EmailTypes.Contact, new { Name = name, Content = content });
            var contact = new ContactRequest {
                Name = name,
                Email = email,
                Content = content,
                UserId = userId
            };
            Context.ContactRequests.Add(contact);
            await Context.SaveChangesAsync();
        }

        public async Task SendEmailToUsers() {
            var users = await Context.UserProfiles.Where(x => x.Email != null).ToListAsync();
            foreach (var user in users) {
                await sendEmailService.SendAsync(user.Email, EmailTypes.Fyli, new { Name = user.Name });
            }
        }
    }
}
