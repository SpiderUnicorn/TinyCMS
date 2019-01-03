using System;
using Microsoft.Extensions.DependencyInjection;
using TinyCMS.Base;
using TinyCMS.FileStorage.Storage;
using TinyCMS.Storage;

namespace TinyCMS.FileStorage.Storage
{
    // <summary>
    /// Extension method for <see cref="ITinyCmsBuilder"/> to register file storage dependencies for TinyCMS.
    /// </summary>
    public static class FileStorageServiceCollectionExtensions
    {
        /// <summary>
        /// Adds TiyCms File Storage to the <see cref="ITinyCmsBuilder" />.
        /// </summary>
        /// <param name="builder"></param>
        public static ITinyCmsBuilder AddFileStorage(this ITinyCmsBuilder builder)
        {
            var settings = new TinyCmsStorageOptions();

            builder.Services
                .AddTransient<IFile, File>()
                .AddTransient<IDirectory, Directory>()
                .AddSingleton<IFileStorageService, FileStorageService>()
                .AddSingleton(typeof(IStorageService), settings.StorageService)
                .AddSingleton(typeof(INodeStorage), settings.NodeStorage)
                .AddSingleton((sp) =>
                {
                    return sp.GetService<INodeStorage>().Load();
                });

            return builder;
        }
    }
}
