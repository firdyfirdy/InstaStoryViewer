using InstaStoryViewer;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.AnonymizeUa;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerSharp;

Console.WriteLine("[+] Instagram Auto view & likes story");
Console.WriteLine();

#region Configuration

var configuration =  new ConfigurationBuilder()
    .AddJsonFile($"config.json", false, true);
var config = configuration.Build();
var browserPath = config["BrowserPath"];
var isLikeStory = Convert.ToBoolean(config["LikeStory"]);

#endregion

#region Puppeteer

var defaultSelectorOptions = new WaitForSelectorOptions {Timeout = 500};
var extra = new PuppeteerExtra().Use(new AnonymizeUaPlugin()).Use(new StealthPlugin());
var options = new LaunchOptions
{
    Headless = false,
    IgnoredDefaultArgs = new[]
    {
        "--ignore-certifcate-errors",
        "--ignore-certifcate-errors-spki-list",
        "--user-agent=\"Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36\""
    },
    ExecutablePath = browserPath
};

var browser = await extra.LaunchAsync(options);
IPage[] pages = await browser.PagesAsync();
var currentPage = pages[0];
await currentPage.SetViewportAsync(new ViewPortOptions
{
    Width = 1280,
    Height = 1024
});
var cookies = File.ReadAllText("cookies.json");

#endregion

await currentPage.SetCookieAsync(JsonConvert.DeserializeObject<CookieParam[]>(cookies));
await currentPage.GoToAsync("https://www.instagram.com/");
await currentPage.WaitForNetworkIdleAsync();
try
{
    await currentPage.WaitForSelectorAsync(ElementConstant.NotificationPopup, defaultSelectorOptions);
    await currentPage.TapAsync(ElementConstant.NotificationPopup);
    Console.WriteLine("[+] Login success");
    while (true)
    {
        try
        {
            await currentPage.WaitForSelectorAsync(ElementConstant.StoriesCheck, defaultSelectorOptions);
            await currentPage.TapAsync(ElementConstant.StoriesCheck);
            await currentPage.WaitForTimeoutAsync(3000);
            while (true)
            {
                try
                {
                    Console.WriteLine($"[+] URL: {currentPage.Url}");
                    Console.Write("[+] View: ");
                    try
                    {
                        var nextStory = await currentPage.WaitForSelectorAsync(ElementConstant.StoriesNext, defaultSelectorOptions);
                        Console.Write("OK!");
                        
                        if (isLikeStory)
                        {
                            Console.Write(" | Like: ");
                            try
                            {
                                var likeStory = await currentPage.WaitForSelectorAsync(ElementConstant.LikeStory, defaultSelectorOptions);
                                await likeStory.TapAsync();
                                Console.Write("OK!");
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error!");
                            }
                        }
                        await nextStory.TapAsync();
                    }
                    catch (Exception e)
                    {
                        Console.Write("Error: Next story not found");
                    }
                    Console.WriteLine();
                    await currentPage.WaitForTimeoutAsync(3000);
                }
                catch (Exception e)
                {
                    Console.WriteLine("[+] Stories finished viewing. Reloading...");
                    await currentPage.ReloadAsync();
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("[!] No stories found...");
            await currentPage.ReloadAsync();
        }
    }
}catch(Exception ex)
{
    Console.WriteLine("[!] Login failed...");
}