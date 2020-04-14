using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cinteros.Xrm.FetchXmlBuilder.DockControls;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors
{
    class FilterTypeDescriptor : CustomTypeDescriptor
    {
        private TreeNode _node;
        private FetchXmlBuilder _fxb;
        private TreeBuilderControl _tree;
        
        public FilterTypeDescriptor(TreeNode node, FetchXmlBuilder fxb, TreeBuilderControl tree)
        {
            _node = node;
            _fxb = fxb;
            _tree = tree;
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            var dictionary = (Dictionary<string, string>)_node.Tag;

            var filterTypeProp = new CustomPropertyDescriptor<filterType>(
                "Type",
                "Filter",
                "Indicates how conditions in this filter should be combined",
                Array.Empty<Attribute>(),
                this,
                filterType.and,
                dictionary,
                "type",
                _tree);

            return new PropertyDescriptorCollection(new[] { filterTypeProp });
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return GetProperties();
        }

        public override object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }
    }
}
