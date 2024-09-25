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
    public class TasksApiController : ControllerBase
    {
        private readonly ProjectManagementSystemContext _context;

        public TasksApiController(ProjectManagementSystemContext context)
        {
            _context = context;
        }

        private IEnumerable<object> FormResult(List<Models.Task> tasks)
        {
            var result = tasks.Select(t => new
            {
                taskId = t.Id,
                name = t.Name,
                description = t.Description,
                dueDate = t.DueDate,
                fileUrl = t.FileUrl,
                statusId = t.StatusId,
                projectId = t.ProjectId,
                employees = t.Employees?.Select(employee => new
                {
                    employeeId = employee?.Id,
                    employeeName = employee?.Name,
                    employeeSurname = employee?.Surname,
                    employeeEmail = employee?.PhoneNumber,
                    employeePosition = employee?.Position?.Name,
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

        // GET: api/TasksApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Models.Task>>> GetTasks()
        {
            var tasks = await _context.Tasks
                .Include(s => s.Status)
                .Include(s => s.Project)
                .Include(s => s.Employees)
                .ThenInclude(employee => employee.Position)
                .ToListAsync();

            var result = FormResult(tasks);

            return Ok(new
            {
                code = 200,
                data = result
            });
        }

        // GET: api/TasksApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Models.Task>> GetTask(int id)
        {
            var task = await _context.Tasks
                .Include(s => s.Status)
                .Include(s => s.Project)
                .Include(s => s.Employees)
                .ThenInclude(employee => employee.Position)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (task == null)
            {
                return NotFound(new { message = "Немає завданян з таким ID.", code = 404 });
            }

            var result = new
            {
                taskId = task.Id,
                name = task.Name,
                description = task.Description,
                dueDate = task.DueDate,
                fileUrl = task.FileUrl,
                statusId = task.StatusId,
                projectId = task.ProjectId,
                employees = task.Employees?.Select(employee => new
                {
                    employeeId = employee?.Id,
                    employeeName = employee?.Name,
                    employeeSurname = employee?.Surname,
                    employeeEmail = employee?.PhoneNumber,
                    employeePosition = employee?.Position?.Name,
                })
            };

            return Ok(new
            {
                code = 200,
                data = result
            });
        }

        // PUT: api/TasksApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTask(int id, Models.Task task)
        {
            if (id != task.Id)
            {
                return BadRequest(FormResponse("Запит на оновлення завдання містить невірний ідентифікатор.", 400));
            }
            var existingTask = await _context.Tasks
                .Include(t => t.Employees) 
                .FirstOrDefaultAsync(t => t.Name == task.Name && t.Description == task.Description && t.Id != id);

            if (existingTask != null)
            {
                bool employeesMatch = task.Employees.All(e => existingTask.Employees.Any(et => et.Id == e.Id)) &&
                                      existingTask.Employees.All(et => task.Employees.Any(e => e.Id == et.Id));

                if (employeesMatch)
                {
                    return Conflict(FormResponse("Таке завдання вже існує.", 409));
                }
            }

            var projectExists = await _context.Positions.AnyAsync(p => p.Id == task.ProjectId);
            if (!projectExists)
            {
                return BadRequest(FormResponse($"Проєкт з ID {task.ProjectId} не знайдено.", 400));
            }

            var statusExists = await _context.Statuses.AnyAsync(s => s.Id == task.StatusId);
            if (!statusExists)
            {
                return BadRequest(FormResponse($"Статус з ID {task.StatusId} не знайдено.", 400));
            }

            _context.Entry(task).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskExists(id))
                {
                    return NotFound(FormResponse("Немає завдання з таким ID.", 404));
                }
                else
                {
                    throw;
                }
            }

            return Ok(FormResponse("Оновлення успішне.", 200));
        }

        // POST: api/TasksApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Models.Task>> PostTask([FromForm] Models.Task task, IFormFile file)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                               .Select(e => e.ErrorMessage)
                                               .ToList();
                return BadRequest(new { message = "Валідація не пройшла успішно.", errors });
            }

            var projectExists = await _context.Projects.AnyAsync(p => p.Id == task.ProjectId);
            if (!projectExists)
            {
                return BadRequest(new { message = $"Проєкт з ID {task.ProjectId} не знайдено." });
            }

            var statusExists = await _context.Statuses.AnyAsync(s => s.Id == task.StatusId);
            if (!statusExists)
            {
                return BadRequest(new { message = $"Статус з ID {task.StatusId} не знайдено." });
            }

            /*if (_context.Tasks.Any(t => t.Name == task.Name && t.Description == task.Description &&
                    t.Employees == task.Employees))
            {
                return Conflict(FormResponse("Таке завдання вже існує.", 409));
            }*/

            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }
            if (file != null && file.Length > 0)
            {
                var filePath = Path.Combine("wwwroot/uploads", file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                task.FileUrl = filePath;
            }

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            var result = new
            {
                code = 201,
                data = new
                {
                    taskId = task.Id,
                    name = task.Name,
                    description = task.Description,
                    dueDate = task.DueDate,
                    fileUrl = task.FileUrl,
                    statusId = task.StatusId,
                    projectId = task.ProjectId,
                    employees = task.Employees
                }
            };

            return CreatedAtAction("GetTask", new { id = task.Id }, result);
        }

        // DELETE: api/TasksApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound(FormResponse("Немає завдання з таким ID.", 404));
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TaskExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
    }
}
