using System;
using Compression;

namespace Facsimile_Compression
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(args[0]);
            if (args.Length < 3 || args[2] == "c")
            {
                Facsimile.Compress(args[0], args[1]);
            }
            else if (args[2] == "d")
            {
                Facsimile.Decompress(args[0], args[1]);
            }
            else
            {
                Console.WriteLine("Invalid input");
                Console.WriteLine("-{input} -{output} -optional:{'c'-compression, 'd'-decompression}");
            }
        }
    }
}
