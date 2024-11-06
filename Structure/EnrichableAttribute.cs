namespace Enricher.Structure;

[AttributeUsage(AttributeTargets.Property)]
public class EnrichableAttribute : Attribute
{
    public EnrichableAttribute(string sourceProperty)
    {
        SourceProperty = sourceProperty;
    }

    public string SourceProperty { get; }
}