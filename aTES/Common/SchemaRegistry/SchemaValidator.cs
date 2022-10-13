using Common.Events;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.SchemaRegistry
{
    internal class SchemaValidator : ISchemaValidator
    {
        public bool ValidateBySchema<T>(string json) where T : EventBase
        {
            // generate schema from type and validate by schema
            var generator = new JSchemaGenerator();
            var schema = generator.Generate(typeof(T));

            try
            {
                var parsedJObject = JObject.Parse(json);
                return parsedJObject.IsValid(schema);
            }
            catch
            {
                // todo: log
                return false;
            }
        }
    }
}
