using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TODO_List.Application.ValidatorFastEndpoint;
using TODO_List.Application.DTO;
using TODO_List.Domain.Model;
using TODO_List.Application.Interfaces;
//using TODO_List.Infrastructure.Services;

namespace TODO_List.API.NHibernateFastEndpoints
{
    public class NHF_AddTask : Endpoint<AddTaskRequest>
    {
        private readonly NHibernate.ISession _session;
        private readonly IRedisService _redis;
        //private readonly ElasticService _elastic;
        public NHF_AddTask(NHibernate.ISession session, IRedisService redis/*, ElasticService elastic*/)
        {
            _session = session;
            _redis = redis;
            //_elastic = elastic;
        }
        public override void Configure()
        {
            Post("/api/NHibernate/AddTask");
            Roles("Admin");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            Validator<AddTaskValidation>();
        }
        public override async Task HandleAsync(AddTaskRequest req, CancellationToken ct)
        {
            var task = new NH_TaskDbModel
            {
                TaskId = req.tid,
                TitleName = req.tname,
                IsCompleted = req.tisCompleted,
                SubTasks = new List<NH_SubTaskDbModel>()
            };
            foreach (var st in req.subTasks)
            {
                var subTask = new NH_SubTaskDbModel
                {
                    SubTaskId = st.subTaskId,
                    SubTaskName = st.subTaskName,
                    Task = task   
                };
                task.SubTasks.Add(subTask);  
            }
            await _session.SaveAsync(task, ct);
            await _session.FlushAsync(ct);
            var response = new NH_TaskResponseDTO
            {
                TaskId = task.TaskId,
                TitleName = task.TitleName,
                IsCompleted = task.IsCompleted,
                SubTasks = task.SubTasks.Select(st => new NH_SubTaskResponseDTO
                {
                    SubTaskId = st.SubTaskId,
                    SubTaskName = st.SubTaskName
                }).ToList()
            };
            //await _elastic.IndexOneTaskAsync(new ElasticIndexModel
            //{
            //    Id = req.tid.ToString(),
            //    Title = req.tname,
            //    Tags = req.Tags,
            //    Description = req.Description,
            //    CreatedAt = DateTime.UtcNow,
            //    IsCompleted = req.tisCompleted
            //});
            await _redis.SetAsync($"nh_task:{response.TaskId}", response, TimeSpan.FromMinutes(20));
            HttpContext.Response.StatusCode = StatusCodes.Status201Created;
            await HttpContext.Response.WriteAsJsonAsync(response, cancellationToken: ct);
        }
    }
}
