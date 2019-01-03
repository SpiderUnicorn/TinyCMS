using System.Collections.Generic;

namespace TinyCMS.Storage
{
    public interface IDirectory
    {
        string Name { get; }
        IDirectory Parent { get; }
        IEnumerable<IFile> GetFiles();
        IEnumerable<IDirectory> GetDirectories();
        IFile GetFile(string fileName);
    }
}
