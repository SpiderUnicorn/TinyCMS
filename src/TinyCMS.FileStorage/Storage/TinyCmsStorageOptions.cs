using System;
using System.Collections.Generic;
using System.Reflection;
using TinyCMS.Data;
using TinyCMS.Data.Builder;
using TinyCMS.FileStorage;
using TinyCMS.Interfaces;
using TinyCMS.Security;
using TinyCMS.Serializer;

namespace TinyCMS.FileStorage.Storage
{
    public class TinyCmsStorageOptions
    {
        public Type NodeStorage { get; set; } = typeof(NodeFileStorage<Container>);
        public Type StorageService { get; set; } = typeof(JsonStorageService);
    }
}
