namespace ImageSharp.Memory
{
    using System;
    using System.Collections.Generic;

    internal abstract class PackedBuffer<T> : IDisposable
        where T : struct
    {
        protected PackedBuffer(int totalLength)
        {
            this.TotalLength = totalLength;
        }

        ~PackedBuffer()
        {
            this.IsDisposed = true;
            this.Dispose(false);
        }

        public bool IsDisposed { get; private set; } = false;

        public int TotalLength { get; }

        public void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.IsDisposed = true;
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IEnumerable<Partition> ReadAllPartitions()
        {
            using (PartitionIterator iterator = this.CreateIterator())
            {
                while (iterator.MoveNext())
                {
                    iterator.ReadCurrent();
                    yield return iterator.CurrentPartition;
                }
            }
        }

        public IEnumerable<Partition> WriteAllPartitions()
        {
            using (PartitionIterator iterator = this.CreateIterator())
            {
                while (iterator.MoveNext())
                {
                    yield return iterator.CurrentPartition;
                    iterator.WriteCurrent();
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        protected abstract PartitionIterator CreateIterator();

        // TODO: Should be replaced with corefxlab Buffer<T> (aka former Memory<T>)
        public struct Partition
        {
            internal static readonly Partition Empty = default(Partition);

            private Buffer<T> buffer;

            private int start;

            private int length;

            internal Partition(Buffer<T> buffer, int start, int length)
            {
                DebugGuard.MustBeLessThanOrEqualTo(start, buffer.Length, nameof(start));
                DebugGuard.MustBeLessThanOrEqualTo(start + length, buffer.Length, nameof(length));

                this.buffer = buffer;
                this.start = start;
                this.length = length;
            }

            public Span<T> Span => this.buffer != null ? this.buffer.Slice(this.start, this.length) : Span<T>.Empty;
        }

        protected abstract class PartitionIterator : IDisposable
        {
#pragma warning disable SA1401 // Fields must be private
            internal Partition CurrentPartition = Partition.Empty;
#pragma warning restore SA1401 // Fields must be private

            public abstract void Dispose();

            internal abstract void ReadCurrent();

            internal abstract void WriteCurrent();

            internal abstract bool MoveNext();
        }
    }
}