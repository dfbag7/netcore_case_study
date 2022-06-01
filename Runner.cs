using NLog;

using ScanApp.Data;
using ScanApp.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Data.Sqlite;
using ScanApp.Utils;

namespace ScanApp
{
    internal class Runner
    {
        private const int HASHING_BUFFER_SIZE = 2048;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly Config _config;
        private readonly IHashesRepository _hashesRepository;

        public Runner(Config config, IHashesRepository hashesRepository)
        {
            _config = config;
            _hashesRepository = hashesRepository;
        }

        private (byte[] md5, byte[] sha1, byte[] sha256, long fileSize) _getFileInfo(string filePathName)
        {
            var inStream = File.Open(filePathName, FileMode.Open, FileAccess.Read, FileShare.Read);

            using var hasherMD5 = IncrementalHash.CreateHash(HashAlgorithmName.MD5);
            using var hasherSHA1 = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
            using var hasherSHA256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

            var buffer = new byte[HASHING_BUFFER_SIZE];
            int bytesRead;
            while((bytesRead = inStream.Read(buffer)) > 0)
            {
                hasherMD5.AppendData(buffer, 0, bytesRead);
                hasherSHA1.AppendData(buffer, 0, bytesRead);
                hasherSHA256.AppendData(buffer, 0, bytesRead);
            }

            return (
                hasherMD5.GetCurrentHash(),
                hasherSHA1.GetCurrentHash(),
                hasherSHA256.GetCurrentHash(),
                inStream.Length
            );
        }

        public int Run(string path)
        {
            _logger.Info($"Using connection string: {_config.ConnectionString}");

            int numberOfSuccessfullyProcessedFiles = 0;

            var enumerator = SafeFilesEnumerator.Enumerate(path);

            enumerator.AsParallel()
                .WithDegreeOfParallelism(_config.Parallelism) // no more than this number of tasks simultaneously
                .ForAll(filePathName => 
                {
                    try
                    {
                        (byte[] md5, byte[] sha1, byte[] sha256, long fileSize) = _getFileInfo(filePathName);

                        using var conn = new SqliteConnection(_config.ConnectionString);
                        var newEntry = _hashesRepository.UpsertBySha256(conn, new HashEntry
                        {
                            Md5 = md5,
                            Sha1 = sha1,
                            Sha256 = sha256,
                            Scanned = 1,
                            FileSize = fileSize,
                            LastSeen = DateTime.UtcNow,
                        });

                        Interlocked.Increment(ref numberOfSuccessfullyProcessedFiles);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Exception '{ex.Message}' when processing file '{filePathName}'");
                    }
                });

            return numberOfSuccessfullyProcessedFiles;
        }
    }
}
