using System.Collections.Generic;
using System.Linq;
using HC.TFPlugin;
using HC.TFPlugin.Attributes;

namespace Terrascape.LocalProvider
{
    public interface IContentSource
    {
        string Content { get; }

        string ContentBase64 { get; }

        string ContentPath { get; }

        string ContentUrl { get; }
    }

    [TFResource("lo_file", Version = 2L)]
    public partial class FileResource : IContentSource
    {
        public static readonly IEnumerable<string> ContentSourceProperties = new[]
        {
            nameof(FileResource.Content),
            nameof(FileResource.ContentBase64),
            nameof(FileResource.ContentPath),
            nameof(FileResource.ContentUrl),
        };
        public static readonly string ContentSourceArgumentNames = string.Join(", ",
            ContentSourceProperties.Select(x => TFArgumentAttribute.Get<FileResource>(x).Name));

        public const string MD5ChecksumKey = "md5";
        public const string SHA1ChecksumKey = "sha1";
        public const string SHA256ChecksumKey = "sha256";
        public const string SourceCodeHashKey = "sc_hash";

        public static readonly IEnumerable<string> AllChecksumKeys = new[]
        {
            MD5ChecksumKey,
            SHA1ChecksumKey,
            SHA256ChecksumKey,
            SourceCodeHashKey,
        };

        public static readonly string AllowedChecksumKeys = string.Join(", ", AllChecksumKeys);

        /// <summary>
        /// The path to the file resource to be written.
        /// </summary>
        [TFArgument("path",
            Required = true,
            ForceNew = true)]
        public string Path { get; set; }

        /// <summary>
        /// Literal string value to use as the content, which will
        /// written to the file resource as UTF-8-encoded text.
        /// </summary>
        [TFArgument("content",
            Optional = true,
            ForceNew = true,
            ConflictsWith = new[] {
                nameof(ContentBase64),
                nameof(ContentPath),
                nameof(ContentUrl),
            })]
        public string Content { get; set; }

        /// <summary>
        /// If set, will compute a checksum using the specified digest method.
        /// </summary>
        /// <value></value>
        [TFArgument("compute_checksum")]
        public string ComputeChecksum { get; set; }

        /// <summary>
        /// (Exactly one of the content source arguments must be specified.)
        /// Base64-encoded data that will be decoded and written as raw bytes
        /// to the file resource. This allows safely uploading non-UTF8 binary
        /// data, but is recommended only for small content such as the result
        /// of the gzipbase64 function with small text strings.
        /// For larger objects, use <c>content_path</c> or <c>content_url</c>
        /// to write the content from an external source stream.
        /// </summary>
        [TFArgument("content_base64",
            Optional = true,
            ConflictsWith = new[] {
                nameof(Content),
                nameof(ContentPath),
                nameof(ContentUrl),
            })]
        public string ContentBase64 { get; set; }

        [TFArgument("content_path",
            Optional = true,
            ConflictsWith = new[] {
                nameof(Content),
                nameof(ContentBase64),
                nameof(ContentUrl),
            })]
        public string ContentPath { get; set; }

        [TFArgument("content_url",
            Optional = true,
            ConflictsWith = new[] {
                nameof(Content),
                nameof(ContentBase64),
                nameof(ContentPath),
            })]
        public string ContentUrl { get; set; }

        [TFNested("append")]
        public IList<Append> Appends { get; set; }


        [TFComputed("full_path")]
        public string FullPath { get; set; }

        /// <summary>
        /// Last modified date of the file in RFC1123 format
        /// (e.g. Mon, 02 Jan 2006 15:04:05 MST).
        /// </summary>
        [TFComputed("last_modified")]
        public string LastModified { get; set; }

        [TFComputed("checksum")]
        public string Checksum { get; set; } = string.Empty;

        public string SourceOfContent
        {
            get
            {
                if (!string.IsNullOrEmpty(Content))
                    return nameof(Content) + "=" + Content;
                if (!string.IsNullOrEmpty(ContentBase64))
                    return nameof(ContentBase64) + "=" + ContentBase64;
                if (!string.IsNullOrEmpty(ContentPath))
                    return nameof(ContentPath) + "=" + ContentPath;
                if (!string.IsNullOrEmpty(ContentUrl))
                    return nameof(ContentUrl) + "=" + ContentUrl;
                return string.Empty;
            }
        }
    }
}