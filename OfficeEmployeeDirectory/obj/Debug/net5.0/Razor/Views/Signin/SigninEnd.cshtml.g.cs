#pragma checksum "C:\projects\AI\coder\blogs\OfficeEmployeeDirectory\Views\Signin\SigninEnd.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "d6a01380ad78c8cb5828f2e8fe1c26e4b866f26f"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Signin_SigninEnd), @"mvc.1.0.razor-page", @"/Views/Signin/SigninEnd.cshtml")]
namespace AspNetCore
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
#line 1 "C:\projects\AI\coder\blogs\OfficeEmployeeDirectory\Views\_ViewImports.cshtml"
using OfficeEmployeeDirectory;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "C:\projects\AI\coder\blogs\OfficeEmployeeDirectory\Views\_ViewImports.cshtml"
using OfficeEmployeeDirectory.Models;

#line default
#line hidden
#nullable disable
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"d6a01380ad78c8cb5828f2e8fe1c26e4b866f26f", @"/Views/Signin/SigninEnd.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"f89b4d8ef1b3b181d070f288717ea6d4d3fbe7d9", @"/Views/_ViewImports.cshtml")]
    public class Views_Signin_SigninEnd : global::Microsoft.AspNetCore.Mvc.RazorPages.Page
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            WriteLiteral("\r\n\r\n<h1>Sign in done</h1>\r\n");
            DefineSection("scripts", async() => {
                WriteLiteral(@"
    <script type=""text/javascript"">
        microsoftTeams.initialize();
        
        localStorage.removeItem(""simple.error"");
        
        let hashParams = getHashParameters();
        
        if (hashParams[""error""]) {
            // Authentication/authorization failed
            localStorage.setItem(""simple.error"", JSON.stringify(hashParams));
            microsoftTeams.authentication.notifyFailure(hashParams[""error""]);
        } else if (hashParams[""access_token""]) {
            // Get the stored state parameter and compare with incoming state
            let expectedState = localStorage.getItem(""simple.state"");
            if (expectedState !== hashParams[""state""]) {
                // State does not match, report error
                localStorage.setItem(""simple.error"", JSON.stringify(hashParams));
                microsoftTeams.authentication.notifyFailure(""StateDoesNotMatch"");
            } else {
                // Success -- return token information to the parent page");
                WriteLiteral(@"
                microsoftTeams.authentication.notifySuccess({
                    idToken: hashParams[""id_token""],
                    accessToken: hashParams[""access_token""],
                    tokenType: hashParams[""token_type""],
                    expiresIn: hashParams[""expires_in""]
                });
            }
        } else {
            // Unexpected condition: hash does not contain error or access_token parameter
            localStorage.setItem(""simple.error"", JSON.stringify(hashParams));
            microsoftTeams.authentication.notifyFailure(""UnexpectedFailure"");
        }


        // Parse hash parameters into key-value pairs
        function getHashParameters() {
            let hashParams = {};
            location.hash.substr(1).split(""&"").forEach(function (item) {
                let s = item.split(""=""),
                    k = s[0],
                    v = s[1] && decodeURIComponent(s[1]);
                hashParams[k] = v;
            });
            return ha");
                WriteLiteral("shParams;\r\n        }\r\n    </script>\r\n");
            }
            );
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<OfficeEmployeeDirectory.Views.Signin.SigninEndModel> Html { get; private set; }
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<OfficeEmployeeDirectory.Views.Signin.SigninEndModel> ViewData => (global::Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<OfficeEmployeeDirectory.Views.Signin.SigninEndModel>)PageContext?.ViewData;
        public OfficeEmployeeDirectory.Views.Signin.SigninEndModel Model => ViewData.Model;
    }
}
#pragma warning restore 1591
