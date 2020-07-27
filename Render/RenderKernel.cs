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

        private static DefaultShader shader = new DefaultShader();

        public static void Init(WebGLContext gl)
        {
            shader.Build(gl);
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
            BeforeDraw(gl);

            foreach (var obj in updatingStoryboardObjects)
            {
                if (!obj.IsVisible)
                    continue;

                //todo 多实例渲染?
                DrawObject(gl,obj);
            }

            //reset
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

        }
    }
}
