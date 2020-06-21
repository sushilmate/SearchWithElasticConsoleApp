using System;
using Elasticsearch.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SearchWithElasticConsoleApp
{
    public class Employee
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("App Started.");

            var client = new ElasticLowLevelClient();

            var health = client.Cat.Health<StringResponse>();

            if (!health.Success)
            {
                Console.WriteLine($"ElasticSearch Instance is down - {health.OriginalException.Message}");
                Console.ReadKey();
                return;
            }

            var employee = new Employee
            {
                FirstName = "Sushil",
                LastName = "Mate"
            };

            var result = client.Index<StringResponse>("employees", "1", PostData.Serializable(employee));

            //result = await client.IndexAsync<StringResponse>("employees", "1", PostData.Serializable(employee));

            Console.WriteLine(JToken.Parse(result.Body).ToString(Formatting.Indented));

            Console.WriteLine("Bulk Copy");

            Console.WriteLine(BulkCopyToElastic(client));

            var searchResponse = client.Search<StringResponse>("employees", PostData.Serializable(new
            {
                from = 0,
                size = 10,
                query = new
                {
                    match = new
                    {
                        firstName = new
                        {
                            query = "Martijn"
                        }
                    }
                }
            }));

            Console.WriteLine($"Search Result {searchResponse.Success}");
            Console.WriteLine(searchResponse.Body.ToString(Formatting.Indented));

            Console.WriteLine("*************** Search with NEST ***************");
            var nest = new ElasticSearchWithNEST();

            if (!nest.CheckHealth())
            {
                return;
            }

            var res = nest.IndexCustomer(new Customer() { Id = 1, Name = "Alok", Address = "Berlin" });

            Console.WriteLine(res.ToString(Formatting.Indented));

            Console.WriteLine("Search Customer on Elastic by NEST");

            var customers = nest.FindDocumentByCustomerName("alok");

            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(customers).ToString(Formatting.Indented));

            Console.ReadKey();
        }

        static string BulkCopyToElastic(ElasticLowLevelClient client)
        {
            var people = new object[]
            {
                new { index = new { _index = "employees", _type = "_doc", _id = "1"  }},
                new { FirstName = "Martijn", LastName = "Laarman" },
                new { index = new { _index = "employees", _type = "_doc", _id = "2"  }},
                new { FirstName = "Greg", LastName = "Marzouka" },
                new { index = new { _index = "employees", _type = "_doc", _id = "3"  }},
                new { FirstName = "Russ", LastName = "Cam" },
            };

            var result = client.Bulk<StringResponse>(PostData.MultiJson(people));

            return JToken.Parse(result.Body).ToString(Formatting.Indented);
        }
    }

    public static class StringExtension
    {
        public static string ToString(this string str, Formatting format = Formatting.Indented)
        {
            try
            {
                return JToken.Parse(str).ToString(format);
            }
            catch (Exception ex)
            {
                return str;
            }
        }
    }
}
