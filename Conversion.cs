namespace Enricher;

public static class Conversion
{
    public static object? ConvertToType(object? value, Type targetType)
    {
        try
        {
            if (value == null)
            {
                // Return null if the target type is nullable
                if (Nullable.GetUnderlyingType(targetType) != null)
                {
                    return null;
                }

                throw new InvalidCastException("Cannot convert null to a non-nullable type.");
            }

            // If the value is already of the target type, return it directly
            if (targetType.IsAssignableFrom(value.GetType()))
            {
                return value;
            }

            // Handle nullable types by getting the underlying type
            Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            // Attempt the conversion
            return Convert.ChangeType(value, underlyingType);
        }
        catch (Exception ex)
        {
            // Handle or log the exception as needed
            Console.Error.WriteLine($"Conversion failed: {ex.Message}");
            return null;
        }
    }
}