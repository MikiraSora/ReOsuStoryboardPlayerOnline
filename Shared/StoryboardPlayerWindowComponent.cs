using Blazor.Extensions;
using Blazor.Extensions.Canvas;
using Blazor.Extensions.Canvas.WebGL;
using Microsoft.AspNetCore.Components;
using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Kernel;
using ReOsuStoryboardPlayer.Core.Utils;
using ReOsuStoryboardPlayerOnline.IO;
using ReOsuStoryboardPlayerOnline.MusicPlayer;
using ReOsuStoryboardPlayerOnline.Render;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace ReOsuStoryboardPlayerOnline.Shared
{
    public class StoryboardPlayerWindowComponent : ComponentBase
    {
        public BECanvasComponent RefCanvas { get; set; }

        [Parameter]
        public long Width { get; set; }

        [Parameter]
        public long Height { get; set; }

        [Parameter]
        public IMusicPlayer Player { get; set; }

        private WebGLContext glContext;
        private StoryboardUpdater storyboardUpdater;

        private CancellationTokenSource currentLoopCancelSource;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            glContext = await RefCanvas.CreateWebGLAsync(new WebGLContextAttributes
            {
                PowerPreference = WebGLContextAttributes.POWER_PREFERENCE_HIGH_PERFORMANCE
            });

            RenderKernel.Init(glContext,800,600);
        }

        public async void PrepareRenderResource(StoryboardUpdater updater,IDirectoryReader reader)
        {
            storyboardUpdater = updater;

            var textureResourceMap = new Dictionary<string, TextureResource>();

            foreach (var obj in updater.StoryboardObjectList)
            {
                switch (obj)
                {
                    case StoryboardBackgroundObject background:
                        var resource = await _get(obj.ImageFilePath);

                        if (resource.IsValid)
                            background.AdjustScale(resource.Size.Height);
                        else
                            Log.Warn($"not found image:{obj.ImageFilePath}");

                        break;
                    case StoryboardAnimation animation:
                        for (int index = 0; index < animation.FrameCount; index++)
                        {
                            string path = animation.FrameBaseImagePath + index + animation.FrameFileExtension;

                            if (!(await _get(path)).IsValid)
                            {
                                Log.Warn($"not found image:{path}");
                                continue;
                            }
                        }
                        break;
                    default:
                        if (!(await _get(obj.ImageFilePath)).IsValid)
                            Log.Warn($"not found image:{obj.ImageFilePath}");
                        break;
                }
            }

            RenderKernel.ApplyRenderResource(glContext, textureResourceMap);

            async Task<TextureResource> _get(string image_name)
            {
                var fix_image = image_name;
                //for Flex
                if (string.IsNullOrWhiteSpace(Path.GetExtension(fix_image)))
                    fix_image += ".png";

                if (textureResourceMap.TryGetValue(image_name, out var resource))
                    return resource;

                //load
                string file_path = fix_image;

                resource = await _load_tex(file_path);

                if (!resource.IsValid)
                {
                    //todo: 从皮肤文件夹获取皮肤文件 file_path = Path.Combine(PlayerSetting.UserSkinPath ?? string.Empty, fix_image);

                    /*
                    if (!_load_tex(file_path, out resource))
                    {
                        if ((!image_name.EndsWith("-0")) && _get(image_name + "-0", out resource))
                            return true;
                    }*/
                }

                if (resource.IsValid)
                {
                    textureResourceMap[image_name] = resource;
                    Log.Debug($"Created Storyboard sprite instance from image file :{fix_image}");
                }

                return resource;
            }

            async Task<TextureResource> _load_tex(string file_path)
            {
                try
                {
                    var stream = reader.ReadFile(file_path);

                    if (stream is null)
                        return default;

                    var texture = await glContext.CreateTextureAsync();

                    using var image = await Image.LoadAsync<Rgba32>(stream);

                    await glContext.BindTextureAsync(TextureType.TEXTURE_2D, texture);

                    await glContext.TexParameterAsync(TextureType.TEXTURE_2D, TextureParameter.TEXTURE_MIN_FILTER, 9729);
                    await glContext.TexParameterAsync(TextureType.TEXTURE_2D, TextureParameter.TEXTURE_MAG_FILTER, 9729);
                    await glContext.TexParameterAsync(TextureType.TEXTURE_2D, TextureParameter.TEXTURE_WRAP_S, 33071);
                    await glContext.TexParameterAsync(TextureType.TEXTURE_2D, TextureParameter.TEXTURE_WRAP_T, 33071);

                    await glContext.TexImage2DAsync(Texture2DType.TEXTURE_2D, 0, PixelFormat.RGBA, image.Width, image.Height,
                        PixelFormat.RGBA, PixelType.UNSIGNED_BYTE, MemoryMarshal.AsBytes(image.GetPixelRowSpan(0)).ToArray());

                    var size = new System.Drawing.Size(image.Width,image.Height);

                    return new TextureResource(size,texture);
                }
                catch (Exception e)
                {
                    Log.Warn($"Load texture \"{file_path}\" failed : {e.Message}");
                    return default;
                }
            }
        }

        public void Play()
        {
            Console.WriteLine("Play!");
            currentLoopCancelSource = new CancellationTokenSource();
            Player.Play();
            Loop(currentLoopCancelSource.Token);
        }

        private async void Loop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (storyboardUpdater != null)
                {
                    OnUpdate();
                    OnRender();
                }

                await Task.Yield();
            }
        }

        private void OnUpdate()
        {
            var currentTime = Player.GetCurrentTime();
            storyboardUpdater.Update(currentTime);
        }

        private void OnRender()
        {
            RenderKernel.Render(glContext, storyboardUpdater.UpdatingStoryboardObjects);
        }
    }
}
