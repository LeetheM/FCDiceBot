using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FChatDicebot.BotCommands.Base;
using FChatDicebot.DiceFunctions;

namespace FChatDicebot.BotCommands
{
    public class TestChar : ChatBotCommand
    {
        public TestChar()
        {
            Name = "testchar";
            RequireBotAdmin = true;
            RequireChannelAdmin = false;
            RequireChannel = true;
            LockCategory = CommandLockCategory.NONE;
        }

        public override void Run(BotMain bot, BotCommandController commandController, string[] rawTerms, string[] terms, string characterName, string channel, UserGeneratedCommand command)
        {
            int numberCharacters = 0;
            if (terms.Length > 0)
            {
                int.TryParse(terms[0], out numberCharacters);
            }

            bot.SendMessageInChannel("[b][ADMIN] [/b]" + Utils.GetStringOfNLength(numberCharacters), channel);
        }
    }
}
