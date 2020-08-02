using Blazor.Extensions.Canvas.WebGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace ReOsuStoryboardPlayerOnline.Render
{
    public struct TextureResource
    {
        public TextureResource(Size size,WebGLTexture texture)
        {
            Size = size;
            Texture = texture;
        }

        public WebGLTexture Texture { get; }
        public Size Size { get; }

        public bool IsValid => !(Texture is null);
    }
}
