using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Util
{
    public enum LanCanvasEditingMode { None, Ink, Eraser, Knife, Printer, Pointer }
    public enum PermissionType { ManipulateOwnLines, ManipulateOtherLines, ManipulateOwnObjects, ManipulateOtherObjects, WipeLines, WipeObjects}

    public static class StaticPenises
    {
        public const int CS_PORT = 3333;
        public const int SC_PORT = 3334;

        public static T[][] Divide<T>(this IEnumerable<T> source, IEnumerable<T> divider)
        {
            T[] sourceArray = source.ToArray();
            T[] dividerArray = divider.ToArray();

            List<T[]> parts = new List<T[]>();

            List<T> selectedBytes = new List<T>();
            for (int x = 0; x < sourceArray.Length; x++)
            {
                T activeByte = sourceArray[x];
                selectedBytes.Add(activeByte);
                // compare every line of bytes with divider
                if (IsSequenceNext<T>(sourceArray, x, dividerArray))
                {
                    // delete first pointskey byte
                    selectedBytes.RemoveAt(selectedBytes.Count - 1);

                    // skip other pointkey bytes
                    x += dividerArray.Length - 1;

                    // add to final list of arrays
                    parts.Add(selectedBytes.ToArray());

                    selectedBytes.Clear();
                }
            }
            parts.Add(selectedBytes.ToArray());

            return parts.ToArray();
        }

        public static bool IsSequenceNext<T>(T[] source, int startIndex, T[] sequence)
        {
            for (int x = 0; x < sequence.Length; x++)
            {
                if (!source[startIndex + x].Equals(sequence[x]))
                    return false;
            }
            return true;
        }
    }
}
