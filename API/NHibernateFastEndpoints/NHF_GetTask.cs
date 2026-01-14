using FastEndpoints;
using NHibernate;
using NHibernate.Linq;
using TODO_List.Application.DTO;
using TODO_List.Application.Interfaces;
using TODO_List.Application.ValidatorFastEndpoint;
using TODO_List.Domain.Model;

namespace TODO_List.API.NHibernateFastEndpoints
{
    public class NHF_GetTask : Endpoint<GetAllPaginatedRequest>
    {
        private readonly NHibernate.ISession _session;
        private readonly IRedisService _redis;
        public NHF_GetTask(NHibernate.ISession session, IRedisService redis)
        {
            _session = session;
            _redis = redis;
        }
        public override void Configure()
        {
            Get("/api/NHibernate/GetAllTaskPaginated");
            AllowAnonymous();
            Validator<GetTaskValidation>();
        }
        public override async Task HandleAsync(GetAllPaginatedRequest req, CancellationToken ct)
        {
            var cachekey = $"task:page:{req.page}:size:{req.pageSize}";
            var cachedtask = await _redis.GetAsync<GetAllPaginatedResponse<TaskModelDTO>>(cachekey);
            if (cachedtask is not null)
            {
                await HttpContext.Response.WriteAsJsonAsync(cachedtask, cancellationToken: ct);
                return;
            }
            int skip = (req.page - 1) * req.pageSize;
            var total = await _session.Query<NH_TaskDbModel>().CountAsync(ct);
            var tasks = await _session.Query<NH_TaskDbModel>()
                                      .FetchMany(x => x.SubTasks)
                                      .OrderBy(t => t.TaskId)
                                      .Skip(skip)
                                      .Take(req.pageSize)
                                      .ToListAsync(ct);
            var data = tasks.Select(t => new TaskModelDTO
            {
                Tid = t.TaskId,
                Tname = t.TitleName,
                TisCompleted = t.IsCompleted,
                subTasks = t.SubTasks.Select(st => new SubTaskModelDTO
                {
                    subTaskId = st.SubTaskId,
                    subTaskName = st.SubTaskName
                }).ToList()
            }).ToList();
            var response = new GetAllPaginatedResponse<TaskModelDTO>
            {
                Items = data,
                Page = req.page,
                PageSize = req.pageSize,
                TotalCount = total
            };
            await _redis.SetAsync(cachekey, response, TimeSpan.FromMinutes(20));
            await HttpContext.Response.WriteAsJsonAsync(response, ct);
        }
    }
}
