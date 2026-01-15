using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Threading.Tasks;
using TODO_List.Application.DTO;
using TODO_List.Application.Interfaces;
using TODO_List.Domain.Entities;
using TODO_List.Domain.Model;
//using TODO_List.Infrastructure.Services;

namespace TODO_List.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ITaskRepo _taskrepo;
        private readonly IRedisService _redis;
        //private readonly ElasticService _elastic;
        public TaskController(ITaskRepo taskrepo, IRedisService redis/*, ElasticService elastic*/)
        {
            _taskrepo = taskrepo;
            _redis = redis;
            //_elastic = elastic;
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("AddTask")]
        public async Task<ActionResult<TaskModelDTO>> AddTask([FromBody] TodoModel task)
        {
            var addedTask = await _taskrepo.AddTask(task);
            await _redis.SetAsync($"api_task:{addedTask.Tid}",addedTask, TimeSpan.FromMinutes(20));
            await _redis.DeleteAsync("api_all_tasks");
            return Ok(addedTask);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("GetAllTasks")]
        public async Task<ActionResult<List<TaskModelDTO>>> GetAllTasks()
        {
            var cachekey = "api_all_tasks";
            var cachedtask = await _redis.GetAsync<List<TaskModelDTO>>(cachekey);
            if (cachedtask != null && cachedtask.Count > 0)
            {
                return Ok(new { source = "redis", data = cachedtask });
            }
            var tasks = await _taskrepo.GetAllTasks();
            if (tasks == null || !tasks.Any())
            {
                return NotFound(new { message = "No tasks found!" });
            }
            await _redis.SetAsync(cachekey, tasks, TimeSpan.FromMinutes(20));
            return Ok(new { source = "Database", data = tasks });
        }
        [HttpGet("SearchTaskById/{TaskID}")]
        public async Task<ActionResult<TaskModelDTO>> SearchTaskById([FromRoute] int TaskID)
        {
            var cachekey = $"api_task:{TaskID}";
            var cachedtask = await _redis.GetAsync<TaskModelDTO>(cachekey);
            if (cachedtask != null)
            {
                return Ok(new { source = "redis", data = cachedtask });
            }
            var task = await _taskrepo.SearchTaskById(TaskID);
            if (task == null)
            {
                return NotFound($"Task with ID {TaskID} not found");
            }
            await _redis.SetAsync(cachekey,task, TimeSpan.FromMinutes(20));
            return Ok(task);
        }
        //[HttpGet("ElasticSearch")]
        //public async Task<IActionResult> ElasticSearch(string keyword, [FromServices] ElasticService es)
        //{
        //    var result = await es.Client.SearchAsync<TaskModelDTO>(s => s
        //        .Index("todos")
        //        .Query(q => q
        //            .Match(m => m
        //                .Field(f => f.Tname) 
        //                .Query(keyword)
        //            )
        //        )
        //    );
        //    return Ok(result.Documents);
        //}
        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateTaskById/{TaskID}")]
        public async Task<ActionResult<TaskModelDTO>> UpdateTaskById([FromRoute] int TaskID, [FromBody] TodoModel task)
        {
            var updatedTask = await _taskrepo.UpdateTaskById(TaskID, task);
            if (updatedTask == null)
            {
                return NotFound($"Task with ID {TaskID} not found");
            }
            var cachekey = $"api_task:{TaskID}";
            await _redis.SetAsync(cachekey, updatedTask, TimeSpan.FromMinutes(20));
            await _redis.DeleteAsync("api_all_tasks");
            return Ok(updatedTask);
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteTaskById/{TaskID}")]
        public async Task<ActionResult> DeleteTaskById([FromRoute] int TaskID)
        {
            var isDeleted = await _taskrepo.DeleteTaskById(TaskID);
            if (!isDeleted)
            {
                return NotFound($"Task with ID {TaskID} not found");
            }
            await _redis.DeleteAsync($"api_task:{TaskID}");
            await _redis.DeleteAsync("api_all_tasks");
            return Ok($"Task with ID {TaskID} has been deleted");
        }
        [HttpGet("ThrowTestError")]
        public IActionResult ThrowTestError()
        {
            throw new Exception("This is a test exception");
        }
    }
}
