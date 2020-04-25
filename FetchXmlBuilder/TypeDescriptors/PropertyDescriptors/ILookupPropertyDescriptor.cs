using Microsoft.Xrm.Sdk;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors.PropertyDescriptors
{
    /// <summary>
    /// A property descriptor that is related to a lookup value
    /// </summary>
    interface ILookupPropertyDescriptor
    {
        string[] Targets { get; }

        IOrganizationService Service { get; }
    }
}
