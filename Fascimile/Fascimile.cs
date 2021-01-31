using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using C5;
using System.Text;

namespace Compression
{
    public static class Fascimile
    {


        #region API

        public static void Compress(string inputFile, string outputFile)
        {
            string data = "";
            data = System.IO.File.ReadAllText(inputFile);
            Color[] colors = GetColorsFromString(data);
            List<Code> codedPaper = GetCodedPaper(colors);
            FNode root = GenerateTree(codedPaper);
            string text = "";
            for (int i = 0; i < codedPaper.Count; i++)
            {
                text += GetPath(root, codedPaper[i]);
            }
            SaveToFile(root, codedPaper, outputFile);
            PrintCode(root, " ");
        }

        public static void Decompress(string inputFile, string outputFile)
        {

            var res = LoadFromFile(inputFile);
            FNode root = res.Item1;
            List<Code> codedPaper = res.Item2;
            SavePaperToTextFile(codedPaper, outputFile);
            PrintCode(root, " ");
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
                SavePaperToFileOptimized(root, codedPaper, writer);
                //SavePaperToFile(root, codedPaper, writer);
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
                writer.Write((byte)runLength);
                SaveTreeToFile(node.Left, writer);
                SaveTreeToFile(node.Right, writer);
            }
        }

        private static void SavePaperToFile(FNode root, List<Code> codes, BinaryWriter writer)
        {
            for (int i = 0; i < codes.Count; i++)
            {
                string code = GetPath(root, codes[i]);
                byte[] bytes = Encoding.ASCII.GetBytes(code);
                //Pretvaram ascii u byte
                for (int j = 0; j < bytes.Length; j++)
                    bytes[j] -= (byte)'0';
                writer.Write(bytes);
            }
        }

        private static void SavePaperToFileOptimized(FNode root, List<Code> codes, BinaryWriter writer)
        {
            int bitPosition = 7;
            byte buf = 0;
            for (int i = 0; i < codes.Count; i++)
            {
                string path = GetPath(root, codes[i]);
                for (int j = 0; j < path.Length; j++)
                {
                    byte bit = (byte)(path[j] - '0');
                    bit = (byte)(bit << bitPosition);
                    bit = (byte)(bit & (0x01 << bitPosition));
                    buf |= (byte)(bit);
                    bitPosition--;
                    if (bitPosition < 0)
                    {
                        writer.Write(buf);
                        buf = 0;
                        bitPosition = 7;
                    }
                }
            }
            if (bitPosition > 0)
            {
                writer.Write(buf);
            }
        }

        private static Tuple<FNode, List<Code>> LoadFromFile(string fileName)
        {
            FNode root;
            List<Code> codedPaper;
            using (BinaryReader reader = new BinaryReader(new FileStream(fileName, FileMode.Open)))
            {
                root = ReadTreeFromFile(reader);
                codedPaper = ReadCodedPaperFromFileOptimized(root, reader);
            }
            return new Tuple<FNode, List<Code>>(root, codedPaper);
        }

        private static FNode ReadTreeFromFile(BinaryReader reader)
        {
            FNode root = new FNode();
            ReadTreeFromFile(ref root, reader);
            return root;
        }

        private static void ReadTreeFromFile(ref FNode node, BinaryReader reader)
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
            ReadTreeFromFile(ref node.Left, reader);
            ReadTreeFromFile(ref node.Right, reader);
        }

        private static List<Code> ReadCodedPaperFromFileOptimized(FNode root, BinaryReader reader)
        {
            List<Code> codedPaper = new List<Code>();
            FNode node = root;
            byte buf = reader.ReadByte();
            int bitPosition = 7;
            //Console.WriteLine(reader.BaseStream.Position + " " + reader.BaseStream.Length);
            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                while (!(node.Left == null && node.Right == null))
                {
                    byte bit = (byte)((buf >> bitPosition) & 0x01);
                    if (bit == 0)
                    {
                        node = node.Left;
                    }
                    else
                    {
                        node = node.Right;
                    }
                    bitPosition--;
                    if (bitPosition < 0)
                    {
                        bitPosition = 7;
                        buf = reader.ReadByte();
                    }
                }
                codedPaper.Add(new Code(node.Code.Color, node.Code.RunLength));
                node = root;
            }
            return codedPaper;
        }

        private static List<Code> ReadCodedPaperFromFile(FNode root, BinaryReader reader)
        {
            List<Code> codedPaper = new List<Code>();
            while (reader.BaseStream.Position != reader.BaseStream.Length - 1)
            {
                FNode node = root;
                while (!(node.Left == null && node.Right == null))
                {
                    byte turn = reader.ReadByte();
                    if (turn == 0)
                    {
                        node = node.Left;
                    }
                    else
                    {
                        node = node.Right;
                    }
                }
                Code code = new Code(node.Code.Color, node.Code.RunLength);
                codedPaper.Add(code);
            }
            return codedPaper;
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
        private static string GetPath(FNode root, Code c)
        {
            var path = new System.Collections.Generic.LinkedList<byte>();
            GetPath(root, path, c, 255);
            string code = "";
            foreach (byte direction in path)
            {
                code = direction + code;
            }
            return code;
        }

        private static bool GetPath(FNode node, System.Collections.Generic.LinkedList<byte> path, Code c, byte code)
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
            if (GetPath(node.Left, path, c, 0) || GetPath(node.Right, path, c, 1))
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