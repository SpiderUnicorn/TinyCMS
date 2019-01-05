using System.Collections.Generic;

namespace TinyCMS.Storage
{
    public interface IDirectory
    {
        IDirectory Parent { get; }
        IFile GetFile(string fileName);
    }
}
