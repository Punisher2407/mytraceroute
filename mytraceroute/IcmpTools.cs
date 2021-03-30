namespace mytraceroute
{
    public static class IcmpTools
    {
        // ICMP-package
        public static byte[] GetEchoIcmpPackage()
        {
            byte[] package = new byte[32];
            package[0] = 8; // Type
            package[1] = 0; // Code
            package[2] = 0xF7;
            package[3] = 0xFF;
            return package;
        }

        public static int GetIcmpType(byte[] data)
        {
            return data[20]; // [20] <--> TYPE
        }
    }
}
