
using HouseOfHound.Api.Repositories;
using HouseOfHoundAPI.Models.Dog;
using System;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Http;

namespace HouseOfHound.Api.Controllers
{
    [RoutePrefix("api/dogimage")]
    public class DogImageController : ApiController
    {
        private readonly DogImageRepository _dogImageRepository;

        public DogImageController()
        {
            _dogImageRepository = new DogImageRepository();
        }

        // GET api/dogimage/dog/5
        [HttpGet]
        [Route("dog/{dogId:int}")]
        public IHttpActionResult GetDogImages(int dogId)
        {
            try
            {
                var images = _dogImageRepository.GetDogImages(dogId);
                return Ok(images);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET api/dogimage/10
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetDogImageById(int id)
        {
            try
            {
                var image = _dogImageRepository.GetDogImageById(id);

                if (image == null)
                    return NotFound();

                return Ok(image);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST api/dogimage/upload
        [HttpPost]
        [Route("upload")]
        public IHttpActionResult UploadDogImage()
        {
            try
            {
                var request = HttpContext.Current.Request;

                int dogId;

                if (!int.TryParse(request.Form["dogId"], out dogId) || dogId <= 0)
                    return BadRequest("A valid dogId is required.");

                if (request.Files.Count == 0)
                    return BadRequest("No file was uploaded.");

                var file = request.Files[0];

                if (file == null || file.ContentLength == 0)
                    return BadRequest("Uploaded file is empty.");

                if (!IsAllowedImageType(file.ContentType))
                    return BadRequest("Only JPG, PNG, GIF or WEBP images are allowed.");

                var note = request.Form["note"];

                var uploadFolder = HttpContext.Current.Server.MapPath("~/Uploads/DogImages");

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                var originalFileName = Path.GetFileName(file.FileName);
                var extension = Path.GetExtension(originalFileName);

                var savedFileName = string.Format(
                    "dog_{0}_{1}{2}",
                    dogId,
                    Guid.NewGuid().ToString("N"),
                    extension
                );

                var fullPath = Path.Combine(uploadFolder, savedFileName);

                file.SaveAs(fullPath);

                var relativePath = "/Uploads/DogImages/" + savedFileName;

                var dogImage = new DogImage
                {
                    DogId = dogId,
                    FileName = savedFileName,
                    OriginalFileName = originalFileName,
                    FilePath = relativePath,
                    ContentType = file.ContentType,
                    FileSizeBytes = file.ContentLength,
                    Note = note
                };

                var createdImage = _dogImageRepository.CreateDogImage(dogImage);

                return Content(HttpStatusCode.Created, createdImage);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT api/dogimage/10/note
        [HttpPut]
        [Route("{id:int}/note")]
        public IHttpActionResult UpdateDogImageNote(int id, [FromBody] UpdateDogImageNoteRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request cannot be null.");

                var updated = _dogImageRepository.UpdateDogImageNote(id, request.Note);

                if (!updated)
                    return NotFound();

                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // DELETE api/dogimage/10
        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult DeleteDogImage(int id)
        {
            try
            {
                var image = _dogImageRepository.GetDogImageById(id);

                if (image == null)
                    return NotFound();

                var deleted = _dogImageRepository.SoftDeleteDogImage(id);

                if (!deleted)
                    return NotFound();

                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private bool IsAllowedImageType(string contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
                return false;

            contentType = contentType.ToLower();

            return contentType == "image/jpeg"
                || contentType == "image/jpg"
                || contentType == "image/png"
                || contentType == "image/gif"
                || contentType == "image/webp";
        }
    }

    public class UpdateDogImageNoteRequest
    {
        public string Note { get; set; }
    }
}