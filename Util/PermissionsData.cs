using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Util
{
    public class PermissionsData
    {
        public static PermissionsData Default
        {
            get
            {
                PermissionsData fap = new PermissionsData();
                fap.ManipulateOwnLines = true;
                fap.ManipulateOtherLines = true;
                fap.ManipulateOwnObjects = true;
                fap.ManipulateOtherObjects = true;
                fap.WipeStrokes = true;
                fap.WipeObjects = true;
                fap.UsePointers = true;
                return fap;
            }
        }

        public event Action Changed = delegate { };

        bool manipulateOwnLines, manipulateOtherLines, manipulateOwnObjects, manipulateOtherObjects, wipeStrokes, wipeObjects, usePointers;

        public bool ManipulateOwnLines
        {
            get
            {
                return manipulateOwnLines;
            }
            set
            {
                manipulateOwnLines = value;
                Changed();
            }
        }
        public bool ManipulateOtherLines
        {
            get
            {
                return manipulateOtherLines;
            }
            set
            {
                manipulateOtherLines = value;
                Changed();
            }
        }
        public bool ManipulateOwnObjects
        {
            get
            {
                return manipulateOwnObjects;
            }
            set
            {
                manipulateOwnObjects = value;
                Changed();
            }
        }
        public bool ManipulateOtherObjects
        {
            get
            {
                return manipulateOtherObjects;
            }
            set
            {
                manipulateOtherObjects = value;
                Changed();
            }
        }
        public bool WipeStrokes
        {
            get
            {
                return wipeStrokes;
            }
            set
            {
                wipeStrokes = value;
                Changed();
            }
        }
        public bool WipeObjects
        {
            get
            {
                return wipeObjects;
            }
            set
            {
                wipeObjects = value;
                Changed();
            }
        }
        public bool UsePointers
        {
            get
            {
                return usePointers;
            }
            set
            {
                usePointers = value;
                Changed();
            }
        }

        public byte Serialize()
        {
            ByteBits bits = new ByteBits(0);
            bits[0] = ManipulateOwnLines;
            bits[1] = ManipulateOtherLines;
            bits[2] = ManipulateOwnObjects;
            bits[3] = ManipulateOtherObjects;
            bits[4] = WipeStrokes;
            bits[5] = WipeObjects;
            bits[6] = UsePointers;
            return bits.GetByte();
        }

        public void Deserialize(byte data)
        {
            ByteBits bits = new ByteBits(data);
            ManipulateOwnLines = bits[0];
            ManipulateOtherLines = bits[1];
            ManipulateOwnObjects = bits[2];
            ManipulateOtherObjects = bits[3];
            WipeStrokes = bits[4];
            WipeObjects = bits[5];
            UsePointers = bits[6];
        }
    }
}
