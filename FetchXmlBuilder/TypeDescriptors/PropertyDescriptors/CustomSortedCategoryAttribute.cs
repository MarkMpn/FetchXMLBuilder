using System.ComponentModel;

namespace Cinteros.Xrm.FetchXmlBuilder.TypeDescriptors.PropertyDescriptors
{
    // https://stackoverflow.com/questions/823327/how-can-i-customize-category-sorting-on-a-propertygrid
    public class CustomSortedCategoryAttribute : CategoryAttribute
    {
        private const char NonPrintableChar = '\t';

        public CustomSortedCategoryAttribute(string category,
                                                ushort categoryPos,
                                                ushort totalCategories)
            : base(category.PadLeft(category.Length + (totalCategories - categoryPos),
                        CustomSortedCategoryAttribute.NonPrintableChar))
        {
        }
    }
}
