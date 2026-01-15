//using FastEndpoints;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using NHibernate;
//using NHibernate.Linq;
//using TODO_List.Application.DTO;
//using TODO_List.Application.Interfaces;
//using TODO_List.Application.ValidatorFastEndpoint;
//using TODO_List.Domain.Model;

//namespace TODO_List.API.NHibernateFastEndpoints
//{
//    public class NHF_DeleteTask : Endpoint<DeleteTaskRequest>
//    {
//        private readonly NHibernate.ISession _session;
//        //private readonly IRedisService _redis;
//        public NHF_DeleteTask(NHibernate.ISession session/*, IRedisService redis*/)
//        {
//            _session = session;
//            //_redis = redis;
//        }
//        public override void Configure()
//        {
//            Delete("/api/NHibernate/DeleteTask");
//            Roles("Admin");
//            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
//            Validator<DeleteTaskValidation>();
//        }
//        public override async Task HandleAsync(DeleteTaskRequest req, CancellationToken ct)
//        {
//            var cachekey = $"nh_task:{req.tid}";
//            using var tx = _session.BeginTransaction();
//            var task = await _session.Query<NH_TaskDbModel>()
//                                     .FetchMany(t => t.SubTasks)
//                                     .Where(t => t.TaskId == req.tid)
//                                     .SingleOrDefaultAsync(ct);
//            if (task == null)
//            {
//                HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
//                await HttpContext.Response.WriteAsync($"No task found with ID {req.tid}", ct);
//                return;
//            }
//            await _session.DeleteAsync(task, ct);
//            await tx.CommitAsync(ct);
//            //await _redis.DeleteAsync(cachekey);
//            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
//            await HttpContext.Response.WriteAsync($"Task with ID {req.tid} deleted successfully.", ct);
//        }
//    }
//}
