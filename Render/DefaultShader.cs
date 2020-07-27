﻿using Blazor.Extensions.Canvas.WebGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReOsuStoryboardPlayerOnline.Render
{
    public class DefaultShader
    {
        public DefaultShader()
        {
            VertexProgramString = @"
                #version 330
                out vec4 varying_color;
                out vec2 varying_texPos;

                uniform mat4 ViewProjection;

                layout(location=0) in vec2 in_texPos;
                layout(location=1) in vec2 in_pos;

                layout(location=2) in vec2 in_anchor;
                layout(location=3) in vec4 in_color;
                layout(location=4) in vec2 in_flip;
                layout(location=5) in mat3x2 in_model;

                void main(){
                    vec2 v = in_model*vec3(in_pos*in_flip-in_anchor,1.0);
	                gl_Position=ViewProjection*vec4(v.x,v.y,0.0,1.0);
	                varying_color=in_color;
	                varying_texPos=in_texPos;
                }
                ";
            FragmentProgramString = @"
                #version 330

                uniform sampler2D diffuse;

                in vec4 varying_color;
                in vec2 varying_texPos;

                out vec4 out_color;

                void main(){
	                vec4 texColor=texture(diffuse,varying_texPos);
	                out_color=(varying_color*texColor);
                }
                ";
        }

        public string VertexProgramString { get; }
        public string FragmentProgramString { get; }

        public WebGLProgram ShaderProgram { get; private set; }

        public void Build(WebGLContext gl)
        {
            InitProgramAsync(gl, VertexProgramString, FragmentProgramString);
        }

        private async void InitProgramAsync(WebGLContext gl, string vsSource, string fsSource)
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
                throw new Exception("An error occured while compiling the shader: " + info);
            }

            return shader;
        }
    }
}
