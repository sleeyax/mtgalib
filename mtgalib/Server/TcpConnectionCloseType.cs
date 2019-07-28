namespace mtgalib.Server
{
    internal enum TcpConnectionCloseType
    {
        Unexpected,
        Expected,
        InvalidMessageFormat
    }
}