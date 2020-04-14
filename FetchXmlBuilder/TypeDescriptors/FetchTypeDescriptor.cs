using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cinteros.Xrm.FetchXmlBuilder.DockControls;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors
{
    class FetchTypeDescriptor : CustomTypeDescriptor
    {
        private TreeNode _node;
        private FetchXmlBuilder _fxb;
        private TreeBuilderControl _tree;
        
        public FetchTypeDescriptor(TreeNode node, FetchXmlBuilder fxb, TreeBuilderControl tree)
        {
            _node = node;
            _fxb = fxb;
            _tree = tree;
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            var dictionary = (Dictionary<string, string>)_node.Tag;

            var topProp = new CustomPropertyDescriptor<int?>(
                "Top",
                "Fetch",
                "The maximum number of records to retrieve",
                Array.Empty<Attribute>(),
                this,
                null,
                dictionary,
                "top",
                _tree);

            var distinctProp = new CustomPropertyDescriptor<bool>(
                "Distinct",
                "Fetch",
                "Indicates if only rows with unique values are to be included in the result set",
                Array.Empty<Attribute>(),
                this,
                false,
                dictionary,
                "distinct",
                _tree);

            var noLockProp = new CustomPropertyDescriptor<bool>(
                "No Lock",
                "Fetch",
                "Indicates if the query is to be run without taking any locks on the database",
                Array.Empty<Attribute>(),
                this,
                false,
                dictionary,
                "no-lock",
                _tree);

            var aggregateProp = new CustomPropertyDescriptor<bool>(
                "Aggregate",
                "Fetch",
                "Indicates if the query includes aggregations",
                Array.Empty<Attribute>(),
                this,
                false,
                dictionary,
                "aggregate",
                _tree);

            var totalRecordCountProp = new CustomPropertyDescriptor<bool>(
                "Total Record Count",
                "Fetch",
                "Indicates if the results should include the total number of records available",
                Array.Empty<Attribute>(),
                this,
                false,
                dictionary,
                "aggregate",
                _tree);

            var pageSizeProp = new CustomPropertyDescriptor<int>(
                "Page Size",
                "Paging",
                "The number of records to retrieve for each page",
                Array.Empty<Attribute>(),
                this,
                5000,
                dictionary,
                "count",
                _tree);

            var pageNumberProp = new CustomPropertyDescriptor<int>(
                "Page Number",
                "Paging",
                "The number of the page to retrieve (first page is 1)",
                Array.Empty<Attribute>(),
                this,
                1,
                dictionary,
                "page",
                _tree);

            var pagingCookieProp = new CustomPropertyDescriptor<string>(
                "Paging Cookie",
                "Paging",
                "The paging cookie returned from the previous RetrieveMultiple request",
                Array.Empty<Attribute>(),
                this,
                null,
                dictionary,
                "paging-cookie",
                _tree);

            return new PropertyDescriptorCollection(new PropertyDescriptor[] { topProp, distinctProp, noLockProp, aggregateProp, totalRecordCountProp, pageSizeProp, pageNumberProp, pagingCookieProp });
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
