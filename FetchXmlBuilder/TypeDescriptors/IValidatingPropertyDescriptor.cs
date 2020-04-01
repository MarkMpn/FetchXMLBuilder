using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors
{
    interface IValidatingPropertyDescriptor
    {
        string GetValidationError(ITypeDescriptorContext context);
    }
}
