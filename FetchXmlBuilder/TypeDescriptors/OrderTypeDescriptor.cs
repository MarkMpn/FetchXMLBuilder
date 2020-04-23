using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Cinteros.Xrm.FetchXmlBuilder.AppCode;
using Cinteros.Xrm.FetchXmlBuilder.DockControls;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors
{
    class OrderTypeDescriptor : CustomTypeDescriptor
    {
        private TreeNode _node;
        private FetchXmlBuilder _fxb;
        private TreeBuilderControl _tree;
        private AttributeMetadata[] _attributes;

        public OrderTypeDescriptor(TreeNode node, FetchXmlBuilder fxb, TreeBuilderControl tree, AttributeMetadata[] attributes)
        {
            _node = node;
            _fxb = fxb;
            _tree = tree;
            _attributes = attributes;
        }

        public TreeNode Node { get => _node; }

        public FetchXmlBuilder FXB { get => _fxb; }

        public override PropertyDescriptorCollection GetProperties()
        {
            var dictionary = (Dictionary<string, string>)_node.Tag;
            var aggregate = _tree.GetFetchType().aggregateSpecified && _tree.GetFetchType().aggregate;

            /*
             * 
        var aggregate = TreeBuilderControl.IsFetchAggregate(Node);
        if (!aggregate)
        {
            cmbAttribute.Items.Clear();
            if (attributes != null)
            {
                foreach (var attribute in attributes)
                {
                    AttributeItem.AddAttributeToComboBox(cmbAttribute, attribute, false, friendly);
                }
            }
        }
        else
        {
            cmbAlias.Items.Clear();
            cmbAlias.Items.Add("");
            cmbAlias.Items.AddRange(GetAliases(Tree.tvFetch.Nodes[0]).ToArray());
        }
        cmbAttribute.Enabled = !aggregate;
        cmbAlias.Enabled = aggregate;
        */

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
                _tree,
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
                _tree);

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
                _tree);

            return new PropertyDescriptorCollection(new PropertyDescriptor[] { nameProp, aliasProp, descProp });
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return GetProperties();
        }

        public override object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
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
                var aliases = GetAliases(order._tree.tvFetch.Nodes[0]);

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
