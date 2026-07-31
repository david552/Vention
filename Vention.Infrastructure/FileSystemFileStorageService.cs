using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using Vention.Application.Abstractions;
using Vention.Application.Exceptions;
using Vention.Application.Options;

namespace Vention.Infrastructure
{
    public sealed class FileSystemFileStorageService : IFileStorageService
    {
        private const int CopyBufferSize = 81920;
        private const string TempDirectoryName = "tmp";

        private readonly string _rootPath;

        public FileSystemFileStorageService(IOptions<FileStorageSettingsOptions> options)
            => _rootPath = Path.GetFullPath(options.Value.RootPath);

        public async Task<FileStorageResult> SaveAsync(
            Stream content,
            Guid organizationId,
            string extension,
            long maxSizeBytes,
            CancellationToken ct = default)
        {
            var tempPath = Path.Combine(_rootPath, TempDirectoryName, $"{Guid.NewGuid():N}.tmp");
            long totalBytes;
            string checksum;


            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

            try
            {
                Directory.CreateDirectory(Path.Combine(_rootPath, TempDirectoryName));

                await using (var tempFile = new FileStream(
                    tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, CopyBufferSize, useAsync: true))
                {
                    var buffer = new byte[CopyBufferSize];
                    totalBytes = 0;
                    int read;

                    while ((read = await content.ReadAsync(buffer.AsMemory(), ct)) > 0)
                    {
                        totalBytes += read;

                        if (totalBytes > maxSizeBytes)
                            throw new ArgumentException(
                                $"File size cannot exceed {maxSizeBytes / (1024 * 1024)}MB.",
                                nameof(content));

                        hash.AppendData(buffer, 0, read);
                        await tempFile.WriteAsync(buffer.AsMemory(0, read), ct);
                    }
                }

                checksum = Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
            {
                TryDeleteFile(tempPath);
                throw new FileStorageException("The uploaded file could not be stored.", ex);
            }
            catch
            {
                TryDeleteFile(tempPath);
                throw;
            }

            try
            {
                var storageKey = $"{organizationId}/files/{checksum}{extension}";
                var finalPath = ResolveSafePath(storageKey);

                if (File.Exists(finalPath))
                {
                    File.Delete(tempPath);
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(finalPath)!);
                    File.Move(tempPath, finalPath);
                }

                return new FileStorageResult(storageKey, totalBytes, checksum);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
            {
                TryDeleteFile(tempPath);
                throw new FileStorageException("The uploaded file could not be stored.", ex);
            }
        }

        public Task DeleteAsync(string storageKey, CancellationToken ct = default)
        {
            try
            {
                var fullPath = ResolveSafePath(storageKey);

                if (File.Exists(fullPath))
                    File.Delete(fullPath);

                return Task.CompletedTask;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
            {
                throw new FileStorageException($"The stored file '{storageKey}' could not be deleted.", ex);
            }
        }

        private string ResolveSafePath(string storageKey)
        {
            if (string.IsNullOrWhiteSpace(storageKey))
                throw new FileStorageException("Storage key cannot be empty.");

            var fullPath = Path.GetFullPath(Path.Combine(_rootPath, storageKey));

            if (!fullPath.StartsWith(_rootPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                throw new FileStorageException($"Storage key '{storageKey}' resolves outside of the storage root.");

            return fullPath;
        }

        private static void TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
            }
        }
    }
}