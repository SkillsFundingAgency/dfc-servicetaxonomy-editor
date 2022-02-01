using System.Text;
using System.Text.RegularExpressions;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.Extensions
{
    internal static class StringExtensions
    {
        public static string HTMLToText(this string htmlString)
        {
            // Remove new lines since they are not visible in HTML
            htmlString = htmlString.Replace("\n", " ");
            // Remove tab spaces
            htmlString = htmlString.Replace("\t", " ");
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
            string[] OldWords = {"&nbsp;", "&amp;", "&quot;", "&lt;", "&gt;", "&reg;", "&copy;", "&bull;", "&trade;","&#39;"};
            string[] NewWords = { " ", "&", "\"", "<", ">", "Â®", "Â©", "â€¢", "â„¢", "\'" };
            for (int i = 0; i < OldWords.Length; i++)
            {
                sbHTML.Replace(OldWords[i], NewWords[i]);
            }
            // Check if there are line breaks (<br>) or paragraph (<p>)
            sbHTML.Replace("<br>", "\n<br>");
            sbHTML.Replace("<br ", "\n<br ");
            sbHTML.Replace("</p> ", "  </p>");
            sbHTML.Replace("<li ", "  <li");
            sbHTML.Replace("<ul ", "  <ul");
            //sbHTML.Replace("<p ", "\n<p ");
            // Finally, remove all HTML tags and return plain text
            return Regex.Replace(
              sbHTML.ToString(), "<[^>]*>", "");
        }
    }
}
