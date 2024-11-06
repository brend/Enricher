namespace Enricher.Structure;

[AttributeUsage(AttributeTargets.Property)]
public class EnrichableAttribute : Attribute
{
    public EnrichableAttribute(string relatedProperty)
    {
        RelatedProperty = relatedProperty;
    }

    public string RelatedProperty { get; }
}