using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using PointBot.Data;
using PointBot.Models;
using PointBot.Values;
using PointBot.Values.Slack;

namespace PointBot.Controllers
{
    [Route("")]
    public class WebhookController
    {
        private readonly ILogger<WebhookController> logger;
        private readonly string expectedToken;
        private readonly Dictionary<string, Command> commands;
        private readonly PointBotContext context;

        public WebhookController(ILogger<WebhookController> logger, IConfiguration configuration, PointBotContext context)
        {
            this.logger = logger;
            expectedToken = configuration.GetValue("SLACK_TOKEN", "invalid_token");
            
            commands = new Dictionary<string, Command>();
            
            registerCommand("give", "Give a user points.", commandGive, new [] {"user", "amount", "?message"});
            registerCommand("leaderboard", "Display the leaderboard.", commandLeaderboard);
            registerCommand("history", "Display point events history.", commandHistory, new [] {"?user", "?limit=20"});
            registerCommand("help", "Display this help message.", commandHelp);

            this.context = context;
        }

        [HttpPost]
        public IActionResult Post(IFormCollection data)
        {
            var clientToken = data["token"];
            
            if (clientToken != expectedToken)
            {
                logger.LogWarning($"Client tried to call service with unrecognized token {clientToken}");
                return new UnauthorizedResult();
            }

            return handleCommand(new SlackParams(data));
        }
        
        private void registerCommand(string commandName, string commandDescription,
            Func<SlackParams, IActionResult> commandAction, string[] commandArguments = null)
        {
            commands.Add(commandName, new Command
            {
                Name = commandName,
                Description = commandDescription,
                CommandAction = commandAction,
                Arguments = commandArguments
            });
        }

        private IActionResult handleCommand(SlackParams slackParams)
        {
            return commands.TryGetValue(slackParams.SubCommand, out var command)
                ? command.CommandAction(slackParams)
                : commandHelp(slackParams);
        }

        private IActionResult commandHelp(SlackParams slackParams)
        {
            return new ObjectResult(new SlackMessage
            {
                Text = "Usage:",
                Attachments = new List<SlackAttachment>(1)
                {
                    new SlackAttachment
                    {
                        Fallback = "Commands listing.",
                        Fields = commands.Select(command => command.Value)
                            .Select(command =>
                            {
                                var args = command.Arguments != null && command.Arguments.Length > 0
                                    ? string.Join(' ', command.Arguments.Select(arg => $"[{arg}]"))
                                    : "";
                                return new SlackField
                                {
                                    Title = $"{slackParams.Command} {command.Name} {args}",
                                    Value = command.Description,
                                    Short = false
                                };
                            })
                            .ToList()
                    }
                }
            });
        }

        private IActionResult commandGive(SlackParams slackParams)
        {
            var pointEvent = new PointEvent
            {
                AffectedUserId = SlackHelper.ParseUser(slackParams.SubCommandParams[0]),
                CreatingUserId = slackParams.UserId,
                ChannelId = slackParams.ChannelId,
                PointDelta = Convert.ToInt32(slackParams.SubCommandParams[1])
            };

            if (slackParams.SubCommandParams.Length > 2)
            {
                pointEvent.Message = string.Join(" ", slackParams.SubCommandParams.TakeLast(slackParams.SubCommandParams.Length - 2));
            }
                
            context.Add(pointEvent);
            context.SaveChanges();
            
            return new ObjectResult("Noted.");
        }
        
        private IActionResult commandLeaderboard(SlackParams slackParams)
        {
            var users = context.PointEvents
                .GroupBy(e => e.AffectedUserId)
                .Select(g => new
                    {UserId = g.Key, Points = g.Sum(e => e.PointDelta)})
                .OrderByDescending(g => g.Points);
            var userLines = users.Select(u => $"{SlackHelper.FormatUser(u.UserId)} {u.Points}");
            return new ObjectResult(new SlackMessage
            {
                Text = "*Leaderboard*\n" + string.Join("\n", userLines)
            });
        }
        
        private IActionResult commandHistory(SlackParams slackParams)
        {
            var history = context.PointEvents.OrderBy(p => p.Created).Take(20);
            var historyLines = history.Select(item => $"{SlackHelper.FormatDate(item.Created)} - {SlackHelper.FormatUser(item.AffectedUserId)} {item.PointDelta} points by {SlackHelper.FormatUser(item.CreatingUserId)} {item.Message}");
            return new ObjectResult(new SlackMessage
            {
                Text = "*History*\n" + string.Join("\n", historyLines)
            });
        }
    }
}