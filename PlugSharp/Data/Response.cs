using System;
using System.Collections.Generic;
using System.Text;

namespace PlugSharp.Data
{
    public struct Response<D, M>
    {
        public D[] data;
        public M meta;
        public string status;
        public float time;
    }
}
