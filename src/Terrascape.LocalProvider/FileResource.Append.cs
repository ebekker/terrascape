using HC.TFPlugin.Attributes;

namespace Terrascape.LocalProvider
{
    public partial class FileResource
    {
        public class Append : IContentSource
        {
            [TFArgument("content",
                Optional = true,
                ForceNew = true,
                ConflictsWith = new[] {
                    nameof(ContentBase64),
                    nameof(ContentPath),
                    nameof(ContentUrl),
                })]
            public string Content { get; set; }

            [TFArgument("compute_checksum")]
            public string ComputeChecksum { get; set; }

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
        }
    }
}