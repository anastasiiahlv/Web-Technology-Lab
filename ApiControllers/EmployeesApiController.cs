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
    public class EmployeesApiController : ControllerBase
    {
        private readonly ProjectManagementSystemContext _context;

        public EmployeesApiController(ProjectManagementSystemContext context)
        {
            _context = context;
        }

        private IEnumerable<object> FormResult(List<Employee> employees)
        {
            var result = employees.Select(e => new
            {
                employeeId = e.Id,
                name = e.Name,
                surname = e.Surname,
                email = e.Email,
                phoneNumber = e.PhoneNumber,
                positionId = e.PositionId,
                tasks = e.Tasks?.Select(task => new
                {
                    taskId = task?.Id,
                    taskName = task?.Name,
                    taskDescription = task?.Description,
                    taskDueDate = task?.DueDate,
                    taskFikeUrl = task?.FileUrl,
                    taskStatus = task?.Status?.Name,
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

        // GET: api/EmployeesApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            var employees = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Tasks)
                    .ThenInclude(task => task.Status)
                .Include(e => e.Tasks)
                    .ThenInclude(task => task.Project)
                .ToListAsync();

            var result = FormResult(employees);

            return Ok(new
            {
                code = 200,
                data = result
            });
        }

        // GET: api/EmployeesApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployee(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Tasks)
                    .ThenInclude(task => task.Status)
                .Include(e => e.Tasks)
                    .ThenInclude(task => task.Project)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                return NotFound(new { message = "Немає працівника з таким ID.", code = 404 });
            }

            var result = new
            {
                employeeId = employee.Id,
                name = employee.Name,
                surname = employee.Surname,
                email = employee.Email,
                phoneNumber = employee.PhoneNumber,
                positionId = employee.PositionId,
                position = employee.Position?.Name,
                tasks = employee.Tasks?.Select(task => new
                {
                    taskId = task.Id,
                    taskName = task.Name,
                    taskDescription = task.Description,
                    taskDueDate = task.DueDate,
                    taskFileUrl = task.FileUrl,
                    taskStatus = task.Status?.Name,
                    taskProject = task.Project?.Name
                })
            };

            return Ok(new
            {
                code = 200,
                data = result
            });
        }

        // PUT: api/EmployeesApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployee(int id, Employee employee)
        {
            if (id != employee.Id)
            {
                return BadRequest(FormResponse("Запит на оновлення працівника містить невірний ідентифікатор.", 400));
            }
            var existingEmployee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.Email == employee.Email && e.Id != id);
            if (existingEmployee != null)
            {
                return Conflict(FormResponse("Працівник з такою електронною поштою вже існує.", 409));
            }

            var positionExists = await _context.Positions.AnyAsync(p => p.Id == employee.PositionId);
            if (!positionExists)
            {
                return BadRequest(new { message = $"Позиція з ID {employee.PositionId} не знайдена." });
            }

            _context.Entry(employee).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(id))
                {
                    return NotFound(FormResponse("Немає працівника з таким ID.", 404));
                }
                else
                {
                    throw;
                }
            }

            return Ok(FormResponse("Оновлення успішне.", 200));
        }

        // POST: api/EmployeesApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                               .Select(e => e.ErrorMessage)
                                               .ToList();
                return BadRequest(new { message = "Валідація не пройшла успішно.", errors });
            }

            var positionExists = await _context.Projects.AnyAsync(p => p.Id == employee.PositionId);
            if (!positionExists)
            {
                return BadRequest(new { message = $"Позиція з ID {employee.PositionId} не знайдена." });
            }

            if (_context.Employees.Any(e => e.Email == employee.Email))
            {
                return Conflict(FormResponse("Працівник з такою електронною поштою вже існує.", 409));
            }

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            var result = new
            {
                code = 201,
                data = new
                {
                    employeeId = employee.Id,
                    name = employee.Name,
                    surname = employee.Surname,
                    email = employee.Email,
                    phoneNumber = employee.PhoneNumber,
                    positionId = employee.PositionId,
                    tasks = employee.Tasks
                }
            };

            return CreatedAtAction("GetEmployee", new { id = employee.Id }, result);
        }

        // DELETE: api/EmployeesApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound(FormResponse("Немає працівника з таким ID.", 404));
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}
