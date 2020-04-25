using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms;
using Cinteros.Xrm.FetchXmlBuilder.DockControls;
using Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors.PropertyDescriptors;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors
{
    /// <summary>
    /// Provides a type descriptor for comment elements
    /// </summary>
    class CommentTypeDescriptor : BaseTypeDescriptor
    {
        public CommentTypeDescriptor(TreeNode node, FetchXmlBuilder fxb, TreeBuilderControl tree)
            : base(node, fxb, tree)
        {
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            var dictionary = (Dictionary<string, string>)Node.Tag;

            var textProp = new CustomPropertyDescriptor<string>(
                "Text",
                "Comment",
                1,
                1,
                "Comment",
                new Attribute[]
                {
                    new TypeConverterAttribute(typeof(MultilineStringConverter)),
                    new EditorAttribute(typeof(MultilineStringEditor), typeof(UITypeEditor))
                },
                this,
                null,
                dictionary,
                "#comment",
                Tree);

            return new PropertyDescriptorCollection(new PropertyDescriptor[] { textProp });
        }
    }
}
