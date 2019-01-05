using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using TinyCMS.Storage;

namespace TinyCMS.FileStorage.Storage
{
    public class FileStorageService : IFileStorageService
    {
        public FileStorageService(IHostingEnvironment hostingEnvironment) : this(hostingEnvironment.ContentRootPath) { }

        public FileStorageService(string rootDir)
        {
            RootDirectory = new Directory(null, new DirectoryInfo(rootDir));
        }

        public IDirectory RootDirectory { get; internal set; }
    }
}
