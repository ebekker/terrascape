using HC.TFPlugin;
using HC.TFPlugin.Attributes;

namespace Terrascape.AcmeProvider
{
    [TFResource("acmelo_file", Version = 2L)]
    public class FileResource
    {
        [TFAttribute("path",
            Required = true,
            ForceNew = true)]
        public string Path { get; set; }

        [TFAttribute("content",
            ConflictsWith = new[] {
                nameof(ContentBase64),
                nameof(SourcePath),
                nameof(SourceUrl),
            })]
        public string Content { get; set; }

        [TFAttribute("content_base64",
            ConflictsWith = new[] {
                nameof(Content),
                nameof(SourcePath),
                nameof(SourceUrl),
            })]
        public string ContentBase64 { get; set; }

        [TFAttribute("source_path",
            ConflictsWith = new[] {
                nameof(Content),
                nameof(ContentBase64),
                nameof(SourceUrl),
            })]
        public string SourcePath { get; set; }

        [TFAttribute("source_url",
            ConflictsWith = new[] {
                nameof(Content),
                nameof(ContentBase64),
                nameof(SourcePath),
            })]
        public string SourceUrl { get; set; }
    }
}