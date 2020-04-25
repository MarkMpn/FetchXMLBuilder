using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Cinteros.Xrm.FetchXmlBuilder.AppCode;
using Cinteros.Xrm.FetchXmlBuilder.DockControls;
using Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors.PropertyDescriptors;
using Microsoft.Xrm.Sdk.Query;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors
{
    /// <summary>
    /// Provides a type descriptor for the &lt;link-entity&gt; element
    /// </summary>
    class LinkEntityTypeDescriptor : BaseTypeDescriptor
    {
        public LinkEntityTypeDescriptor(TreeNode node, FetchXmlBuilder fxb, TreeBuilderControl tree)
            : base(node, fxb, tree)
        {
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            var dictionary = (Dictionary<string, string>)Node.Tag;

            var entities = FXB.GetDisplayEntities();
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
                Tree,
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
                Tree);

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
                Tree,
                FXB.GetDisplayAttributes((string)nameProp.GetValue(this)));

            var parententityname = TreeNodeHelper.GetAttributeFromNode(Node.Parent, "name");
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
                Tree,
                FXB.GetDisplayAttributes(parententityname));

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
                Tree);

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
                Tree);

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
                Tree);

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
                Tree);

            return new PropertyDescriptorCollection(new PropertyDescriptor[] { nameProp, aliasProp, fromProp, toProp, typeProp, intersectProp, visibleProp, relationshipProp });
        }
    }
}
