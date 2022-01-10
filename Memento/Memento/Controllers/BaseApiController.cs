using Domain.Repository;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Memento.Web.Controllers
{
    [ApiController]
    public class BaseApiController : ControllerBase, IDisposable
    {
        public BaseApiController() {

        }

        private int? userId;

        protected int CurrentUserId
        {
            get
            {
                if (!userId.HasValue) {
                    userId = getUserId();
                }
                return userId.Value;
            }
        }

        private int getUserId() {
            var userIdString = (int?)HttpContext.Items["UserId"];
            int userId = 0;
            if (userIdString.HasValue)
            {
                userId = userIdString.Value;
            }
            return userId;
        }

        private DropsService dropsService;

        protected DropsService DropsService
        {
            get
            {
                if (dropsService == null)
                {
                    dropsService = new DropsService();
                }
                return dropsService;
            }
        }

        private GroupService groupService;

        protected GroupService GroupsService
        {
            get
            {
                if (groupService == null)
                {
                    groupService = new GroupService();
                }
                return groupService;
            }
        }

        private ImageService imageService;

        protected ImageService ImageService
        {
            get
            {
                if (imageService == null)
                {
                    imageService = new ImageService();
                }
                return imageService;
            }
        }

        private MovieService movieService;

        protected MovieService MovieService
        {
            get
            {
                if (movieService == null)
                {
                    movieService = new MovieService();
                }
                return movieService;
            }
        }

        private UserService userService;

        protected UserService UserService
        {
            get
            {
                if (userService == null)
                {
                    userService = new UserService();
                }
                return userService;
            }
        }

        private NotificationService notificationService;

        protected NotificationService NotificationService
        {
            get
            {
                if (notificationService == null)
                {
                    notificationService = new NotificationService();
                }
                return notificationService;
            }
        }

        private SharingService sharingService;

        protected SharingService SharingService
        {
            get
            {
                if (sharingService == null)
                {
                    sharingService = new SharingService();
                }
                return sharingService;
            }
        }

        private ContactService contactService;

        protected ContactService ContactService
        {
            get
            {
                if (contactService == null)
                {
                    contactService = new ContactService();
                }
                return contactService;
            }
        }

        private PlanService planService;

        protected PlanService PlanService
        {
            get
            {
                if (planService == null)
                {
                    planService = new PlanService();
                }
                return planService;
            }
        }

        private TransactionService transactionService;

        protected TransactionService TransactionService
        {
            get
            {
                if (transactionService == null)
                {
                    transactionService = new TransactionService();
                }
                return transactionService;
            }
        }

        private AlbumService albumService;

        protected AlbumService AlbumService
        {
            get
            {
                if (albumService == null)
                {
                    albumService = new AlbumService();
                }
                return albumService;
            }
        }

        private TimelineService timelineService;

        protected TimelineService TimelineService
        {
            get
            {
                if (timelineService == null)
                {
                    timelineService = new TimelineService();
                }
                return timelineService;
            }
        }

        private PromptService promptService;

        protected PromptService PromptService
        {
            get
            {
                if (promptService == null)
                {
                    promptService = new PromptService();
                }
                return promptService;
            }
        }

        private ExportService exportService;

        protected ExportService ExportService 
        {
            get 
            {
                if (exportService == null) {
                    exportService = new ExportService();
                }
                return exportService;
            }
        }

        void IDisposable.Dispose()
        {
            if (dropsService != null)
            {
                dropsService.Dispose();
            }

            if (groupService != null)
            {
                groupService.Dispose();
            }

            if (imageService != null)
            {
                imageService.Dispose();
            }

            if (userService != null)
            {
                userService.Dispose();
            }

            if (notificationService != null)
            {
                notificationService.Dispose();
            }

            if (contactService != null)
            {
                contactService.Dispose();
            }

            if (planService != null)
            {
                planService.Dispose();
            }

            if (transactionService != null)
            {
                transactionService.Dispose();
            }

            if (albumService != null)
            {
                albumService.Dispose();
            }

            if (promptService != null)
            {
                promptService.Dispose();
            }
        }
    }
}
