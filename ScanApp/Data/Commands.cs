using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Data
{
    internal class Commands
    {
        public const string FIND_BY_SHA256 = @"
            select id Id, md5 Md5, sha1 Sha1, sha256 Sha256, file_size FileSize, scanned Scanned, last_seen LastSeen
            from hashes
            where sha256 = @sha256
        ";

        public const string UPDATE_BY_ID = @"
            update hashes
               set md5 = @Md5, 
                   sha1 = @Sha1,
                   sha256 = @Sha256, 
                   scanned = @Scanned, 
                   file_size = @FileSize, 
                   last_seen = @LastSeen)
            where id = @Id
        ";

        public const string INSERT_ENTRY = @"
            insert into hashes(md5, sha1, sha256, scanned, file_size, last_seen)
            values (@Md5, @Sha1, @Sha256, 1, @FileSize, @LastSeen)
            returning *
        ";

        public const string UPSERT_BY_SHA256 = @"
            insert into hashes(md5, sha1, sha256, scanned, file_size, last_seen)
            values (@Md5, @Sha1, @Sha256, 1, @FileSize, @LastSeen)
            on conflict (sha256) do update set
            scanned = scanned + 1,
            last_seen = excluded.last_seen
            returning id Id, md5 Md5, sha1 Sha1, sha256 Sha256, file_size FileSize, scanned Scanned, last_seen LastSeen
        ";
    }
}
