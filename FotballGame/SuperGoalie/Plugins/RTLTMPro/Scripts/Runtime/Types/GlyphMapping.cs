namespace RTLTMPro
{
    public struct GlyphMapping
    {
        public readonly int From;
        public readonly int To;

        public GlyphMapping(int from, int to)
        {
            From = from;
            To = to;
        }
    }
}
