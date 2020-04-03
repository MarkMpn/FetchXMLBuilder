﻿using System;
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
    class ConditionOperatorPropertyDescriptor : CustomPropertyDescriptor<ConditionOperator>
    {
        public ConditionOperatorPropertyDescriptor(string name, string category, string description, Attribute[] attrs, object owner, ConditionOperator defaultValue, Dictionary<string,string> dictionary, string key, TreeBuilderControl tree, AttributeMetadata attribute) :
            base(name, category, description, CreateAttributes(attrs), owner, defaultValue, dictionary, key, tree)
        {
            AttributeMetadata = attribute;
        }

        static Attribute[] CreateAttributes(Attribute[] attributes)
        {
            var attrs = new List<Attribute>(attributes);
            attrs.Add(new TypeConverterAttribute(typeof(FilteredOperatorConverter)));
            return attrs.ToArray();
        }

        public AttributeMetadata AttributeMetadata { get; }

        protected override object ConvertValue(Type targetType, object value)
        {
            if (targetType == typeof(ConditionOperator) && value is string str)
            {
                var values = OperatorItem.GetConditionsByAttributeType(null);
                var match = values.SingleOrDefault(op => op.GetValue() == str);

                if (match != null)
                    return match.Operator;
            }
            else if (targetType == typeof(string) && value is ConditionOperator op)
            {
                var oper = new OperatorItem(op);
                return oper.GetValue();
            }
            else if (targetType == typeof(string) && value is string opStr)
            {
                var oper = new OperatorItem((ConditionOperator)Enum.Parse(typeof(ConditionOperator), opStr));
                return oper.GetValue();
            }

            return base.ConvertValue(targetType, value);
        }

        private class FilteredOperatorConverter : TypeConverter
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
                var attribute = ((ConditionOperatorPropertyDescriptor)context.PropertyDescriptor).AttributeMetadata;

                if (attribute == null)
                    return new StandardValuesCollection(OperatorItem.GetConditionsByAttributeType(null).Select(op => op.Operator).ToArray());

                return new StandardValuesCollection(OperatorItem.GetConditionsByAttributeType(attribute.AttributeType).Select(op => op.Operator).ToArray());
            }
        }
    }
}
