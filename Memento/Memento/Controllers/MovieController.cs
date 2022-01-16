using System.Threading.Tasks;
using Domain.Repository;
using Memento.Libs;
using Microsoft.AspNetCore.Mvc;

namespace Memento.Web.Controllers
{
    [CustomAuthorization]
    [Route("api/movies")]
    public class MovieController : BaseApiController
    {
        private MovieService movieService;
        public MovieController(MovieService movieService) {
            this.movieService = movieService;
        }

        [HttpGet]
        [Route("{id}.mp4")]
        public async Task<IActionResult> Get(int id)
        {
            var video = await movieService.Get(id, CurrentUserId);
            if (video != null)
            {
                var videoFile = File(video, "video/mp4", enableRangeProcessing: true);
                return Ok(videoFile);
            }
            else
            {
                return NotFound();
            }
        }


        [HttpGet]
        [Route("{id}/thumb")]
        public async Task<IActionResult> Thumb(int id)
        {
            var image = await movieService.GetThumb(id, CurrentUserId);
            if (image != null)
            {
                var file = File(image, "image/jpg");
                return Ok(file);
            }
            else
            {
                return NotFound();
            }
        }
    }
}