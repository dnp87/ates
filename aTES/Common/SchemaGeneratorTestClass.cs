using Common.Events;
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
            JSchemaGenerator generator = new JSchemaGenerator();

            JSchema schema1 = generator.Generate(typeof(TaskCompletedEventV1));            ;
            JSchema schema2 = generator.Generate(typeof(TaskAssignedEventV1));
            JSchema schema3 = generator.Generate(typeof(TaskCreatedEventV1));
            JSchema schema4 = generator.Generate(typeof(TaskCreatedEventV2));


            string test1 = schema1.ToString();
            string test2 = schema2.ToString();
            string test3 = schema3.ToString();
            string test4 = schema4.ToString();
        }
    }
}
