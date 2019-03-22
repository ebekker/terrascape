using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using HC.TFPlugin;
using Microsoft.Extensions.Logging;

namespace Terrascape.LocalProvider
{
    public partial class LocalProvider : IDataSourceProvider<FileInfoDataSource>
    {
        public ValidateDataSourceConfigResult<FileInfoDataSource> ValidateConfig(ValidateDataSourceConfigInput<FileInfoDataSource> input)
        { 
            return new ValidateDataSourceConfigResult<FileInfoDataSource>();
        }

        public ReadDataSourceResult<FileInfoDataSource> Read(ReadDataSourceInput<FileInfoDataSource> input)
        {
            var result = new ReadDataSourceResult<FileInfoDataSource>();

            var fi = new FileInfo(input.Config.Path);
            var attrs = new Dictionary<string, bool>();
            foreach (var fa in Enum.GetValues(typeof(FileAttributes)))
            {
                attrs[Enum.GetName(typeof(FileAttributes), fa)] =
                    (fi.Attributes & ((FileAttributes)fa)) != 0;
            }

            result.State = new FileInfoDataSource
            {
                Path = input.Config.Path,
                Exists = fi.Exists,
                FullPath = fi.FullName,
                Attributes = attrs,
                Name = fi.Name,
                Extension = fi.Extension,
                CreationTime = fi.CreationTime.ToString("r"),
                LastAccessTime = fi.LastAccessTime.ToString("r"),
                LastWriteTime = fi.LastWriteTime.ToString("r"),
                Length = fi.Length,
            };

            return result;
        }
    }
}