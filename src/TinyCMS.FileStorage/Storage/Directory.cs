using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using TinyCMS.Storage;

namespace TinyCMS.FileStorage.Storage
{
    public class Directory : IDirectory
    {
        public IDirectory Parent { get; internal set; }

        private DirectoryInfo _directoryInfo;

        public Directory(IDirectory parent, DirectoryInfo directoryInfo1)
        {
            Parent = parent;
            this._directoryInfo = directoryInfo1;
        }

        private DirectoryInfo directoryInfo
        {
            get
            {
                return _directoryInfo;
            }
        }

        public IFile GetFile(string fileName)
        {
            return new File(new FileInfo(directoryInfo.FullName + Path.DirectorySeparatorChar + fileName), this);
        }
    }
}
