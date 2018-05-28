using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Neptune.Core.Engine.Resources;
using Neptune.JobSystem.Native;
using Veldrid;
using Texture = Neptune.Core.Engine.Resources.Texture;

namespace Neptune.Core.Engine.Primitives
{
    public class SpritePrimitive : IRenderingPrimitive
    {
        private ResourceLink<Texture> _texture;
        private Vector2 _size = new Vector2(500f, 500f);
        private Vector2 _position = new Vector2(0f, 0f);
        private float _zIndex;
        private Vector2 _scale = Vector2.One;
        private float _rotation;
        private Vector2 _origin = Vector2.Zero;
        private RgbaFloat _color = RgbaFloat.White;
        private ParallelNative.TransformNative _nativeTransform;
        private ResourceSet _resourceSet;

        public SpritePrimitive(ResourceLink<Texture> texture, GraphicsDevice graphicsDevice)
        {
            _texture = texture;
            _texture.ResourceUpdated += (resource, newResource) =>
            {
                UpdateTexture(newResource);
            };
            _modelMatrix = Matrix4x4.Identity;
            _vertexBuffer = graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(4 * VertexInfo.SizeInBytes,
                BufferUsage.VertexBuffer));
            _modelMatrixBuffer = graphicsDevice.ResourceFactory.CreateBuffer(
                new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            _zIndexBuffer = graphicsDevice.ResourceFactory.CreateBuffer(
                new BufferDescription(16, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            
            UpdateTexture(_texture.Get());
        }

        private void UpdateTexture(Texture newTexture)
        {
            Size = _texture.Get().Size;
            _dirty = true;
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
                _nativeTransform.SizeX = _size.X * _scale.X;
                _nativeTransform.SizeY = _size.Y * _scale.Y;
            }
        }

        public Vector2 Position
        {
            get => _position;
            set
            {
                _position = value;
                _dirty = true;
                _nativeTransform.X = _position.X;
                _nativeTransform.Y = _position.Y;
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
                _nativeTransform.SizeX = _size.X * _scale.X;
                _nativeTransform.SizeY = _size.Y * _scale.Y;
            }
        }

        public float Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                _dirty = true;
                _nativeTransform.Rotation = _rotation;
            }
        }

        public Vector2 Origin
        {
            get => _origin;
            set
            {
                _origin = value;
                _dirty = true;
                _nativeTransform.OriginY = _origin.Y;
                _nativeTransform.OriginX = _origin.X;
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

        public Texture Texture
        {
            get => _texture.Get();
        }

        public ResourceLink<Texture> TextureLink
        {
            get => _texture;
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

        public ParallelNative.TransformNative NativeTransform
        {
            get { return _nativeTransform; }
            set { _nativeTransform = value; }
        }

        public ResourceSet ResourceSet
        {
            get { return _resourceSet; }
            set { _resourceSet = value; }
        }
    }
}