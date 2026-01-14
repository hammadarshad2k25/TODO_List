using FastEndpoints;
using NHibernate;
using NHibernate.Linq;
using TODO_List.Application.DTO;
using TODO_List.Application.Interfaces;
using TODO_List.Application.ValidatorFastEndpoint;
using TODO_List.Domain.Model;

namespace TODO_List.API.NHibernateFastEndpoints
{
    public class NHF_SearchTask : Endpoint<SearchTaskRequest>
    {
        private readonly NHibernate.ISession _session;
        private readonly IRedisService _redis;
        public NHF_SearchTask(NHibernate.ISession session, IRedisService redis)
        {
            _session = session;
            _redis = redis;
        }
        public override void Configure()
        {
            Get("/api/NHibernate/SearchTask");
            AllowAnonymous();
            Validator<SearchTaskValidation>();
        }
        public override async Task HandleAsync(SearchTaskRequest req, CancellationToken ct)
        {
            var cachekey = $"nh_task:{req.tid}";
            var cachedtask = await _redis.GetAsync<TaskModelDTO>(cachekey);
            if (cachedtask is not null)
            {
                await HttpContext.Response.WriteAsJsonAsync(cachedtask, cancellationToken: ct);
                return;
            }
            var task = await _session.Query<NH_TaskDbModel>()
                .FetchMany(x => x.SubTasks)
                .Where(t => t.TaskId == req.tid)
                .SingleOrDefaultAsync(ct);
            if (task == null)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                await HttpContext.Response.WriteAsync($"No tasks found with ID {req.tid}.", ct);
                return;
            }
            var response = new TaskModelDTO
            {
                Tid = task.TaskId,
                Tname = task.TitleName,
                TisCompleted = task.IsCompleted,
                subTasks = task.SubTasks.Select(st => new SubTaskModelDTO
                {
                    subTaskId = st.SubTaskId,
                    subTaskName = st.SubTaskName
                }).ToList()
            };
            await _redis.SetAsync(cachekey, response, TimeSpan.FromMinutes(20));
            await HttpContext.Response.WriteAsJsonAsync(response, cancellationToken: ct);
        }
    }
}
