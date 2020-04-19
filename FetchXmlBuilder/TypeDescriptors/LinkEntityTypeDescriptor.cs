using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cinteros.Xrm.FetchXmlBuilder.AppCode;
using Cinteros.Xrm.FetchXmlBuilder.DockControls;
using Microsoft.Xrm.Sdk.Query;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors
{
    class LinkEntityTypeDescriptor : CustomTypeDescriptor
    {
        private TreeNode _node;
        private FetchXmlBuilder _fxb;
        private TreeBuilderControl _tree;

        public LinkEntityTypeDescriptor(TreeNode node, FetchXmlBuilder fxb, TreeBuilderControl tree)
        {
            _node = node;
            _fxb = fxb;
            _tree = tree;
        }

        public TreeNode Node { get => _node; }

        public FetchXmlBuilder FXB { get => _fxb; }

        public override PropertyDescriptorCollection GetProperties()
        {
            var dictionary = (Dictionary<string, string>)_node.Tag;

            var entities = _fxb.GetDisplayEntities();
            var nameProp = new EntityPropertyDescriptor(
                "Name",
                "Entity",
                1,
                3,
                "The logical name of the entity to query",
                new Attribute[]
                {
                    new RefreshPropertiesAttribute(RefreshProperties.All)
                },
                this,
                null,
                dictionary,
                "name",
                _tree,
                entities.Keys.ToArray());

            var aliasProp = new CustomPropertyDescriptor<string>(
                "Alias",
                "Entity",
                1,
                3,
                "The alias name to apply to the linked entity",
                Array.Empty<Attribute>(),
                this,
                null,
                dictionary,
                "alias",
                _tree);

            var fromProp = new AttributePropertyDescriptor(
                "From",
                "Join",
                2,
                3,
                "Attribute in this entity to join from",
                Array.Empty<Attribute>(),
                this,
                null,
                dictionary,
                "from",
                _tree,
                _fxb.GetDisplayAttributes((string)nameProp.GetValue(this)));

            var parententityname = TreeNodeHelper.GetAttributeFromNode(_node.Parent, "name");
            var toProp = new AttributePropertyDescriptor(
                "To",
                "Join",
                2,
                3,
                "Attribute in the related entity to join this entity to",
                Array.Empty<Attribute>(),
                this,
                null,
                dictionary,
                "to",
                _tree,
                _fxb.GetDisplayAttributes(parententityname));

            var typeProp = new CustomPropertyDescriptor<JoinOperator>(
                "Type",
                "Join",
                2,
                3,
                "Type of join operator to apply",
                Array.Empty<Attribute>(),
                this,
                JoinOperator.Inner,
                dictionary,
                "link-type",
                _tree);

            var intersectProp = new CustomPropertyDescriptor<bool>(
                "Intersect",
                "Advanced",
                3,
                3,
                "Indicates if this is a many-to-many intersect attribute that should be hidden in the Advanced Find query editor",
                Array.Empty<Attribute>(),
                this,
                false,
                dictionary,
                "intersect",
                _tree);

            var visibleProp = new CustomPropertyDescriptor<bool>(
                "Visible",
                "Advanced",
                3,
                3,
                "Indicates if this link-entity should be hidden in the Advanced Find query editor",
                Array.Empty<Attribute>(),
                this,
                false,
                dictionary,
                "visible",
                _tree);

            var relationshipProp = new RelationshipPropertyDescriptor(
                "Relationship",
                "Join",
                2,
                3,
                "The relationship to use as a template for this join",
                new Attribute[]
                {
                    new RefreshPropertiesAttribute(RefreshProperties.All)
                },
                this,
                _tree);

            return new PropertyDescriptorCollection(new PropertyDescriptor[] { nameProp, aliasProp, fromProp, toProp, typeProp, intersectProp, visibleProp, relationshipProp });
        }

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
