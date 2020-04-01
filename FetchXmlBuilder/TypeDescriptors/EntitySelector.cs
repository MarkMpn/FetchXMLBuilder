using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Cinteros.Xrm.FetchXmlBuilder.Controls;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors
{
    class EntitySelector : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var descriptor = (EntityPropertyDescriptor)context.PropertyDescriptor;
            var svc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

            var listBox = new ListBox();
            listBox.BorderStyle = BorderStyle.None;

            foreach (var entity in descriptor.Entities.OrderBy(a => a.ToString()))
                listBox.Items.Add(entity);

            listBox.SelectedItem = value;
            listBox.DoubleClick += (s, e) => svc.CloseDropDown();
            listBox.KeyPress += (s, e) => { if (e.KeyChar == '\r') svc.CloseDropDown(); };

            svc.DropDownControl(listBox);

            return ((EntityNode)listBox.SelectedItem).ToString();
        }
    }
}
