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
            RequireChannel = false;
            LockCategory = CommandLockCategory.NONE;
        }

        public override void Run(BotMain bot, BotCommandController commandController, string[] rawTerms, string[] terms, string characterName, string channel, UserGeneratedCommand command)
        {
            int channelsNumber = bot.ChannelsJoined.Count();
            TimeSpan onlineTime = DateTime.UtcNow - bot.LoginTime;

            string resultMessageString = "Dice Bot was developed by [user]Ambitious Syndra[/user] on 10/12/2020"
                + "\nCurrent version " + BotMain.Version
                + "\nCurrently operating in " + channelsNumber + " channels."
                + "\nOnline for " + Utils.GetTimeSpanPrint(onlineTime)
                + "\nFor a list of commands, use !help. See the profile [user]Dice Bot[/user] for more detailed information.";
                
            if (!commandController.MessageCameFromChannel(channel))
            {
                bot.SendPrivateMessage(resultMessageString, characterName);
            }
            else
            {
                bot.SendMessageInChannel(resultMessageString, channel);
            }
        }
    }
}
