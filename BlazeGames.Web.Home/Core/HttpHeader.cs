using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlazeGames.Web.Core
{
    public class HttpHeader
    {
        public string Theme = "Default";
        public Dictionary<string, string> ThemeParameters = new Dictionary<string, string>();

        public List<string> Headers = new List<string>();

        public void AddCSS(string CSS)
        {
            if (Regex.IsMatch(CSS, @"(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?"))
                Headers.Add("<link href=\"" + CSS + "\" rel=\"stylesheet\" type=\"text/css\" />");
            else
                Headers.Add("<style type=\"text/css\">\r\n" + CSS + "\r\n</style>");
        }

        public void AddJS(string JS)
        {
            if (Regex.IsMatch(JS, @"(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?"))
                Headers.Add("<script type=\"text/javascript\" src=\"" + JS + "\"></script>");
            else
                Headers.Add("<script type=\"text/javascript\">\r\n" + JS + "\r\n</script>");
        }

        public void AddMetaTag(string Key, string Value)
        {
            Headers.Add("<meta name=\"" + Key + "\" content=\"" + Value + "\">");
        }

        public override string ToString()
        {
            return String.Join("\r\n\t\t", Headers.ToArray());
        }
    }
}