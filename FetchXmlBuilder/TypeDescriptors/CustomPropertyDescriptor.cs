using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cinteros.Xrm.FetchXmlBuilder.DockControls;
using Microsoft.Xrm.Sdk;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors
{
    class CustomPropertyDescriptor<T> : PropertyDescriptor, IValidatingPropertyDescriptor, ITypeConvertingPropertyDescriptor
    {
        private object _owner;
        private T _defaultValue;
        private Dictionary<string, string> _dictionary;
        private string _key;
        private TreeBuilderControl _tree;

        public CustomPropertyDescriptor(string name, string category, int categoryOrder, int categoryCount, string description, Attribute[] attrs, object owner, T defaultValue, Dictionary<string,string> dictionary, string key, TreeBuilderControl tree) : 
            base(name, CreateAttributes(attrs, category, categoryOrder, categoryCount, description))
        {
            _owner = owner;
            _defaultValue = defaultValue;
            _dictionary = dictionary;
            _key = key;
            _tree = tree;
        }

        static Attribute[] CreateAttributes(Attribute[] attributes, string category, int categoryOrder, int categoryCount, string description)
        {
            var attrs = new List<Attribute>(attributes);

            if (!String.IsNullOrEmpty(category))
                attrs.Add(new CustomSortedCategoryAttribute(category, (ushort) categoryOrder, (ushort) categoryCount));

            if (!String.IsNullOrEmpty(description))
                attrs.Add(new DescriptionAttribute(description));

            return attrs.ToArray();
        }

        public override Type ComponentType => _owner.GetType();

        public override bool IsReadOnly
        {
            get
            {
                var attr = Attributes.OfType<ReadOnlyAttribute>().SingleOrDefault();

                if (attr == null)
                    return false;

                return attr.IsReadOnly;
            }
        }

        public override Type PropertyType => typeof(T);

        public override bool CanResetValue(object component)
        {
            return true;
        }

        public override object GetValue(object component)
        {
            if (!_dictionary.TryGetValue(_key, out var str))
                return _defaultValue;

            var targetType = typeof(T);

            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                targetType = targetType.GetGenericArguments()[0];

            try
            {
                return ConvertValue(targetType, str);
            }
            catch
            {
                return _defaultValue;
            }
        }

        public virtual object ConvertValue(Type targetType, object value)
        {
            if (value == null)
                return null;

            if (value.GetType() == targetType)
                return value;

            if (targetType == typeof(string))
            {
                if (value is bool b)
                    return b ? "true" : "false";

                if (value is DateTime dt)
                    return dt.ToString("yyyy-MM-dd HH:mm:ss");

                if (value is Lookup lookup)
                    value = lookup?.EntityReference.Id;

                if (value is PicklistValue picklist)
                    value = picklist?.OptionSetValue.Value;

                return value.ToString();
            }

            if (value is string str)
            {
                if (targetType.IsEnum)
                    return Enum.Parse(targetType, str);

                if (targetType == typeof(Lookup))
                {
                    _dictionary.TryGetValue("uitype", out var uitype);
                    _dictionary.TryGetValue("uiname", out var uiname);
                    return new Lookup { EntityReference = new EntityReference(uitype, Guid.Parse(str)) { Name = uiname } };
                }

                if (targetType == typeof(PicklistValue))
                    return new PicklistValue { OptionSetValue = new OptionSetValue(Int32.Parse(str)) };
            }

            return Convert.ChangeType(value, targetType);
        }

        public override void ResetValue(object component)
        {
            _dictionary.Remove(_key);
            _tree.CtrlSaved(this, new XmlEditorUtils.SaveEventArgs { AttributeCollection = _dictionary });
        }

        public override void SetValue(object component, object value)
        {
            if (value == null || value.Equals(_defaultValue))
            {
                _dictionary.Remove(_key);
            }
            else
            {
                var str = (string)ConvertValue(typeof(string), value);
                _dictionary[_key] = str;

                if (value is Lookup lookup)
                {
                    if (!String.IsNullOrEmpty(lookup.EntityReference.LogicalName))
                        _dictionary["uitype"] = lookup.EntityReference.LogicalName;
                    else
                        _dictionary.Remove("uitype");

                    if (!String.IsNullOrEmpty(lookup.EntityReference.Name))
                        _dictionary["uiname"] = lookup.EntityReference.Name;
                    else
                        _dictionary.Remove("uiname");
                }
            }

            _tree.CtrlSaved(this, new XmlEditorUtils.SaveEventArgs { AttributeCollection = _dictionary });
        }

        public override bool ShouldSerializeValue(object component)
        {
            return _dictionary.ContainsKey(_key);
        }

        public virtual string GetValidationError(ITypeDescriptorContext context)
        {
            return null;
        }
    }
}
