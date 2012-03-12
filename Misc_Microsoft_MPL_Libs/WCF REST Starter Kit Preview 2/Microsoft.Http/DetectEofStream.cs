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

    abstract class DetectEofStream : DelegatingStream
    {
        bool isAtEof;
        protected DetectEofStream(Stream stream)
            : base(stream)
        {
        }

        protected bool IsAtEof
        {
            get
            {
                return this.isAtEof;
            }
        }

        public override int EndRead(IAsyncResult result)
        {
            int num = base.EndRead(result);
            if (num == 0)
            {
                this.ReceivedEof();
            }
            return num;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int num = base.Read(buffer, offset, count);
            if (num == 0)
            {
                this.ReceivedEof();
            }
            return num;
        }

        public override int ReadByte()
        {
            int num;
            if (base.CanRead)
            {
                num = base.ReadByte();
            }
            else
            {
                num = -1;
            }
            if (num == -1)
            {
                this.ReceivedEof();
            }
            return num;
        }

        protected virtual void OnReceivedEof()
        {
        }

        void ReceivedEof()
        {
            if (!this.isAtEof)
            {
                this.isAtEof = true;
                this.OnReceivedEof();
            }
        }
    }
}
