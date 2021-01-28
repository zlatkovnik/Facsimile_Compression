using System;
using Compression;

namespace Fascimile_Compression
{
    class Program
    {
        static void Main(string[] args)
        {
            //Kodira se jedan nothing na kraj
            Color[] paper = new Color[] { Color.White, Color.White, Color.Black, Color.Black, Color.White, Color.Nothing };
            Fascimile fas = new Fascimile(paper);
            fas.Print();
            string compressed = fas.Compress();
            string decompressed = fas.Decompress(compressed);
            Console.WriteLine(decompressed);
        }
    }
}
