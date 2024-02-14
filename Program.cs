using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient(); // DI registration
builder.Services.AddHttpContextAccessor();

var app = builder.Build();
app.UseStaticFiles();

app.MapGet("/", async context =>
{
    try
    {
        var domainName = await GetDomainNameFromAuthMe(context);
        var isValidDomainName = IsValidDomainName(domainName, "期待ドメイン");
        var filePath = isValidDomainName ? GetStaticHtmlFilePath() : GetForbiddenHtmlFilePath();
        await context.Response.SendFileAsync(filePath);
    }
    catch (Exception ex)
    {
        await context.Response.WriteAsync($"エラー発生: {ex.Message}");
    }
});

app.Run();

/// <summary>
/// AuthMeサービスからテナントIDを非同期に取得します。
/// </summary>
/// <param name="context">リクエストとレスポンスオブジェクトを含むHTTPコンテキスト。</param>
/// <returns>非同期操作を表すタスク。結果はテナントIDです。</returns>
/// <exception cref="Exception">テナントIDを取得できない場合にスローされます。</exception>
async Task<string> GetDomainNameFromAuthMe(HttpContext context)
{
    const string authMeUrl = "/.auth/me";
    var httpClient = context.RequestServices.GetRequiredService<HttpClient>();
    var response = await httpClient.GetAsync(authMeUrl);

    if (!response.IsSuccessStatusCode)
    {
        throw new Exception($"ドメイン取得失敗: {response.ReasonPhrase}");
    }

    var content = await response.Content.ReadAsStringAsync();
    var jsonDocument = JsonDocument.Parse(content);
    var userId = jsonDocument.RootElement.GetProperty("user_id").GetString();
    var atSymbolPosition = userId.IndexOf('@');

    if (atSymbolPosition >=  0)
    {
        userId = userId.Substring(atSymbolPosition +  1); //'@'以降取り出し
    }

    return userId;
}

/// <summary>
///  指定されたテナントIDが有効かどうかをチェックします。
/// </summary>
/// <param name="domainName">チェックするテナントID</param>
/// <param name="expectedDomainName">期待されるテナントID</param>
/// <returns>テナントIDが有効であればtrue、それ以外の場合はfalse</returns>
bool IsValidDomainName(string domainName, string expectedDomainName)
{
    if (domainName == null)
    {
        return false;
    }
    return domainName.Equals(expectedDomainName);
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
