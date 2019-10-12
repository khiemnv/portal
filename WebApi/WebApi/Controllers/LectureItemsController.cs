using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LectureItemsController : ControllerBase
    {
        private readonly LectureContext _context;

        public LectureItemsController(LectureContext context)
        {
            _context = context;
        }

        // GET: api/LectureItems
        [HttpGet]
        public IEnumerable<LectureItem> GetLectureItems()
        {
            return _context.LectureItems;
        }

        // GET: api/LectureItems/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLectureItem([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var lectureItem = await _context.LectureItems.FindAsync(id);

            if (lectureItem == null)
            {
                return NotFound();
            }

            return Ok(lectureItem);
        }

        // PUT: api/LectureItems/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLectureItem([FromRoute] string id, [FromBody] LectureItem lectureItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != lectureItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(lectureItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LectureItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/LectureItems
        [HttpPost]
        public async Task<IActionResult> PostLectureItem([FromBody] LectureItem lectureItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.LectureItems.Add(lectureItem);
            await _context.SaveChangesAsync();

            //return CreatedAtAction("GetLectureItem", new { id = lectureItem.Id }, lectureItem);
            return CreatedAtAction(nameof(GetLectureItem), new { id = lectureItem.Id }, lectureItem);
        }

        // DELETE: api/LectureItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLectureItem([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var lectureItem = await _context.LectureItems.FindAsync(id);
            if (lectureItem == null)
            {
                return NotFound();
            }

            _context.LectureItems.Remove(lectureItem);
            await _context.SaveChangesAsync();

            return Ok(lectureItem);
        }

        private bool LectureItemExists(string id)
        {
            return _context.LectureItems.Any(e => e.Id == id);
        }
    }
}