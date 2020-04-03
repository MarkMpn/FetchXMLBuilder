using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cinteros.Xrm.FetchXmlBuilder.DockControls;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors
{
    class CustomPropertyDescriptor<T> : PropertyDescriptor, IValidatingPropertyDescriptor
    {
        private object _owner;
        private T _defaultValue;
        private Dictionary<string, string> _dictionary;
        private string _key;
        private TreeBuilderControl _tree;

        public CustomPropertyDescriptor(string name, string category, string description, Attribute[] attrs, object owner, T defaultValue, Dictionary<string,string> dictionary, string key, TreeBuilderControl tree) : 
            base(name, CreateAttributes(attrs, category, description))
        {
            _owner = owner;
            _defaultValue = defaultValue;
            _dictionary = dictionary;
            _key = key;
            _tree = tree;
        }

        static Attribute[] CreateAttributes(Attribute[] attributes, string category, string description)
        {
            var attrs = new List<Attribute>(attributes);

            if (!String.IsNullOrEmpty(category))
                attrs.Add(new CategoryAttribute(category));

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

        protected virtual object ConvertValue(Type targetType, object value)
        {
            if (value == null)
                return null;

            if (value.GetType() == targetType)
                return value;

            if (targetType == typeof(string))
            {
                if (value is bool b)
                    return b ? "true" : "false";

                return value.ToString();
            }

            if (value is string str && targetType.IsEnum)
                return Enum.Parse(targetType, str);

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
