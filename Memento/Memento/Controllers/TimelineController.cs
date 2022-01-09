using Domain.Models;
using log4net;
using Memento.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Memento.Libs;

namespace Memento.Web.Controllers
{
    [CustomAuthorization]
    [Route("api/timelines")]
    public class TimelineController : BaseApiController
    {

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Timeline(int id)
        {
            return Ok(await TimelineService.GetTimeline(CurrentUserId, id));
        }

        [HttpGet]
        [Route("{id}/prompts/all")]
        public async Task<IActionResult> TimelinePromptsAll(int id)
        {
            return Ok(await PromptService.GetAllTimelineQuestions(CurrentUserId, id));
        }

        [HttpGet]
        [Route("{id}/prompts")]
        public async Task<IActionResult> TimelinePrompts(int id)
        {
            return Ok(await PromptService.GetTimelineQuestions(CurrentUserId, id));
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> AllTimelines()
        {
            return Ok(await TimelineService.GetAllTimelines(CurrentUserId));
        }

        [HttpGet]
        [Route("{id}/invited")]
        public async Task<IActionResult> GetByIdAsked(int id)
        {
            var asked = new List<Asked>();
            var invitations = await SharingService.GetExistingRequests(CurrentUserId, 0, id);
            if (invitations.Any())
            {
                asked = invitations.Select(s => new Asked
                {
                    Connection = false,
                    Name = s.ContactName,
                    DropId = 0,
                    Id = s.RequestId
                }).ToList();
            }
            return Ok(asked);
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> AddTimeline(TimelineCreateModel timeline)
        {
            if (!string.IsNullOrWhiteSpace(timeline.Name))
            {
                return Ok(await TimelineService.AddTimeline(CurrentUserId, timeline.Name, timeline.Description));
            }
            return BadRequest("Name is required for a timeline.");
        }

        [HttpPost]
        [Route("{id}")]
        public async Task<IActionResult> FollowTimeline(int id)
        {
            return Ok(await TimelineService.FollowTimeline(CurrentUserId, id));
        }

        [HttpPost]
        [Route("{id}/invite")]
        public async Task<IActionResult> InviteToTimeline(SelectedIdModel invited, int id)
        {
            await TimelineService.InviteToTimeline(CurrentUserId, id, invited.Ids);
            return Ok();
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateTimeline(TimelineCreateModel timeline, int id)
        {
            return Ok(await TimelineService.UpdateTimeline(CurrentUserId, id, timeline.Name, timeline.Description));
        }

        [HttpPut]
        [Route("{id}/prompts")]
        public async Task<IActionResult> AddQuestionToPrompt(SelectedIdModel promptIds, int id)
        {
            await TimelineService.AddPromptToTimeline(CurrentUserId, promptIds.Ids, id);
            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return Ok(await TimelineService.SoftDeleteTimeline(CurrentUserId, id));
        }

        [HttpGet]
        [Route("drops/{id}")]
        public async Task<IActionResult> TimelinesForDrop(int id)
        {
            var timelines = await TimelineService.GetTimelinesForDrop(CurrentUserId, id);
            return Ok(timelines);
        }

        [HttpPost]
        [Route("drops/{id}/timelines/{timelineId}")]
        public async Task<IActionResult> AddTimelineForDrop(int id, int timelineId)
        {
            await TimelineService.AddDropToTimeline(CurrentUserId, id, timelineId);
            return Ok();
        }

        [HttpDelete]
        [Route("drops/{id}/timelines/{timelineId}")]
        public async Task<IActionResult> RemoveTimelineForDrop(int id, int timelineId)
        {
            await TimelineService.RemoveDropFromTimeline(CurrentUserId, id, timelineId);
            return Ok();
        }



        private ILog logger = LogManager.GetLogger(nameof(TimelineController));
    }
}