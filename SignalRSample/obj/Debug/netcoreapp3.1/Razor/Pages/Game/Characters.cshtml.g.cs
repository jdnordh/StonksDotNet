#pragma checksum "D:\Repos\StonksStable\SignalRSample\Pages\Game\Characters.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "51ee8ac6db8efe897910a9bf11498de65bdbf556"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(SignalRSample.Pages.Game.Pages_Game_Characters), @"mvc.1.0.razor-page", @"/Pages/Game/Characters.cshtml")]
namespace SignalRSample.Pages.Game
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#nullable restore
#line 1 "D:\Repos\StonksStable\SignalRSample\Pages\_ViewImports.cshtml"
using SignalRSample;

#line default
#line hidden
#nullable disable
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"51ee8ac6db8efe897910a9bf11498de65bdbf556", @"/Pages/Game/Characters.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"d1e47127102eeefebecfe9a2686eee1fc3d4a78d", @"/Pages/_ViewImports.cshtml")]
    #nullable restore
    public class Pages_Game_Characters : global::Microsoft.AspNetCore.Mvc.RazorPages.Page
    #nullable disable
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            WriteLiteral("\r\n<div id=\"menu-character-container\" class=\"center-absolute\">\r\n\t<div class=\"menu-character-select-grid\">\r\n\t\t<h1>Select Your Character</h1>\r\n");
#nullable restore
#line 7 "D:\Repos\StonksStable\SignalRSample\Pages\Game\Characters.cshtml"
         foreach(var kvp in Model.Characters)
		{
			string buttonId = $"selectCharacter{kvp.Value.Id}";
			string nameId = $"characterName{kvp.Value.Id}";

#line default
#line hidden
#nullable disable
            WriteLiteral("\t\t\t<div class=\"character-card\">\r\n\t\t\t\t<div class=\"character-text-grid grid-column-1\">\r\n\t\t\t\t\t<h2 class=\"grid-row-1\"");
            BeginWriteAttribute("id", " id=\"", 461, "\"", 473, 1);
#nullable restore
#line 13 "D:\Repos\StonksStable\SignalRSample\Pages\Game\Characters.cshtml"
WriteAttributeValue("", 466, nameId, 466, 7, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(">");
#nullable restore
#line 13 "D:\Repos\StonksStable\SignalRSample\Pages\Game\Characters.cshtml"
                                                   Write(Html.DisplayFor(m => kvp.Value.Name));

#line default
#line hidden
#nullable disable
            WriteLiteral("</h2>\r\n\t\t\t\t\t<p class=\"grid-row-2\">");
#nullable restore
#line 14 "D:\Repos\StonksStable\SignalRSample\Pages\Game\Characters.cshtml"
                                     Write(Html.DisplayFor(m => kvp.Value.Description));

#line default
#line hidden
#nullable disable
            WriteLiteral("</p>\r\n\t\t\t\t</div>\r\n");
            WriteLiteral("\t\t\t\t<button");
            BeginWriteAttribute("id", " id=\"", 707, "\"", 721, 1);
#nullable restore
#line 17 "D:\Repos\StonksStable\SignalRSample\Pages\Game\Characters.cshtml"
WriteAttributeValue("", 712, buttonId, 712, 9, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(" class=\"btn btn-primary grid-column-2\">Select</button>\r\n\t\t\t</div>\r\n");
#nullable restore
#line 19 "D:\Repos\StonksStable\SignalRSample\Pages\Game\Characters.cshtml"
		}

#line default
#line hidden
#nullable disable
            WriteLiteral("\t\t<div class=\"character-card-button\">\r\n\t\t\t<button id=\"backButton\" class=\"btn btn-outline-primary menu-button fill\">Back</button>\r\n\t\t</div>\r\n\t</div>\r\n</div>");
        }
        #pragma warning restore 1998
        #nullable restore
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; } = default!;
        #nullable disable
        #nullable restore
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; } = default!;
        #nullable disable
        #nullable restore
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; } = default!;
        #nullable disable
        #nullable restore
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; } = default!;
        #nullable disable
        #nullable restore
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<StonkTrader.Pages.Game.CharactersModel> Html { get; private set; } = default!;
        #nullable disable
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<StonkTrader.Pages.Game.CharactersModel> ViewData => (global::Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<StonkTrader.Pages.Game.CharactersModel>)PageContext?.ViewData;
        public StonkTrader.Pages.Game.CharactersModel Model => ViewData.Model;
    }
}
#pragma warning restore 1591
