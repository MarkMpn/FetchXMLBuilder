using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Cinteros.Xrm.FetchXmlBuilder.DockControls;
using Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors.PropertyDescriptors;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors
{
    /// <summary>
    /// Provides a type descriptor for the &lt;filter&gt; element
    /// </summary>
    class FilterTypeDescriptor : BaseTypeDescriptor
    {   
        public FilterTypeDescriptor(TreeNode node, FetchXmlBuilder fxb, TreeBuilderControl tree)
            : base(node, fxb, tree)
        {
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            var dictionary = (Dictionary<string, string>)Node.Tag;

            var filterTypeProp = new CustomPropertyDescriptor<filterType>(
                "Type",
                "Filter",
                1,
                1,
                "Indicates how conditions in this filter should be combined",
                Array.Empty<Attribute>(),
                this,
                filterType.and,
                dictionary,
                "type",
                Tree);

            return new PropertyDescriptorCollection(new[] { filterTypeProp });
        }
    }
}
