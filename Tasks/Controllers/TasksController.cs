using Domain.Models;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Tasks.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TasksController : ControllerBase
    {

        private readonly ILogger<TasksController> _logger;
        private readonly TasksContext _context;
        private List<AllTasks> _task;

        public TasksController(ILogger<TasksController> logger, TasksContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public IEnumerable<AllTasks> GetTasks()
        {
            var userId = HttpContext.User.Claims.First().Value;
            return _context.allTasks
                .Where(q=>q.OwnerId == userId);
        }

        [Authorize]
        [HttpGet]
        [Route("GetTasksById")]
        public IEnumerable<AllTasks> GetTasksById(int id)
        {
            return _context.allTasks.Where(x => x.Id == id);
        }

        [Authorize]
        [HttpDelete]
        public void DeleteTask(int id)
        {
            AllTasks task = _context.allTasks.Where(x => x.Id == id).FirstOrDefault();
            if (task != null)
            {
                _context.allTasks.Remove(task);
                _context.SaveChanges();
            }
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> PutTask(int id, AllTasks allTasks)
        {
            if (id != allTasks.Id)
            {
                return BadRequest();
            }
            var userId = HttpContext.User.Claims.First().Value;
            allTasks.OwnerId = userId;
            _context.Entry(allTasks).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(allTasks);

        }

        [Authorize]
        [HttpPost]
        public void PostTasks([FromBody]AllTasks tasks)
        {
            var userId = HttpContext.User.Claims.First().Value;
            tasks.OwnerId = userId;
            _context.allTasks.Add(tasks);
            _context.SaveChanges();
        }
    }
}