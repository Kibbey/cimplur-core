using System;
using System.Threading.Tasks;

namespace Domain.Repository
{
    public class EventService : BaseService
    {
        public static async Task EmitEvent(string name) {
            await EmitEvent(name, -1).ConfigureAwait(false);
        }

        public static async Task EmitEvent(string name, int userId) {
            await Task.Run(() => new EventService().SaveEvent(name, userId)).ConfigureAwait(false);
        }

        private async Task SaveEvent(string name, int userId) {
            Context.Usages.Add(new Entities.Usage {
                EventName = name,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });
            await Context.SaveChangesAsync();
        }

        public static string AddDrop = "ADD_DROP";

        public static string EditDrop = "EDIT_DROP";

        public static string ViewDrop = "VIEW_DROP";


    }
}
