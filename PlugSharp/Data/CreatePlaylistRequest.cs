using System;
using System.Collections.Generic;
using System.Text;

namespace PlugSharp.Data
{
    public struct CreatePlaylistRequest
    {
        public string name;

        public Media[] media;
    }
}
