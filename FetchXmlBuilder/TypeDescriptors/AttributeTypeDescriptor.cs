using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Cinteros.Xrm.FetchXmlBuilder.DockControls;
using Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors.PropertyDescriptors;
using Microsoft.Xrm.Sdk.Metadata;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors
{
    /// <summary>
    /// Provides a type descriptor for the &lt;attribute&gt; element
    /// </summary>
    class AttributeTypeDescriptor : BaseTypeDescriptor
    {
        private readonly AttributeMetadata[] _attributes;

        public AttributeTypeDescriptor(TreeNode node, FetchXmlBuilder fxb, TreeBuilderControl tree, AttributeMetadata[] attributes)
            : base(node, fxb, tree)
        {
            _attributes = attributes;
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            var aggregate = Tree.GetFetchType().aggregateSpecified && Tree.GetFetchType().aggregate;

            var dictionary = (Dictionary<string, string>)Node.Tag;
            
            var nameProp = new AttributePropertyDescriptor(
                "(Name)",
                "Attribute",
                1,
                3,
                "The logical name of the attribute to select",
                new Attribute[]
                {
                    new RefreshPropertiesAttribute(RefreshProperties.All)
                },
                this,
                String.Empty,
                dictionary,
                "name",
                Tree,
                _attributes);

            var attributeName = (string) nameProp.GetValue(this);
            var attr = _attributes?.SingleOrDefault(a => a.LogicalName == attributeName);

            var aliasProp = new CustomPropertyDescriptor<string>(
                "Alias",
                "Attribute",
                1,
                3,
                "A different name to use for the attribute in the result set",
                Array.Empty<Attribute>(),
                this,
                String.Empty,
                dictionary,
                "alias",
                Tree);

            var groupByProp = new CustomPropertyDescriptor<bool>(
                "Group By",
                "Aggregate",
                2,
                3,
                "Indicates if the results should be grouped by the value of this attribute",
                new Attribute[]
                {
                    new ReadOnlyAttribute(!aggregate)
                },
                this,
                false,
                dictionary,
                "groupby",
                Tree);

            var groupBy = (bool?)groupByProp.GetValue(this);

            var aggregateProp = new CustomPropertyDescriptor<AggregateType?>(
                "Aggregate",
                "Aggregate",
                2,
                3,
                "The aggregate function to apply to this attribute",
                new Attribute[]
                {
                    new ReadOnlyAttribute(!aggregate || groupBy == true)
                },
                this,
                null,
                dictionary,
                "aggregate",
                Tree);

            var aggregateType = (AggregateType?)aggregateProp.GetValue(this);

            var distinctProp = new CustomPropertyDescriptor<bool>(
                "Distinct",
                "Aggregate",
                2,
                3,
                "Indicates if only distinct values in this attribute should be used when calculating the aggregate",
                new Attribute[]
                {
                    new ReadOnlyAttribute(!aggregate || aggregateType == null)
                },
                this,
                false,
                dictionary,
                "distinct",
                Tree);

            var userTimeZoneProp = new CustomPropertyDescriptor<bool>
                (
                "User Time Zone",
                "Time Zone",
                3,
                3,
                "Indicates if the date/time values in this attribute should be retrieved in the user's time zone rather than UTC",
                new Attribute[]
                {
                    new ReadOnlyAttribute(attr != null && !(attr is DateTimeAttributeMetadata))
                },
                this,
                false,
                dictionary,
                "usertimezone",
                Tree);

            var dateGroupingProp = new CustomPropertyDescriptor<DateGroupingType?>(
                "Date Grouping",
                "Aggregate",
                2,
                3,
                "Indicates the granularity of the date grouping",
                new Attribute[]
                {
                    new ReadOnlyAttribute(!aggregate || groupBy != true || (attr != null && !(attr is DateTimeAttributeMetadata)))
                },
                this,
                null,
                dictionary,
                "dategrouping",
                Tree);

            return new PropertyDescriptorCollection(new PropertyDescriptor[] { nameProp, aliasProp, aggregateProp, groupByProp, distinctProp, userTimeZoneProp, dateGroupingProp });
        }
    }
}
