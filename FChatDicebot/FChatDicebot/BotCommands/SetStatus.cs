using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FChatDicebot.BotCommands.Base;
using FChatDicebot.DiceFunctions;
using FChatDicebot.Model;

namespace FChatDicebot.BotCommands
{
    public class SetStatus : ChatBotCommand
    {
        public SetStatus()
        {
            Name = "setstatus";
            RequireBotAdmin = true;
            RequireChannelAdmin = false;
            RequireChannel = true;
            LockCategory = CommandLockCategory.NONE;
        }

        public override void Run(BotMain bot, BotCommandController commandController, string[] rawTerms, string[] terms, string characterName, string channel, UserGeneratedCommand command)
        {
            string newStatus = Utils.GetFullStringOfInputs(rawTerms);

            bot.SetStatus(STAStatus.Online, newStatus);
            bot.SendMessageInChannel("[b][ADMIN] Status updated.[/b]", channel);
        }
    }
}
