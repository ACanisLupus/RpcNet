using System;
using System.Collections.Generic;
using System.Text;

namespace RpcNet
{
    public class RpcException : Exception
    {
        public RpcException(string message) : base(message)
        {
        }
    }
}
