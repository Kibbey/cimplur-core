using System.Threading.Tasks;
using Domain.Repository;
using Memento.Libs;
using Microsoft.AspNetCore.Mvc;

namespace Memento.Web.Controllers
{
    [CustomAuthorization]
    [Route("api/streams")]
    public class StreamController : BaseApiController
    {

        private UserService userService;
        private GroupService groupService;
        public StreamController(
            UserService userService,
            GroupService groupService) {
            this.userService = userService;
            this.groupService = groupService;
        }
        [HttpGet]
        [Route("filters")]
        public async Task<IActionResult> GetFilters()
        {
            return Ok(await userService.GetFilter(CurrentUserId));
        }

        [HttpPut]
        [Route("groups")]
        public async Task<IActionResult> ClearGroups()
        {
            groupService.ClearTags(CurrentUserId);
            return Ok();
        }

        [HttpPut]
        [Route("groups/{id}")]
        public async Task<IActionResult> UpdateTag(int id)
        {
            groupService.UpdateCurrentNetwork(CurrentUserId, id);
            return Ok();
        }

        [HttpPut]
        [Route("people/{id}")]
        public async Task<IActionResult> AddPerson(int id)
        {
            groupService.AddToCurrentPeople(CurrentUserId, id);
            return Ok();
        }

        [HttpDelete]
        [Route("groups/{id}")]
        public async Task<IActionResult> RemoveTag(int id)
        {
            groupService.RemoveFromCurrentNetworks(CurrentUserId, id);
            return Ok();
        }

        [HttpDelete]
        [Route("people/{id}")]
        public async Task<IActionResult> RemovePerson(int id)
        {
            groupService.RemoveFromCurrentPeople(CurrentUserId, id);
            return Ok();
        }
    }
}
