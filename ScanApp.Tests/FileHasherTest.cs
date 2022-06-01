using System.Text;

using FluentAssertions;

using ScanApp.Utils;

namespace ScanApp.Tests
{
    public class FileHasherTest
    {
        [Fact]
        public void HashesCalculatedCorrectly()
        {
            const string TEST_STRING = "The quick brown fox jumps over the lazy dog";
            
            var memoryStream = new MemoryStream();
            memoryStream.Write(Encoding.UTF8.GetBytes(TEST_STRING));
            memoryStream.Seek(0, SeekOrigin.Begin);

            var hasher = new FileHasher();

            (byte[] md5, byte[] sha1, byte[] sha256) = hasher.HashStream(memoryStream);

            md5.Should().Equal(Convert.FromHexString("9e107d9d372bb6826bd81d3542a419d6"));
            sha1.Should().Equal(Convert.FromHexString("2fd4e1c67a2d28fced849ee1bb76e7391b93eb12"));
            sha256.Should().Equal(Convert.FromHexString("d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592"));
        }
    }
}