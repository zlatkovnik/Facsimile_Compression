using System;
using System.Collections.Generic;
using C5;

namespace Compression
{
    public class Huffman
    {
        public Node Root { get; set; }
        public string Text { get; set; }
        public Huffman()
        {
            Root = null;
            Text = "";
        }
        public Huffman(string text)
        {
            Text = text;
            GenerateTree();
        }

        #region file
        private static List<Node> GetNodesFromString(string text)
        {
            //Hashmapa pojavljivanja karaktera
            //Indeks je karakter, a vrednost broj pojavljivanja
            uint[] arr = new uint[char.MaxValue + 1];
            int n = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (arr[text[i]] == 0)
                {
                    n++;
                }
                arr[text[i]]++;
            }

            //Lista cvorova od svih karaktera koji se pojavljuju u tekstu
            List<Node> nodes = new List<Node>(n);
            for (int i = 0; i < char.MaxValue + 1; i++)
            {
                //Ako je vece od 0 znaci da se pojavljuje
                if (arr[i] > 0)
                {
                    nodes.Add(new Node
                    {
                        Frequency = arr[i],
                        Character = Convert.ToChar(i),
                        Left = null,
                        Right = null
                    });
                }
            }
            return nodes;
        }

        #endregion

        #region API
        public virtual void GenerateTree()
        {
            List<Node> nodes = GetNodesFromString(Text);

            IntervalHeap<Node> pQueue = new IntervalHeap<Node>(nodes.Count);
            foreach (Node node in nodes)
            {
                pQueue.Add(node);
            }

            Root = null;

            while (pQueue.Count > 1)
            {
                Node first = pQueue.DeleteMin();
                Node second = pQueue.DeleteMin();
                Node mixed = new Node
                {
                    Frequency = first.Frequency + second.Frequency,
                    Character = (char)255,
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
            for (int i = 0; i < Text.Length; i++)
            {
                text += GetCode(Text[i]);
            }
            return text;
        }

        public string Decompress(string data)
        {
            string text = "";
            int i = 0;
            while (i < data.Length)
            {
                Node node = Root;
                while (!(node.Left == null && node.Right == null) && i < data.Length)
                {
                    if (data[i] == '0')
                        node = node.Left;
                    else
                        node = node.Right;
                    i++;
                }
                text += node.Character;
            }
            return text;
        }
        public void Print(string s)
        {
            PrintCode(Root, s);
        }

        #endregion

        #region Utility
        private string GetCode(char c)
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

        private static bool GetCode(Node node, System.Collections.Generic.LinkedList<byte> path, char c, byte code)
        {
            if (node == null)
                return false;
            //Ubacujem 0 ako idem levo, 1 ako idem desno
            if (code != 255)
                path.AddFirst(code);

            //Ako sam nasao karakter
            if (node.Character == c)
                return true;

            // Ako nisam nasao trazim levo pa desno
            if (GetCode(node.Left, path, c, 0) || GetCode(node.Right, path, c, 1))
                return true;

            // Ako nije ni u levom ni u desnom podstablu izbacuje se putanja  
            path.RemoveFirst();
            return false;
        }
        private static void PrintCode(Node node, string s)
        {
            if (node.Left == null && node.Right == null && node.Character != (char)255)
            {
                Console.WriteLine(node.Character + ":" + s);
                return;
            }
            PrintCode(node.Left, s + "0");
            PrintCode(node.Right, s + "1");
        }
        #endregion
    }
}