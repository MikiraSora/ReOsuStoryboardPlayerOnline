using Blazor.Extensions;
using Blazor.Extensions.Canvas;
using Blazor.Extensions.Canvas.WebGL;
using Microsoft.AspNetCore.Components;
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

        private WebGLContext GLContext { get; set; }

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

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            this.GLContext = await this.RefCanvas.CreateWebGLAsync(new WebGLContextAttributes
            {
                PowerPreference = WebGLContextAttributes.POWER_PREFERENCE_HIGH_PERFORMANCE
            });

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
        }

        private async Task<WebGLProgram> InitProgramAsync(WebGLContext gl, string vsSource, string fsSource)
        {
            var vertexShader = await this.LoadShaderAsync(gl, ShaderType.VERTEX_SHADER, vsSource);
            var fragmentShader = await this.LoadShaderAsync(gl, ShaderType.FRAGMENT_SHADER, fsSource);

            var program = await gl.CreateProgramAsync();
            await gl.AttachShaderAsync(program, vertexShader);
            await gl.AttachShaderAsync(program, fragmentShader);
            await gl.LinkProgramAsync(program);

            await gl.DeleteShaderAsync(vertexShader);
            await gl.DeleteShaderAsync(fragmentShader);

            if (!await gl.GetProgramParameterAsync<bool>(program, ProgramParameter.LINK_STATUS))
            {
                string info = await gl.GetProgramInfoLogAsync(program);
                throw new Exception("An error occured while linking the program: " + info);
            }

            return program;
        }

        private async Task<WebGLShader> LoadShaderAsync(WebGLContext gl, ShaderType type, string source)
        {
            var shader = await gl.CreateShaderAsync(type);

            await gl.ShaderSourceAsync(shader, source);
            await gl.CompileShaderAsync(shader);

            if (!await gl.GetShaderParameterAsync<bool>(shader, ShaderParameter.COMPILE_STATUS))
            {
                string info = await gl.GetShaderInfoLogAsync(shader);
                await gl.DeleteShaderAsync(shader);
                throw new Exception("An error occured while compiling the shader: " + info);
            }

            return shader;
        }

        public void Play()
        {
            Console.WriteLine("Play!");
            Task.Run(onLoop);
        }

        private async void onLoop()
        {
            while (true)
            {
                onUpdate();
                onRender();
                await Task.Yield();
            }
        }

        private void onUpdate()
        {

        }
        private void onRender()
        {

        }
    }
}
