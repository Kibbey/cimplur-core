using Domain.Models;
using Memento.Libs;
using Memento.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Memento.Web.Controllers
{
    [CustomAuthorization]
    [Route("api/albums")]
    public class AlbumController : BaseApiController
    {

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await AlbumService.GetAll(CurrentUserId));
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await AlbumService.Get(CurrentUserId, id));
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> Update(AlbumModel albumModel, int id)
        {
            if (string.IsNullOrWhiteSpace(albumModel.Name)) {
                return BadRequest("Albums must have a name.");
            }

            return Ok(await AlbumService.Update(CurrentUserId, id, albumModel.Name, albumModel.Archived));
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Create(AlbumModel album)
        {
            if (string.IsNullOrWhiteSpace(album.Name))
            {
                return BadRequest("Albums must have a name.");
            }

            return Ok(await AlbumService.Create(CurrentUserId, album.Name));
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await AlbumService.Delete(CurrentUserId, id);
            return Ok();
        }

        // This should go in a moments controller
        [HttpGet]
        [Route("moments/{id}")]
        public async Task<IActionResult> GetAlbumByMoment(int id)
        {
            return Ok(await AlbumService.GetAlbumsForMoment(CurrentUserId, id));
        }

        [HttpPost]
        [Route("{id}/exports")]
        public async Task<IActionResult> Export(int id)
        {
            await ExportService.ExportAlbum(CurrentUserId, id);
            return Ok();
        }

        [HttpPost]
        [Route("{id}/moments")]
        public async Task<IActionResult> AddToMoment(int id, MomentAlbumRequestModel momentAlbumRequestModel)
        {
            await AlbumService.AddToMoment(CurrentUserId, id, momentAlbumRequestModel.MomentId);
            return Ok();
        }

        [HttpDelete]
        [Route("{id}/moments/{momentId}")]
        public async Task<IActionResult> DeleteToMoment(int id, int momentId)
        {
            await AlbumService.RemoveToMoment(CurrentUserId, id, momentId);
            return Ok();
        }
    }
}