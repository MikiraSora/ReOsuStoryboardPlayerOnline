using Blazor.Extensions.Canvas.WebGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ReOsuStoryboardPlayerOnline.Render
{
    public class DefaultShader
    {
        private WebGLUniformLocation ViewProjectionLocation;
        private WebGLUniformLocation AnchorLocation;
        private WebGLUniformLocation ColorLocation;
        private WebGLUniformLocation FilpLocation;
        private WebGLUniformLocation ModelLocation;

        public DefaultShader()
        {
            VertexProgramString = @"
                varying vec4 varying_color;
                varying vec2 varying_texPos;

                uniform mat4 ViewProjection;
                uniform vec2 in_anchor;
                uniform vec4 in_color;
                uniform vec2 in_flip;
                uniform mat3 in_model;

                attribute vec2 in_texPos;
                attribute vec2 in_pos;

                void main(){
                    vec3 v = in_model*vec3(in_pos*in_flip-in_anchor,1.0);
	                gl_Position=ViewProjection*vec4(v.x,v.y,0.0,1.0);
	                varying_color=in_color;
	                varying_texPos=in_texPos;
                }
                ";
            FragmentProgramString = @"
                precision mediump float;
                uniform sampler2D diffuse;

                varying mediump vec4 varying_color;
                varying mediump vec2 varying_texPos;

                void main(){
	                vec4 texColor = texture2D(diffuse,varying_texPos);
	                gl_FragColor = (varying_color*texColor);
                }
                ";
        }

        public string VertexProgramString { get; private set; }
        public string FragmentProgramString { get; private set; }

        public uint TexturePositionAttributeLocaltion { get; private set; }
        public uint PositionAttributeLocaltion { get; private set; }

        public WebGLProgram ShaderProgram { get; private set; }

        public async void Build(WebGLContext gl)
        {
            await InitProgramAsync(gl, VertexProgramString, FragmentProgramString);

            ViewProjectionLocation = await gl.GetUniformLocationAsync(ShaderProgram, "ViewProjection");
            AnchorLocation = await gl.GetUniformLocationAsync(ShaderProgram, "in_anchor");
            ColorLocation = await gl.GetUniformLocationAsync(ShaderProgram, "in_color");
            FilpLocation = await gl.GetUniformLocationAsync(ShaderProgram, "in_flip");
            ModelLocation = await gl.GetUniformLocationAsync(ShaderProgram, "in_model");

            PositionAttributeLocaltion = (uint)await gl.GetAttribLocationAsync(ShaderProgram, "in_pos");
            TexturePositionAttributeLocaltion = (uint)await gl.GetAttribLocationAsync(ShaderProgram, "in_texPos");
        }

        private async Task InitProgramAsync(WebGLContext gl, string vsSource, string fsSource)
        {
            var vertexShader = await LoadShaderAsync(gl, ShaderType.VERTEX_SHADER, vsSource);
            var fragmentShader = await LoadShaderAsync(gl, ShaderType.FRAGMENT_SHADER, fsSource);

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

            Console.WriteLine($"shader program : id = {program.Id} , type = {program.WebGLType}");
            ShaderProgram = program;
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
                throw new Exception($"An error occured while compiling the {type} shader: " + info);
            }

            return shader;
        }

        public Task UseProgramAsync(WebGLContext gl) => gl.UseProgramAsync(ShaderProgram);

        public Task UpdateViewProjection(WebGLContext gl,bool transpose,float[] matrixArray) => 
            gl.UniformMatrixAsync(ViewProjectionLocation, transpose, matrixArray);

        public Task UpdateModel(WebGLContext gl, bool transpose, float[] matrixArray) =>
            gl.UniformMatrixAsync(ModelLocation, transpose, matrixArray);

        public Task UpdateAnchor(WebGLContext gl,params float[] matrixArray) =>
            gl.UniformAsync(AnchorLocation, matrixArray);

        public Task UpdateColor(WebGLContext gl, params float[] matrixArray) =>
            gl.UniformAsync(ColorLocation, matrixArray);

        public Task UpdateFlip(WebGLContext gl, params float[] matrixArray) =>
            gl.UniformAsync(FilpLocation, matrixArray);

        public Task UpdateTexture(WebGLContext gl, WebGLTexture texture) =>
            gl.BindTextureAsync(TextureType.TEXTURE_2D, texture);
    }
}
