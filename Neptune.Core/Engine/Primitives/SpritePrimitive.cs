using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Neptune.Core.Engine.Resources;
using Veldrid;

namespace Neptune.Core.Engine.Primitives
{
    public class SpritePrimitive : IRenderingPrimitive
    {
        private Texture2D _texture;
        private Vector2 _size = new Vector2(500f, 500f);
        private Vector2 _position = new Vector2(0f, 0f);
        private float _zIndex = 0f;
        private Vector2 _scale = Vector2.One;
        private float _rotation = 0f;
        private Vector2 _origin = Vector2.Zero;
        private RgbaFloat _color = RgbaFloat.White;

        public SpritePrimitive(Texture2D texture, GraphicsDevice graphicsDevice)
        {
            _texture = texture;
            _size = _texture.Size;
            _vertexBuffer = graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(4 * VertexInfo.SizeInBytes,
                BufferUsage.VertexBuffer));
            _modelMatrixBuffer = graphicsDevice.ResourceFactory.CreateBuffer(
                new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            _zIndexBuffer = graphicsDevice.ResourceFactory.CreateBuffer(
                new BufferDescription(16, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        }

        private bool _dirty = true;
        private Matrix4x4 _modelMatrix;
        private List<VertexInfo> _vertices;
        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _modelMatrixBuffer;
        private DeviceBuffer _zIndexBuffer;

        public Vector2 Size
        {
            get => _size;
            set
            {
                _size = value;
                _dirty = true;
            }
        }

        public Vector2 Position
        {
            get => _position;
            set
            {
                _position = value;
                _dirty = true;
            }
        }

        public float ZIndex
        {
            get => _zIndex;
            set
            {
                _zIndex = value;
                _dirty = true;
            }
        }

        public Vector2 Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                _dirty = true;
            }
        }

        public float Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                _dirty = true;
            }
        }

        public Vector2 Origin
        {
            get => _origin;
            set
            {
                _origin = value;
                _dirty = true;
            }
        }

        public Matrix4x4 ModelMatrix
        {
            get => _modelMatrix;
            set
            {
                _modelMatrix = value;
            }
        }

        public List<VertexInfo> Vertices
        {
            get => _vertices;
            set
            {
                _vertices = value;
            }
        }

        public RgbaFloat Color
        {
            get => _color;
            set
            {
                _color = value;
                _dirty = true;
            }
        }

        public bool Dirty
        {
            get => _dirty;
            set => _dirty = value;
        }

        public Texture2D Texture
        {
            get => _texture;
            set => _texture = value;
        }

        public DeviceBuffer VertexBuffer
        {
            get => _vertexBuffer;
            set => _vertexBuffer = value;
        }

        public DeviceBuffer ModelMatrixBuffer
        {
            get => _modelMatrixBuffer;
            set => _modelMatrixBuffer = value;
        }

        public DeviceBuffer ZIndexBuffer
        {
            get => _zIndexBuffer;
            set => _zIndexBuffer = value;
        }
    }
}