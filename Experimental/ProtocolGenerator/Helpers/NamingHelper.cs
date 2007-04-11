namespace ProtocolGenerator.Helpers
{
    internal static class NamingHelper
    {
        public static string GetPrefix(string name, string conventionName)
        {
            string prefix;
            int pos = name.LastIndexOf(conventionName);
            if (0 < pos)
            {
                prefix = name.Substring(0, pos);
            }
            else
            {
                prefix = name;
            }
            return prefix;
        }
    }
}