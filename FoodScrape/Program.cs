using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Playwright;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/test", async () =>
{
    const string LocalStoreCache = @"{""store"":{""id"":""10642"",""browseId"":""10642"",""name"":""University Station""},""pieBrowseType"":""my-store"",""isUSStore"":true}";
    using var playwright = await Playwright.CreateAsync();

    await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions()
    {
        Headless = true
    });

    var page = await browser.NewPageAsync();

    await page.GotoAsync("https://www.wholefoodsmarket.com");

    await page.EvaluateAsync($@"() => {{
            sessionStorage.setItem('wfm_pie_browse', '{LocalStoreCache}');
        }}");

    await page.GotoAsync("https://www.wholefoodsmarket.com/product/bobs-red-mill-1-to-1-gluten-free-baking-flour-44-oz-b07fxyj5nt");

    // Find the first span element with the class "test-class", then extract the text
    var elementHandle = await page.QuerySelectorAsync("span.regular_price");
    var elementText = await elementHandle.TextContentAsync();

    Console.WriteLine($"Found element text: {elementText}");

    return new JsonResult(elementText);
})
.WithName("Test")
.WithOpenApi();

app.Run();