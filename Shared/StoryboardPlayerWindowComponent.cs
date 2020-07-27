using Blazor.Extensions;
using Blazor.Extensions.Canvas;
using Blazor.Extensions.Canvas.WebGL;
using Microsoft.AspNetCore.Components;
using ReOsuStoryboardPlayer.Core.Kernel;
using ReOsuStoryboardPlayerOnline.MusicPlayer;
using ReOsuStoryboardPlayerOnline.Render;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        private WebGLContext GLContext { get; set; }
        private StoryboardUpdater StoryboardUpdater { get; set; }

        private const string VS_SOURCE = "attribute vec3 aPos;" +
                                         "attribute vec3 aColor;" +
                                         "varying vec3 vColor;" +

                                         "void main() {" +
                                            "gl_Position = vec4(aPos, 1.0);" +
                                            "vColor = aColor;" +
                                         "}";

        private const string FS_SOURCE = "precision mediump float;" +
                                         "varying vec3 vColor;" +

                                         "void main() {" +
                                            "gl_FragColor = vec4(vColor, 1.0);" +
                                         "}";
        private CancellationTokenSource currentLoopCancelSource;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            GLContext = await this.RefCanvas.CreateWebGLAsync(new WebGLContextAttributes
            {
                PowerPreference = WebGLContextAttributes.POWER_PREFERENCE_HIGH_PERFORMANCE
            });

            RenderKernel.Init(GLContext);

            /* 示例代码
            await this.GLContext.ClearColorAsync(0, 0, 0, 1);
            await this.GLContext.ClearAsync(BufferBits.COLOR_BUFFER_BIT);

            var program = await this.InitProgramAsync(this.GLContext, VS_SOURCE, FS_SOURCE);

            var vertexBuffer = await this.GLContext.CreateBufferAsync();
            await this.GLContext.BindBufferAsync(BufferType.ARRAY_BUFFER, vertexBuffer);

            var vertices = new[]
            {
                -0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 0.0f,
                0.5f, -0.5f, 0.0f, 0.0f, 1.0f, 0.0f,
                0.0f,  0.5f, 0.0f, 0.0f, 0.0f, 1.0f
            };
            await this.GLContext.BufferDataAsync(BufferType.ARRAY_BUFFER, vertices, BufferUsageHint.STATIC_DRAW);

            await this.GLContext.VertexAttribPointerAsync(0, 3, DataType.FLOAT, false, 6 * sizeof(float), 0);
            await this.GLContext.VertexAttribPointerAsync(1, 3, DataType.FLOAT, false, 6 * sizeof(float), 3 * sizeof(float));
            await this.GLContext.EnableVertexAttribArrayAsync(0);
            await this.GLContext.EnableVertexAttribArrayAsync(1);

            await this.GLContext.UseProgramAsync(program);

            await this.GLContext.DrawArraysAsync(Primitive.TRIANGLES, 0, 3);
            */
        }

        public void RunInstance(StoryboardUpdater updater)
        {
            StoryboardUpdater = updater;
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
                if (StoryboardUpdater != null)
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
            StoryboardUpdater.Update(currentTime);
        }

        private void OnRender()
        {
            RenderKernel.Render(GLContext, StoryboardUpdater.UpdatingStoryboardObjects);
        }
    }
}
