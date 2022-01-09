using Domain.Models;
using Memento.Web.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Memento.Libs;

namespace Memento.Web.Controllers
{
    [CustomAuthorization]
    [Route("api/prompts")]
    public class PromptController : BaseApiController
    {
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Get()
        {
            return Ok(await PromptService.GetActivePrompts(CurrentUserId));
        }

        [HttpGet]
        [Route("all")]
        public async Task<IActionResult> GetAll(bool? take)
        {
            var prompts = await PromptService.GetAllPrompts(CurrentUserId);
            return Ok(prompts);
        }

        [HttpGet]
        [Route("asked")]
        public async Task<IActionResult> GetAllAsked()
        {
            var prompts = await PromptService.GetPromptsAskedByMe(CurrentUserId);
            return Ok(prompts);
        }

        [HttpGet]
        [Route("toAnswer")]
        public async Task<IActionResult> GetAllToAnswer()
        {
            var prompts = await PromptService.GetPromptsAskedToMe(CurrentUserId);
            return Ok(prompts);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            return Ok(await PromptService.GetPrompt(CurrentUserId, id));
        }

        [HttpGet]
        [Route("{id}/asked")]
        public async Task<IActionResult> GetByIdAsked(int id)
        {
            var asked = await PromptService.GetAskedPrompt(CurrentUserId, id);
            var invitations = await SharingService.GetExistingRequests(CurrentUserId, id, 0);
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
            var prompt = await PromptService.CreatePrompt(CurrentUserId, questionModel.Question);
            return Ok(prompt);
        }

        [HttpPost]
        [Route("{id}/ask")]
        public async Task<IActionResult> Ask(int id, SelectedUserModel questionModel)
        {
            await PromptService.AskQuestion(CurrentUserId, questionModel.UserIds, id);
            foreach(var targetId in questionModel.UserIds) {
                await NotificationService.AddNotificationGeneric(CurrentUserId, targetId, id, NotificationType.Question);
            }

            return Ok();
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> Update(int id, QuestionModel questionModel)
        {
            await PromptService.UpdatePrompt(CurrentUserId, questionModel.Question, id);
            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await PromptService.DismissPrompt(CurrentUserId, id);
            return Ok();
        }
    }
}