using Domain.Models;
using log4net;
using Memento.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Memento.Libs;
using Domain.Repository;

namespace Memento.Web.Controllers
{
    [CustomAuthorization]
    [Route("api/timelines")]
    public class TimelineController : BaseApiController
    {
        private TimelineService timelineService;
        private PromptService promptService;
        private SharingService sharingService;
        public TimelineController(
            TimelineService timelineService,
            PromptService promptService,
            SharingService sharingService) {
            this.timelineService = timelineService;
            this.promptService = promptService;
            this.sharingService = sharingService;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Timeline(int id)
        {
            return Ok(await timelineService.GetTimeline(CurrentUserId, id));
        }

        [HttpGet]
        [Route("{id}/prompts/all")]
        public async Task<IActionResult> TimelinePromptsAll(int id)
        {
            return Ok(await promptService.GetAllTimelineQuestions(CurrentUserId, id));
        }

        [HttpGet]
        [Route("{id}/prompts")]
        public async Task<IActionResult> TimelinePrompts(int id)
        {
            return Ok(await promptService.GetTimelineQuestions(CurrentUserId, id));
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> AllTimelines()
        {
            return Ok(await timelineService.GetAllTimelines(CurrentUserId));
        }

        [HttpGet]
        [Route("{id}/invited")]
        public async Task<IActionResult> GetByIdAsked(int id)
        {
            var asked = new List<Asked>();
            var invitations = await sharingService.GetExistingRequests(CurrentUserId, 0, id);
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
                return Ok(await timelineService.AddTimeline(CurrentUserId, timeline.Name, timeline.Description));
            }
            return BadRequest("Name is required for a timeline.");
        }

        [HttpPost]
        [Route("{id}")]
        public async Task<IActionResult> FollowTimeline(int id)
        {
            return Ok(await timelineService.FollowTimeline(CurrentUserId, id));
        }

        [HttpPost]
        [Route("{id}/invite")]
        public async Task<IActionResult> InviteToTimeline(SelectedIdModel invited, int id)
        {
            await timelineService.InviteToTimeline(CurrentUserId, id, invited.Ids);
            return Ok();
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateTimeline(TimelineCreateModel timeline, int id)
        {
            return Ok(await timelineService.UpdateTimeline(CurrentUserId, id, timeline.Name, timeline.Description));
        }

        [HttpPut]
        [Route("{id}/prompts")]
        public async Task<IActionResult> AddQuestionToPrompt(SelectedIdModel promptIds, int id)
        {
            await timelineService.AddPromptToTimeline(CurrentUserId, promptIds.Ids, id);
            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return Ok(await timelineService.SoftDeleteTimeline(CurrentUserId, id));
        }

        [HttpGet]
        [Route("drops/{id}")]
        public async Task<IActionResult> TimelinesForDrop(int id)
        {
            var timelines = await timelineService.GetTimelinesForDrop(CurrentUserId, id);
            return Ok(timelines);
        }

        [HttpPost]
        [Route("drops/{id}/timelines/{timelineId}")]
        public async Task<IActionResult> AddTimelineForDrop(int id, int timelineId)
        {
            await timelineService.AddDropToTimeline(CurrentUserId, id, timelineId);
            return Ok();
        }

        [HttpDelete]
        [Route("drops/{id}/timelines/{timelineId}")]
        public async Task<IActionResult> RemoveTimelineForDrop(int id, int timelineId)
        {
            await timelineService.RemoveDropFromTimeline(CurrentUserId, id, timelineId);
            return Ok();
        }



        private ILog logger = LogManager.GetLogger(nameof(TimelineController));
    }
}