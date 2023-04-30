namespace RaimeWebScraper
{
    public static class CreateIDExtensions
    {
        public static string CreateID(this string s)
        {
            return s.ToLower().Replace(" ", "-");
        }
    }
}