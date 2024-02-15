using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.IO;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
//builder.Services.AddHttpClient(); // DI登録

var app = builder.Build();
app.UseStaticFiles();

app.MapGet("/", async context =>
{
    context.Response.Headers.TryGetValue("X-MS-CLIENT-PRINCIPAL-NAME",out var principalName);
    var domainName = GetDomainNameFromString(principalName);
    //domainName = "期待ドメイン名";
    // nullか所定のドメイン名でなければfalse
    bool isValidDomainName = !string.IsNullOrEmpty(domainName) && IsValidDomainName(domainName, "期待ドメイン名");
    string filePath = isValidDomainName ? GetStaticHtmlFilePath() : GetForbiddenHtmlFilePath();
    await context.Response.SendFileAsync(filePath);
});

app.Run();

/// <summary>
///  指定されたドメイン名が有効かどうかをチェックします。
/// </summary>
/// <param name="domainName">チェックするドメイン名</param>
/// <param name="expecteddomainName">期待されるドメイン名</param>
/// <returns>ドメイン名が有効であればtrue、それ以外の場合はfalse</returns>
bool IsValidDomainName(string domainName, string expectedDomainName)
{
    if (domainName == null)
    {
        return false;
    }
    return domainName.Equals(expectedDomainName);
}

/// <summary>
///  AD認証済みのアプリのヘッダーから抽出可能なprincipalNameからドメイン部分を抽出します。
/// </summary>
/// <param name="principalName">ドメインを抽出する対象のprincipalName。</param>
/// <returns>principalNameのドメイン部分、または'@'文字が含まれていない場合は空文字列。</returns>
string GetDomainNameFromString(string principalName)
{
    int indexOfAt = string.IsNullOrEmpty(principalName) ? -1 : principalName.IndexOf('@');
    if (indexOfAt >=  0) {
        return principalName.Substring(indexOfAt +  1);
    }
    return string.Empty;
}

/// <summary>
/// 静的HTMLファイルへのパスを取得します。
/// </summary>
/// <returns>静的HTMLファイルへのパス</returns>
string GetStaticHtmlFilePath()
{
    return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "static.html");
}

/// <summary>
///  403エラーページへのパスを取得します。
/// </summary>
/// <returns>403エラーページへのパス</returns>
string GetForbiddenHtmlFilePath()
{
    return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "403.html");
}
