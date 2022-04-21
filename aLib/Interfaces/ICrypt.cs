using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aLib.Interfaces
{
    interface ICrypto
    {
        string Encrypt(string data);
        string Decrypt(string data);
    }
}
