﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RpcNet.Internal
{
    // TODO: Implement
    public class TcpBufferReader : INetworkReader
    {
        private byte[] buffer = new byte[65536];

        public void BeginReading()
        {

        }

        public void EndReading()
        {

        }

        public Span<byte> Read(int length)
        {
            return this.buffer.AsSpan();
        }
    }
}