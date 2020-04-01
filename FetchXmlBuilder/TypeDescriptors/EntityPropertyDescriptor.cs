using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cinteros.Xrm.FetchXmlBuilder.Controls;
using Cinteros.Xrm.FetchXmlBuilder.DockControls;
using Microsoft.Xrm.Sdk.Metadata;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors
{
    class EntityPropertyDescriptor : CustomPropertyDescriptor<string>
    {
        public EntityPropertyDescriptor(string name, string category, string description, Attribute[] attrs, object owner, string defaultValue, Dictionary<string,string> dictionary, string key, TreeBuilderControl tree, EntityNode[] entities) :
            base(name, category, description, CreateAttributes(attrs), owner, defaultValue, dictionary, key, tree)
        {
            Entities = entities;
        }

        static Attribute[] CreateAttributes(Attribute[] attributes)
        {
            var attrs = new List<Attribute>(attributes);
            attrs.Add(new EditorAttribute(typeof(EntitySelector), typeof(UITypeEditor)));
            return attrs.ToArray();
        }

        public EntityNode[] Entities { get; }

        public override string GetValidationError(ITypeDescriptorContext context)
        {
            if (Entities != null && !Entities.Any(e => e.ToString() == (string)GetValue(context.Instance)))
                return "Unknown entity";

            return base.GetValidationError(context);
        }
    }
}
