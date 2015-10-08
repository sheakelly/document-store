namespace Prim
{
    public static class StringExtension
    {
        public static string Pluralise(this string word)
        {
            return word.EndsWith("s") ? word : word + "s";
        }
    }
}
