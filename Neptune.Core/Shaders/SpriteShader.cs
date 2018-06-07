using System;
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
            [TextureCoordinateSemantic] public int TransformDataIndex;
            [ColorSemantic] public Vector4 Color;
        }

        public struct PixelInput
        {
            [SystemPositionSemantic] public Vector4 Position;
            [TextureCoordinateSemantic] public Vector2 UV;
            [ColorSemantic] public Vector4 Color;
        }

        public struct TransformData
        {
            public Vector3 Position;
            public float Rotation;
            public Vector2 Size;
            public Vector2 Origin;
        }

        // Camera resource set
        [ResourceSet(0)] public Matrix4x4 Projection;

        // Group resource set
        [ResourceSet(1)] public Texture2DResource Texture;
        [ResourceSet(1)] public SamplerResource Sampler;
        [ResourceSet(1)] public StructuredBuffer<TransformData> TransformDatas;

        [VertexShader]
        public PixelInput VS(VertexInput input)
        {
            PixelInput output;
            var transformData = TransformDatas[input.TransformDataIndex];
            var model = CreateTranslation(new Vector3(transformData.Position.XY(), 0));
            model = model * CreateRotationZ(transformData.Rotation);
            model = model * CreateTranslation(new Vector3(-transformData.Origin.X * transformData.Size.X, -transformData.Origin.Y * transformData.Size.Y, 0.0f)); 
            model = model * CreateScale(new Vector3(transformData.Size.X, transformData.Size.Y, 1.0f));

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

            if (ShaderGen.ShaderBuiltins.Abs(textureColor.W) < 0.1)
                ShaderGen.ShaderBuiltins.Discard();

            textureColor *= input.Color;

            return textureColor;
        }

        private Matrix4x4 CreateTranslation(Vector3 position)
        {
            Matrix4x4 result;

            result.M11 = 1.0f;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M14 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = 1.0f;
            result.M23 = 0.0f;
            result.M24 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = 1.0f;
            result.M34 = 0.0f;
            result.M41 = 0.0f;
            result.M42 = 0.0f;
            result.M43 = 0.0f;

            result.M14 = position.X;
            result.M24 = position.Y;
            result.M34 = position.Z;
            result.M44 = 1.0f;

            return result;
        }

        private Matrix4x4 CreateScale(Vector3 scales)
        {
            Matrix4x4 result;

            result.M11 = scales.X;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M14 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = scales.Y;
            result.M23 = 0.0f;
            result.M24 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = scales.Z;
            result.M34 = 0.0f;
            result.M41 = 0.0f;
            result.M42 = 0.0f;
            result.M43 = 0.0f;
            result.M44 = 1.0f;

            return result;
        }

        private Matrix4x4 CreateRotationZ(float radians)
        {
            Matrix4x4 result;

            float c = ShaderGen.ShaderBuiltins.Cos(radians);
            float s = ShaderGen.ShaderBuiltins.Sin(radians);

            result.M11 = c;
            result.M12 = s;
            result.M13 = 0.0f;
            result.M14 = 0.0f;
            result.M21 = -s;
            result.M22 = c;
            result.M23 = 0.0f;
            result.M24 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = 1.0f;
            result.M34 = 0.0f;
            result.M41 = 0.0f;
            result.M42 = 0.0f;
            result.M43 = 0.0f;
            result.M44 = 1.0f;

            return result;
        }
    }
}