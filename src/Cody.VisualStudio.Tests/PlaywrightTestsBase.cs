using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;
using Microsoft.Playwright;
using Xunit;

namespace Cody.VisualStudio.Tests
{
    public abstract class PlaywrightTestsBase: TestsBase
    {
        protected string CdpAddress = $"http://127.0.0.1:{9222}";
        
        protected IPlaywright Playwright;
        protected IBrowser Browser;
        
        protected IBrowserContext Context;

        protected IPage Page;

        private async Task InitializeAsync()
        {
            CodyPackage = await GetPackageAsync();
            CodyPackage.Logger.Debug("CodyPackage loaded.");

            await WaitForChat();
            CodyPackage.Logger.Debug("Chat initialized and loaded.");

            Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            Browser = await Playwright.Chromium.ConnectOverCDPAsync(CdpAddress);

            CodyPackage.Logger.Debug("Playwright initialized.");

            Context = Browser.Contexts[0];
            Page = Context.Pages[0];
        }

        protected async Task WaitForPlaywrightAsync()
        {
            await InitializeAsync();
        }

        protected async Task ShowChatTab() => await Page.GetByTestId("tab-chat").ClickAsync();

        protected async Task ShowHistoryTab() => await Page.GetByTestId("tab-history").ClickAsync();

        protected async Task ShowPromptsTab() => await Page.GetByTestId("tab-prompts").ClickAsync();

        protected async Task ShowAccountTab() => await Page.GetByTestId("tab-account").ClickAsync();

        protected async Task ClickSend() => await Page.GetByTitle("Send").ClickAsync();

        protected async Task EnterChatText(string prompt)
        {
            await Page.Locator("[data-keep-toolbar-open=true]").PressSequentiallyAsync(prompt);
        }

        protected async Task<IReadOnlyCollection<ContextTag>> GetChatContextTags()
        {
            var tagsList = new List<ContextTag>();

            var chatBox = await Page.QuerySelectorAsync("[aria-label='Chat message']");
            var list = await chatBox.QuerySelectorAllAsync("span[data-lexical-decorator='true']");
            foreach (var item in list)
            {
                var tag = new ContextTag();
                var content = await item.TextContentAsync();
                var parts = content.Split(':');
                tag.Name = parts.First();
                if(parts.Length > 1)
                {
                    var lines = parts[1].Split('-');
                    if(lines.Length > 1)
                    {
                        tag.StartLine = int.Parse(lines[0]);
                        tag.EndLine = int.Parse(lines[1]);
                    }
                    else
                    {
                        tag.StartLine = int.Parse(parts[1]);
                        tag.EndLine = int.Parse(parts[1]);
                    }
                }

                tagsList.Add(tag);
            }

            return tagsList;
        }
    }

    public class ContextTag
    {
        public string Name { get; set; }

        public int? StartLine { get; set; }

        public int? EndLine { get; set; }
    }
}
