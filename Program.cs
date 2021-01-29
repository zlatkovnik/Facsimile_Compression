using System;
using Compression;

namespace Fascimile_Compression
{
    class Program
    {
        static void Main(string[] args)
        {
            Fascimile.Compress("input.txt", "output.bin", FileType.Text);
            //Fascimile.Decompress("output.bin", "treci.txt", FileType.Text);
        }
    }
}
