using System;
using Compression;

namespace Fascimile_Compression
{
    class Program
    {
        static void Main(string[] args)
        {
            Huffman huf = new Huffman("Tu tu ru tu tu tu ru tu broj je zauzet");
            huf.Print("");
            string compressed = huf.Compress();
            string decompressed = huf.Decompress(compressed);
            Console.WriteLine(decompressed);
        }
    }
}
