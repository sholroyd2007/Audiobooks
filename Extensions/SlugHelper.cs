using Audiobooks.Models;
using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Audiobooks.Helpers
{
    public static class SlugHelper
    {
        // white space, em-dash, en-dash, underscore
        static readonly Regex WordDelimiters = new Regex(@"[\s—–_]", RegexOptions.Compiled);

        // characters that are not valid
        static readonly Regex InvalidChars = new Regex(@"[^a-z0-9\-%]", RegexOptions.Compiled);

        // multiple hyphens
        static readonly Regex MultipleHyphens = new Regex(@"-{2,}", RegexOptions.Compiled);

        public static string Slugify(string value)
        {
            string uriEscapedData = !string.IsNullOrWhiteSpace(value) ? Uri.EscapeDataString(value) : "";
            string replacedData = ReplaceSpecialCharacters(uriEscapedData);
            var generatedSlug = ToUrlSlug(replacedData);
            return generatedSlug;
        }

        public static string Slugify(this Entity value)
        {
            string uriEscapedData = !string.IsNullOrWhiteSpace(value.Name) ? Uri.EscapeDataString(value.Name) : "";
            string replacedData = ReplaceSpecialCharacters(uriEscapedData);
            var generatedSlug = ToUrlSlug(replacedData);
            return string.Concat(generatedSlug, $"-{value.Id}");
        }

        private static string ToUrlSlug(string value)
        {
            // convert to lower case
            value = value.ToLowerInvariant();

            // remove diacritics (accents)
            value = RemoveDiacritics(value);

            // ensure all word delimiters are hyphens
            value = WordDelimiters.Replace(value, "-");

            // strip out invalid characters
            value = InvalidChars.Replace(value, "");

            // replace multiple hyphens (-) with a single hyphen
            value = MultipleHyphens.Replace(value, "-");

            // trim hyphens (-) from ends
            return value.Trim('-');
        }

        private static string RemoveDiacritics(string stIn)
        {
            string stFormD = stIn.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }

            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }

        private static string ReplaceSpecialCharacters(string input)
        {
            input = input.Replace("%20", "-");
            input = input.Replace("%2F", "-");
            input = input.Replace("(", "-");
            input = input.Replace(")", "-");
            input = input.Replace("?", "-");
            input = input.Replace(",", "-");
            input = input.Replace(":", "-");
            input = input.Replace(";", "-");
            input = input.Replace("|", "-");
            input = input.Replace("\\", "-");
            input = input.Replace("%", "-");
            input = input.Replace("\"", "-");
            input = input.Replace("'", "-");
            input = input.Replace("$", "-");
            input = input.Replace("+", "-");
            input = input.Replace("=", "-");
            input = input.Replace("&", "-");
            input = input.Replace("#", "-");
            input = input.Replace("{", "-");
            input = input.Replace("}", "-");
            input = input.Replace("[", "-");
            input = input.Replace("]", "-");
            return input;
        }
    }
}
