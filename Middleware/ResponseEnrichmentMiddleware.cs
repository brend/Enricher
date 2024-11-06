using System.Reflection;
using System.Text.Json;
using Enricher.Models;
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

        // Use a MemoryStream to capture the response
        using (var responseBody = new MemoryStream())
        {
            context.Response.Body = responseBody;

            // Proceed with the next middleware
            await _next(context);

            // Reset the stream position for reading
            responseBody.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(responseBody);
            responseText = await reader.ReadToEndAsync();
        }

        // Deserialize the response object
        var responseObject = JsonSerializer.Deserialize<object?>(responseText, _serializerOptions);

        context.Response.Body = originalBodyStream;

        if (responseObject == null)
        {
            return;
        }

        // Enrich the response
        EnrichProperties(responseObject);

        // Serialize back to JSON
        var enrichedResponse = JsonSerializer.Serialize(responseObject);
        await context.Response.WriteAsync(enrichedResponse);
    }

    private void EnrichProperties(object responseObject)
    {
        foreach (var prop in responseObject.GetType().GetProperties())
        {
            if (prop.Name.StartsWith("ID_Text"))
            {
                var relatedProperty = responseObject.GetType().GetProperty(prop.Name.Substring("ID_Text_".Length));
                if (relatedProperty == null)
                {
                    continue;
                }
                // Set enriched property based on related property value
                var value = relatedProperty.GetValue(responseObject);
                prop.SetValue(responseObject, value?.ToString());
            }
        }
    }
}