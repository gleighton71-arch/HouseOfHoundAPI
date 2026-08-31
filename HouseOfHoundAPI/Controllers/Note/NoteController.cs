using HouseOfHoundAPI.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;

namespace HouseOfHound.Api.Controllers
{
    [RoutePrefix("api/note")]
    public class NoteController : ApiController
    {
        private readonly NoteRepository _noteRepository;

        public NoteController()
        {
            _noteRepository = new NoteRepository();
        }

        // GET api/note/dog/5
        [HttpGet]
        [Route("dog/{dogId:int}")]
        public IHttpActionResult GetDogNotes(int dogId)
        {
            try
            {
                List<Note> notes = _noteRepository.GetDogNotes(dogId);
                return Ok(notes);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET api/note/10
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetNoteById(int id)
        {
            try
            {
                Note note = _noteRepository.GetNoteById(id);

                if (note == null)
                    return NotFound();

                return Ok(note);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST api/note
        [HttpPost]
        [Route("")]
        public IHttpActionResult CreateNote([FromBody] Note note)
        {
            try
            {
                if (note == null)
                    return BadRequest("Note cannot be null.");

                if (note.DogId <= 0)
                    return BadRequest("DogId is required.");

                if (string.IsNullOrWhiteSpace(note.Content))
                    return BadRequest("Content is required.");

                Note createdNote = _noteRepository.CreateNote(note);

                return Content(HttpStatusCode.Created, createdNote);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT api/note/10
        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult UpdateNote(int id, [FromBody] Note note)
        {
            try
            {
                if (note == null)
                    return BadRequest("Note cannot be null.");

                if (id != note.Id)
                    return BadRequest("The note ID in the URL does not match the note ID in the body.");

                if (string.IsNullOrWhiteSpace(note.Content))
                    return BadRequest("Content is required.");

                bool updated = _noteRepository.UpdateNote(note);

                if (!updated)
                    return NotFound();

                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // DELETE api/note/10
        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult DeleteNote(int id)
        {
            try
            {
                bool deleted = _noteRepository.DeleteNote(id);

                if (!deleted)
                    return NotFound();

                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}