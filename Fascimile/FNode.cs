using System;

namespace Compression
{
    public enum Color
    {
        Nothing = 2,
        White = 0,
        Black = 1
    }
    public class FNode : IComparable
    {
        public uint Frequency { get; set; }
        public Code Code { get; set; }
        public FNode Left { get; set; }
        public FNode Right { get; set; }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            FNode otherNode = obj as FNode;
            if (otherNode != null)
                return this.Frequency.CompareTo(otherNode.Frequency);
            else
                throw new ArgumentException("Object is not a Node");
        }
    }
}