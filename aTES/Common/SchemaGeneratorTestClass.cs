using Common.Constants;
using Common.Events;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Program
    {
        public static void Main()
        {
           /*var adminClient = new AdminClientBuilder(
                new AdminClientConfig
                {
                    BootstrapServers = "localhost:9092"
                }).Build();
            try
            {
                adminClient.CreateTopicsAsync(new TopicSpecification[] {
                    new TopicSpecification { Name = TopicNames.AccountLogCreatedV1, ReplicationFactor = 1, NumPartitions = 1 }
                }).Wait();                    
            }
            catch (CreateTopicsException e)
            {
                Console.WriteLine($"An error occured creating topic {e.Results[0].Topic}: {e.Results[0].Error.Reason}");
            }*/

            var gen = new JSchemaGenerator();
            var sc1 = gen.Generate(typeof(AccountLogCreatedV1));
            
            string str1 = sc1.ToString();
        }
    }
}
