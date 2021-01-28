using System;
using System.Collections.Generic;
using System.IO;
using C5;

namespace Compression
{
    public class Fascimile
    {
        public FNode Root { get; set; }
        public List<Code> CodedPaper { get; set; }
        public Fascimile()
        {
            Root = null;
            CodedPaper = null;
        }

        public Fascimile(string fileName)
        {
            Root = null;
            string data = System.IO.File.ReadAllText(fileName);
            Color[] colors = GetColorsFromString(data);
            CodedPaper = GetCodedPaper(colors);
            GenerateTree();

        }


        #region API
        public virtual void GenerateTree()
        {
            List<FNode> nodes = GetNodesFromCodes(CodedPaper);

            IntervalHeap<FNode> pQueue = new IntervalHeap<FNode>(nodes.Count);
            foreach (FNode node in nodes)
            {
                pQueue.Add(node);
            }

            Root = null;

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
                Root = mixed;
                pQueue.Add(mixed);
            }
        }

        public string Compress()
        {
            string text = "";
            for (int i = 0; i < CodedPaper.Count; i++)
            {
                text += GetCode(CodedPaper[i]);
            }
            return text;
        }

        public string Decompress(string data)
        {
            string text = "";
            int i = 0;
            while (i < data.Length)
            {
                FNode node = Root;
                while (!(node.Left == null && node.Right == null) && i < data.Length)
                {
                    if (data[i] == '0')
                        node = node.Left;
                    else
                        node = node.Right;
                    i++;
                }
                for (int j = 0; j < node.Code.RunLength; j++)
                {
                    text += (int)node.Code.Color;
                }
            }
            return text;
        }
        public void Print()
        {
            PrintCode(Root, "");
        }

        #endregion

        #region file

        public void SavePaperToTextFile(string fileName)
        {
            using (TextWriter writer = File.CreateText(fileName))
            {
                for (int i = 0; i < CodedPaper.Count; i++)
                {
                    for (int j = 0; j < CodedPaper[i].RunLength; j++)
                    {
                        writer.Write((byte)CodedPaper[i].Color);
                    }
                }
                writer.Write((byte)Color.Nothing);
            }
        }

        public void SaveToFile(string fileName)
        {
            using (BinaryWriter writer = new BinaryWriter(new FileStream(fileName, FileMode.Create)))
            {
                SaveTreeToFile(Root, writer);
                SavePaperToFile(CodedPaper, writer);
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

        public void LoadFromFile(string fileName)
        {
            using (BinaryReader reader = new BinaryReader(new FileStream(fileName, FileMode.Open)))
            {
                ReadTreeFromFile(Root, reader);
                CodedPaper = new List<Code>();
                ReadCodedPaperFromFile(CodedPaper, reader);
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

        public static void ReadCodedPaperFromFile(List<Code> codedPaper, BinaryReader reader)
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

        #endregion

        #region Utility


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
        private List<Code> GetCodedPaper(Color[] colors)
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
        private Color[] GetColorsFromString(string data)
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
        private string GetCode(Code c)
        {
            var path = new System.Collections.Generic.LinkedList<byte>();
            GetCode(Root, path, c, 255);
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
        #endregion
    }
}