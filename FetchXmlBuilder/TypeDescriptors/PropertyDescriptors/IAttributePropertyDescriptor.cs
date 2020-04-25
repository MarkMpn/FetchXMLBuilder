using Microsoft.Xrm.Sdk.Metadata;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors.PropertyDescriptors
{
    /// <summary>
    /// A property descriptor that is specific to an attribute
    /// </summary>
    interface IAttributePropertyDescriptor
    {
        AttributeMetadata AttributeMetadata { get; }
    }
}
