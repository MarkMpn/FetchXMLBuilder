using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Cinteros.Xrm.FetchXmlBuilder.Forms
{
    public partial class LookupSingle : Form
    {
        class ViewInfo
        {
            public Entity Entity { get; set; }

            public override string ToString()
            {
                return Entity["name"].ToString();
            }
        }

        /// <summary>
        /// Compares two listview items for sorting
        /// </summary>
        internal class ListViewItemComparer : IComparer
        {
            #region Variables

            /// <summary>
            /// Index of sorting column
            /// </summary>
            private readonly int col;

            /// <summary>
            /// Sort order
            /// </summary>
            private readonly SortOrder innerOrder;

            #endregion Variables

            #region Constructors

            /// <summary>
            /// Initializes a new instance of class ListViewItemComparer
            /// </summary>
            public ListViewItemComparer()
            {
                col = 0;
                innerOrder = SortOrder.Ascending;
            }

            /// <summary>
            /// Initializes a new instance of class ListViewItemComparer
            /// </summary>
            /// <param name="column">Index of sorting column</param>
            /// <param name="order">Sort order</param>
            public ListViewItemComparer(int column, SortOrder order)
            {
                col = column;
                innerOrder = order;
            }

            #endregion Constructors

            #region Methods

            /// <summary>
            /// Compare tow objects
            /// </summary>
            /// <param name="x">object 1</param>
            /// <param name="y">object 2</param>
            /// <returns></returns>
            public int Compare(object x, object y)
            {
                return Compare((ListViewItem)x, (ListViewItem)y);
            }

            /// <summary>
            /// Compare tow listview items
            /// </summary>
            /// <param name="x">Listview item 1</param>
            /// <param name="y">Listview item 2</param>
            /// <returns></returns>
            public int Compare(ListViewItem x, ListViewItem y)
            {
                if (innerOrder == SortOrder.Ascending)
                {
                    return String.CompareOrdinal(x.SubItems[col].Text, y.SubItems[col].Text);
                }

                return String.CompareOrdinal(y.SubItems[col].Text, x.SubItems[col].Text);
            }

            #endregion Methods
        }

        private readonly IOrganizationService service;
        private EntityMetadata metadata;

        public LookupSingle(string[] entityNames, IOrganizationService service)
        {
            InitializeComponent();

            this.service = service;
            cbbEntities.Items.AddRange(entityNames);

            cbbEntities.SelectedIndex = 0;
        }

        public string LogicalName => (string)cbbEntities.SelectedItem;

        public EntityReference SelectedRecord { get; private set; }

        private void BtnCancelClick(object sender, EventArgs e)
        {
            SelectedRecord = null;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void BtnOkClick(object sender, EventArgs e)
        {
            var entity = (Entity)lvResults.SelectedItems[0].Tag;
            SelectedRecord = entity.ToEntityReference();
            SelectedRecord.Name = entity.GetAttributeValue<string>(metadata.PrimaryNameAttribute);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ProcessFilter(XmlNode node, string searchTerm)
        {
            foreach (XmlNode condition in node.SelectNodes("condition"))
            {
                if (!condition.Attributes["value"].Value.StartsWith("{"))
                {
                    continue;
                }
                var attr = metadata.Attributes.First(a => a.LogicalName == condition.Attributes["attribute"].Value);

                #region Manage each attribute type

                switch (attr.AttributeType.Value)
                {
                    case AttributeTypeCode.Memo:
                    case AttributeTypeCode.String:
                        {
                            condition.Attributes["value"].Value = searchTerm.Replace("*", "%") + "%";
                        }
                        break;
                    case AttributeTypeCode.Boolean:
                        {
                            if (searchTerm != "0" && searchTerm != "1")
                            {
                                node.RemoveChild(condition);
                                continue;
                            }

                            condition.Attributes["value"].Value = (searchTerm == "1").ToString();
                        }
                        break;
                    case AttributeTypeCode.Customer:
                    case AttributeTypeCode.Lookup:
                    case AttributeTypeCode.Owner:
                        {
                            if (
                                metadata.Attributes.FirstOrDefault(
                                    a => a.LogicalName == condition.Attributes["attribute"].Value + "name") == null)
                            {
                                node.RemoveChild(condition);

                                continue;
                            }


                            condition.Attributes["attribute"].Value += "name";
                            condition.Attributes["value"].Value = searchTerm.Replace("*", "%") + "%";
                        }
                        break;
                    case AttributeTypeCode.DateTime:
                        {
                            DateTime dt;
                            if (!DateTime.TryParse(searchTerm, out dt))
                            {
                                condition.Attributes["value"].Value = new DateTime(1754, 1, 1).ToString("yyyy-MM-dd");
                            }
                            else
                            {
                                condition.Attributes["value"].Value = dt.ToString("yyyy-MM-dd");
                            }
                        }
                        break;
                    case AttributeTypeCode.Decimal:
                    case AttributeTypeCode.Double:
                    case AttributeTypeCode.Money:
                        {
                            decimal d;
                            if (!decimal.TryParse(searchTerm, out d))
                            {
                                condition.Attributes["value"].Value = int.MinValue.ToString(CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                condition.Attributes["value"].Value = d.ToString(CultureInfo.InvariantCulture);
                            }
                        }
                        break;
                    case AttributeTypeCode.Integer:
                        {
                            int d;
                            if (!int.TryParse(searchTerm, out d))
                            {
                                condition.Attributes["value"].Value = int.MinValue.ToString(CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                condition.Attributes["value"].Value = d.ToString(CultureInfo.InvariantCulture);
                            }
                        }
                        break;
                    case AttributeTypeCode.Picklist:
                        {
                            var opt = ((PicklistAttributeMetadata)attr).OptionSet.Options.FirstOrDefault(
                                o => o.Label.UserLocalizedLabel.Label == searchTerm);

                            if (opt == null)
                            {
                                condition.Attributes["value"].Value = int.MinValue.ToString(CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                condition.Attributes["value"].Value = opt.Value.Value.ToString(CultureInfo.InvariantCulture);
                            }
                        }
                        break;
                    case AttributeTypeCode.State:
                        {
                            var opt = ((StateAttributeMetadata)attr).OptionSet.Options.FirstOrDefault(
                                o => o.Label.UserLocalizedLabel.Label == searchTerm);

                            if (opt == null)
                            {
                                condition.Attributes["value"].Value = int.MinValue.ToString(CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                condition.Attributes["value"].Value = opt.Value.Value.ToString(CultureInfo.InvariantCulture);
                            }
                        }
                        break;
                    case AttributeTypeCode.Status:
                        {
                            var opt = ((StatusAttributeMetadata)attr).OptionSet.Options.FirstOrDefault(
                                o => o.Label.UserLocalizedLabel.Label == searchTerm);

                            if (opt == null)
                            {
                                condition.Attributes["value"].Value = int.MinValue.ToString(CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                condition.Attributes["value"].Value = opt.Value.Value.ToString(CultureInfo.InvariantCulture);
                            }
                        }
                        break;
                }

                #endregion
            }

            foreach (XmlNode filter in node.SelectNodes("filter"))
            {
                ProcessFilter(filter, searchTerm);
            }
        }

        private void BtnSearchClick(object sender, EventArgs e)
        {
            lvResults.Items.Clear();

            string newFetchXml = "";
            try
            {
                if (txtSearch.Text.Length == 0) txtSearch.Text = "*";

                var view = ((ViewInfo)cbbViews.SelectedItem).Entity;
                var layout = new XmlDocument();
                layout.LoadXml(view["layoutxml"].ToString());


                string fetchXml = view["fetchxml"].ToString();
                var fetchDoc = new XmlDocument();
                fetchDoc.LoadXml(fetchXml);
                var filterNodes = fetchDoc.SelectNodes("fetch/entity/filter");
                foreach (XmlNode filterNode in filterNodes)
                    ProcessFilter(filterNode, txtSearch.Text);

                newFetchXml = fetchDoc.OuterXml;

                var result = service.RetrieveMultiple(new FetchExpression { Query = newFetchXml });

                foreach (var entity in result.Entities)
                {
                    bool isFirstCell = true;

                    var item = new ListViewItem();
                    item.Tag = entity;

                    foreach (XmlNode cell in layout.SelectNodes("//cell"))
                    {
                        var attributeName = cell.Attributes["name"].Value;
                        if (!entity.FormattedValues.TryGetValue(attributeName, out var value))
                        {
                            if (entity.Attributes.TryGetValue(attributeName, out var rawValue))
                                value = rawValue?.ToString();

                            if (value == null)
                                value = "";
                        }

                        if (isFirstCell)
                        {
                            item.Text = value;
                            isFirstCell = false;
                        }
                        else
                        {
                            item.SubItems.Add(value);
                        }
                    }

                    lvResults.Items.Add(item);
                }

                if (result.MoreRecords)
                {
                    MessageBox.Show(this,
                        "There is more than 250 records that match your search! Please refine your search",
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

            }
            catch (Exception error)
            {
                MessageBox.Show(this,
                    "An error occured: " + error.ToString() + " --> " + newFetchXml,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CbbViewsSelectedIndexChanged(object sender, EventArgs e)
        {
            lvResults.Columns.Clear();

            var view = ((ViewInfo)cbbViews.SelectedItem).Entity;
            var layout = new XmlDocument();
            layout.LoadXml(view["layoutxml"].ToString());

            foreach (XmlNode cell in layout.SelectNodes("//cell"))
            {
                var ch = new ColumnHeader();
                try
                {
                    ch.Text =
                        metadata.Attributes.First(a => a.LogicalName == cell.Attributes["name"].Value)
                            .DisplayName.UserLocalizedLabel.Label;
                    ch.Width = int.Parse(cell.Attributes["width"].Value);
                }
                catch
                {
                    ch.Text = cell.Attributes["name"].Value;
                }
                lvResults.Columns.Add(ch);
            }
        }

        private void CbbEntitiesSelectedIndexChanged(object sender, EventArgs e)
        {
            cbbViews.Items.Clear();
            SelectedRecord = null;

            var qe = new QueryExpression("savedquery");
            qe.ColumnSet = new ColumnSet(true);
            qe.Criteria.AddCondition("returnedtypecode", ConditionOperator.Equal, LogicalName);
            qe.Criteria.AddCondition("querytype", ConditionOperator.Equal, 4);
            var records = service.RetrieveMultiple(qe);

            if (records.Entities.Count == 0)
            {
                MessageBox.Show(this, "Cannot load views since this entity does not have Quick Find view defined", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int index = 0;
            int defaultViewIndex = 0;

            foreach (var record in records.Entities)
            {
                if ((bool)record["isdefault"])
                    defaultViewIndex = index;

                var view = new ViewInfo();
                view.Entity = record;

                cbbViews.Items.Add(view);

                index++;
            }

            cbbViews.SelectedIndex = defaultViewIndex;
            metadata = ((RetrieveEntityResponse)service.Execute(new RetrieveEntityRequest { LogicalName = LogicalName, EntityFilters = EntityFilters.Attributes })).EntityMetadata;
        }

        private void LvResultsColumnClick(object sender, ColumnClickEventArgs e)
        {
            lvResults.Sorting = lvResults.Sorting == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            lvResults.ListViewItemSorter = new ListViewItemComparer(e.Column, lvResults.Sorting);
        }

        private void LvResultsDoubleClick(object sender, EventArgs e)
        {
            BtnOkClick(null, null);
        }

        private void txtSearch_Enter(object sender, EventArgs e)
        {
            AcceptButton = btnSearch;
        }

        private void txtSearch_Leave(object sender, EventArgs e)
        {
            AcceptButton = btnOK;
        }

        private void LookupSingle_Load(object sender, EventArgs e)
        {
            btnSearch.Height = txtSearch.Height;
            btnSearch.Width = btnSearch.Height;
        }

        private void lvResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = lvResults.SelectedItems.Count == 1;
        }
    }
}