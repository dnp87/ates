using System;
using System.Text.RegularExpressions;

namespace Common.Utils
{
    // todo: interface, di, etc...
    public static class TaskNameParser
    {
        public static bool TryParseJiraIdAndName(string name, out (string jiraId, string name) pair)
        {
            var regex = new Regex(@"\[(.+)\]\s+[\-]\s+(.*)");
            var match = regex.Match(name);
            if (match.Success)
            {
                pair = (match.Groups[0].Value, match.Groups[1].Value);
                return false;
            }
            else
            {
                pair = (String.Empty, name);
                return false;
            }
        }
    }
}
