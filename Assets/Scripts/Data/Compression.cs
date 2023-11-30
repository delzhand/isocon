using System.IO;
using System.IO.Compression;
using System.Text;

public class Compression
{
    public static byte[] CompressString(string input)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);

        using (MemoryStream outputStream = new MemoryStream())
        {
            using (DeflateStream gzipStream = new DeflateStream(outputStream, CompressionMode.Compress))
            {
                gzipStream.Write(inputBytes, 0, inputBytes.Length);
            }

            return outputStream.ToArray();
        }
    }

    public static string DecompressString(byte[] compressedBytes)
    {
        using (MemoryStream inputStream = new MemoryStream(compressedBytes))
        {
            using (DeflateStream gzipStream = new DeflateStream(inputStream, CompressionMode.Decompress))
            {
                using (StreamReader reader = new StreamReader(gzipStream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}