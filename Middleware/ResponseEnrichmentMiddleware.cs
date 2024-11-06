using System.Reflection;
using System.Text.Json;
using Enricher.Structure;

namespace Enricher.Middleware;

public class ResponseEnrichmentMiddleware(RequestDelegate _next)
{
    readonly JsonSerializerOptions _serializerOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task InvokeAsync(HttpContext context)
    {
        // Save the original response body stream
        var originalBodyStream = context.Response.Body;
        string responseText;
        Type responseType;

        // Use a MemoryStream to capture the response
        using (var responseBody = new MemoryStream())
        {
            context.Response.Body = responseBody;

            // Proceed with the next middleware
            await _next(context);

            // Check if response needs enrichment
            if (context.Response.StatusCode == StatusCodes.Status200OK)
            {
                var endpoint = context.GetEndpoint();
                var enrichableAttribute = endpoint?.Metadata.GetMetadata<ResponseTypeAttribute>();

                if (enrichableAttribute?.ResponseType == null)
                {
                    context.Response.Body = originalBodyStream;
                    return;
                }

                responseType = enrichableAttribute.ResponseType;
            }
            else 
            {
                context.Response.Body = originalBodyStream;
                return;
            }

            // Reset the stream position for reading
            responseBody.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(responseBody);
            responseText = await reader.ReadToEndAsync();
        }

        // Deserialize the response object
        var responseObject = JsonSerializer.Deserialize(responseText, responseType, _serializerOptions);

        context.Response.Body = originalBodyStream;

        if (responseObject == null)
        {
            return;
        }

        // Enrich the response
        EnrichProperties(responseObject, responseType);

        // Serialize back to JSON
        var enrichedResponse = JsonSerializer.Serialize(responseObject);
        await context.Response.WriteAsync(enrichedResponse);
    }

    private void EnrichProperties(object responseObject, Type type)
    {
        // iterate through all properties marked with the EnrichableAttribute
        foreach (var enrichableProperty in type.GetProperties())
        {
            var enrichableAttribute = enrichableProperty.GetCustomAttribute<EnrichableAttribute>();
            if (enrichableAttribute == null)
            {
                continue;
            }

            // Get the property value
            var sourceProperty = type.GetProperty(enrichableAttribute.SourceProperty);
            var sourceValue = sourceProperty?.GetValue(responseObject);

            // Convert the source value to the target type
            var targetType = enrichableProperty.PropertyType;
            var convertedValue = Conversion.ConvertToType(sourceValue, targetType);

            // Set the property value
            enrichableProperty.SetValue(responseObject, convertedValue);
        }
    }
}