using ScanApp.Models;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dapper;

namespace ScanApp.Data
{
    internal class HashesRepository : IHashesRepository
    {
        public HashEntry? FindBySha256(IDbConnection conn, byte[] sha256)
        {
            return conn.Query<HashEntry>(Commands.FIND_BY_SHA256, new { sha256 }).FirstOrDefault();
        }

        public bool UpdateById(IDbConnection conn, HashEntry entry)
        {
            var affected = conn.Execute(Commands.UPDATE_BY_ID,
                new
                {
                    entry.Md5,
                    entry.Sha1,
                    entry.Sha256,
                    entry.Scanned,
                    entry.FileSize,
                    entry.LastSeen,
                    entry.Id,
                });

            return affected > 0;
        }

        public HashEntry UpsertBySha256(IDbConnection conn, HashEntry entry)
        {
            var result = conn.QueryFirst<HashEntry>(Commands.UPSERT_BY_SHA256,
                new
                {
                    entry.Md5,
                    entry.Sha1,
                    entry.Sha256,
                    entry.FileSize,
                    entry.LastSeen,
                });

            return result;
        }
    }
}
