using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ProjectManagementSystemContext _context;

        public ProjectsController(ProjectManagementSystemContext context)
        {
            _context = context;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            var projects = await _context.Projects
                .Include(e => e.Tasks).ThenInclude(t => t.Status)
                .ToListAsync();
            return View(projects);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .Include(p => p.Tasks).ThenInclude(t => t.Status)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null) return NotFound();

            return View(project);
        }

        // GET: Projects/Create
        public IActionResult Create()
        {
            ViewData["Tasks"] = new MultiSelectList(_context.Tasks, "Id", "Name");
            return View();
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] Project project, int[] Tasks)
        {
            if (ModelState.IsValid)
            {
                project.Tasks = await GetSelectedTasks(Tasks);
                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["Tasks"] = new MultiSelectList(_context.Tasks, "Id", "Name", Tasks);
            return View(project);
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .Include(p => p.Tasks).ThenInclude(t => t.Status)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) return NotFound();

            ViewData["Tasks"] = new MultiSelectList(_context.Tasks, "Id", "Name", project.Tasks.Select(t => t.Id));
            return View(project);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Project project, int[] Tasks)
        {
            if (id != project.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var projectToUpdate = await _context.Projects
                    .Include(p => p.Tasks).ThenInclude(t => t.Status)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (projectToUpdate == null) return NotFound();

                // Оновлення даних проєкту
                projectToUpdate.Name = project.Name;
                projectToUpdate.Description = project.Description;

                // Оновлення зв'язків із завданнями
                await UpdateProjectTasks(projectToUpdate, Tasks);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["Tasks"] = new MultiSelectList(_context.Tasks, "Id", "Name", Tasks);
            return View(project);
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .Include(p => p.Tasks).ThenInclude(t => t.Status)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null) return NotFound();

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }

        private async Task<List<Models.Task>> GetSelectedTasks(int[] taskIds)
        {
            return await _context.Tasks
                .Where(t => taskIds.Contains(t.Id))
                .ToListAsync();
        }

        private async System.Threading.Tasks.Task UpdateProjectTasks(Project project, int[] taskIds)
        {
            var selectedTasks = await GetSelectedTasks(taskIds);

            project.Tasks.Clear();
            foreach (var task in selectedTasks)
            {
                project.Tasks.Add(task);
            }
        }

        public async Task<IActionResult> SearchTasks(int id, string term)
        {
            var project = await _context.Projects
                .Include(p => p.Tasks).ThenInclude(t => t.Status)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) return NotFound();

            var filteredTasks = project.Tasks
                .Where(t => t.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return PartialView("_TaskList", filteredTasks);
        }
    }
}

