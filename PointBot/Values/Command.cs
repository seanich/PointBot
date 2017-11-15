using System;

using Microsoft.AspNetCore.Mvc;

using PointBot.Values.Slack;

namespace PointBot.Values
{
    public struct Command
    {
        public string Name;
        public string Description;
        public string[] Arguments;
        public Func<SlackParams, IActionResult> CommandAction;
    }
}