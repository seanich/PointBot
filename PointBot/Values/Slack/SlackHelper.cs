using System;
using System.Text.RegularExpressions;

namespace PointBot.Values.Slack
{
    public static class SlackHelper
    {
        private static readonly Regex userRegex = new Regex(@"^<@([^|]+)(|.+)?>$");
        
        public static string FormatDate(DateTime dateTime)
        {
            var unixTime = ((DateTimeOffset) dateTime).ToUnixTimeSeconds();
            var fallbackString = dateTime.ToString();
            return $"<!date^{unixTime}^{{date_num}} {{time}}|{fallbackString}>";
        } 
        
        public static string FormatUser(string userId)
        {
            return $"<@{userId}>";
        }

        public static string ParseUser(string userEscapeSequence)
        {
            var matches = userRegex.Matches(userEscapeSequence);
            return matches[0].Groups[1].Value;
        }
    }
}