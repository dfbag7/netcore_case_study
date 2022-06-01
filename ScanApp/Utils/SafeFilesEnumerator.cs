using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NLog;

namespace ScanApp.Utils
{
    /// <summary>
    /// The rationale for this class is that current implementation of the <see cref="Directory.EnumerateFiles()"/> method 
    /// fails with <see cref="UnauthorizedAccessException"/> when it encounters a folder which the user doesn't have access to, 
    /// and there is no way to continue enumeration ignoring the error.
    /// 
    /// So, I had to implement a custom enumerator class which overcomes that limitation.
    /// </summary>
    internal static class SafeFilesEnumerator
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private static IEnumerable<string> _enumerateFilesInDirectory(string currentPath)
        {
            try
            {
                return Directory.EnumerateFiles(currentPath, "*", SearchOption.TopDirectoryOnly);
            }
            catch(UnauthorizedAccessException ex)
            {
                _logger.Error($"Exception '{ex.Message}' when enumerating files in '{currentPath}'");
                return Enumerable.Empty<string>();
            }
        }

        private static IEnumerable<string> _enumerateSubDirectories(string currentPath)
        {
            try
            {
                return Directory.EnumerateDirectories(currentPath, "*", SearchOption.TopDirectoryOnly);
            }
            catch(UnauthorizedAccessException)
            {
                // Dont log error message here because it would be just a duplicate.
                // Corresponding message is already logged when enumerating files.
                return Enumerable.Empty<string>();
            }
        }

        public static IEnumerable<string> Enumerate(string path)
        {
            var dirStack = new Stack<string>();
            dirStack.Push(path);

            while(dirStack.Any())
            {
                var current = dirStack.Pop();

                foreach(var fileEntry in _enumerateFilesInDirectory(current))
                {
                    yield return fileEntry;
                }

                foreach(var dirEntry in  _enumerateSubDirectories(current))
                {
                    dirStack.Push(dirEntry);
                }
            }
        }
    }
}
