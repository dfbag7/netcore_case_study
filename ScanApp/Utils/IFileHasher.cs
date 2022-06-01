using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Utils
{
    internal interface IFileHasher
    {
        (byte[] md5, byte[] sha1, byte[] sha256) HashStream(Stream inStream);
    }
}
