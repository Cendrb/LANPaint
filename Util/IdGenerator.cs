using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Util
{
    public class IdGenerator
    {
        public uint ActiveId { get; set; }

        public IdGenerator(uint startingId)
        {
            ActiveId = startingId;
        }

        public IdGenerator()
        {
            ActiveId = 0;
        }

        public uint GetNextId()
        {
            ActiveId++;
            return ActiveId;
        }
    }
}
