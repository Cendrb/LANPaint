using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Ink;
using System.Windows.Input;
using Util;

namespace penis
{
    class Program
    {
        static void Main(string[] args)
        {
            SignedStroke falus = null;
            StylusPointCollection porn = new StylusPointCollection();
            porn.Add(new StylusPoint(69, 69));
            SignedStroke dildus = new SignedStroke(porn);

            bool dildo = dildus != null;

            String gay = "dan";

            Task.Factory.StartNew(new Action(() => fap(gay)));
            Task.Factory.StartNew(new Action(() => paf(gay)));

            Console.WriteLine("penis".GetHashCode());
            Console.WriteLine("penis".GetHashCode());
            Console.WriteLine("peniss".GetHashCode());
            Console.ReadKey();
        }

        private static void fap(string gay)
        {
            lock(gay)
            {
                Console.WriteLine(gay);
                Console.WriteLine("fap begin");
                Thread.Sleep(3000);
                Console.WriteLine("fap end");
            }
        }

        private static void paf(string gay)
        {
            lock (gay)
            {
                Console.WriteLine(gay);
                Console.WriteLine("paf begin");
                Thread.Sleep(3000);
                Console.WriteLine("paf end");
            }
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
