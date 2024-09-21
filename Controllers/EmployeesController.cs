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
    public class EmployeesController : Controller
    {
        private readonly ProjectManagementSystemContext _context;

        public EmployeesController(ProjectManagementSystemContext context)
        {
            _context = context;
        }

        // GET: Employees
        public async Task<IActionResult> Index()
        {
            var projectManagementSystemContext = _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Tasks);
            return View(await projectManagementSystemContext.ToListAsync());
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Tasks)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Name");
            ViewData["Tasks"] = new MultiSelectList(_context.Tasks, "Id", "Name");
            ViewData["Employees"] = new MultiSelectList(_context.Employees, "Id", "FullName");
            return View();
        }

        // POST: Employees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Surname,Email,PhoneNumber,PositionId,Tasks")] Employee employee, int[] Tasks)
        {
            if (Tasks != null)
            {
                foreach (var taskId in Tasks)
                {
                    var task = await _context.Tasks.FindAsync(taskId);
                    if (task != null)
                    {
                        employee.Tasks.Add(task);
                    }
                }
            }

            _context.Add(employee);
            await _context.SaveChangesAsync();
                
            ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Name", employee.PositionId);
            ViewData["Tasks"] = new MultiSelectList(_context.Tasks, "Id", "Name", employee.Tasks.Select(t => t.Id));
            //return View(employee);
            return RedirectToAction(nameof(Index));
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Tasks)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }
            ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Name", employee.PositionId);
            ViewData["Tasks"] = new MultiSelectList(_context.Tasks, "Id", "Name", employee.Tasks.Select(t => t.Id));
            return View(employee);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Surname,Email,PhoneNumber,PositionId")] Employee employee, int[] Tasks)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            try
            {
                _context.Update(employee);
                await _context.SaveChangesAsync();

                var employeeInDb = await _context.Employees
                .Include(e => e.Tasks)
                .FirstOrDefaultAsync(m => m.Id == id);

                employeeInDb?.Tasks.Clear();
                foreach (var taskId in Tasks)
                {
                    var task = await _context.Tasks.FindAsync(taskId);
                    if (task != null)
                    {
                        employeeInDb?.Tasks.Add(task);
                    }
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(employee.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.Tasks)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}
