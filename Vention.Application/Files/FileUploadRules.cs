namespace Vention.Application.Files
{
    public static class FileUploadRules
    {
        public const long MaxFileSizeBytes = 50L * 1024 * 1024;

        public const int MaxFilenameLength = 255;

        public const int SignatureLength = 8;

        private static readonly IReadOnlyDictionary<string, string> ExtensionsByContentType =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["application/pdf"] = ".pdf",
                ["application/msword"] = ".doc",
                ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"] = ".docx",
                ["text/plain"] = ".txt"
            };

        public static bool IsAllowedContentType(string contentType)
            => !string.IsNullOrWhiteSpace(contentType) && ExtensionsByContentType.ContainsKey(contentType.Trim());

        public static string GetExtensionFor(string contentType)
        {
            if (!IsAllowedContentType(contentType))
                throw new ArgumentException($"Content type '{contentType}' is not allowed.", nameof(contentType));

            return ExtensionsByContentType[contentType.Trim()];
        }


        public static bool MatchesSignature(string contentType, ReadOnlySpan<byte> header)
        {
            string cleanContentType = contentType.Trim().ToLowerInvariant();

            switch (cleanContentType)
            {
                case "application/pdf":
                    return StartsWith(header, 0x25, 0x50, 0x44, 0x46);

                case "application/msword":
                    return StartsWith(header, 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1);

                case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                    return StartsWith(header, 0x50, 0x4B, 0x03, 0x04);

                case "text/plain":
                    return header.IndexOf((byte)0x00) < 0;

                default:
                    return false;
            }
        }


        public static string SanitizeFilename(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentException("Filename cannot be empty.", nameof(filename));
            }

            string name = filename.Trim();

            char[] separators = new char[] { '/', '\\' };
            int lastSeparatorIndex = name.LastIndexOfAny(separators);

            if (lastSeparatorIndex >= 0)
            {
                name = name.Substring(lastSeparatorIndex + 1);
            }

            char[] invalidChars = Path.GetInvalidFileNameChars();
            name = new string(name.Where(c => !invalidChars.Contains(c)).ToArray());

            name = name.TrimStart('.').Trim();

            if (name.Length == 0)
            {
                throw new ArgumentException("Filename does not contain any valid characters.", nameof(filename));
            }

            if (name.Length > MaxFilenameLength)
                name = name[^MaxFilenameLength..];

            return name;
        }
        private static bool StartsWith(ReadOnlySpan<byte> header, params byte[] signature)
        {
            if (header.Length < signature.Length)
            {
                return false;
            }

            for (int i = 0; i < signature.Length; i++)
            {
                if (header[i] != signature[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}