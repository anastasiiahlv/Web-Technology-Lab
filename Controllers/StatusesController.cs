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
                // Cache not found, retrieving data from the database
                statuses = await _context.Statuses.Include(s => s.Tasks).ToListAsync();

                // Set cache entry options
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))  // Cache sliding expiration time
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1)); // Total cache lifespan

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

            // Define a cache key specific to the status details
            var cacheKey = $"statusDetails-{id}";

            if (!_cache.TryGetValue(cacheKey, out Status? status))
            {
                // Cache not found, retrieve from the database
                status = await _context.Statuses
                    .Include(s => s.Tasks)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (status == null)
                {
                    return NotFound();
                }

                // Set cache options
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))  // Cache expiration time
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1)); // Total cache lifespan

                // Save the status in cache
                _cache.Set(cacheKey, status, cacheEntryOptions);
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
                    ModelState.AddModelError("Name", "A status with this name already exists.");
                    return View(status);
                }

                _context.Add(status);
                await _context.SaveChangesAsync();

                // Clear the cache after creating a new status
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

                    // Clear the cache after editing the status
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

                // Clear the cache after deleting the status
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


