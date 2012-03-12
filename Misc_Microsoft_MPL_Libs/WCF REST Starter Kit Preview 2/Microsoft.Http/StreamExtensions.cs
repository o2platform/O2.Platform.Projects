//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;

    static class StreamExtensions
    {
        const int maxSize = 65536;
        static byte[] drainBuffer;

        public static long ConsumeAllBytesAndClose(this Stream stream)
        {
            if (drainBuffer == null)
            {
                drainBuffer = new byte[maxSize];
            }

            return stream.CopyAndCloseSource(null, null);
        }

        public static void CopyStreamTo(this Stream source, Stream destination)
        {
            CopyAndCloseSource(source, destination, null);
        }

        public static long CopyAndCloseSource(this Stream source, Stream destination, long? size)
        {
            int reads = 0;
            long total = 0;
            int bufferSize;
            using (source)
            {
                if (size.HasValue && size < 0)
                {
                    size = null;
                }
                if (size == null || (size.HasValue && size.Value > maxSize))
                {
                    bufferSize = maxSize; // max socket read
                }
                else
                {
                    bufferSize = (int)size.Value;
                }
                byte[] buffer;
                int got;

                if (destination == null)
                {
                    buffer = drainBuffer;
                    while ((got = source.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ++reads;
                        total += got;
                        if (size != null && total == size.Value)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    buffer = new byte[bufferSize];

                    while ((got = source.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ++reads;
                        destination.Write(buffer, 0, got);
                        total += got;
                        if (size != null && total == size.Value)
                        {
                            break;
                        }
                    }

                    destination.Flush();
                }
            }
            return total;
        }
        public static byte[] ReadAllBytes(this Stream source, long? length)
        {
            var memorySource = source as MemoryStream;
            if (memorySource != null)
            {
                var array = memorySource.ToArray();
                memorySource.Close();
                return array;
            }
            var memory = new MemoryStream();
            CopyAndCloseSource(source, memory, length);
            return memory.ToArray();
        }
        public static byte[] ReadAllBytes(this Stream source)
        {
            return ReadAllBytes(source, null);
        }
    }
}