namespace Compression
{
    public class Code
    {
        public uint RunLength { get; set; }
        public Color Color { get; set; }

        public Code(Color color, uint runLength)
        {
            Color = color;
            RunLength = runLength;
        }

        public override int GetHashCode()
        {
            return (Color.Black == Color) ? (int)RunLength : -(int)RunLength;
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as Code);
        }

        public bool Equals(Code obj)
        {
            return obj != null && obj.Color == this.Color && obj.RunLength == this.RunLength;
        }
    }
}