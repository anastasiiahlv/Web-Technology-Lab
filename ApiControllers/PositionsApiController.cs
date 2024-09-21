using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PositionsApiController : ControllerBase
    {
        private readonly ProjectManagementSystemContext _context;

        public PositionsApiController(ProjectManagementSystemContext context)
        {
            _context = context;
        }

        private IEnumerable<object> FormResult(List<Position> positions)
        {
            var result = positions.Select(p => new
            {
                positionId = p.Id,
                name = p.Name,
                employees = p.Employees?.Select(employee => new
                {
                    participantId = employee?.Id,
                    name = employee?.Name,
                    surname = employee?.Surname,
                    email = employee?.Email,
                    phoneNumber = employee?.PhoneNumber,
                })
            }).ToList();

            return result;
        }

        private object FormResponse(string m, int c)
        {
            object response = new
            {
                code = c,
                message = m
            };

            return response;
        }


        // GET: api/PositionsApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Position>>> GetPositions()
        {
            var positions = await _context.Positions
                .Include(p => p.Employees)
                .ToListAsync();

            var result = FormResult(positions);

            return Ok(new
            {
                code = 200,
                data = result
            });
        }

        // GET: api/PositionsApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Position>> GetPosition(int id)
        {
            var positions = await _context.Positions
                .Include(p => p.Employees)
                .ToListAsync();

            var position = await _context.Positions.FindAsync(id);

            if (position == null)
            {
                return NotFound(FormResponse("Немає позиції з таким ID.", 404));
            }

            var result = FormResult(new List<Position> { position });
            return Ok(new
            {
                code = 200,
                data = result
            });
        }

        // PUT: api/PositionsApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPosition(int id, Position position)
        {
            if (id != position.Id)
            {
                return BadRequest(FormResponse("Запит на оновлення позиції містить невірний ідентифікатор.", 400));
            }

            _context.Entry(position).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PositionExists(id))
                {
                    return NotFound(FormResponse("Немає позиції з таким ID.", 404));
                }
                else
                {
                    throw;
                }
            }

            return Ok(FormResponse("Оновлення успішне.", 200));
        }

        // POST: api/PositionsApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Position>> PostPosition(Position position)
        {
            if (_context.Positions.Any(p => p.Name == position.Name))
            {
                return Conflict(FormResponse("Позиція з таким іменем вже існує.", 409));
            }

            _context.Positions.Add(position);
            await _context.SaveChangesAsync();

            var result = new
            {
                code = 201,
                data = position
            };

            return CreatedAtAction("GetPosition", new { id = position.Id }, result);
        }

        // DELETE: api/PositionsApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePosition(int id)
        {
            var position = await _context.Positions.FindAsync(id);
            if (position == null)
            {
                return NotFound(FormResponse("Немає позиції з таким ID.", 404));
            }

            _context.Positions.Remove(position);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PositionExists(int id)
        {
            return _context.Positions.Any(e => e.Id == id);
        }
    }
}
