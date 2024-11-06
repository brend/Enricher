namespace Enricher.Middleware;

[AttributeUsage(AttributeTargets.Method)]
class ResponseTypeAttribute : Attribute
{
    public Type ResponseType { get; }

    public ResponseTypeAttribute(Type responseType)
    {
        ResponseType = responseType;
    }
}