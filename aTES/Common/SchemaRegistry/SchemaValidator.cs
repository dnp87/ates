using Common.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.IO;

namespace Common.SchemaRegistry
{
    public class SchemaValidator : ISchemaValidator
    {
        private string _basePath;

        public SchemaValidator()
        {
            _basePath = System.Web.Hosting.HostingEnvironment.MapPath("JsonSchemas");
        }

        public bool ValidateBySchema(string json, string eventName, int version)
        {
            try
            {
                JSchema schema;
                using (StreamReader file = File.OpenText(Path.Combine(_basePath, $"{eventName}.{version}.json")))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    schema = JSchema.Load(reader);
                }

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
