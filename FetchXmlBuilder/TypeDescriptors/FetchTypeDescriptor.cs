using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Cinteros.Xrm.FetchXmlBuilder.DockControls;
using Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors.PropertyDescriptors;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors
{
    /// <summary>
    /// Provides a type descriptor for the &lt;fetch&gt; element
    /// </summary>
    class FetchTypeDescriptor : BaseTypeDescriptor
    {   
        public FetchTypeDescriptor(TreeNode node, FetchXmlBuilder fxb, TreeBuilderControl tree)
            : base(node, fxb, tree)
        {
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            var dictionary = (Dictionary<string, string>)Node.Tag;

            var topProp = new CustomPropertyDescriptor<int?>(
                "Top",
                "Fetch",
                1,
                2,
                "The maximum number of records to retrieve",
                Array.Empty<Attribute>(),
                this,
                null,
                dictionary,
                "top",
                Tree);

            var distinctProp = new CustomPropertyDescriptor<bool>(
                "Distinct",
                "Fetch",
                1,
                2,
                "Indicates if only rows with unique values are to be included in the result set",
                Array.Empty<Attribute>(),
                this,
                false,
                dictionary,
                "distinct",
                Tree);

            var noLockProp = new CustomPropertyDescriptor<bool>(
                "No Lock",
                "Fetch",
                1,
                2,
                "Indicates if the query is to be run without taking any locks on the database",
                Array.Empty<Attribute>(),
                this,
                false,
                dictionary,
                "no-lock",
                Tree);

            var aggregateProp = new CustomPropertyDescriptor<bool>(
                "Aggregate",
                "Fetch",
                1,
                2,
                "Indicates if the query includes aggregations",
                Array.Empty<Attribute>(),
                this,
                false,
                dictionary,
                "aggregate",
                Tree);

            var totalRecordCountProp = new CustomPropertyDescriptor<bool>(
                "Total Record Count",
                "Fetch",
                1,
                2,
                "Indicates if the results should include the total number of records available",
                Array.Empty<Attribute>(),
                this,
                false,
                dictionary,
                "returntotalrecordcount",
                Tree);

            var pageSizeProp = new CustomPropertyDescriptor<int>(
                "Page Size",
                "Paging",
                2,
                2,
                "The number of records to retrieve for each page",
                Array.Empty<Attribute>(),
                this,
                5000,
                dictionary,
                "count",
                Tree);

            var pageNumberProp = new CustomPropertyDescriptor<int>(
                "Page Number",
                "Paging",
                2,
                2,
                "The number of the page to retrieve (first page is 1)",
                Array.Empty<Attribute>(),
                this,
                1,
                dictionary,
                "page",
                Tree);

            var pagingCookieProp = new CustomPropertyDescriptor<string>(
                "Paging Cookie",
                "Paging",
                2,
                2,
                "The paging cookie returned from the previous RetrieveMultiple request",
                Array.Empty<Attribute>(),
                this,
                null,
                dictionary,
                "paging-cookie",
                Tree);

            return new PropertyDescriptorCollection(new PropertyDescriptor[] { topProp, distinctProp, noLockProp, aggregateProp, totalRecordCountProp, pageSizeProp, pageNumberProp, pagingCookieProp });
        }
    }
}
