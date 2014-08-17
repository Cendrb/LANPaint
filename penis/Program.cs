using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;
using System.Windows.Input;

namespace penis
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("penis".GetHashCode());
            Console.WriteLine("penis".GetHashCode());
            Console.WriteLine("peniss".GetHashCode());
            Console.ReadKey();
        }

        static bool Penis<T>(T fap, T paf)
        {
            return fap.Equals(paf);
        }
    }

    class Penios
    {
        public int Length { get; private set; }
        public Penios(int length)
        {
            this.Length = length;
        }

        public static bool operator == (Penios first, Penios second)
        {
            return first.Length == second.Length;
        }

        public static bool operator !=(Penios first, Penios second)
        {
            return first.Length != second.Length;
        }
        public override bool Equals(object obj)
        {
            return this == obj as Penios;
        }
        public override int GetHashCode()
        {
            return Length;
        }
    }
}
