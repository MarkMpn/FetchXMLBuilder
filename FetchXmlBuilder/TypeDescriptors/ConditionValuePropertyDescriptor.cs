﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Cinteros.Xrm.FetchXmlBuilder.AppCode;
using Cinteros.Xrm.FetchXmlBuilder.DockControls;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors
{
    class ConditionValuePropertyDescriptor<T> : CustomPropertyDescriptor<T>, IAttributePropertyDescriptor, ILookupPropertyDescriptor
    {
        private TreeNode _node;
        private FetchXmlBuilder _fxb;

        public ConditionValuePropertyDescriptor(string name, string category, string description, Attribute[] attrs, object owner, T defaultValue, Dictionary<string,string> dictionary, string key, TreeBuilderControl tree, AttributeMetadata attribute, TreeNode node, FetchXmlBuilder fxb) :
            base(name, category, description, CreateAttributes(attrs, attribute), owner, defaultValue, dictionary, key, tree)
        {
            AttributeMetadata = attribute;
            _node = node;
            _fxb = fxb;
        }

        static Attribute[] CreateAttributes(Attribute[] attributes, AttributeMetadata attribute)
        {
            var attrs = new List<Attribute>(attributes);

            if (typeof(T).IsArray)
            {
                attrs.Add(new TypeConverterAttribute(typeof(ArrayValueConverter)));

                if (attribute is LookupAttributeMetadata || attribute is UniqueIdentifierAttributeMetadata)
                    attrs.Add(new EditorAttribute(typeof(LookupEditor), typeof(UITypeEditor)));
            }
            else if (attribute is EnumAttributeMetadata)
                attrs.Add(new TypeConverterAttribute(typeof(OptionSetValueConverter)));
            else if (attribute is LookupAttributeMetadata || attribute is UniqueIdentifierAttributeMetadata)
            {
                attrs.Add(new EditorAttribute(typeof(LookupEditor), typeof(UITypeEditor)));
                attrs.Add(new TypeConverterAttribute(typeof(LookupConverter)));
            }

            return attrs.ToArray();
        }

        public AttributeMetadata AttributeMetadata { get; }

        public string[] Targets
        {
            get
            {
                if (AttributeMetadata is LookupAttributeMetadata lookup)
                    return lookup.Targets;

                if (AttributeMetadata is UniqueIdentifierAttributeMetadata guid)
                    return new[] { guid.EntityLogicalName };

                return Array.Empty<string>();
            }
        }

        public override object GetValue(object component)
        {
            if (typeof(T).IsArray)
            {
                var elementType = typeof(T).GetElementType();
                var value = _node.Nodes
                    .Cast<TreeNode>()
                    .Select(n => (Dictionary<string,string>) n.Tag)
                    .Select(dic => dic["#text"])
                    .Select(val => ConvertValue(elementType, val))
                    .ToArray();
                return value;
            }

            return base.GetValue(component);
        }

        public override void SetValue(object component, object value)
        {
            if (value != null && typeof(T).IsArray)
            {
                _node.Nodes.Clear();

                // Create the child <value> nodes
                foreach (var val in (Array) value)
                {
                    var valStr = (string) ConvertValue(typeof(string), val);
                    var attrNode = TreeNodeHelper.AddChildNode(_node, "value");
                    var coll = new Dictionary<string, string>();
                    coll.Add("#text", valStr);
                    attrNode.Tag = coll;
                    TreeNodeHelper.SetNodeText(attrNode, _fxb);
                }

                // Remove the value attribute
                base.SetValue(component, null);
            }
            else
            {
                base.SetValue(component, value);
            }
        }

        public override bool ShouldSerializeValue(object component)
        {
            if (typeof(T).IsArray)
                return true;

            return base.ShouldSerializeValue(component);
        }
    }

    class OptionSetValueConverter : TypeConverter
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
            var attribute = (EnumAttributeMetadata)((IAttributePropertyDescriptor)context.PropertyDescriptor).AttributeMetadata;

            return new StandardValuesCollection(attribute.OptionSet.Options.Select(option => option.Value).ToArray());
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string str)
            {
                if (Int32.TryParse(str, out var val))
                    return val;

                var regex = new Regex("\\[(\\d+)\\]$");
                var match = regex.Match(str);

                if (match.Success)
                    return Int32.Parse(match.Groups[1].Value);

                var attribute = (EnumAttributeMetadata)((IAttributePropertyDescriptor)context.PropertyDescriptor).AttributeMetadata;
                var matchingOptions = attribute.OptionSet.Options.Where(option => option.Label.UserLocalizedLabel.Label == str).ToList();

                if (matchingOptions.Count == 1)
                    return matchingOptions[0].Value;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (context != null && destinationType == typeof(string) && value is int i)
            {
                var attribute = (EnumAttributeMetadata)((IAttributePropertyDescriptor)context.PropertyDescriptor).AttributeMetadata;

                var option = attribute.OptionSet.Options.SingleOrDefault(o => o.Value == i);

                if (option != null)
                    return $"{option.Label.UserLocalizedLabel.Label} [{option.Value}]";
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    class ArrayValueConverter : TypeConverter
    {
        public ArrayValueConverter()
        {

        }

        public override object CreateInstance(ITypeDescriptorContext context, System.Collections.IDictionary propertyValues)
        {
            return base.CreateInstance(context, propertyValues);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
                return ((string)value).Split(',')
                    .Select(str => str.Trim())
                    .Select(val => ((ITypeConvertingPropertyDescriptor)context.PropertyDescriptor).ConvertValue(context.PropertyDescriptor.PropertyType.GetElementType(), val))
                    .ToArray();

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
                return String.Join(", ", ((System.Collections.IEnumerable)value).Cast<object>());

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    class Lookup
    {
        [TypeConverter(typeof(LookupConverter))]
        [Editor(typeof(LookupEditor), typeof(UITypeEditor))]
        public EntityReference EntityReference { get; set; }

        [Browsable(false)]
        public string[] Targets { get; set; }

        public override string ToString()
        {
            return EntityReference?.Id.ToString() ?? "<null>";
        }
    }

    class LookupConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override bool IsValid(ITypeDescriptorContext context, object value)
        {
            if (value is string str)
                return Guid.TryParse(str, out _);

            return base.IsValid(context, value);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string str)
            {
                var entRef = new EntityReference("account", Guid.Parse(str));

                if (context.PropertyDescriptor.PropertyType == typeof(EntityReference))
                    return entRef;

                return new Lookup { EntityReference = entRef };
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                if (!(value is EntityReference entRef))
                    entRef = ((Lookup)value).EntityReference;

                return entRef.Id.ToString();
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    class LookupEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var svc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            using (var form = new Form())
            {
                string[] targets = null;

                if (context.PropertyDescriptor is ILookupPropertyDescriptor prop)
                    targets = prop.Targets;
                else if (context.Instance is Lookup lookup)
                    targets = lookup.Targets;

                if (targets != null)
                    form.Text = String.Join(", ", targets);

                svc.ShowDialog(form);
            }

            var entRef = new EntityReference("account", Guid.NewGuid());

            if (context.PropertyDescriptor.PropertyType == typeof(EntityReference))
                return entRef;

            return new Lookup { EntityReference = entRef };
        }
    }

    class LookupArrayEditor : ArrayEditor
    {
        public LookupArrayEditor() : base(typeof(Lookup))
        {
        }

        protected override object CreateInstance(Type itemType)
        {
            var lookup = new Lookup();
            lookup.Targets = ((ILookupPropertyDescriptor)this.Context.PropertyDescriptor).Targets;
            return lookup;
        }

        protected override object SetItems(object editValue, object[] value)
        {
            foreach (Lookup lookup in value)
                lookup.Targets = ((ILookupPropertyDescriptor)this.Context.PropertyDescriptor).Targets;

            return base.SetItems(editValue, value);
        }
    }
}
