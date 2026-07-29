namespace Vention.Application.Common
{
    public sealed class PrefixedStream : Stream
    {
        private readonly byte[] _prefix;
        private readonly int _prefixLength;
        private readonly Stream _inner;
        private int _prefixPosition;

        public PrefixedStream(byte[] prefix, int prefixLength, Stream inner)
        {
            _prefix = prefix;
            _prefixLength = prefixLength;
            _inner = inner;
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
            => Read(buffer.AsSpan(offset, count));

        public override int Read(Span<byte> buffer)
        {
            if (_prefixPosition < _prefixLength)
            {
                var toCopy = Math.Min(buffer.Length, _prefixLength - _prefixPosition);
                _prefix.AsSpan(_prefixPosition, toCopy).CopyTo(buffer);
                _prefixPosition += toCopy;

                return toCopy;
            }

            return _inner.Read(buffer);
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_prefixPosition < _prefixLength)
            {
                var toCopy = Math.Min(buffer.Length, _prefixLength - _prefixPosition);
                _prefix.AsMemory(_prefixPosition, toCopy).CopyTo(buffer);
                _prefixPosition += toCopy;

                return ValueTask.FromResult(toCopy);
            }

            return _inner.ReadAsync(buffer, cancellationToken);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}