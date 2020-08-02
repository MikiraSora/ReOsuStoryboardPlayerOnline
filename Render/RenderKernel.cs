using Blazor.Extensions.Canvas.WebGL;
using OpenToolkit.Mathematics;
using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.PrimitiveValue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ReOsuStoryboardPlayerOnline.Render
{
    public static class RenderKernel
    {
        private static bool additiveTrigger = false;
        private static bool init = false;
        private static DefaultShader shader = new DefaultShader();

        public const float SB_WIDTH = 640f, SB_WIDE_WIDTH = SB_WIDTH + 2 * 107, SB_HEIGHT = 480f;

        private static WebGLBuffer vertexBuffer;
        private static WebGLBuffer texBuffer;
        private static Dictionary<string, TextureResource> textureResourceMap;

        public static Matrix4 CameraViewMatrix { get; set; } = Matrix4.Identity;
        public static Matrix4 ProjectionMatrix { get; set; } = Matrix4.Identity;

        public static void Init(WebGLContext gl,int viewWidth,int viewHeight)
        {
            ProjectionMatrix = Matrix4.Identity * Matrix4.CreateOrthographic(viewWidth, viewHeight, -1, 1);

            //todo 对渲染进行初始化，比如说初始化着色器，顶点等。
            shader.Build(gl);

            InitStaticBuffer(gl);

            init = true;
        }

        private static async void InitStaticBuffer(WebGLContext gl)
        {
            vertexBuffer = await gl.CreateBufferAsync();
            await gl.BindBufferAsync(BufferType.ARRAY_BUFFER, vertexBuffer);
            {
                var vertexArrayDef = new[]{
                -0.5f, 0.5f,
                 0.5f, 0.5f,
                 0.5f, -0.5f,
                -0.5f, -0.5f,};

                await gl.BufferDataAsync(BufferType.ARRAY_BUFFER, vertexArrayDef, BufferUsageHint.STATIC_DRAW);
                await gl.EnableVertexAttribArrayAsync(shader.PositionAttributeLocaltion);
                await gl.VertexAttribPointerAsync(shader.PositionAttributeLocaltion, 2, DataType.FLOAT, false, sizeof(float) * 2, 0);
            }

            texBuffer = await gl.CreateBufferAsync();
            await gl.BindBufferAsync(BufferType.ARRAY_BUFFER, texBuffer);
            {
                var texArrayDef = new[]
                {
                     0,0,
                     1,0,
                     1,1,
                     0,1
                };

                await gl.BufferDataAsync(BufferType.ARRAY_BUFFER, texArrayDef, BufferUsageHint.STATIC_DRAW);
                await gl.EnableVertexAttribArrayAsync(shader.TexturePositionAttributeLocaltion);
                await gl.VertexAttribPointerAsync(shader.TexturePositionAttributeLocaltion, 2, DataType.FLOAT, false, sizeof(float) * 2, 0);
            }
        }

        private static float[] vpBuffer = new float[16];

        public static unsafe void BeforeDraw(WebGLContext gl)
        {
            shader.UseProgramAsync(gl);

            var VP = CameraViewMatrix * ProjectionMatrix;

            fixed (float* ptr = &vpBuffer[0])
            {
                Unsafe.CopyBlock(ptr, &VP.Row0.X, 16 * sizeof(float));
            }

            shader.UpdateViewProjection(gl, false, vpBuffer);
        }

        public static async void AfterDraw(WebGLContext gl)
        {
            var error = await gl.GetErrorAsync();
            Debug.Assert(error == Error.NO_ERROR,"DEBUG : OpenGL got error after rendering. ERROR = " + error);
        }

        public static async void ApplyRenderResource(WebGLContext gl,Dictionary<string, TextureResource> textureResourceMap)
        {
            if (RenderKernel.textureResourceMap != null)
                foreach (var resource in RenderKernel.textureResourceMap.Values.Where(x => x.IsValid))
                    await gl.DeleteTextureAsync(resource.Texture);

            RenderKernel.textureResourceMap = textureResourceMap;
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

        private static float[] martrix3Buffer = new float[3 * 3];

        public static async void DrawObject(WebGLContext gl, StoryboardObject obj)
        {
            if (!textureResourceMap.TryGetValue(obj.ImageFilePath, out var textureResource))
                return;

            ChangeAdditiveStatus(gl, obj.IsAdditive);

            var is_xflip = Math.Sign(obj.Scale.X);
            var is_yflip = Math.Sign(obj.Scale.Y);

            //adjust scale transform which value is negative
            var horizon_flip = obj.IsHorizonFlip | (is_xflip < 0);
            var vertical_flip = obj.IsHorizonFlip | (is_yflip < 0);
            float scalex = is_xflip * obj.Scale.X * textureResource.Size.Width;
            float scaley = is_yflip * obj.Scale.Y * textureResource.Size.Height;

            shader.UpdateColor(gl, obj.Color.X, obj.Color.Y, obj.Color.Z, obj.Color.W);
            shader.UpdateFlip(gl, horizon_flip ? -1 : 1, vertical_flip ? -1 : 1);

            //anchor
            shader.UpdateAnchor(gl, obj.OriginOffset.X, obj.OriginOffset.Y);

            //Create ModelMatrix
            Matrix3 model = Matrix3.Zero;
            float cosa = (float)Math.Cos(obj.Rotate);
            float sina = (float)Math.Sin(obj.Rotate);
            model.Row0.X = cosa * scalex;
            model.Row0.Y = -sina * scalex;
            model.Row1.X = sina * scaley;
            model.Row1.Y = cosa * scaley;

            model.Row2.X = obj.Postion.X - SB_WIDTH / 2f;
            model.Row2.Y = -obj.Postion.Y + SB_HEIGHT / 2f;

            unsafe
            {
                fixed (float* ptr = &martrix3Buffer[0])
                {
                    Unsafe.CopyBlock(ptr, &model.Row0.X, 9 * sizeof(float));
                }
            }

            shader.UpdateModel(gl, false, martrix3Buffer);

            shader.UpdateTexture(gl, textureResource.Texture);
            await gl.DrawArraysAsync(Primitive.TRIANGLE_FAN, 0, 4);
        }
    }
}
