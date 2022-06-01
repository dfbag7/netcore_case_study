using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Utils
{
    public class FileHasher : IFileHasher
    {
        private const int HASHING_BUFFER_SIZE = 2048;

        public (byte[] md5, byte[] sha1, byte[] sha256) HashStream(Stream inStream)
        {
            using var hasherMD5 = IncrementalHash.CreateHash(HashAlgorithmName.MD5);
            using var hasherSHA1 = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
            using var hasherSHA256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

            var buffer = new byte[HASHING_BUFFER_SIZE];
            int bytesRead;
            while ((bytesRead = inStream.Read(buffer)) > 0)
            {
                hasherMD5.AppendData(buffer, 0, bytesRead);
                hasherSHA1.AppendData(buffer, 0, bytesRead);
                hasherSHA256.AppendData(buffer, 0, bytesRead);
            }

            return (
                hasherMD5.GetCurrentHash(),
                hasherSHA1.GetCurrentHash(),
                hasherSHA256.GetCurrentHash()
            );
        }
    }
}
