using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors.PropertyDescriptors;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors
{
    /// <summary>
    /// Provides a site for the property grid to get additional services
    /// </summary>
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
            // Return an IPropertyValueUIService to add validation feedback
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
            // Allow each property descriptor to implement it's own validation logic
            if (propDesc is IValidatingPropertyDescriptor validating)
            {
                var msg = validating.GetValidationError(context);

                if (msg == null)
                    return;

                var image = new Bitmap(8, 8);

                using (var g = Graphics.FromImage(image))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(SystemIcons.Warning.ToBitmap(), new Rectangle(Point.Empty, image.Size));
                }

                valueUIItemList.Add(new PropertyValueUIItem(image, NoOp, msg));
            }
        }

        private void NoOp(ITypeDescriptorContext context, PropertyDescriptor descriptor, PropertyValueUIItem invokedItem)
        {
        }
    }
}
