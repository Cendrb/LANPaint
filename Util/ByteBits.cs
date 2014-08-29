using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Util
{
    public struct ByteBits
    {
        byte bits;
        public ByteBits(byte initialValue)
        {
            bits = initialValue;
        }

        public bool this[byte index]
        {
            get
            {
                return (bits & (1 << index)) != 0;
            }
            set
            {
                if (value)
                    bits |= (byte)(1 << index);
                else
                    bits &= (byte)~(1 << index);
            }
        }

        public byte GetByte()
        {
            return bits;
        }
    }
}
