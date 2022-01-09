using Memento.Web.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Domain.Models;
using Memento.Libs;

namespace Memento.Web.Controllers
{
    [CustomAuthorization]
    [Route("api/comments")]
    public class CommentController : BaseApiController
    {

        [HttpGet]
        [Route("{dropId}")]
        public async Task<IActionResult> Get(int dropId)
        {
            return Ok(await DropsService.GetComments(dropId, CurrentUserId));
        }

        [HttpPost]
        [Route("{dropId}/thanks")]
        public async Task<IActionResult> Thanks(int dropId)
        {
            var comment = await DropsService.Thank(CurrentUserId, dropId);
            return Ok(comment);
        }

        [HttpPost]
        [Route("")]
        public async Task<ActionResult<CommentModel>> Add(CommentRequestModel commentRequestModel)
        {
            var comment = await DropsService.AddComment(commentRequestModel.Comment, CurrentUserId, commentRequestModel.DropId);
            if (comment == null) {
                return NotFound();
            }
            return comment;
        }

        [HttpPut]
        [Route("{commentId}")]
        public async Task<IActionResult> Update(CommentRequestModel commentRequestModel, int commentId)
        {
            var result = DropsService.UpdateComment(commentRequestModel.Comment, commentId, CurrentUserId);
            return Ok(result);
        }

        [HttpDelete]
        [Route("{commentId}")]
        public async Task<IActionResult> Remove(int commentId)
        {
            await DropsService.RemoveComment(commentId, CurrentUserId);
            return Ok();
        }
    }
}