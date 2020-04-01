using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cinteros.Xrm.FetchXmlBuilder.DockControls;
using Microsoft.Xrm.Sdk.Metadata;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors
{
    class AttributeTypeDescriptor : CustomTypeDescriptor
    {
        private TreeNode _node;
        private AttributeMetadata[] _attributes;
        private TreeBuilderControl _tree;

        public AttributeTypeDescriptor(TreeNode node, AttributeMetadata[] attributes, TreeBuilderControl tree)
        {
            _node = node;
            _attributes = attributes;
            _tree = tree;
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            var aggregate = _tree.GetFetchType().aggregateSpecified && _tree.GetFetchType().aggregate;

            var dictionary = (Dictionary<string, string>)_node.Tag;
            
            var nameProp = new AttributePropertyDescriptor(
                "(Name)",
                "(Attribute)",
                "The logical name of the attribute to select",
                new Attribute[]
                {
                    new RefreshPropertiesAttribute(RefreshProperties.All)
                },
                this,
                String.Empty,
                dictionary,
                "name",
                _tree,
                _attributes);

            var attributeName = (string) nameProp.GetValue(this);
            var attr = _attributes?.SingleOrDefault(a => a.LogicalName == attributeName);

            var aliasProp = new CustomPropertyDescriptor<string>(
                "Alias",
                "(Attribute)",
                "A different name to use for the attribute in the result set",
                Array.Empty<Attribute>(),
                this,
                String.Empty,
                dictionary,
                "alias",
                _tree);

            var groupByProp = new CustomPropertyDescriptor<bool>(
                "Group By",
                "Aggregate",
                "Indicates if the results should be grouped by the value of this attribute",
                new Attribute[]
                {
                    new ReadOnlyAttribute(!aggregate)
                },
                this,
                false,
                dictionary,
                "groupby",
                _tree);

            var groupBy = (bool?)groupByProp.GetValue(this);

            var aggregateProp = new CustomPropertyDescriptor<AggregateType?>(
                "Aggregate",
                "Aggregate",
                "The aggregate function to apply to this attribute",
                new Attribute[]
                {
                    new ReadOnlyAttribute(!aggregate || groupBy == true)
                },
                this,
                null,
                dictionary,
                "aggregate",
                _tree);

            var aggregateType = (AggregateType?)aggregateProp.GetValue(this);

            var distinctProp = new CustomPropertyDescriptor<bool>(
                "Distinct",
                "Aggregate",
                "Indicates if only distinct values in this attribute should be used when calculating the aggregate",
                new Attribute[]
                {
                    new ReadOnlyAttribute(!aggregate || aggregateType == null)
                },
                this,
                false,
                dictionary,
                "distinct",
                _tree);

            var userTimeZoneProp = new CustomPropertyDescriptor<bool>
                (
                "User Time Zone",
                "Time Zone",
                "Indicates if the date/time values in this attribute should be retrieved in the user's time zone rather than UTC",
                new Attribute[]
                {
                    new ReadOnlyAttribute(attr != null && !(attr is DateTimeAttributeMetadata))
                },
                this,
                false,
                dictionary,
                "usertimezone",
                _tree);

            var dateGroupingProp = new CustomPropertyDescriptor<DateGroupingType?>(
                "Date Grouping",
                "Aggregate",
                "Indicates the granularity of the date grouping",
                new Attribute[]
                {
                    new ReadOnlyAttribute(!aggregate || groupBy != true || (attr != null && !(attr is DateTimeAttributeMetadata)))
                },
                this,
                null,
                dictionary,
                "dategrouping",
                _tree);

            return new PropertyDescriptorCollection(new PropertyDescriptor[] { nameProp, aliasProp, aggregateProp, groupByProp, distinctProp, userTimeZoneProp, dateGroupingProp });
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
