using Domain.Models;
using Domain.Repository;
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
        private AlbumService albumService;
        private ExportService exportService;
        public AlbumController(AlbumService albumService, ExportService exportService) {
            this.albumService = albumService;
            this.exportService = exportService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await albumService.GetAll(CurrentUserId));
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await albumService.Get(CurrentUserId, id));
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> Update(AlbumModel albumModel, int id)
        {
            if (string.IsNullOrWhiteSpace(albumModel.Name)) {
                return BadRequest("Albums must have a name.");
            }

            return Ok(await albumService.Update(CurrentUserId, id, albumModel.Name, albumModel.Archived));
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Create(AlbumModel album)
        {
            if (string.IsNullOrWhiteSpace(album.Name))
            {
                return BadRequest("Albums must have a name.");
            }

            return Ok(await albumService.Create(CurrentUserId, album.Name));
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await albumService.Delete(CurrentUserId, id);
            return Ok();
        }

        // This should go in a moments controller
        [HttpGet]
        [Route("moments/{id}")]
        public async Task<IActionResult> GetAlbumByMoment(int id)
        {
            return Ok(await albumService.GetAlbumsForMoment(CurrentUserId, id));
        }

        [HttpPost]
        [Route("{id}/exports")]
        public async Task<IActionResult> Export(int id)
        {
            await exportService.ExportAlbum(CurrentUserId, id);
            return Ok();
        }

        [HttpPost]
        [Route("{id}/moments")]
        public async Task<IActionResult> AddToMoment(int id, MomentAlbumRequestModel momentAlbumRequestModel)
        {
            await albumService.AddToMoment(CurrentUserId, id, momentAlbumRequestModel.MomentId);
            return Ok();
        }

        [HttpDelete]
        [Route("{id}/moments/{momentId}")]
        public async Task<IActionResult> DeleteToMoment(int id, int momentId)
        {
            await albumService.RemoveToMoment(CurrentUserId, id, momentId);
            return Ok();
        }
    }
}