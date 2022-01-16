using Domain.Models;
using Memento.Web.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Memento.Libs;
using Domain.Repository;

namespace Memento.Web.Controllers
{
    [CustomAuthorization]
    [Route("api/prompts")]
    public class PromptController : BaseApiController
    {
        private PromptService promptService;
        private SharingService sharingService;
        private NotificationService notificationService;
        public PromptController(PromptService promptService,
            SharingService sharingService,
            NotificationService notificationService) {
            this.sharingService = sharingService;
            this.promptService = promptService;
            this.notificationService = notificationService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Get()
        {
            return Ok(await promptService.GetActivePrompts(CurrentUserId));
        }

        [HttpGet]
        [Route("all")]
        public async Task<IActionResult> GetAll(bool? take)
        {
            var prompts = await promptService.GetAllPrompts(CurrentUserId);
            return Ok(prompts);
        }

        [HttpGet]
        [Route("asked")]
        public async Task<IActionResult> GetAllAsked()
        {
            var prompts = await promptService.GetPromptsAskedByMe(CurrentUserId);
            return Ok(prompts);
        }

        [HttpGet]
        [Route("toAnswer")]
        public async Task<IActionResult> GetAllToAnswer()
        {
            var prompts = await promptService.GetPromptsAskedToMe(CurrentUserId);
            return Ok(prompts);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            return Ok(await promptService.GetPrompt(CurrentUserId, id));
        }

        [HttpGet]
        [Route("{id}/asked")]
        public async Task<IActionResult> GetByIdAsked(int id)
        {
            var asked = await promptService.GetAskedPrompt(CurrentUserId, id);
            var invitations = await sharingService.GetExistingRequests(CurrentUserId, id, 0);
            if (invitations.Any()) {
                var inviteAsks = invitations.Select(s => new Asked
                {
                    Connection = false,
                    Name = s.ContactName,
                    DropId = 0,
                    Id = s.RequestId
                }).ToList();
                inviteAsks.AddRange(asked.Askeds);
                asked.Askeds = inviteAsks;
            }
            return Ok(asked);
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Add(QuestionModel questionModel)
        {
            var prompt = await promptService.CreatePrompt(CurrentUserId, questionModel.Question);
            return Ok(prompt);
        }

        [HttpPost]
        [Route("{id}/ask")]
        public async Task<IActionResult> Ask(int id, SelectedUserModel questionModel)
        {
            await promptService.AskQuestion(CurrentUserId, questionModel.UserIds, id);
            foreach(var targetId in questionModel.UserIds) {
                await notificationService.AddNotificationGeneric(CurrentUserId, targetId, id, NotificationType.Question);
            }

            return Ok();
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> Update(int id, QuestionModel questionModel)
        {
            await promptService.UpdatePrompt(CurrentUserId, questionModel.Question, id);
            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await promptService.DismissPrompt(CurrentUserId, id);
            return Ok();
        }
    }
}