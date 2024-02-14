using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient(); // DI登録

var app = builder.Build();
app.UseStaticFiles();

app.MapGet("/", async context =>
{
    var httpClient = context.RequestServices.GetRequiredService<HttpClient>(); // DIから
    HttpResponseMessage response = await httpClient.GetAsync("https://www.meijiyasuda.co.jp/");
    var tenantID = response.Headers.GetValues("Date").FirstOrDefault();
    // var tenantID = response.Headers.GetValues("X-MS-CLIENT-PRINCIPAL-ID").FirstOrDefault();
    //tenantID = "期待ID";
    // nullか所定のテナントIDでなければfalse
    bool isValidTenantId = !string.IsNullOrEmpty(tenantID) && IsValidTenantId(tenantID, "期待ID");
    string filePath = isValidTenantId ? GetStaticHtmlFilePath() : GetForbiddenHtmlFilePath();
    await context.Response.SendFileAsync(filePath);
});

app.Run();

/// <summary>
///  指定されたテナントIDが有効かどうかをチェックします。
/// </summary>
/// <param name="tenantId">チェックするテナントID</param>
/// <param name="expectedTenantId">期待されるテナントID</param>
/// <returns>テナントIDが有効であればtrue、それ以外の場合はfalse</returns>
bool IsValidTenantId(string tenantId, string expectedTenantId)
{
    if (tenantId == null)
    {
        return false;
    }
    return tenantId.Equals(expectedTenantId);
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