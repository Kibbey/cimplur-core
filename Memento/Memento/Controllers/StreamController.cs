using System.Threading.Tasks;
using Memento.Libs;
using Microsoft.AspNetCore.Mvc;

namespace Memento.Web.Controllers
{
    [CustomAuthorization]
    [Route("api/streams")]
    public class StreamController : BaseApiController
    {

        [HttpGet]
        [Route("filters")]
        public async Task<IActionResult> GetFilters()
        {
            return Ok(await UserService.GetFilter(CurrentUserId));
        }

        [HttpPut]
        [Route("groups")]
        public async Task<IActionResult> ClearGroups()
        {
            GroupsService.ClearTags(CurrentUserId);
            return Ok();
        }

        [HttpPut]
        [Route("groups/{id}")]
        public async Task<IActionResult> UpdateTag(int id)
        {
            GroupsService.UpdateCurrentNetwork(CurrentUserId, id);
            return Ok();
        }

        [HttpPut]
        [Route("people/{id}")]
        public async Task<IActionResult> AddPerson(int id)
        {
            GroupsService.AddToCurrentPeople(CurrentUserId, id);
            return Ok();
        }

        [HttpDelete]
        [Route("groups/{id}")]
        public async Task<IActionResult> RemoveTag(int id)
        {
            GroupsService.RemoveFromCurrentNetworks(CurrentUserId, id);
            return Ok();
        }

        [HttpDelete]
        [Route("people/{id}")]
        public async Task<IActionResult> RemovePerson(int id)
        {
            GroupsService.RemoveFromCurrentPeople(CurrentUserId, id);
            return Ok();
        }
    }
}
