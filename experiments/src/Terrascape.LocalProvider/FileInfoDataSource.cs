using System.Collections.Generic;
using HC.TFPlugin.Attributes;

namespace Terrascape.LocalProvider
{
    [TFDataSource("lo_file_info")]
    public class FileInfoDataSource
    {
        [TFArgument("path",
            Required = true)]
        public string Path { get; set; }

        [TFComputed("exists")]
        public bool Exists { get; set; }

        [TFComputed("full_path")]
        public string FullPath { get; set; }

        [TFComputed("attributes")]
        public Dictionary<string, bool> Attributes { get; set; }

        [TFComputed("name")]
        public string Name { get; set; }

        [TFComputed("extension")]
        public string Extension { get; set; }

        [TFComputed("creation_time")]
        public string CreationTime { get; set; }

        [TFComputed("last_access_time")]
        public string LastAccessTime { get; set; }

        [TFComputed("last_write_time")]
        public string LastWriteTime { get; set; }

        [TFComputed("length")]
        public long Length { get; set; }
    }
}