using System;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors.PropertyDescriptors
{
    /// <summary>
    /// A property descriptor that can convert values to a different type
    /// </summary>
    interface ITypeConvertingPropertyDescriptor
    {
        object ConvertValue(Type targetType, object value);
    }
}
