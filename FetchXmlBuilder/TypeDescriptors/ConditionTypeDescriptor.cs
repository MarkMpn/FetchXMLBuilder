using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cinteros.Xrm.FetchXmlBuilder.Controls;
using Cinteros.Xrm.FetchXmlBuilder.DockControls;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors
{
    class ConditionTypeDescriptor : CustomTypeDescriptor
    {
        private TreeNode _node;
        private FetchXmlBuilder _fxb;
        private TreeBuilderControl _tree;

        public ConditionTypeDescriptor(TreeNode node, FetchXmlBuilder fxb, TreeBuilderControl tree)
        {
            _node = node;
            _fxb = fxb;
            _tree = tree;
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            var dictionary = (Dictionary<string, string>)_node.Tag;

            var closestEntity = GetClosestEntityNode();
            var entities = new List<EntityNode>();
            if (closestEntity != null && closestEntity.Name == "entity")
            {
                entities.Add(null);
                entities.AddRange(GetEntities(_tree.tvFetch.Nodes[0]).ToArray());
            }
            var entityReadOnly = entities.Count == 0;

            var entityProp = new EntityPropertyDescriptor(
                "(Entity)",
                "(Condition)",
                "The name or alias of the entity to apply the condition to",
                new Attribute[]
                {
                    new RefreshPropertiesAttribute(RefreshProperties.All),
                    new ReadOnlyAttribute(entityReadOnly)
                },
                this,
                String.Empty,
                dictionary,
                "entity",
                _tree,
                entities.ToArray());

            var entity = (string)entityProp.GetValue(this);
            var entityNode = entities.FirstOrDefault(e => e.ToString() == entity);
            if (entityNode == null)
                entityNode = new EntityNode(GetClosestEntityNode());

            var entityName = entityNode.EntityName;
            if (_fxb.NeedToLoadEntity(entityName))
            {
                // TODO: Load async within editor control
                _fxb.LoadEntityDetails(entityName, null, false);
            }
            var attributes = _fxb.GetDisplayAttributes(entityName);

            var attributeProp = new AttributePropertyDescriptor(
                "(Attribute)",
                "(Condition)",
                "The logical name of the attribute to apply the condition to",
                new Attribute[]
                {
                    new RefreshPropertiesAttribute(RefreshProperties.All)
                },
                this,
                String.Empty,
                dictionary,
                "attribute",
                _tree,
                attributes);

            return new PropertyDescriptorCollection(new PropertyDescriptor[] { entityProp, attributeProp });
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return GetProperties();
        }

        public override object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        private List<EntityNode> GetEntities(TreeNode node)
        {
            var result = new List<EntityNode>();
            if (node.Name == "link-entity")
            {
                result.Add(new EntityNode(node));
            }
            foreach (TreeNode child in node.Nodes)
            {
                result.AddRange(GetEntities(child));
            }
            return result;
        }

        private TreeNode GetClosestEntityNode()
        {
            var parentNode = _node.Parent;
            while (parentNode != null && parentNode.Name != "entity" && parentNode.Name != "link-entity")
            {
                parentNode = parentNode.Parent;
            }
            return parentNode;
        }
    }
}
