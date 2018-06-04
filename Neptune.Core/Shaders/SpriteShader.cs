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
            [TextureCoordinateSemantic] public Vector4 TransformColumn0;
            [TextureCoordinateSemantic] public Vector4 TransformColumn1;
            [TextureCoordinateSemantic] public Vector4 TransformColumn2;
            [TextureCoordinateSemantic] public Vector4 TransformColumn3;
            [TextureCoordinateSemantic] public float ZIndex;
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

        [VertexShader]
        public PixelInput VS(VertexInput input)
        {
            PixelInput output;
            
            var model = new Matrix4x4(
                input.TransformColumn0.X, input.TransformColumn1.X, input.TransformColumn2.X, input.TransformColumn3.X,
                input.TransformColumn0.Y, input.TransformColumn1.Y, input.TransformColumn2.Y, input.TransformColumn3.Y,
                input.TransformColumn0.Z, input.TransformColumn1.Z, input.TransformColumn2.Z, input.TransformColumn3.Z,
                input.TransformColumn0.W, input.TransformColumn1.W, input.TransformColumn2.W, input.TransformColumn3.W
            );      

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