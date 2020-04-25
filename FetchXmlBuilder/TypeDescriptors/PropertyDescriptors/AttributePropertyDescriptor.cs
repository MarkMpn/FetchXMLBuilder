using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Cinteros.Xrm.FetchXmlBuilder.DockControls;
using Microsoft.Xrm.Sdk.Metadata;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors.PropertyDescriptors
{
    /// <summary>
    /// A property descriptor that allows selecting an attribute from a list of metadata
    /// </summary>
    class AttributePropertyDescriptor : CustomPropertyDescriptor<string>
    {
        public AttributePropertyDescriptor(string name, string category, int categoryOrder, int categoryCount, string description, Attribute[] attrs, object owner, string defaultValue, Dictionary<string,string> dictionary, string key, TreeBuilderControl tree, AttributeMetadata[] attributes) :
            base(name, category, categoryOrder, categoryCount, description, CreateAttributes(attrs), owner, defaultValue, dictionary, key, tree)
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
            {
                return "Unknown attribute";
            }

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

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(string))
                {
                    return true;
                }

                return base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value is string)
                {
                    return value;
                }

                return base.ConvertFrom(context, culture, value);
            }
        }
    }
}
