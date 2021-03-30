namespace mytraceroute
{
    class Program
    {
        private static Tracert traceroute = new Tracert();
        private const string DNS_VIEW_PARAM = "-d";

        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                traceroute.Start(args[0], false);
            }
            if (args.Length > 1)
            {
                traceroute.Start(args[0], args[1].Equals(DNS_VIEW_PARAM));
            }
        }
    }
}