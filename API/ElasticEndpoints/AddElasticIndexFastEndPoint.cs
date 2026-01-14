using FastEndpoints;
using TODO_List.Domain.Model;
using TODO_List.Infrastructure.Services;

namespace TODO_List.API.ElasticEndpoints
{
    public class AddElasticIndexFastEndPoint : Endpoint<ElasticIndexModel>
    {
        private readonly ElasticService _elastic;
        public AddElasticIndexFastEndPoint(ElasticService elastic)
        {
            _elastic = elastic;
        }
        public override void Configure()
        {
            Post("/api/Elastic/AddIndex");
        }
        public override async Task HandleAsync(ElasticIndexModel req, CancellationToken ct)
        {
            await _elastic.IndexOneTaskAsync(new ElasticIndexModel
            {
                Id = req.Id,
                Title = req.Title,
                Description = req.Description,
                Tags = req.Tags,
                CreatedAt = DateTime.UtcNow,
                IsCompleted = req.IsCompleted
            });
        }
    }
}
