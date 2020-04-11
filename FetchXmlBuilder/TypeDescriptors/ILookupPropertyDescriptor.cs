using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors
{
    interface ILookupPropertyDescriptor
    {
        string[] Targets { get; }
    }
}
