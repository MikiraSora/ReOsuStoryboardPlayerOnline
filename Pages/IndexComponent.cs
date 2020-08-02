using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Kernel;
using ReOsuStoryboardPlayer.Core.Parser.Collection;
using ReOsuStoryboardPlayer.Core.Parser.CommandParser;
using ReOsuStoryboardPlayer.Core.Parser.Reader;
using ReOsuStoryboardPlayerOnline.IO;
using ReOsuStoryboardPlayerOnline.IO.Utils;
using ReOsuStoryboardPlayerOnline.MusicPlayer;
using ReOsuStoryboardPlayerOnline.Shared;
using ReOsuStoryboardPlayerOnline.Storyboard;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ReOsuStoryboardPlayerOnline.Pages
{
    public class IndexComponent : ComponentBase
    {
        [Inject]
        public NavigationManager NavigationManager { get; set; }
        [Inject]
        public IJSRuntime JSRuntime { get; set; }
        public StoryboardPlayerWindow StoryboardWindow { get; set; }

        public SimplePlayer Player { get; set; } = new SimplePlayer();

        public string Text { get; set; }

        public IDirectoryReader ResourceReader { get; set; }

        protected async override Task OnAfterRenderAsync(bool isFirstRender)
        {
            await JSRuntime.InvokeVoidAsync("doUserSelectDir");

            var queryString = QueryHelpers.ParseQuery(new Uri(NavigationManager.Uri).Query);

            ResourceReader = await BeatmapHelper.LoadNetworkResources(94790);
            /*
            if (queryString.TryGetValue("sid", out var beatmapSetId))
                ResourceReader = await BeatmapHelper.LoadNetworkResources(int.Parse(beatmapSetId.ToString()));
            else
                //尝试发起本地上传文件请求
                ResourceReader = await BeatmapHelper.LoadLocalResources();

            if (ResourceReader is null)
            {
                //错误处理
                return;
            }
            */

            Console.WriteLine("Start to select a .osb file and a .osu file (if it exist.)");
            var osbFilePath = ResourceReader.EnumeratePath("*.osb").FirstOrDefault();
            var osuFilePath = ResourceReader.EnumeratePath("*.osu").FirstOrDefault();

            Console.WriteLine("osu file : " + osuFilePath);
            Console.WriteLine("osb file : " + osbFilePath);

            var updater = StoryboardHelper.ParseStoryboard(ResourceReader.ReadFile(osuFilePath), ResourceReader.ReadFile(osbFilePath));

            Console.WriteLine(updater.StoryboardObjectList.Count);

            StoryboardWindow.PrepareRenderResource(updater, ResourceReader);
            StoryboardWindow.Play();
        }
    }
}
