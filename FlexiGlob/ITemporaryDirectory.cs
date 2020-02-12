using System;

namespace FlexiGlob
{
    public interface ITemporaryDirectory : IDisposable
    {
        string FullPath { get; }
        void CleanUp();
    }
}
