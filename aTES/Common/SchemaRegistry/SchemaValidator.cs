using Common.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.IO;
using System.Reflection;
using System.Web;

namespace Common.SchemaRegistry
{
    public class SchemaValidator : ISchemaValidator
    {
        private string _basePath;

        public SchemaValidator()
        {
            if (HttpContext.Current != null)
            {
                _basePath = System.Web.Hosting.HostingEnvironment.MapPath("/bin/JsonSchemas");
            }
            else
            {
                _basePath = Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location), "JsonSchemas");
            }
        }

        public bool ValidateBySchema(string json, string eventName, int version, out System.Collections.Generic.IList<string> errors)
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
                return parsedJObject.IsValid(schema, out errors);
            }
            catch
            {
                // todo: log
                errors = null;
                return false;
            }
        }
    }
}
