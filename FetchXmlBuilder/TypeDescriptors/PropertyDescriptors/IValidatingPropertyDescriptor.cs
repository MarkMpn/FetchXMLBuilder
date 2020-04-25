using System.ComponentModel;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors.PropertyDescriptors
{
    /// <summary>
    /// A property descriptor that can provide validation feedback
    /// </summary>
    interface IValidatingPropertyDescriptor
    {
        string GetValidationError(ITypeDescriptorContext context);
    }
}
