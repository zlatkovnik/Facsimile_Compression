using System;
using System.Collections.Generic;
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
        }
        public Fascimile(Color[] paper)
        {
            Root = null;
            CodedPaper = GetCodedPaper(paper);
            GenerateTree();
        }

        #region file
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
        #endregion

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

        #region Utility
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