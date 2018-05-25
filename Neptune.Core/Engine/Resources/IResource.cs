using System;

namespace Neptune.Core.Engine.Resources
{
    public interface IResource: IDisposable
    {
        string Hash { get; }
    }
}