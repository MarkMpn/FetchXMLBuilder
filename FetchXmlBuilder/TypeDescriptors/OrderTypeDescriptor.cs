using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using Cinteros.Xrm.FetchXmlBuilder.AppCode;
using Cinteros.Xrm.FetchXmlBuilder.DockControls;
using Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors.PropertyDescriptors;
using Microsoft.Xrm.Sdk.Metadata;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors
{
    /// <summary>
    /// Provides a type descriptor for the &lt;order&gt; element
    /// </summary>
    class OrderTypeDescriptor : BaseTypeDescriptor
    {
        private readonly AttributeMetadata[] _attributes;

        public OrderTypeDescriptor(TreeNode node, FetchXmlBuilder fxb, TreeBuilderControl tree, AttributeMetadata[] attributes)
            : base(node, fxb, tree)
        {
            _attributes = attributes;
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            var dictionary = (Dictionary<string, string>)Node.Tag;
            var aggregate = Tree.GetFetchType().aggregateSpecified && Tree.GetFetchType().aggregate;

            var nameProp = new AttributePropertyDescriptor(
                "Atttribute",
                "Sort",
                1,
                1,
                "The logical name of the attribute to sort on",
                new Attribute[]
                {
                    new ReadOnlyAttribute(aggregate)
                },
                this,
                null,
                dictionary,
                "attribute",
                Tree,
                _attributes);

            var aliasProp = new CustomPropertyDescriptor<string>(
                "Alias",
                "Sort",
                1,
                1,
                "The name of the aliased attribute to sort on",
                new Attribute[]
                {
                    new ReadOnlyAttribute(!aggregate),
                    new TypeConverterAttribute(typeof(AliasConverter))
                },
                this,
                null,
                dictionary,
                "alias",
                Tree);

            var descProp = new CustomPropertyDescriptor<bool>(
                "Descending",
                "Sort",
                1,
                1,
                "Indicates if the sort order should be reversed",
                Array.Empty<Attribute>(),
                this,
                false,
                dictionary,
                "descending",
                Tree);

            return new PropertyDescriptorCollection(new PropertyDescriptor[] { nameProp, aliasProp, descProp });
        }

        class AliasConverter : TypeConverter
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
                var order = (OrderTypeDescriptor)context.Instance;
                var aliases = GetAliases(order.Tree.tvFetch.Nodes[0]);

                return new StandardValuesCollection(aliases);
            }

            private List<string> GetAliases(TreeNode node)
            {
                var result = new List<string>();
                if (node.Name == "entity" || node.Name == "link-entity")
                {
                    foreach (TreeNode child in node.Nodes)
                    {
                        if (child.Name == "attribute")
                        {
                            var alias = TreeNodeHelper.GetAttributeFromNode(child, "alias");
                            if (!string.IsNullOrEmpty(alias))
                            {
                                result.Add(alias);
                            }
                        }
                    }
                }
                foreach (TreeNode child in node.Nodes)
                {
                    result.AddRange(GetAliases(child));
                }
                return result;
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

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (destinationType == typeof(string))
                    return true;

                return base.CanConvertTo(context, destinationType);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string) && value is string)
                    return value;

                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}
