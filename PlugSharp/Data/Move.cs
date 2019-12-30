using System;
using System.Collections.Generic;
using System.Text;

namespace PlugSharp.Data
{
    public struct Move
    {
        /// <summary>
        /// 
        /// The userid of the user you want to move
        /// </summary>
        public int userID;

        /// <summary>
        /// The wanted 0 based position in the waitlist
        /// </summary>
        public ushort position;
    }
}
