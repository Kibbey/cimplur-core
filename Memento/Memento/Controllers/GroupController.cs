using Domain.Repository;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Memento.Libs;
using Memento.Web.Models;

namespace Memento.Web.Controllers
{
    [CustomAuthorization]
    [Route("api/groups")]
    public class GroupController : BaseApiController
    {

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> Rename(int id, NameRequest name)
        {
            string newName = await GroupsService.Rename(CurrentUserId, id, name.Name);
            return Ok(new { Name = newName });
        }


        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Groups()
        {
            return Ok(new TagsViewModel(
                 await GroupsService.AllGroups(CurrentUserId)));
        }

        [HttpGet]
        [Route("editable")]
        public async Task<IActionResult> EditableGroups()
        {
            return Ok(new TagsViewModel(await GroupsService.EditableGroups(CurrentUserId)));
        }

        [HttpGet]
        [Route("allSelected")]
        public async Task<IActionResult> GroupsAllSelected()
        {
            var tagViewer = new TagsViewModel(await GroupsService.AllGroups(CurrentUserId));
            var user = await UserService.GetUser(CurrentUserId);
            if (!user.PrivateMode)
            {
                tagViewer.ActiveTags.ForEach(item =>
                {
                    item.Selected = item.Name == GroupService.GetEveryone;
                });
            }
            return Ok(tagViewer);
        }

        [HttpGet]
        [Route("includeMembers")]
        public async Task<IActionResult> GroupsAndViewers()
        {
            var tagViewers = await GroupsService.GetNetworksAndViewersModels(CurrentUserId);
            return Ok(tagViewers);
        }
        /*
        [HttpGet]
        [Route("availableConnections")]
        public async Task<IActionResult> GetPeople()
        {
            var people = await GroupsService.People(CurrentUserId);
            return Ok(people);
        }
        */

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> NewTag(NameRequest nameRequest)
        {
            var networkId = GroupsService.Add(nameRequest.Name, CurrentUserId);
            return Ok(networkId);
        }

        // update who can view a group
        [HttpPut]
        [Route("{groupId}/members")]
        public async Task<IActionResult> UpdateTagViewers(int groupId, SelectedIdModel model)
        {
            return Ok(await GroupsService.UpdateNetworkViewers(CurrentUserId, groupId, model.Ids));
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            await GroupsService.Archive(id, CurrentUserId);
            return Ok(true);
        }

        [HttpGet]
        [Route("viewers")]
        public async Task<IActionResult> Viewers([FromQuery] LongCollectionModel model)
        {
            return Ok(await GroupsService.GetViewers(CurrentUserId, model.Ids));
        }
    }
}