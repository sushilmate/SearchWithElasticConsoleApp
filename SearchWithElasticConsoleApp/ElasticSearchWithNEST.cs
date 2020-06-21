using Nest;
using System;
using System.Collections.Generic;

namespace SearchWithElasticConsoleApp
{
    public class ElasticSearchWithNEST
    {
        private readonly ElasticClient _client;
        private const string MyElasticAzureCloud = "https://fd6b03c3df4e49dcba356a2e369b39d6.westeurope.azure.elastic-cloud.com:9243/";
        public ElasticSearchWithNEST()
        {
            //"http://localhost:9200"
            var connection = new ConnectionSettings(new Uri(MyElasticAzureCloud))
                                .DefaultIndex("customer");
            connection.BasicAuthentication("elastic", "cIBP8KZRrWgT5w7wEyTQEJuk");
            _client = new ElasticClient(connection);
        }

        public bool CheckHealth()
        {
            var healthRe = new CatHealthRequest();
            healthRe.Pretty = true;

            var health = _client.Cat.Health(healthRe);

            if (!health.IsValid)
            {
                Console.WriteLine($"ElasticSearch Instance is down - {health.OriginalException.Message}");
                Console.ReadKey();
                return false;
            }
            return true;
        }

        public ElasticSearchWithNEST(ElasticClient client)
        {
            _client = client;
        }

        public string IndexCustomer(Customer customer)
        {
            var response = _client.IndexDocument(customer);

            return response.Result.ToString();
        }

        public IEnumerable<Customer> FindDocumentByCustomerName(string name)
        {
            var res = _client.Search<Customer>(x =>
                                    x.From(0)
                                    .Size(5)
                                    .Query(q =>
                                        q.Match(m =>
                                            m.Field(f => f.Name == name))));

            return res.Documents;
        }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
    }
}
