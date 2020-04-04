using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cinteros.Xrm.FetchXmlBuilder.AppCode;
using Cinteros.Xrm.FetchXmlBuilder.DockControls;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors
{
    class ConditionValuePropertyDescriptor<T> : CustomPropertyDescriptor<T>
    {
        public ConditionValuePropertyDescriptor(string name, string category, string description, Attribute[] attrs, object owner, T defaultValue, Dictionary<string,string> dictionary, string key, TreeBuilderControl tree, AttributeMetadata attribute) :
            base(name, category, description, CreateAttributes(attrs, attribute), owner, defaultValue, dictionary, key, tree)
        {
            AttributeMetadata = attribute;
        }

        static Attribute[] CreateAttributes(Attribute[] attributes, AttributeMetadata attribute)
        {
            var attrs = new List<Attribute>(attributes);

            if (attribute is EnumAttributeMetadata)
                attrs.Add(new TypeConverterAttribute(typeof(OptionSetValueConverter)));

            return attrs.ToArray();
        }

        public AttributeMetadata AttributeMetadata { get; }

        protected override object ConvertValue(Type targetType, object value)
        {
            return base.ConvertValue(targetType, value);
        }

        private class OptionSetValueConverter : TypeConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return true;
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                var attribute = (EnumAttributeMetadata) ((ConditionOperatorPropertyDescriptor)context.PropertyDescriptor).AttributeMetadata;

                return new StandardValuesCollection(attribute.OptionSet.Options.Select(option => $"{option.Label.UserLocalizedLabel.Label} [{option.Value}]").ToArray());
            }
        }
    }
}
