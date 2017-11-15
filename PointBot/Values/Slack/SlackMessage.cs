using System.Collections.Generic;

namespace PointBot.Values.Slack
{
    public struct SlackMessage
    {
        public string Text;
        public List<SlackAttachment> Attachments; 
    }
}