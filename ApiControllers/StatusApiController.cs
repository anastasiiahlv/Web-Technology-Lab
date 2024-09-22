using System;
using System.Collections.Generic;
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
    public class StatusApiController : ControllerBase
    {
        private readonly ProjectManagementSystemContext _context;

        public StatusApiController(ProjectManagementSystemContext context)
        {
            _context = context;
        }

        private IEnumerable<object> FormResult(List<Status> statuses)
        {
            var result = statuses.Select(s => new
            {
                statusId = s.Id,
                name = s.Name,
                tasks = s.Tasks?.Select(task => new
                {
                    taskId = task?.Id,
                    taskName = task?.Name,
                    taskDescription = task?.Description,
                    taskDueDate = task?.DueDate,
                    taskFileUrl = task?.FileUrl,
                    taskProject = task?.Project?.Name
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

        // GET: api/StatusApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Status>>> GetStatuses()
        {
            var statuses = await _context.Statuses
                .Include(s => s.Tasks)
                .ThenInclude(task => task.Project)
                .ToListAsync();

            var result = FormResult(statuses);

            return Ok(new
            {
                code = 200,
                data = result
            });
        }

        // GET: api/StatusApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Status>> GetStatus(int id)
        {
            var status = await _context.Statuses
                .Include(s => s.Tasks)
                .ThenInclude(task => task.Project)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (status == null)
            {
                return NotFound(new { message = "Немає статуса з таким ID.", code = 404 });
            }

            var result = new
            {
                statusId = status.Id,
                name = status.Name,
                tasks = status.Tasks?.Select(task => new
                {
                    taskId = task?.Id,
                    taskName = task?.Name,
                    taskDescription = task?.Description,
                    taskDueDate = task?.DueDate,
                    taskFileUrl = task?.FileUrl,
                    taskProject = task?.Project?.Name
                })
            };
        

            return Ok(new
            {
                code = 200,
                data = result
            });
        }

        // PUT: api/StatusApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStatus(int id, Status status)
        {
            if (id != status.Id)
            {
                return BadRequest(FormResponse("Запит на оновлення статуса містить невірний ідентифікатор.", 400));
            }
            var existingStatus = await _context.Statuses
                    .FirstOrDefaultAsync(s => s.Name == status.Name && s.Id != id);
            if (existingStatus != null)
            {
                return Conflict(FormResponse("Статус з такою назвою та описом вже існує.", 409));
            }

            _context.Entry(status).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StatusExists(id))
                {
                    return NotFound(FormResponse("Немає статуса з таким ID.", 404));
                }
                else
                {
                    throw;
                }
            }

            return Ok(FormResponse("Оновлення успішне.", 200));
        }

        // POST: api/StatusApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Status>> PostStatus(Status status)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                               .Select(e => e.ErrorMessage)
                                               .ToList();
                return BadRequest(new { message = "Валідація не пройшла успішно.", errors });
            }

            if (_context.Statuses.Any(s => s.Name == status.Name))
            {
                return Conflict(FormResponse("Статус з такою назвою вже існує.", 409));
            }

            _context.Statuses.Add(status);
            await _context.SaveChangesAsync();

            var result = new
            {
                code = 201,
                data = new
                {
                    statusId = status.Id,
                    name = status.Name,
                    tasks = status.Tasks
                }
            };

            return CreatedAtAction("GetStatus", new { id = status.Id }, result);
        }

        // DELETE: api/StatusApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStatus(int id)
        {
            var status = await _context.Statuses.FindAsync(id);
            if (status == null)
            {
                return NotFound(FormResponse("Немає статусу з таким ID.", 404));
            }

            _context.Statuses.Remove(status);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StatusExists(int id)
        {
            return _context.Statuses.Any(e => e.Id == id);
        }
    }
}
