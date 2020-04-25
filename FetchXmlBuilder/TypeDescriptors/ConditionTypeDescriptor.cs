using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cinteros.Xrm.FetchXmlBuilder.AppCode;
using Cinteros.Xrm.FetchXmlBuilder.Controls;
using Cinteros.Xrm.FetchXmlBuilder.DockControls;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors
{
    class ConditionTypeDescriptor : CustomTypeDescriptor
    {
        private TreeNode _node;
        private FetchXmlBuilder _fxb;
        private TreeBuilderControl _tree;
        private static IDictionary<AttributeTypeCode, Type> _attributeTypes;

        static ConditionTypeDescriptor()
        {
            _attributeTypes = new Dictionary<AttributeTypeCode, Type>
            {
                [AttributeTypeCode.BigInt] = typeof(long),
                [AttributeTypeCode.Boolean] = typeof(bool),
                [AttributeTypeCode.Customer] = typeof(Lookup),
                [AttributeTypeCode.DateTime] = typeof(DateTime),
                [AttributeTypeCode.Decimal] = typeof(decimal),
                [AttributeTypeCode.Double] = typeof(double),
                [AttributeTypeCode.Integer] = typeof(int),
                [AttributeTypeCode.Lookup] = typeof(Lookup),
                [AttributeTypeCode.Memo] = typeof(string),
                [AttributeTypeCode.Money] = typeof(decimal),
                [AttributeTypeCode.Owner] = typeof(Lookup),
                [AttributeTypeCode.Picklist] = typeof(PicklistValue),
                [AttributeTypeCode.State] = typeof(PicklistValue),
                [AttributeTypeCode.Status] = typeof(PicklistValue),
                [AttributeTypeCode.String] = typeof(string),
                [AttributeTypeCode.Uniqueidentifier] = typeof(Lookup)
            };
        }

        public ConditionTypeDescriptor(TreeNode node, FetchXmlBuilder fxb, TreeBuilderControl tree)
        {
            _node = node;
            _fxb = fxb;
            _tree = tree;
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            var dictionary = (Dictionary<string, string>)_node.Tag;

            // Same type descriptor used for <condition> and <value> nodes. If this is a <value> node, find the <condition>
            var conditionNode = _node;
            if (_node.Name == "value")
                conditionNode = _node.Parent;
            var conditionDictionary = (Dictionary<string, string>)conditionNode.Tag;

            var closestEntity = GetClosestEntityNode();
            var entities = new List<EntityNode>();
            if (closestEntity != null && closestEntity.Name == "entity")
            {
                entities.Add(null);
                entities.AddRange(GetEntities(_tree.tvFetch.Nodes[0]).ToArray());
            }
            var entityReadOnly = entities.Count <= 1;

            var entityProp = new EntityPropertyDescriptor(
                "Entity",
                "Condition",
                1,
                1,
                "The name or alias of the entity to apply the condition to",
                new Attribute[]
                {
                    new RefreshPropertiesAttribute(RefreshProperties.All),
                    new ReadOnlyAttribute(entityReadOnly)
                },
                this,
                String.Empty,
                conditionDictionary,
                "entity",
                _tree,
                entities.Select(e => e?.ToString()).ToArray());

            var entity = (string)entityProp.GetValue(this);
            var entityNode = entities.FirstOrDefault(e => e != null && e.ToString() == entity);
            if (entityNode == null)
                entityNode = new EntityNode(GetClosestEntityNode());

            var entityName = entityNode.EntityName;
            if (_fxb.NeedToLoadEntity(entityName))
            {
                _fxb.LoadEntityDetails(entityName, () => _tree.RefreshSelectedNode());
            }
            var attributes = _fxb.GetDisplayAttributes(entityName);

            var attributeProp = new AttributePropertyDescriptor(
                "Attribute",
                "Condition",
                1,
                1,
                "The logical name of the attribute to apply the condition to",
                new Attribute[]
                {
                    new RefreshPropertiesAttribute(RefreshProperties.All)
                },
                this,
                String.Empty,
                conditionDictionary,
                "attribute",
                _tree,
                attributes);

            var attributeName = (string) attributeProp.GetValue(this);
            var attribute = attributes.SingleOrDefault(a => a.LogicalName == attributeName);

            var operatorProp = new ConditionOperatorPropertyDescriptor(
                "Operator",
                "Condition",
                1,
                1,
                "The comparison operator to apply",
                new Attribute[]
                {
                    new RefreshPropertiesAttribute(RefreshProperties.All)
                },
                this,
                ConditionOperator.Equal,
                conditionDictionary,
                "operator",
                _tree,
                attribute);

            // Value property varies depending on the attribute and/or operator
            var op = (ConditionOperator)operatorProp.GetValue(this);
            var oper = new OperatorItem(op);
            var valueType = oper.ValueType;
            PropertyDescriptor valueProp = null;

            if (valueType != null)
            {
                if (valueType == AttributeTypeCode.ManagedProperty)
                {
                    // Type is dependant on the attribute type
                    valueType = attribute?.AttributeType ?? AttributeTypeCode.String;
                }

                if (_attributeTypes.TryGetValue(valueType.Value, out var convertedValueType))
                {
                    if (oper.IsMultipleValuesType && dictionary == conditionDictionary)
                        convertedValueType = convertedValueType.MakeArrayType();

                    var propertyDescriptorType = typeof(ConditionValuePropertyDescriptor<>).MakeGenericType(convertedValueType);
                    valueProp = (PropertyDescriptor)Activator.CreateInstance(propertyDescriptorType,
                        "Value",
                        "Condition",
                        1,
                        1,
                        "The value to compare the attribute to",
                        Array.Empty<Attribute>(),
                        this,
                        null,
                        dictionary,
                        dictionary == conditionDictionary ? "value" : "#text",
                        _tree,
                        attribute,
                        _node,
                        _fxb);
                }
            }

            if (dictionary == conditionDictionary)
            {
                if (valueProp == null)
                    return new PropertyDescriptorCollection(new PropertyDescriptor[] { entityProp, attributeProp, operatorProp });
                else
                    return new PropertyDescriptorCollection(new PropertyDescriptor[] { entityProp, attributeProp, operatorProp, valueProp });
            }
            else
            {
                if (valueProp == null)
                {
                    // We're in a <value> node but the parent <condition> doesn't expect a value. We still need to give the user an option
                    // to edit this node though
                    valueProp = new CustomPropertyDescriptor<string>(
                        "Value",
                        "Condition",
                        1,
                        1,
                        "The value to compare the attribute to",
                        Array.Empty<Attribute>(),
                        this,
                        null,
                        dictionary,
                        "#text",
                        _tree);
                }

                return new PropertyDescriptorCollection(new PropertyDescriptor[] { valueProp });
            }
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
