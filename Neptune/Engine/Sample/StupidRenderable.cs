using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using Veldrid;
using Vector = System.Numerics.Vector;

namespace Neptune.Engine.Sample
{
    public class StupidRenderable: IRenderable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private Matrix4x4 _transformScale = Matrix4x4.Identity;
        private Matrix4x4 _transformRotate = Matrix4x4.Identity;

        private float _size = 500f;
        
        private Vector2 _position = new Vector2(100, 100);
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                RecalculateTransform();
            }
        }
        
        private Vector2 _scale = Vector2.One;
        public Vector2 Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                RecalculateTransform();
            }
        }

        private float _rotation = 0f;
        public float Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                RecalculateTransform();
            }
        }

        public StupidRenderable(GraphicsDevice graphicsDevice, Vector2 position, Vector2 scale)
        {
            _graphicsDevice = graphicsDevice;
            Position = position;
            Scale = scale;
        }

        private void RecalculateTransform()
        {
            _transformRotate = Matrix4x4.Identity;
            _transformRotate.M11 = _scale.X;
            _transformRotate.M22 = _scale.Y;
            
            _transformScale = Matrix4x4.Identity;
            _transformScale.M11 = (float)Math.Cos(_rotation);
            _transformScale.M22 = (float)Math.Cos(_rotation);
            _transformScale.M12 = (float)Math.Sin(_rotation);
            _transformScale.M21 = -(float)Math.Sin(_rotation);
        }

        public (List<VertexInfo>, List<ushort>) GetTriangles()
        {
            var vertices = new List<VertexInfo>()
            {
                new VertexInfo(TransformVertex(new Vector2(0, 1)), new Vector2(0, 1), RgbaFloat.White),
                new VertexInfo(TransformVertex(new Vector2(1, 1)), new Vector2(1, 1), RgbaFloat.White),
                new VertexInfo(TransformVertex(new Vector2(0, 0)), new Vector2(0, 0), RgbaFloat.White),
                new VertexInfo(TransformVertex(new Vector2(1, 0)), new Vector2(1, 0), RgbaFloat.White)
            };
            var triangles = new List<ushort>()
            {
                0, 1, 2, 3
            };

            return (vertices, triangles);
        }

        private Vector2 TransformVertex(Vector2 vertex)
        {
            var width = _graphicsDevice.SwapchainFramebuffer.Width;
            var height = _graphicsDevice.SwapchainFramebuffer.Height;

            var transformed = Vector2.Transform(vertex, _transformRotate);
            transformed = Vector2.Transform(transformed, _transformScale);
            
            var normalized = new Vector2(transformed.X * _size / width, transformed.Y * _size / height);
            return normalized - Vector2.One + new Vector2(_position.X / width, _position.Y / height);
        }
    }
}