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
using Cinteros.Xrm.FetchXmlBuilder.DockControls;
using Microsoft.Xrm.Sdk.Metadata;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors
{
    class EntityPropertyDescriptor : CustomPropertyDescriptor<string>
    {
        public EntityPropertyDescriptor(string name, string category, string description, Attribute[] attrs, object owner, string defaultValue, Dictionary<string,string> dictionary, string key, TreeBuilderControl tree, EntityNode[] entities) :
            base(name, category, description, CreateAttributes(attrs), owner, defaultValue, dictionary, key, tree)
        {
            Entities = entities;
        }

        static Attribute[] CreateAttributes(Attribute[] attributes)
        {
            var attrs = new List<Attribute>(attributes);
            attrs.Add(new EditorAttribute(typeof(EntitySelector), typeof(UITypeEditor)));
            return attrs.ToArray();
        }

        public EntityNode[] Entities { get; }

        public override string GetValidationError(ITypeDescriptorContext context)
        {
            var entityName = (string)GetValue(context.Instance);

            if (!String.IsNullOrEmpty(entityName) && Entities != null && !Entities.Any(e => e != null && e.ToString() == entityName))
                return "Unknown entity";

            return base.GetValidationError(context);
        }
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

                foreach (var entity in descriptor.Entities.OrderBy(a => a?.ToString()))
                    listBox.Items.Add(entity);

                listBox.SelectedItem = value;
                listBox.DoubleClick += (s, e) => svc.CloseDropDown();
                listBox.KeyPress += (s, e) => { if (e.KeyChar == '\r') svc.CloseDropDown(); };

                svc.DropDownControl(listBox);

                return ((EntityNode)listBox.SelectedItem)?.ToString();
            }
        }
    }
}
