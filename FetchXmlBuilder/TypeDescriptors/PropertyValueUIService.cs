using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors
{
    /// <summary>
    /// Simple implementation of <see cref="IPropertyValueUIService"/> to allow adding property validation callbacks
    /// </summary>
    class PropertyValueUIService : IPropertyValueUIService
    {
        private readonly List<PropertyValueUIHandler> _list;

        public PropertyValueUIService()
        {
            _list = new List<PropertyValueUIHandler>();
        }

        public event EventHandler PropertyUIValueItemsChanged;

        public void AddPropertyValueUIHandler(PropertyValueUIHandler newHandler)
        {
            _list.Add(newHandler);
        }

        public void RemovePropertyValueUIHandler(PropertyValueUIHandler newHandler)
        {
            _list.Remove(newHandler);
        }

        public PropertyValueUIItem[] GetPropertyUIValueItems(ITypeDescriptorContext context, PropertyDescriptor propDesc)
        {
            var list = new ArrayList();

            foreach (var handler in _list)
            {
                handler(context, propDesc, list);
            }

            return list.Cast<PropertyValueUIItem>().ToArray();
        }

        public void NotifyPropertyValueUIItemsChanged()
        {
            PropertyUIValueItemsChanged(this, EventArgs.Empty);
        }
    }
}
