using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LecturesController : ControllerBase
    {
        private readonly LectureService _lectureService;

        public LecturesController(LectureService lectService)
        {
            _lectureService = lectService;
        }

        [HttpGet]
        public ActionResult<List<Lecture>> Get() =>
            _lectureService.Get();

        [HttpGet("{id:length(24)}", Name = "GetLecture")]
        public ActionResult<Lecture> Get(string id)
        {
            var lect = _lectureService.Get(id);

            if (lect == null)
            {
                return NotFound();
            }

            return lect;
        }

        [HttpPost]
        public ActionResult<Lecture> Create(Lecture lect)
        {
            _lectureService.Create(lect);

            return CreatedAtRoute("GetLecture", new { id = lect.Id.ToString() }, lect);
        }

        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, Lecture lectIn)
        {
            var lect = _lectureService.Get(id);

            if (lect == null)
            {
                return NotFound();
            }

            _lectureService.Update(id, lectIn);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var lect = _lectureService.Get(id);

            if (lect == null)
            {
                return NotFound();
            }

            _lectureService.Remove(lect.Id);

            return NoContent();
        }
    }
}