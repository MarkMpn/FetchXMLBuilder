using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cinteros.Xrm.FetchXmlBuilder.AppCode;
using Cinteros.Xrm.FetchXmlBuilder.DockControls;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors
{
    class EntityTypeDescriptor : CustomTypeDescriptor
    {
        private TreeNode _node;
        private FetchXmlBuilder _fxb;
        private TreeBuilderControl _tree;

        public EntityTypeDescriptor(TreeNode node, FetchXmlBuilder fxb, TreeBuilderControl tree)
        {
            _node = node;
            _fxb = fxb;
            _tree = tree;
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            var dictionary = (Dictionary<string, string>)_node.Tag;

            var entities = _fxb.GetDisplayEntities();
            var nameProp = new EntityPropertyDescriptor(
                "Name", 
                "Entity",
                1,
                1,
                "The logical name of the entity to query", 
                Array.Empty<Attribute>(), 
                this, 
                null, 
                dictionary, 
                "name", 
                _tree, 
                entities.Keys.ToArray());

            return new PropertyDescriptorCollection(new[] { nameProp });
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
