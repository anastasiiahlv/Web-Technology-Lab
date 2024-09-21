using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.Controllers
{
    public class TasksController : Controller
    {
        private readonly ProjectManagementSystemContext _context;

        public TasksController(ProjectManagementSystemContext context)
        {
            _context = context;
        }

        // GET: Tasks
        public async Task<IActionResult> Index()
        {
            var projectManagementSystemContext = _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.Status)
                .Include(t => t.Employees);
            return View(await projectManagementSystemContext.ToListAsync());
        }

        // GET: Tasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.Status)
                .Include(t => t.Employees)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // GET: Tasks/Create
        public IActionResult Create(int projectId)
        {
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", projectId);
            ViewData["StatusId"] = new SelectList(_context.Statuses, "Id", "Name");
            ViewData["Employees"] = new MultiSelectList(_context.Employees, "Id", "FullName");
            return View();
        }

        // POST: Tasks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,DueDate,FileUrl,ProjectId,StatusId, Employees")] Models.Task task, int[] Employees, IFormFile File)
        {
            if (Employees != null)
            {
                foreach (var employeeId in Employees)
                {
                    var employee = await _context.Employees.FindAsync(employeeId);
                    if (employee != null)
                    {
                        task.Employees.Add(employee);
                    }
                }
            }

            if (File != null && File.Length > 0)
            { 
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + File.FileName;

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await File.CopyToAsync(stream);
                }

                task.FileUrl = "/files/" + uniqueFileName;
            }

            _context.Add(task);
            await _context.SaveChangesAsync();

            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", task.ProjectId);
            ViewData["StatusId"] = new SelectList(_context.Statuses, "Id", "Name", task.StatusId);
            ViewData["Employees"] = new MultiSelectList(_context.Employees, "Id", "FullName", task.Employees.Select(e => e.Id));
            return RedirectToAction("Details", "Tasks", new { id = task.Id });
        }

        // GET: Tasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.Status)
                .Include(t => t.Employees)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (task == null)
            {
                return NotFound();
            }
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", task.ProjectId);
            ViewData["StatusId"] = new SelectList(_context.Statuses, "Id", "Name", task.StatusId);
            ViewData["Employees"] = new MultiSelectList(_context.Employees, "Id", "FullName", task.Employees.Select(e => e.Id));
            return View(task);
        }

        // POST: Tasks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,DueDate,FileUrl,ProjectId,StatusId,Employees")] Models.Task task, int[] Employees, IFormFile File)
        {
            if (id != task.Id)
            {
                return NotFound();
            }

            var taskInDb = await _context.Tasks
                .Include(t => t.Employees)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (taskInDb == null)
            {
                return NotFound();
            }

            taskInDb.Name = task.Name;
            taskInDb.Description = task.Description;
            taskInDb.DueDate = task.DueDate;
            taskInDb.ProjectId = task.ProjectId;
            taskInDb.StatusId = task.StatusId;

            taskInDb.Employees.Clear();
            foreach (var employeeId in Employees)
            {
                var employee = await _context.Employees.FindAsync(employeeId);
                if (employee != null)
                {
                    taskInDb.Employees.Add(employee);
                }
            }

            if (File != null && File.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + File.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await File.CopyToAsync(stream);
                }

                if (!string.IsNullOrEmpty(taskInDb.FileUrl))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", taskInDb.FileUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                taskInDb.FileUrl = "/files/" + uniqueFileName;
            }

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskExists(task.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // GET: Tasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.Status)
                .Include(t => t.Employees)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // POST: Tasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task != null)
            {
                _context.Tasks.Remove(task);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TaskExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
    }
}
