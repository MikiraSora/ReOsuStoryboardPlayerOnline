using Blazor.Extensions.Canvas.WebGL;
using ReOsuStoryboardPlayer.Core.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ReOsuStoryboardPlayerOnline.Render
{
    public static class RenderKernel
    {
        private static bool additiveTrigger = false;
        private static bool init = false;

        private static DefaultShader shader = new DefaultShader();

        public static void Init(WebGLContext gl)
        {
            //todo 对渲染进行初始化，比如说初始化着色器，顶点等。
            shader.Build(gl);

            init = true;
        }

        public static async void BeforeDraw(WebGLContext gl)
        {
            var program = shader.ShaderProgram;
            await gl.UseProgramAsync(program);

            //todo
        }

        public static async void AfterDraw(WebGLContext gl)
        {
            var error = await gl.GetErrorAsync();
            Debug.Assert(error == Error.NO_ERROR,"DEBUG : OpenGL got error after rendering. ERROR = " + error);
        }

        public static void Render(WebGLContext gl, List<StoryboardObject> updatingStoryboardObjects)
        {
            if (!init)
                return;

            BeforeDraw(gl);

            foreach (var obj in updatingStoryboardObjects)
            {
                if (!obj.IsVisible)
                    continue;

                DrawObject(gl,obj);
            }

            ChangeAdditiveStatus(gl,false);
            AfterDraw(gl);
        }

        private static async void ChangeAdditiveStatus(WebGLContext gl,bool isAdditiveBlend)
        {
            if (additiveTrigger == isAdditiveBlend)
                return;
            additiveTrigger = isAdditiveBlend;
            await gl.BlendFuncAsync(BlendingMode.SRC_ALPHA, additiveTrigger ? BlendingMode.ONE : BlendingMode.ONE_MINUS_SRC_ALPHA);
        }

        public static async void DrawObject(WebGLContext gl,StoryboardObject obj)
        {
            ChangeAdditiveStatus(gl,obj.IsAdditive);

            //todo 实质对物件进行渲染,一次drawcall
        }
    }
}
