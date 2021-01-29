using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using C5;

namespace Compression
{
    public enum FileType
    {
        Text = 0,
        PNG = 1,
        Binary = 2
    }
    public static class Fascimile
    {


        #region API

        public static void Compress(string inputFile, string outputFile, FileType type)
        {
            string data = "";
            if (type == FileType.Text)
            {
                data = System.IO.File.ReadAllText(inputFile);
            }
            else if (type == FileType.PNG)
            {
                Bitmap bitmap = new Bitmap(inputFile);
                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        var pixel = bitmap.GetPixel(x, y);
                        var brightness = (byte)(pixel.R * 0.2126 + pixel.G * 0.7152 + pixel.B * 0.0722);
                        if (brightness < 128)
                        {
                            data += '1';
                        }
                        else
                        {
                            data += '0';
                        }
                    }
                }
                bitmap.Dispose();
            }
            Color[] colors = GetColorsFromString(data);
            List<Code> codedPaper = GetCodedPaper(colors);
            FNode root = GenerateTree(codedPaper);
            string text = "";
            for (int i = 0; i < codedPaper.Count; i++)
            {
                text += GetCode(root, codedPaper[i]);
            }
            SaveToFile(root, codedPaper, outputFile);

            PrintCode(root, " ");
        }

        public static void Decompress(string inputFile, string outputFile, FileType type)
        {
            FNode root = null;
            List<Code> codedPaper = null;
            LoadFromFile(ref root, ref codedPaper, inputFile);
            if (type == FileType.Text)
            {
                SavePaperToTextFile(codedPaper, outputFile);
            }
            else
            {

            }
        }

        #endregion
        private static FNode GenerateTree(List<Code> codedPaper)
        {
            List<FNode> nodes = GetNodesFromCodes(codedPaper);

            IntervalHeap<FNode> pQueue = new IntervalHeap<FNode>(nodes.Count);
            foreach (FNode node in nodes)
            {
                pQueue.Add(node);
            }

            FNode root = null;

            while (pQueue.Count > 1)
            {
                FNode first = pQueue.DeleteMin();
                FNode second = pQueue.DeleteMin();
                FNode mixed = new FNode
                {
                    Frequency = first.Frequency + second.Frequency,
                    Code = new Code(Color.Nothing, 0),
                    Left = first,
                    Right = second
                };
                root = mixed;
                pQueue.Add(mixed);
            }
            return root;
        }



        private static void SavePaperToTextFile(List<Code> codedPaper, string fileName)
        {
            using (TextWriter writer = File.CreateText(fileName))
            {
                for (int i = 0; i < codedPaper.Count; i++)
                {
                    for (int j = 0; j < codedPaper[i].RunLength; j++)
                    {
                        writer.Write((byte)codedPaper[i].Color);
                    }
                }
                writer.Write((byte)Color.Nothing);
            }
        }

        private static void SaveToFile(FNode root, List<Code> codedPaper, string fileName)
        {
            using (BinaryWriter writer = new BinaryWriter(new FileStream(fileName, FileMode.Create)))
            {
                SaveTreeToFile(root, writer);
                SavePaperToFile(codedPaper, writer);
                writer.Write((byte)Color.Nothing);
            }
        }

        private static void SaveTreeToFile(FNode node, BinaryWriter writer)
        {
            if (node == null)
            {
                writer.Write('#');
            }
            else
            {
                byte color = (byte)node.Code.Color;
                uint runLength = node.Code.RunLength;
                writer.Write(color);
                writer.Write(runLength);
                SaveTreeToFile(node.Left, writer);
                SaveTreeToFile(node.Right, writer);
            }
        }

        private static void SavePaperToFile(List<Code> codes, BinaryWriter writer)
        {
            for (int i = 0; i < codes.Count; i++)
            {
                writer.Write((byte)codes[i].Color);
                writer.Write(codes[i].RunLength);
            }
        }

        private static void LoadFromFile(ref FNode root, ref List<Code> codedPaper, string fileName)
        {
            using (BinaryReader reader = new BinaryReader(new FileStream(fileName, FileMode.Open)))
            {
                ReadTreeFromFile(root, reader);
                codedPaper = new List<Code>();
                ReadCodedPaperFromFile(codedPaper, reader);
            }
        }

        private static void ReadTreeFromFile(FNode node, BinaryReader reader)
        {
            byte color = reader.ReadByte();
            if (color == '#')
            {
                return;
            }
            uint rl = reader.ReadUInt32();
            node = new FNode
            {
                Code = new Code((Color)color, rl),
                Frequency = 1,
                Left = null,
                Right = null
            };
            ReadTreeFromFile(node.Left, reader);
            ReadTreeFromFile(node.Right, reader);
        }

        private static void ReadCodedPaperFromFile(List<Code> codedPaper, BinaryReader reader)
        {
            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                byte color = reader.ReadByte();
                if ((Color)color == Color.Nothing)
                {
                    break;
                }
                uint rl = reader.ReadUInt32();
                codedPaper.Add(new Code((Color)color, rl));
            }
        }



        private static List<FNode> GetNodesFromCodes(List<Code> codes)
        {

            var map = new Dictionary<Code, FNode>();
            for (int i = 0; i < codes.Count; i++)
            {
                //Ako se vec pojavljivala ova varijanta
                FNode node;
                if (map.TryGetValue(codes[i], out node))
                {
                    node.Frequency++;
                }
                //Ako se prvi put pojavljuje
                else
                {
                    node = new FNode
                    {
                        Code = codes[i],
                        Frequency = 1,
                        Left = null,
                        Right = null
                    };
                    map.Add(codes[i], node);
                }
            }

            //Lista cvorova od svih varijanti ponavljanja
            List<FNode> nodes = new List<FNode>();
            foreach (var keyValuePair in map)
            {
                nodes.Add(keyValuePair.Value);
            }
            return nodes;
        }
        private static List<Code> GetCodedPaper(Color[] colors)
        {
            var nodes = new List<Code>();
            uint count = 1;
            Color color = colors[0];
            for (int i = 1; i < colors.Length; i++)
            {
                //Ako se promenila boja
                if (color != colors[i])
                {
                    nodes.Add(new Code(color, count));
                    color = colors[i];
                    count = 1;
                }
                else
                {
                    count++;
                }
            }
            return nodes;
        }
        private static Color[] GetColorsFromString(string data)
        {
            Color[] colors = new Color[data.Length + 1];
            for (int i = 0; i < data.Length; i++)
            {
                switch (data[i])
                {
                    case '1':
                        colors[i] = Color.Black;
                        break;
                    case '0':
                        colors[i] = Color.White;
                        break;
                    default:
                        colors[i] = Color.Nothing;
                        break;
                }
            }
            //Ovo radim zato sto zadnji element mora da bude Color.Nothing
            colors[colors.Length - 1] = Color.Nothing;
            return colors;
        }
        private static string GetCode(FNode root, Code c)
        {
            var path = new System.Collections.Generic.LinkedList<byte>();
            GetCode(root, path, c, 255);
            string code = "";
            foreach (byte direction in path)
            {
                code = direction + code;
            }
            return code;
        }

        private static bool GetCode(FNode node, System.Collections.Generic.LinkedList<byte> path, Code c, byte code)
        {
            if (node == null)
                return false;
            //Ubacujem 0 ako idem levo, 1 ako idem desno
            if (code != 255)
                path.AddFirst(code);

            //Ako sam nasao karakter
            if (node.Code.Equals(c))
                return true;

            // Ako nisam nasao trazim levo pa desno
            if (GetCode(node.Left, path, c, 0) || GetCode(node.Right, path, c, 1))
                return true;

            // Ako nije ni u levom ni u desnom podstablu izbacuje se putanja  
            path.RemoveFirst();
            return false;
        }
        private static void PrintCode(FNode node, string s)
        {
            if (node.Left == null && node.Right == null)
            {
                Console.WriteLine(node.Code.Color + " " + node.Code.RunLength + ": " + s);
                return;
            }
            PrintCode(node.Left, s + "0");
            PrintCode(node.Right, s + "1");
        }
    }


}