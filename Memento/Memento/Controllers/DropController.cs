using Domain.Models;
using log4net;
using Memento.Web.Models;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Memento.Libs;

namespace Memento.Web.Controllers
{
    [CustomAuthorization]
    [Route("api/drops")]
    public class DropController : BaseApiController
    {

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Moment(int id)
        {
            return Ok(await DropsService.Drop(CurrentUserId, id));
        }

        [HttpPost]
        [Route("{id}/timeline/{timelineId}")]
        public async Task<IActionResult> AddToTimeline(int id, int timelineId)
        {
            await TimelineService.AddDropToTimeline(CurrentUserId, id, timelineId);
            return Ok();
        }

        [HttpDelete]
        [Route("{id}/timeline/{timelineId}")]
        public async Task<IActionResult> RemoveFromTimeline(int id, int timelineId)
        {
            await TimelineService.RemoveDropFromTimeline(CurrentUserId, id, timelineId);
            return Ok();
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Service([FromQuery] StreamRequestModel model)
        {
            var includeMe = model.IncludeMe ?? false;
            var chronological = model.Chronological ?? false;
            int? dayOfYear = null;
            if (model.Day.HasValue && model.Month.HasValue)
            {
                dayOfYear = new DateTime(1999, model.Month.Value, model.Day.Value).DayOfYear;
            }
            var stream = await DropsService.GetDrops(CurrentUserId, model.AlbumIds, model.PeopleIds, includeMe, chronological, dayOfYear, model.Year, model.Skip);
            await UserService.UpdateMe(CurrentUserId, includeMe);
            return Ok(stream);
        }

        [HttpGet]
        [Route("timelines/{id}")]
        public async Task<IActionResult> GetTimeline([FromQuery] TimelineRequestModel model, int id)
        {
            return Ok(await DropsService.GetTimelineDrops(CurrentUserId, id, model.Skip, model.Ascending));
        }

        [HttpGet]
        [Route("albums/{id}")]
        public async Task<IActionResult> GetAlbums([FromQuery] TimelineRequestModel model, int id)
        {
            return Ok(await DropsService.GetAlbumDrops(CurrentUserId, id, model.Skip, model.Ascending));
        }

        [HttpGet]
        [Route("{id}/viewers")]
        public async Task<IActionResult> Viewers(int id)
        {
            return Ok(await GroupsService.DropViewers(CurrentUserId,id));
        }

        [HttpGet]
        [Route("{id}/groups")]
        public async Task<IActionResult> GetDropGroups(int id)
        {
            var tags = GroupsService.DropTags(id, CurrentUserId, null);
            var peps = GroupsService.DropPeople(id, CurrentUserId, null);
            if (tags == null && peps == null)
            {
                return Ok();
            }

            return Ok(new { People = peps, Tags = tags });
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> AddDrop(Models.DropModel dropModel)
        {
            try
            {
                dropModel.Date = dropModel.Date ?? DateTime.UtcNow;
                var content = new ContentModel(0, dropModel.Information);
                var dropTuple = await DropsService.Add(new Domain.Models.DropModel
                {
                        Date = dropModel.Date.Value,
                        DateType = dropModel.DateType,
                        Content = content
                    }, 
                    dropModel.TagIds, CurrentUserId, dropModel.TimelineIds, dropModel.PromptId);
                if (dropModel.PromptId > 0) await PromptService.UsePrompt(CurrentUserId, dropModel.PromptId);
                return Ok(new { dropId = dropTuple.Item1, isTask = dropTuple.Item2 });
            }
            catch (Exception e)
            {
                logger.Error("Add drop - important:",e);
                return BadRequest("There was an error processing your reqeust.  We are on it - thank you for your patience.");
            }
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteDrop(int id)
        {
            await DropsService.Delete(id, CurrentUserId);
            return Ok(true);
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateDrop(UpdateDropModel dropModel, int id)
        {
            var content = new ContentModel(0, dropModel.Information);

            var isTask = await DropsService.Edit(new Domain.Models.DropModel
            {
                Date = dropModel.Date.Value,
                DateType = dropModel.DateType,
                Content = content,
                DropId = id
            }, dropModel.TagIds, dropModel.Images, dropModel.Movies, CurrentUserId);

            return Ok(new { date = dropModel.Date, isTask = isTask });
        }


        private ILog logger = LogManager.GetLogger(nameof(UserService));
    }
}