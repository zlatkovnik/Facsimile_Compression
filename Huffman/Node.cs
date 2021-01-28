using System;

namespace Compression
{
    public class Node : IComparable
    {
        public uint Frequency { get; set; }
        public char Character { get; set; }
        public Node Left { get; set; }
        public Node Right { get; set; }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            Node otherNode = obj as Node;
            if (otherNode != null)
                return this.Frequency.CompareTo(otherNode.Frequency);
            else
                throw new ArgumentException("Object is not a Node");
        }
    }
}