using System.Text.RegularExpressions;

namespace VoDA.WebSockets.Utilities
{
    internal static class UriUtilite
    {
        public static bool IsPathMatch(string requestPath, string template)
        {
            var regexPattern = "^" + Regex.Replace(template, "{(.*?)}", "(.*?)") + "$";
            return Regex.IsMatch(requestPath, regexPattern);
        }

        public static string GetPathToFunction(string requestPath, string controllerTemplate)
        {
            var regexPattern = "^" + Regex.Replace(controllerTemplate, "{(.*?)}", ".*?") + "/(.*?)$";
            var match = Regex.Match(requestPath, regexPattern);
            return match.Groups[1].Value;
        }

        public static Dictionary<string, string> ExtractRouteValues(string requestPath, string template)
        {
            var routeValues = new Dictionary<string, string>();
            string regexPattern = Regex.Replace(template, @"\{([^{}]+)\}", @"(?<$1>[^/]+)");
            Match match = Regex.Match(requestPath, regexPattern);

            if (match.Success)
            {
                foreach (Group group in match.Groups)
                {
                    if (group.Success && group.Name != "0")
                    {
                        routeValues[group.Name] = group.Value;
                    }
                }
            }

            return routeValues;
        }
    }
}
