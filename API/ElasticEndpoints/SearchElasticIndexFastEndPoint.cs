//using FastEndpoints;
//using TODO_List.Application.DTO;
//using TODO_List.Domain.Model;
//using TODO_List.Infrastructure.Services;

//namespace TODO_List.API.ElasticEndpoints
//{
//    public class SearchElasticIndexFastEndPoint : Endpoint<SearchElasticRequest>
//    {
//        private readonly ElasticService _elastic;
//        public SearchElasticIndexFastEndPoint(ElasticService elastic)
//        {
//            _elastic = elastic;
//        }
//        public override void Configure()
//        {
//            Get("/api/Elastic/MultiSearchIndex/{Title}");
//            AllowAnonymous();
//            Options(o => o.CacheOutput(p =>
//            {
//                p.Expire(TimeSpan.FromMinutes(2));
//            }));
//        }
//        public override async Task HandleAsync(SearchElasticRequest req, CancellationToken ct)
//        {
//            var response = await _elastic.SearchTasksAsync(req.Title);
//            await HttpContext.Response.WriteAsJsonAsync(response, cancellationToken: ct);
//        }
//    }
//}
