using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FChatDicebot.BotCommands.Base;

namespace FChatDicebot.BotCommands
{
    public class BotInfo : ChatBotCommand
    {
        public BotInfo()
        {
            Name = "botinfo";
            RequireBotAdmin = false;
            RequireChannelAdmin = false;
            RequireChannel = true;
            LockCategory = CommandLockCategory.NONE;
        }

        public override void Run(BotMain bot, BotCommandController commandController, string[] rawTerms, string[] terms, string characterName, string channel, UserGeneratedCommand command)
        {
            int channelsNumber = bot.ChannelsJoined.Count();
            TimeSpan onlineTime = DateTime.UtcNow - bot.LoginTime;
            bot.SendMessageInChannel("Dice Bot was developed by [user]Darkness Syndra[/user] on 10/12/2020"
                + "\nversion " + BotMain.Version
                + "\ncurrently operating in " + channelsNumber + " channels."
                + "\nonline for " + Utils.GetTimeSpanPrint(onlineTime)
                + "\nfor a list of commands, see the profile [user]Dice Bot[/user].", channel);
        }
    }
}
