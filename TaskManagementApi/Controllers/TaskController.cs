using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Data;
using TaskManagementApi.Models;

namespace TaskManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly TaskContext _context;

        public TaskController(TaskContext context)
        {
            _context = context;
        }

        // GET: api/Task/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItem>> GetTaskItem(int id)
        {
            var taskItem = await _context.Tasks.FindAsync(id);
            if (taskItem == null)
            {
                return NotFound();
            }
            return taskItem;
        }

        // POST: api/Task
        [HttpPost]
        public async Task<ActionResult<TaskItem>> CreateTaskItem(TaskItem item)
        {
            _context.Tasks.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTaskItem), new { id = item.Id }, item);
        }

        // GET: api/Task
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks(string status = null, string sortBy = "name", bool sortAscending = true, bool onlyFavorites = false)
        {
            var tasksQuery = _context.Tasks.AsQueryable();

            // Filtering
            if (!string.IsNullOrEmpty(status))
            {
                tasksQuery = tasksQuery.Where(t => t.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }
            if (onlyFavorites)
            {
                tasksQuery = tasksQuery.Where(t => t.isFavorite);
            }

            // Sorting
            tasksQuery = sortBy.ToLower() switch
            {
                "name" => sortAscending ? tasksQuery.OrderBy(t => t.Name) : tasksQuery.OrderByDescending(t => t.Name),
                "deadline" => sortAscending ? tasksQuery.OrderBy(t => t.Deadline) : tasksQuery.OrderByDescending(t => t.Deadline),
                _ => tasksQuery.OrderBy(t => t.Name)
            };

            return await tasksQuery.ToListAsync();
        }

        // PUT: api/Task/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTaskItem(int id, TaskItem taskItem)
        {
            if (id != taskItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(taskItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(taskItem);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }

        }

        // DELETE: api/Task/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTaskItem(int id)
        {
            var taskItem = await _context.Tasks.FindAsync(id);
            if (taskItem == null)
            {
                return NotFound();
            }

            _context.Tasks.Remove(taskItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Task item deleted successfully" });
        }


        [HttpPost("{id}/upload")]
        public async Task<IActionResult> UploadFile(int id, IFormFile file)
        {
            var taskItem = await _context.Tasks.FindAsync(id);
            if (taskItem == null)
            {
                return NotFound();
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "upload");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var fileName = "testfile.jpg";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            taskItem.ImageUrl = $"/upload/{fileName}";
            _context.Entry(taskItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var response = new UploadFileResponse
            {
                Id = taskItem.Id,
                ImageUrl = $"/upload/{fileName}"
            };

            return Ok(response);
        }

    }
}
