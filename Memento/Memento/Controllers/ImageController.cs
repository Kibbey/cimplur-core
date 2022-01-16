using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Memento.Libs;
using Microsoft.AspNetCore.Http;
using Domain.Repository;

namespace Memento.Web.Controllers
{
    [CustomAuthorization]
    [Route("api/images")]
    public class ImageController : BaseApiController
    {
        private ImageService imageService;
        private MovieService movieService;
        public ImageController(
            ImageService imageService,
            MovieService movieService) {
            this.imageService = imageService;
            this.movieService = movieService;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            Stream image = null;
            try
            {
                image = await this.imageService.Get(id, CurrentUserId);
            }
            catch (Exception e)
            {
                // do nothing
            }
            if (image != null)
            {
                var file = File(image, "image / jpg");
                return Ok(file);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> PostFormData(IFormFile formFile)
        {
            /*
            var file = HttpContext.Current.Request.Files.Count > 0 ?
                HttpContext.Current.Request.Files[0] : null;
            */
            var file = formFile;
            // TODO - verify below - this may NOT be the way to pull this from the query / path
            var dropId = Int32.Parse(HttpContext.Request.Query["dropId"]);
            var commentIdParam = (String)HttpContext.Request.Query["commentId"];
            int? commentId = null;
            if (commentIdParam != null) {
                commentId = int.Parse(commentIdParam);
            }
            bool result = false;
            if (file != null)
            {
                if (file.ContentType.Split('/')[0] == "video")
                {
                    result = await this.movieService.Add(file, CurrentUserId, dropId, commentId);
                }
                if (file.ContentType.Split('/')[0] == "image" || file.FileName.ToLower().Contains(".heic"))
                {
                    result = await imageService.Add(file, CurrentUserId, dropId, commentId);
                }
            }
            return Ok(result);
        }
    }
}
