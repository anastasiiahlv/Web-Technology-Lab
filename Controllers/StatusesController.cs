using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Models;
using Microsoft.Extensions.Caching.Memory;

namespace ProjectManagementSystem.Controllers
{
    public class StatusesController : Controller
    {
        private readonly ProjectManagementSystemContext _context;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "statusesList";

        public StatusesController(ProjectManagementSystemContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: Statuses
        public async Task<IActionResult> Index()
        {
            if (!_cache.TryGetValue(CacheKey, out List<Status>? statuses))
            {
                // Кеш не знайдено, отримуємо дані з БД
                statuses = await _context.Statuses.Include(s => s.Tasks).ToListAsync();

                // Налаштування параметрів кешування
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))  // Час збереження в кеші
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1)); // Загальний час існування в кеші

                _cache.Set(CacheKey, statuses, cacheEntryOptions);
            }

            return View(statuses);
        }

        // GET: Statuses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var status = await _context.Statuses
                .Include(s => s.Tasks)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (status == null)
            {
                return NotFound();
            }

            return View(status);
        }

        // GET: Statuses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Statuses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Status status)
        {
            if (ModelState.IsValid)
            {
                var existingStatus = await _context.Statuses.FirstOrDefaultAsync(s => s.Name == status.Name);
                if (existingStatus != null)
                {
                    ModelState.AddModelError("Name", "Статус з такою назвою вже існує.");
                    return View(status);
                }

                _context.Add(status);
                await _context.SaveChangesAsync();

                // Очистка кешу після створення нового статусу
                _cache.Remove(CacheKey);

                return RedirectToAction(nameof(Index));
            }
            return View(status);
        }

        // GET: Statuses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var status = await _context.Statuses.FindAsync(id);
            if (status == null)
            {
                return NotFound();
            }
            return View(status);
        }

        // POST: Statuses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Status status)
        {
            if (id != status.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(status);
                    await _context.SaveChangesAsync();

                    // Очистка кешу після редагування статусу
                    _cache.Remove(CacheKey);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StatusExists(status.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(status);
        }

        // GET: Statuses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var status = await _context.Statuses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (status == null)
            {
                return NotFound();
            }

            return View(status);
        }

        // POST: Statuses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var status = await _context.Statuses.FindAsync(id);
            if (status != null)
            {
                _context.Statuses.Remove(status);
                await _context.SaveChangesAsync();

                // Очистка кешу після видалення статусу
                _cache.Remove(CacheKey);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool StatusExists(int id)
        {
            return _context.Statuses.Any(e => e.Id == id);
        }

        public IActionResult CheckCache()
        {
            if (_cache.TryGetValue(CacheKey, out List<Status>? statuses))
            {
                return Json(new { cached = true, count = statuses.Count });
            }
            return Json(new { cached = false });
        }
    }
}

