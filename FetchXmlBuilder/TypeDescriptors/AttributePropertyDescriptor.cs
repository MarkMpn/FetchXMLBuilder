using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cinteros.Xrm.FetchXmlBuilder.DockControls;
using Microsoft.Xrm.Sdk.Metadata;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors
{
    class AttributePropertyDescriptor : CustomPropertyDescriptor<string>
    {
        public AttributePropertyDescriptor(string name, string category, string description, Attribute[] attrs, object owner, string defaultValue, Dictionary<string,string> dictionary, string key, TreeBuilderControl tree, AttributeMetadata[] attributes) :
            base(name, category, description, CreateAttributes(attrs), owner, defaultValue, dictionary, key, tree)
        {
            AttributeMetadata = attributes;
        }

        static Attribute[] CreateAttributes(Attribute[] attributes)
        {
            var attrs = new List<Attribute>(attributes);
            attrs.Add(new TypeConverterAttribute(typeof(AttributeConverter)));
            return attrs.ToArray();
        }

        public AttributeMetadata[] AttributeMetadata { get; }

        public override string GetValidationError(ITypeDescriptorContext context)
        {
            if (Attributes != null && !AttributeMetadata.Any(a => a.LogicalName == (string)GetValue(context.Instance)))
                return "Unknown attribute";

            return base.GetValidationError(context);
        }

        class AttributeConverter : TypeConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return false;
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                var descriptor = (AttributePropertyDescriptor)context.PropertyDescriptor;

                return new StandardValuesCollection(descriptor.AttributeMetadata.OrderBy(a => a.LogicalName).Select(a => a.LogicalName).ToArray());
            }
        }
    }
}
