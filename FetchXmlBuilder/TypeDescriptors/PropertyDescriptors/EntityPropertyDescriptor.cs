using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Cinteros.Xrm.FetchXmlBuilder.DockControls;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors.PropertyDescriptors
{
    /// <summary>
    /// A property descriptor to select an entity from the list of entities currently in the FetchXML tree
    /// </summary>
    class EntityPropertyDescriptor : CustomPropertyDescriptor<string>
    {
        public EntityPropertyDescriptor(string name, string category, int categoryOrder, int categoryCount, string description, Attribute[] attrs, object owner, string defaultValue, Dictionary<string,string> dictionary, string key, TreeBuilderControl tree, string[] entities) :
            base(name, category, categoryOrder, categoryCount, description, CreateAttributes(attrs), owner, defaultValue, dictionary, key, tree)
        {
            Entities = entities;
        }

        static Attribute[] CreateAttributes(Attribute[] attributes)
        {
            var attrs = new List<Attribute>(attributes);
            attrs.Add(new TypeConverterAttribute(typeof(EntityConverter)));
            return attrs.ToArray();
        }

        public string[] Entities { get; }

        public override string GetValidationError(ITypeDescriptorContext context)
        {
            var entityName = (string)GetValue(context.Instance);

            if (!String.IsNullOrEmpty(entityName) && Entities != null && !Entities.Contains(entityName))
                return "Unknown entity";

            return base.GetValidationError(context);
        }

        class EntityConverter : TypeConverter
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
                var descriptor = (EntityPropertyDescriptor)context.PropertyDescriptor;

                return new StandardValuesCollection(descriptor.Entities.OrderBy(a => a).ToArray());
            }

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(string))
                    return true;

                return base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value is string)
                    return value;

                return base.ConvertFrom(context, culture, value);
            }
        }
    }
}
