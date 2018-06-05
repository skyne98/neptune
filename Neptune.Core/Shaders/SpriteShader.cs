using System.Numerics;
using ShaderGen;
using SharpDX.Direct3D11;
using Veldrid;
using Vulkan;

[assembly: ShaderSet("SpriteShader", "Neptune.Core.Shaders.SpriteShader.VS", "Neptune.Core.Shaders.SpriteShader.PS")]

namespace Neptune.Core.Shaders
{
    public class SpriteShader: IShader
    {
        public struct VertexInput
        {
            // Per vertex
            [PositionSemantic] public Vector2 Position;
            [TextureCoordinateSemantic] public Vector2 UV;

            // Per instance
            [TextureCoordinateSemantic] public float ZIndex;
            [TextureCoordinateSemantic] public int MatrixIndex;
            [ColorSemantic] public Vector4 Color;
        }

        public struct PixelInput
        {
            [SystemPositionSemantic] public Vector4 Position;
            [TextureCoordinateSemantic] public Vector2 UV;
            [ColorSemantic] public Vector4 Color;
        }

        // Camera resource set
        [ResourceSet(0)] public Matrix4x4 Projection;

        // Group resource set
        [ResourceSet(1)] public Texture2DResource Texture;
        [ResourceSet(1)] public SamplerResource Sampler;
        [ResourceSet(1)] public StructuredBuffer<Matrix4x4> ModelMatrices;

        [VertexShader]
        public PixelInput VS(VertexInput input)
        {
            PixelInput output;
            var model = ModelMatrices[input.MatrixIndex];

            output.Position = Vector4.Transform(input.Position, Projection * model);
            output.Position.Z = input.ZIndex;
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