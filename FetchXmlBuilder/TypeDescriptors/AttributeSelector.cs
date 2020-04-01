using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors
{
    class AttributeSelector : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var descriptor = (AttributePropertyDescriptor)context.PropertyDescriptor;
            var svc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

            var listBox = new ListBox();
            listBox.BorderStyle = BorderStyle.None;

            foreach (var attr in descriptor.AttributeMetadata.OrderBy(a => a.LogicalName))
                listBox.Items.Add(attr.LogicalName);

            listBox.SelectedItem = value;
            listBox.DoubleClick += (s, e) => svc.CloseDropDown();
            listBox.KeyPress += (s, e) => { if (e.KeyChar == '\r') svc.CloseDropDown(); };

            svc.DropDownControl(listBox);

            return listBox.SelectedItem;
        }
    }
}
