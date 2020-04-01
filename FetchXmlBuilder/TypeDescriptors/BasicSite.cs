using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors
{
    class BasicSite : ISite
    {
        public BasicSite()
        {
        }

        public IComponent Component => null;

        public IContainer Container => null;

        public bool DesignMode => false;

        public string Name { get; set; }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IPropertyValueUIService))
            {
                var service = new PropertyValueUIService();
                service.AddPropertyValueUIHandler(ValidateProperty);
                return service;
            }

            return null;
        }

        private void ValidateProperty(ITypeDescriptorContext context, PropertyDescriptor propDesc, ArrayList valueUIItemList)
        {
            if (propDesc is AttributePropertyDescriptor attr && !attr.Attributes.Any(a => a.LogicalName == (string)attr.GetValue(context.Instance)))
            {
                var image = new Bitmap(8, 8);

                using (var g = Graphics.FromImage(image))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(SystemIcons.Warning.ToBitmap(), new Rectangle(Point.Empty, image.Size));
                }

                valueUIItemList.Add(new PropertyValueUIItem(image, NoOp, "Unknown attribute"));
            }
        }

        private void NoOp(ITypeDescriptorContext context, PropertyDescriptor descriptor, PropertyValueUIItem invokedItem)
        {
        }
    }
}
