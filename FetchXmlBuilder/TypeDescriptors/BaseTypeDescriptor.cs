using System;
using System.ComponentModel;
using System.Windows.Forms;
using Cinteros.Xrm.FetchXmlBuilder.DockControls;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors
{
    /// <summary>
    /// Provides the basic implementation of a type descriptor to represent an element in the FetchXML tree
    /// </summary>
    abstract class BaseTypeDescriptor : CustomTypeDescriptor
    {
        public BaseTypeDescriptor(TreeNode node, FetchXmlBuilder fxb, TreeBuilderControl tree)
        {
            Node = node;
            FXB = fxb;
            Tree = tree;
        }

        public TreeNode Node { get; }

        public FetchXmlBuilder FXB { get; }

        public TreeBuilderControl Tree { get; }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return GetProperties();
        }

        public override object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }
    }
}
