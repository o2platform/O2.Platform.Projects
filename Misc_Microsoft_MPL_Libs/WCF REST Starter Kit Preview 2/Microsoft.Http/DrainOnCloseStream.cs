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
    using System.Diagnostics;

    class DrainOnCloseStream : DetectEofStream
    {

        static byte[] drain;
        public DrainOnCloseStream(Stream innerStream)
            : base(innerStream)
        {
        }

        public override void Close()
        {
            if (!base.IsAtEof)
            {
                if (drain == null)
                {
                    drain = new byte[65536];
                }
                byte[] buffer = drain;
                int drained = 0;
                while (!base.IsAtEof)
                {
                    drained += base.Read(buffer, 0, buffer.Length);
                }
            }
            base.Close();
        }
    }
}
