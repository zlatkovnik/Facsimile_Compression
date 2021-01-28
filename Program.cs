using System;
using Compression;

namespace Fascimile_Compression
{
    class Program
    {
        static void Main(string[] args)
        {
            Compressor.Compress("input.txt", "output.bin");
            Compressor.Decompress("output.bin", "treci.txt");
        }
    }
}
