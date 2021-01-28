namespace Compression
{
    public class Compressor
    {
        public static void Compress(string inputFile, string outputFile)
        {
            var fas = new Fascimile(inputFile);
            fas.SaveToFile(outputFile);
        }

        public static void Decompress(string inputFile, string outputFile)
        {
            var fas = new Fascimile();
            fas.LoadFromFile(inputFile);
            fas.SavePaperToTextFile(outputFile);
        }
    }
}