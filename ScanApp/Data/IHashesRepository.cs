using ScanApp.Models;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Data
{
    internal interface IHashesRepository
    {
        public HashEntry? FindBySha256(IDbConnection conn, byte[] sha256);
        public bool UpdateById(IDbConnection conn, HashEntry entry);
        public HashEntry UpsertBySha256(IDbConnection conn, HashEntry entry);
    }
}
