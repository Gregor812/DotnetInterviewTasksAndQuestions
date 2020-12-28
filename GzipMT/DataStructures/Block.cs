namespace GzipMT.DataStructures
{
    public abstract class Block
    {
        public int Number { get; set; }
        public byte[] Data { get; set; }
    }
}
