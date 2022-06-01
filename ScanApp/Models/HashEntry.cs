using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Models
{
    internal class HashEntry
    {
        public int Id { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public byte[] Md5 { get; set; }
        public byte[] Sha1 { get; set; }
        public byte[] Sha256 { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public int Scanned { get; set; }
        public long FileSize { get; set; }
        public DateTime LastSeen { get; set; }
    }
}
