using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Metadata;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors
{
    interface IAttributePropertyDescriptor
    {
        AttributeMetadata AttributeMetadata { get; }
    }
}
