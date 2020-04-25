using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Cinteros.Xrm.FetchXmlBuilder.DockControls;
using Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors.PropertyDescriptors;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors
{
    /// <summary>
    /// Provides a type descriptor for the &lt;entity&gt; element
    /// </summary>
    class EntityTypeDescriptor : BaseTypeDescriptor
    {
        public EntityTypeDescriptor(TreeNode node, FetchXmlBuilder fxb, TreeBuilderControl tree)
            : base(node, fxb, tree)
        {
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            var dictionary = (Dictionary<string, string>)Node.Tag;

            var entities = FXB.GetDisplayEntities();
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
                Tree, 
                entities.Keys.ToArray());

            return new PropertyDescriptorCollection(new[] { nameProp });
        }
    }
}
