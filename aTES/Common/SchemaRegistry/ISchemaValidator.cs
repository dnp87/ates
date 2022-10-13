using Common.Events;

namespace Common.SchemaRegistry
{
    public interface ISchemaValidator
    {
        bool ValidateBySchema<T>(string json) where T : EventBase;
    }
}
