#pragma checksum "C:\projects\AI\coder\blogs\OfficeEmployeeDirectory\Views\Home\Index.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "2c0186f6ffaa354e93d06d3fdcd45e582f3b2132"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Home_Index), @"mvc.1.0.view", @"/Views/Home/Index.cshtml")]
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
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"2c0186f6ffaa354e93d06d3fdcd45e582f3b2132", @"/Views/Home/Index.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"f89b4d8ef1b3b181d070f288717ea6d4d3fbe7d9", @"/Views/_ViewImports.cshtml")]
    public class Views_Home_Index : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<dynamic>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#nullable restore
#line 1 "C:\projects\AI\coder\blogs\OfficeEmployeeDirectory\Views\Home\Index.cshtml"
  
    ViewData["Title"] = "Employee Directory";

#line default
#line hidden
#nullable disable
            WriteLiteral(@"
<style>
    .card{
        width:400px;
        height:200px;
        border:1px solid black;
        margin-bottom: 20px;
        padding: 5px;
    }
</style>
<h1>Employee Directory</h1>

    <label id=""jsonResponse""></label>
</div>

<script>

window.addEventListener(""load"", function(){
     microsoftTeams.initialize();

    authenticateParams = {
        successCallback: function(result){
            var token = result[""idToken""];
            var access_token = result[""accessToken""];
            GetEmployeeDirectory(token);
        },
        failureCallback: function(reason){
            alert(""failed: "" + reason);
        },
        height: 200,
        width: 200,
        url: """);
#nullable restore
#line 35 "C:\projects\AI\coder\blogs\OfficeEmployeeDirectory\Views\Home\Index.cshtml"
         Write(ViewBag.SignInUrl);

#line default
#line hidden
#nullable disable
            WriteLiteral(@"""
    }

    microsoftTeams.authentication.authenticate(authenticateParams);

});
    


    function GetEmployeeDirectory(token){
            fetch(""/Home/GetDirectory"", {
                method: ""GET"",
                headers: {
                    ""content-type"": ""application/json;odata=verbose"",
                    ""Accept"": ""application/json; odata=verbose"",
                    ""Authorization"": ""Bearer "" + token
                }
            })
            .then(response =>response.json())
            .then(results =>{
                console.log(results);   
                 let html = """";

                 for(var i = 0; i < results.length; i++){
                     let div = document.createElement(""div"")
                     div.className = ""card""
                     div.append(results[i].displayName);
                     div.append(document.createElement(""br""))
                     div.append( results[i].mail)
                     div.append(document.createElement(""br""");
            WriteLiteral(@"))
                     div.append(results[i].businessPhones.length > 0 ? results[i].businessPhones[0] : document.createElement(""label""))
                     document.getElementById(""jsonResponse"").append(div);
                 }
            });
    }
    
</script>
");
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
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<dynamic> Html { get; private set; }
    }
}
#pragma warning restore 1591