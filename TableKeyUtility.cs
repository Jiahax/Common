namespace Common
{
    using System.Text.RegularExpressions;

    public static class TableKeyUtility
    {
        public const string InvalidTableKeyCharactersRegexString = @"[\u0000-\u001f\u007f-\u009f\/\\\#\?]";

        public static string EncodeInvalidTableKeyCharacters(string key)
        {
            Guard.ArgumentNotNull(key, nameof(key));

            return Regex.Replace(key, InvalidTableKeyCharactersRegexString, m => m.ToString().ToBase64String());
        }
    }
}
