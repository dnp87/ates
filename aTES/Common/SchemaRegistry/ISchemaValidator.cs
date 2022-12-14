using Common.Events;

namespace Common.SchemaRegistry
{
    public interface ISchemaValidator
    {
        bool ValidateBySchema(string json, string eventName, int version, out System.Collections.Generic.IList<string> errors);
    }
}
