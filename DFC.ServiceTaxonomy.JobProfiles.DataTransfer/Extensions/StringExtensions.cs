using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Extensions
{
    internal static class StringExtensions
    {
        public static string HTMLToText(this string htmlString)
        {
            // Remove new lines since they are not visible in HTML and Remove tab spaces
            htmlString = htmlString.Replace("\n", " ").Replace("\t", " ");
            // Remove multiple white spaces from HTML
            htmlString = Regex.Replace(htmlString, "\\s+", " ");
            // Remove HEAD tag
            htmlString = Regex.Replace(htmlString, "<head.*?</head>", ""
                                , RegexOptions.IgnoreCase | RegexOptions.Singleline);
            // Remove any JavaScript
            htmlString = Regex.Replace(htmlString, "<script.*?</script>", ""
              , RegexOptions.IgnoreCase | RegexOptions.Singleline);
            // Replace special characters like &, <, >, " etc.
            StringBuilder sbHTML = new StringBuilder(htmlString);
            // Note: There are many more special characters, these are just
            // most common. You can add new characters in this arrays if needed
            string[] OldWords = { "&nbsp;", "&amp;", "&quot;", "&lt;", "&gt;", "&reg;", "&copy;", "&bull;", "&trade;", "&#39;" };
            string[] NewWords = { " ", "&", "\"", "<", ">", "Â®", "Â©", "â€¢", "â„¢", "\'" };
            for (int i = 0; i < OldWords.Length; i++)
            {
                sbHTML.Replace(OldWords[i], NewWords[i]);
            }
            // Check if there are line breaks (<br>) or paragraph (<p>)
            sbHTML = new StringBuilder(Regex.Replace(sbHTML.ToString(), "^<ul><li>|</li></ul>$", ""));
            sbHTML.Replace("<br>", "\n<br>").Replace("<br ", "\n<br ").Replace("</p>", "  </p>").Replace("<li", "  <li").Replace("<ul", "  <ul");
            // Finally, remove all HTML tags and return plain text
            return Regex.Replace(
              sbHTML.ToString(), "<[^>]*>", "");
        }

        public static string FirstCharToUpper(this string input)
        {
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(input);
        }

        public static string GetSlugValue(this string input)
        {
            string UrlNameRegexPattern = @"[^\w\-\!\$\'\(\)\=\@\d_]+";
            return string.IsNullOrWhiteSpace(input) ? string.Empty : Regex.Replace(input.ToLower().Trim(), UrlNameRegexPattern, "-");
        }

        public static string GetHyphenated(this string input)
        {
            return input.Replace(" ", "-");
        }
    }
}
