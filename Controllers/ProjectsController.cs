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
            var projectManagementSystemContext = _context.Projects
                .Include(e => e.Tasks);
            return View(await projectManagementSystemContext.ToListAsync());
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        public IActionResult Create()
        {
            ViewData["Tasks"] = new MultiSelectList(_context.Tasks, "Id", "Name");
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Tasks")] Project project, int[] Tasks)
        {
            if (Tasks != null)
            {
                foreach (var taskId in Tasks)
                {
                    var task = await _context.Tasks.FindAsync(taskId);
                    if (task != null)
                    {
                        project.Tasks.Add(task);
                    }
                }
            }

            _context.Add(project);
            await _context.SaveChangesAsync();

            ViewData["Tasks"] = new MultiSelectList(_context.Tasks, "Id", "Name", project.Tasks.Select(t => t.Id));
            return RedirectToAction(nameof(Index));
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(e => e.Tasks)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }
            ViewData["Tasks"] = new MultiSelectList(_context.Tasks, "Id", "Name", project.Tasks.Select(t => t.Id));
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Project project, int[] Tasks)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            try
            {
                _context.Update(project);
                await _context.SaveChangesAsync();

                var projectInDb = await _context.Projects
                .Include(e => e.Tasks)
                .FirstOrDefaultAsync(m => m.Id == id);

                projectInDb?.Tasks.Clear();
                foreach (var taskId in Tasks)
                {
                    var task = await _context.Tasks.FindAsync(taskId);
                    if (task != null)
                    {
                        projectInDb?.Tasks.Add(task);
                    }
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(project.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

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
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}
