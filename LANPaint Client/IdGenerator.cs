using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LANPaint_Client
{
    static class IdGenerator
    { 
        public static uint NextId { get; set; }

        public static uint GetNextId()
        {
            uint id = NextId;
            NextId++;
            return id;
        }
    }
}
