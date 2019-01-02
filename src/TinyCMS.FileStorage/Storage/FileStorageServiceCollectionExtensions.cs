using System;
using Microsoft.Extensions.DependencyInjection;
using TinyCMS.Base;
using TinyCMS.FileStorage.Storage;
using TinyCMS.Storage;

namespace TinyCMS.FileStorage.Storage
{
    public static class FileStorageServiceCollectionExtensions
    {
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
