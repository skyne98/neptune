using System.Numerics;
using ShaderGen;
using SharpDX.Direct3D11;
using Veldrid;
using Vulkan;

[assembly: ShaderSet("SpriteShader", "Neptune.Core.Shaders.SpriteShader.VS", "Neptune.Core.Shaders.SpriteShader.PS")]

namespace Neptune.Core.Shaders
{
    public class SpriteShader
    {
        public struct VertexInput
        {
            [PositionSemantic] public Vector2 Position;
            [TextureCoordinateSemantic] public Vector2 UV;
            [ColorSemantic] public Vector4 Color;
        }

        public struct PixelInput
        {
            [SystemPositionSemantic] public Vector4 Position;
            [TextureCoordinateSemantic] public Vector2 UV;
            [ColorSemantic] public Vector4 Color;
        }
        
        [ResourceSet(0)] public Matrix4x4 Model;
        [ResourceSet(0)] public float ZIndex;

        [ResourceSet(1)] public Matrix4x4 Projection;
        [ResourceSet(1)] public Texture2DResource Texture;
        [ResourceSet(1)] public SamplerResource Sampler;

        [VertexShader]
        public PixelInput VS(VertexInput input)
        {
            PixelInput output;
            
            output.Position = Vector4.Transform(input.Position, Projection * Model);
            output.Position.Z = ZIndex;
            output.UV = input.UV;
            output.Color = input.Color;

            return output;
        }

        [FragmentShader]
        public Vector4 PS(PixelInput input)
        {
            var textureColor = ShaderBuiltins.Sample(Texture, Sampler, input.UV);

            textureColor *= input.Color;

            return textureColor;
        }
    }
}