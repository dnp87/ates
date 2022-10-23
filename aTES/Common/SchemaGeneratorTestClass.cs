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
                    new TopicSpecification { Name = TopicNames.TaskCreatedV1, ReplicationFactor = 1, NumPartitions = 1 },
                    new TopicSpecification { Name = TopicNames.TaskCreatedV2, ReplicationFactor = 1, NumPartitions = 1 },
                    new TopicSpecification { Name = TopicNames.TaskAssignedV1, ReplicationFactor = 1, NumPartitions = 1 },
                    new TopicSpecification { Name = TopicNames.TaskCompletedV1, ReplicationFactor = 1, NumPartitions = 1 },
                }).Wait();                    
            }
            catch (CreateTopicsException e)
            {
                Console.WriteLine($"An error occured creating topic {e.Results[0].Topic}: {e.Results[0].Error.Reason}");
            }*/

            var gen = new JSchemaGenerator();
            var sc1 = gen.Generate(typeof(TaskAssignedEventV2));
            var sc2 = gen.Generate(typeof(TaskCompletedEventV2));
            var sc3 = gen.Generate(typeof(TaskCreatedEventV3));

            string str1 = sc1.ToString();
            string str2 = sc2.ToString();
            string str3 = sc3.ToString();
        }
    }
}
