using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChartsController : ControllerBase
    {

        private readonly ProjectManagementSystemContext _context;

        public ChartsController(ProjectManagementSystemContext context)
        {
            _context = context;
        }

        [HttpGet("TaskStatusCountsJson")]
        public JsonResult TaskStatusCountsJson()
        {
            var tasks = _context.Tasks.Include(t => t.Status).ToList();

            var statusCounts = tasks.GroupBy(t => t.Status?.Name)
                                    .Select(group => new { Status = group.Key, Count = group.Count() })
                                    .ToList();

            List<object> taskStatusCounts = new List<object>();
            taskStatusCounts.Add(new[] { "Статус завдання", "Кількість" });

            foreach (var statusCount in statusCounts)
            {
                taskStatusCounts.Add(new object[] { statusCount.Status, statusCount.Count });
            }

            return new JsonResult(taskStatusCounts);
        }

        [HttpGet("TaskCountsByProjectJson")]
        public JsonResult TaskCountsByProjectJson()
        {
            var tasks = _context.Tasks.Include(t => t.Project).Include(t => t.Status).ToList();

            var statuses = tasks.Select(t => t.Status?.Name).Distinct().ToList();
            var projects = tasks.Select(t => t.Project?.Name).Distinct().ToList();

            List<object> taskCounts = new List<object>();

            var headers = new List<string> { "Проект" };
            headers.AddRange(statuses);
            taskCounts.Add(headers.ToArray());

            foreach (var project in projects)
            {
                var counts = new List<object> { project };

                foreach (var status in statuses)
                {
                    var count = tasks.Count(t => t.Project.Name == project && t.Status.Name == status);
                    counts.Add(count);
                }

                taskCounts.Add(counts.ToArray());
            }

            return new JsonResult(taskCounts);
        }
    }
}
