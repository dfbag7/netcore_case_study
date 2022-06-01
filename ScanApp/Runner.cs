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
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly Config _config;
        private readonly IHashesRepository _hashesRepository;
        private readonly IFileHasher _fileHasher;

        public Runner(Config config, IHashesRepository hashesRepository, IFileHasher fileHasher)
        {
            _config = config;
            _hashesRepository = hashesRepository;
            _fileHasher = fileHasher;
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
                        using var inStream = File.Open(filePathName, FileMode.Open, FileAccess.Read, FileShare.Read);

                        (byte[] md5, byte[] sha1, byte[] sha256) = _fileHasher.HashStream(inStream);

                        using var conn = new SqliteConnection(_config.ConnectionString);
                        var newEntry = _hashesRepository.UpsertBySha256(conn, new HashEntry
                        {
                            Md5 = md5,
                            Sha1 = sha1,
                            Sha256 = sha256,
                            Scanned = 1,
                            FileSize = inStream.Length,
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
