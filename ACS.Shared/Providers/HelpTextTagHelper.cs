using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Reflection;
using System.Text.Encodings.Web;

namespace ACS.Shared.Providers
{
    [HtmlTargetElement("label", Attributes = "asp-for")]
    public class HelpTextTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-for")]
        public ModelExpression For { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            HelpTextAttribute? helpTextAttribute = For.Metadata.ContainerType?.GetProperty(For.Name)?.GetCustomAttribute<HelpTextAttribute>();

            if (helpTextAttribute != null)
            {
                output.Attributes.SetAttribute("title", helpTextAttribute.HelpText);
                output.AddClass("help-text", HtmlEncoder.Default);
            }
        }
    }
}
