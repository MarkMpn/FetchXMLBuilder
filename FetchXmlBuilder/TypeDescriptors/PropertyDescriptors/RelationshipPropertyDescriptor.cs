using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Cinteros.Xrm.FetchXmlBuilder.AppCode;
using Cinteros.Xrm.FetchXmlBuilder.DockControls;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors.PropertyDescriptors
{
    /// <summary>
    /// A property descriptor that links to a relationship
    /// </summary>
    class RelationshipPropertyDescriptor : PropertyDescriptor
    {
        private readonly object _owner;
        private readonly TreeBuilderControl _tree;

        public RelationshipPropertyDescriptor(string name, string category, int categoryOrder, int categoryCount, string description, Attribute[] attrs, object owner, TreeBuilderControl tree) :
            base(name, CreateAttributes(attrs, category, categoryOrder, categoryCount, description))
        {
            _owner = owner;
            _tree = tree;
        }

        static Attribute[] CreateAttributes(Attribute[] attributes, string category, int categoryOrder, int categoryCount, string description)
        {
            var attrs = new List<Attribute>(attributes);

            if (!String.IsNullOrEmpty(category))
            {
                attrs.Add(new CustomSortedCategoryAttribute(category, (ushort)categoryOrder, (ushort)categoryCount));
            }

            if (!String.IsNullOrEmpty(description))
            {
                attrs.Add(new DescriptionAttribute(description));
            }

            attrs.Add(new EditorAttribute(typeof(RelationshipEditor), typeof(UITypeEditor)));

            return attrs.ToArray();
        }

        public override Type ComponentType => _owner.GetType();

        public override bool IsReadOnly => false;

        public override Type PropertyType => typeof(EntityRelationship);

        public override bool CanResetValue(object component)
        {
            return true;
        }

        public override object GetValue(object component)
        {
            var link = (LinkEntityTypeDescriptor)component;

            var entity = (string) link.GetProperties()["Name"].GetValue(component);
            var from = (string) link.GetProperties()["From"].GetValue(component);
            var to = (string) link.GetProperties()["To"].GetValue(component);

            var parententityname = TreeNodeHelper.GetAttributeFromNode(link.Node.Parent, "name");
            var entities = link.FXB.GetDisplayEntities();
            if (entities != null && entities.ContainsKey(parententityname))
            {
                var parententity = entities[parententityname];
                var mo = parententity.ManyToOneRelationships;
                var om = parententity.OneToManyRelationships;
                var mm = parententity.ManyToManyRelationships;

                foreach (var rel in mo)
                {
                    if (entity == rel.ReferencedEntity && from == rel.ReferencedAttribute && to == rel.ReferencingAttribute)
                    {
                        return new EntityRelationship(rel, EntityRole.Referencing, parententityname, link.FXB);
                    }
                }

                foreach (var rel in om)
                {
                    if (entity == rel.ReferencingEntity && from == rel.ReferencingAttribute && to == rel.ReferencedAttribute)
                    {
                        return new EntityRelationship(rel, EntityRole.Referenced, parententityname, link.FXB);
                    }
                }

                var greatparententityname = link.Node.Parent.Parent != null ? TreeNodeHelper.GetAttributeFromNode(link.Node.Parent.Parent, "name") : "";
                foreach (var rel in mm)
                {
                    if (parententityname == rel.IntersectEntityName)
                    {
                        if (to == rel.Entity1IntersectAttribute && greatparententityname == rel.Entity2LogicalName && entity == rel.Entity1LogicalName)
                        {
                            return new EntityRelationship(rel, EntityRole.Referenced, parententityname, link.FXB, greatparententityname);
                        }

                        if (to == rel.Entity2IntersectAttribute && greatparententityname == rel.Entity1LogicalName && entity == rel.Entity2LogicalName)
                        {
                            return new EntityRelationship(rel, EntityRole.Referencing, parententityname, link.FXB, greatparententityname);
                        }
                    }

                    if (entity == rel.IntersectEntityName)
                    {
                        if (from == rel.Entity1IntersectAttribute)
                        {
                            return new EntityRelationship(rel, EntityRole.Referencing, parententityname, link.FXB);
                        }

                        if (from == rel.Entity2IntersectAttribute)
                        {
                            return new EntityRelationship(rel, EntityRole.Referenced, parententityname, link.FXB);
                        }
                    }
                }
            }

            return null;
        }

        public override void ResetValue(object component)
        {
        }

        public override void SetValue(object component, object value)
        {
            if (value == null)
            {
                return;
            }

            var rel = (EntityRelationship)value;
            var link = (LinkEntityTypeDescriptor)component;

            var parent = TreeNodeHelper.GetAttributeFromNode(link.Node.Parent, "name");
            string entity;
            string from;
            string to;
            bool intersect;

            if (rel.Relationship is OneToManyRelationshipMetadata)
            {
                var om = (OneToManyRelationshipMetadata)rel.Relationship;
                if (parent == om.ReferencedEntity && rel.Role == EntityRole.Referenced)
                {
                    entity = om.ReferencingEntity;
                    from = om.ReferencingAttribute;
                    to = om.ReferencedAttribute;
                }
                else if (parent == om.ReferencingEntity && rel.Role == EntityRole.Referencing)
                {
                    entity = om.ReferencedEntity;
                    from = om.ReferencedAttribute;
                    to = om.ReferencingAttribute;
                }
                else
                {
                    throw new ApplicationException("Not a valid relationship. Please enter entity and attributes manually.");
                }
                intersect = false;
            }
            else if (rel.Relationship is ManyToManyRelationshipMetadata)
            {
                var mm = (ManyToManyRelationshipMetadata)rel.Relationship;
                
                if (link.FXB.NeedToLoadEntity(mm.Entity1LogicalName))
                {
                    link.FXB.LoadEntityDetails(mm.Entity1LogicalName, null, false);
                }

                if (link.FXB.NeedToLoadEntity(mm.Entity2LogicalName))
                {
                    link.FXB.LoadEntityDetails(mm.Entity2LogicalName, null, false);
                }

                var entity1PrimaryKey = link.FXB.GetPrimaryIdAttribute(mm.Entity1LogicalName);
                var entity2PrimaryKey = link.FXB.GetPrimaryIdAttribute(mm.Entity2LogicalName);

                if (parent == mm.IntersectEntityName)
                {
                    var greatparent = TreeNodeHelper.GetAttributeFromNode(link.Node.Parent.Parent, "name");
                    if (greatparent == mm.Entity1LogicalName && rel.Role == EntityRole.Referencing)
                    {
                        entity = mm.Entity2LogicalName;
                        from = entity2PrimaryKey;
                        to = mm.Entity2IntersectAttribute;
                    }
                    else if (greatparent == mm.Entity2LogicalName && rel.Role == EntityRole.Referenced)
                    {
                        entity = mm.Entity1LogicalName;
                        from = entity1PrimaryKey;
                        to = mm.Entity1IntersectAttribute;
                    }
                    else
                    {
                        throw new ApplicationException("Not a valid M:M-relationship. Please enter entity and attributes manually.");
                    }
                    intersect = true;
                }
                else
                {
                    entity = mm.IntersectEntityName;
                    if (parent == mm.Entity1LogicalName && rel.Role == EntityRole.Referencing)
                    {
                        from = mm.Entity1IntersectAttribute;
                        to = entity1PrimaryKey;
                    }
                    else if (parent == mm.Entity2LogicalName && rel.Role == EntityRole.Referenced)
                    {
                        from = mm.Entity2IntersectAttribute;
                        to = entity2PrimaryKey;
                    }
                    else
                    {
                        throw new ApplicationException("Not a valid M:M-relationship. Please enter entity and attributes manually.");
                    }
                    intersect = true;
                }
            }
            else
            {
                throw new ApplicationException("Not a valid relationship. Please enter entity and attributes manually.");
            }

            link.GetProperties()["Name"].SetValue(component, entity);
            link.GetProperties()["From"].SetValue(component, from);
            link.GetProperties()["To"].SetValue(component, to);
            link.GetProperties()["Intersect"].SetValue(component, intersect);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return true;
        }

        class RelationshipEditor : UITypeEditor
        {
            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.DropDown;
            }

            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                var link = (LinkEntityTypeDescriptor)context.Instance;
                var listBox = new ListBox();
                var parententityname = TreeNodeHelper.GetAttributeFromNode(link.Node.Parent, "name");
                var entities = link.FXB.GetDisplayEntities();
                if (entities != null && entities.ContainsKey(parententityname))
                {
                    var parententity = entities[parententityname];
                    var mo = parententity.ManyToOneRelationships;
                    var om = parententity.OneToManyRelationships;
                    var mm = parententity.ManyToManyRelationships;
                    var list = new List<EntityRelationship>();
                    if (mo.Length > 0)
                    {
                        listBox.Items.Add("- M:1 -");
                        list.Clear();
                        foreach (var rel in mo)
                        {
                            list.Add(new EntityRelationship(rel, EntityRole.Referencing, parententityname, link.FXB));
                        }
                        list.Sort();
                        listBox.Items.AddRange(list.ToArray());
                    }
                    if (om.Length > 0)
                    {
                        listBox.Items.Add("- 1:M -");
                        list.Clear();
                        foreach (var rel in om)
                        {
                            list.Add(new EntityRelationship(rel, EntityRole.Referenced, parententityname, link.FXB));
                        }
                        list.Sort();
                        listBox.Items.AddRange(list.ToArray());
                    }
                    if (mm.Length > 0)
                    {
                        var greatparententityname = link.Node.Parent.Parent != null ? TreeNodeHelper.GetAttributeFromNode(link.Node.Parent.Parent, "name") : "";
                        listBox.Items.Add("- M:M -");
                        list.Clear();
                        foreach (var rel in mm)
                        {
                            if (rel.Entity1LogicalName == parententityname ||
                                rel.Entity1LogicalName == greatparententityname && rel.IntersectEntityName == parententityname)
                            {
                                list.Add(new EntityRelationship(rel, EntityRole.Referencing, parententityname, link.FXB, greatparententityname));
                            }

                            if (rel.Entity2LogicalName == parententityname ||
                                rel.Entity2LogicalName == greatparententityname && rel.IntersectEntityName == parententityname)
                            {
                                list.Add(new EntityRelationship(rel, EntityRole.Referenced, parententityname, link.FXB, greatparententityname));
                            }
                        }
                        list.Sort();
                        listBox.Items.AddRange(list.ToArray());
                    }
                }

                var edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                listBox.Click += (s, e) => edSvc.CloseDropDown();
                listBox.KeyPress += (s, e) => { if (e.KeyChar == '\r') edSvc.CloseDropDown(); };
                edSvc.DropDownControl(listBox);

                return listBox.SelectedItem as EntityRelationship;
            }
        }
    }
}
