using Domain.Repository;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Memento.Web.Controllers
{
    [ApiController]
    public class BaseApiController : ControllerBase
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

    }
}
