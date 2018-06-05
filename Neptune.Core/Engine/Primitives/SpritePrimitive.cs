using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Neptune.Core.Engine.Renderers;
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

        internal SpritePrimitiveRenderer _spritePrimitiveRenderer;
        internal bool _dirty = true;

        public SpritePrimitive(ResourceLink<Texture> texture)
        {
            _texture = texture;
            _texture.ResourceUpdated += (resource, newResource) =>
            {
                UpdateTexture(newResource);
            };
            
            UpdateTexture(_texture.Get());
        }

        private void UpdateTexture(Texture newTexture)
        {
            Size = _texture.Get().Size;
            SetDirty();
        }

        private void SetDirty()
        {
            if (_dirty == false)
            {
                _dirty = true;
                if (_spritePrimitiveRenderer != null)
                {
                    _spritePrimitiveRenderer.AddDirty(this);
                }
            }
        }
        
        private Matrix4x4 _modelMatrix;

        public Vector2 Size
        {
            get => _size;
            set
            {
                _size = value;
                SetDirty();
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
                SetDirty();
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
                SetDirty();
            }
        }

        public Vector2 Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                SetDirty();
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
                SetDirty();
                _nativeTransform.Rotation = _rotation;
            }
        }

        public Vector2 Origin
        {
            get => _origin;
            set
            {
                _origin = value;
                SetDirty();
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

        public RgbaFloat Color
        {
            get => _color;
            set
            {
                _color = value;
                SetDirty();
            }
        }

        public Texture Texture
        {
            get => _texture.Get();
        }

        public ResourceLink<Texture> TextureLink
        {
            get => _texture;
        }

        public ParallelNative.TransformNative NativeTransform
        {
            get { return _nativeTransform; }
            set { _nativeTransform = value; }
        }
    }
}