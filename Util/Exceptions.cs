using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Util
{
    public class NameDuplicateException : Exception
    {
        public string Name { get; private set; }

        public NameDuplicateException(string name)
            : base(String.Format("Client with name \"{0}\" is already connected\nTry connecting with other name", name))
        {
            Name = name;
        }
    }
}
