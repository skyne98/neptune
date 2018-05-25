using System;

namespace Neptune.Core.Engine.Resources
{
    public class ResourceLink<T> where T: IResource
    {
        private T _resource;

        public delegate void ResourceUpdatedHandler(T oldResource, T newResource);
        
        public event ResourceUpdatedHandler ResourceUpdated;

        public ResourceLink(T resource)
        {
            _resource = resource;
        }

        public T Get()
        {
            return _resource;
        }

        public void Set(T newResource)
        {
            var oldResource = _resource;
            _resource = newResource;
            
            OnResourceUpdated(oldResource, newResource);
        }

        protected virtual void OnResourceUpdated(T oldresource, T newresource)
        {
            ResourceUpdated?.Invoke(oldresource, newresource);
        }
    }
}