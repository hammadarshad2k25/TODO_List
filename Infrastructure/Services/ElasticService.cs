using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Transport;
using System.Security.Cryptography.X509Certificates;
using TODO_List.Domain.Model;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace TODO_List.Infrastructure.Services
{
    public class ElasticService
    {
        public ElasticsearchClient Client { get; }
        public async Task IndexOneTaskAsync(ElasticIndexModel model)
        {
            var response = await Client.IndexAsync(model, idx => idx.Index("tasks_index"));
            if (!response.IsValidResponse)
            {
                Console.WriteLine("Index creation failed:");
                Console.WriteLine(response.DebugInformation);
            }
            else
            {
                Console.WriteLine("Index created successfully");
            }
        }
        public async Task<IEnumerable<ElasticIndexModel>> SearchTasksAsync(string Keyword)
        {
            var response = await Client.SearchAsync<ElasticIndexModel>(s => s
        .Index("tasks_index")
        .Query(q => q
            .Bool(b => b
                .Must(m => m
                    .MultiMatch(mm => mm
                        .Query(Keyword)
                        .Fields(new[] { "title", "description" })
                    )
                )
                .Filter(f => f
                    .Term(t => t
                        .Field("isCompleted")
                        .Value(false)
                    )
                )
                .Should(sh => sh
                    .Match(ma => ma
                        .Field("tags")
                        .Query("OutFiters")
                        .Boost(2) 
                    )
                )
            )
        )
    );
            return response.Documents;
        }
        public ElasticService()
        {
            var certpath = @"D:\ElasticSearch\elasticsearch-9.2.2-windows-x86_64\elasticsearch-9.2.2\config\certs\http_ca.crt";
            var caCert = new X509Certificate2(certpath);
            var settings = new ElasticsearchClientSettings(new Uri("https://localhost:9200"))
                .ServerCertificateValidationCallback((sender, cert, chain, errors) =>
                {
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
                    chain.ChainPolicy.ExtraStore.Add(caCert);
                    bool isValid = chain.Build((X509Certificate2)cert);
                    return isValid;
                })
                .Authentication(new BasicAuthentication("elastic", "jTe9Uryfg9rW4unvKk=h"));
            Client = new ElasticsearchClient(settings);
        }
    }
}



