namespace DeafTelephone.Web.Core.Extensions
{
    public static class StringExtensions
    {
        public static string Truncate(this string value, int maxChars)
        {
            const string truncate_symbols = "...";
            return value.Length <= maxChars
                ? value
                : $"{value.Substring(0, maxChars - truncate_symbols.Length)}{truncate_symbols}";
        }
    }
}
