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
    public class ProjectsApiController : ControllerBase
    {
        private readonly ProjectManagementSystemContext _context;

        public ProjectsApiController(ProjectManagementSystemContext context)
        {
            _context = context;
        }

        private IEnumerable<object> FormResult(List<Project> projects)
        {
            var result = projects.Select(p => new
            {
                projectId = p.Id,
                name = p.Name,
                description = p.Description,
                tasks = p.Tasks?.Select(task => new
                {
                    taskId = task?.Id,
                    taskName = task?.Name,
                    taskDescription = task?.Description,
                    taskDueDate = task?.DueDate,
                    taskFileUrl = task?.FileUrl,
                    taskStatus = task?.Status?.Name,
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

        // GET: api/ProjectsApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
        {
            var projects = await _context.Projects
                .Include(e => e.Tasks)
                .ThenInclude(task => task.Status)
                .ToListAsync();

            var result = FormResult(projects);

            return Ok(new
            {
                code = 200,
                data = result
            });
        }

        // GET: api/ProjectsApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetProject(int id)
        {
            var project = await _context.Projects
                .Include(e => e.Tasks)
                .ThenInclude(task => task.Status)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (project == null)
            {
                return NotFound(new { message = "Немає проєкта з таким ID.", code = 404 });
            }

            var result = new
            {
                projectId = project.Id,
                name = project.Name,
                description = project.Description,
                tasks = project.Tasks?.Select(task => new
                {
                    taskId = task?.Id,
                    taskName = task?.Name,
                    taskDescription = task?.Description,
                    taskDueDate = task?.DueDate,
                    taskFileUrl = task?.FileUrl,
                    taskStatus = task?.Status?.Name,
                })
            };

            return Ok(new
            {
                code = 200,
                data = result
            });
        }

        // PUT: api/ProjectsApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProject(int id, Project project)
        {
            if (id != project.Id)
            {
                return BadRequest(FormResponse("Запит на оновлення проєкту містить невірний ідентифікатор.", 400));
            }
            var existingProject = await _context.Projects
                    .FirstOrDefaultAsync(p => p.Name == project.Name && p.Description == project.Description && p.Id != id);
            if (existingProject != null)
            {
                return Conflict(FormResponse("Проєкт з такою назвою та описом вже існує.", 409));
            }

            _context.Entry(project).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(id))
                {
                    return NotFound(FormResponse("Немає проєкта з таким ID.", 404));
                }
                else
                {
                    throw;
                }
            }

            return Ok(FormResponse("Оновлення успішне.", 200));
        }

        // POST: api/ProjectsApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Project>> PostProject(Project project)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                               .Select(e => e.ErrorMessage)
                                               .ToList();
                return BadRequest(new { message = "Валідація не пройшла успішно.", errors });
            }

            if (_context.Projects.Any(p => p.Name == project.Name && p.Description == project.Description))
            {
                return Conflict(FormResponse("Проєкт з такою назвою та описом вже існує.", 409));
            }

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var result = new
            {
                code = 201,
                data = new
                {
                    projectId = project.Id,
                    name = project.Name,
                    description = project.Description,
                    tasks = project.Tasks
                }
            };

            return CreatedAtAction("GetProject", new { id = project.Id }, result);
        }

        // DELETE: api/ProjectsApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound(FormResponse("Немає проєкта з таким ID.", 404));
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}
