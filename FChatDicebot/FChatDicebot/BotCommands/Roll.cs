using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FChatDicebot.BotCommands.Base;

namespace FChatDicebot.BotCommands
{
    public class Roll : ChatBotCommand
    {
        public Roll()
        {
            Name = "roll";
            RequireBotAdmin = false;
            RequireChannelAdmin = false;
            RequireChannel = true;
            LockCategory = CommandLockCategory.NONE;
        }

        public override void Run(BotMain bot, BotCommandController commandController, string[] rawTerms, string[] terms, string characterName, string channel, UserGeneratedCommand command)
        {
            if (BotMain._debug)
                bot.SendMessageInChannel("Command recieved: " + Utils.PrintList(terms), channel);

            bool debugOverride = false;
            string finalResult = bot.DiceBot.GetRollResult(terms, debugOverride);

            string userName = "[i]" + Utils.GetCharacterUserTags(characterName) + "[/i]: ";
            bot.SendMessageInChannel(userName + finalResult, channel);

            if (BotMain._debug)
                Console.WriteLine("Command finished: " + finalResult);
        }
    }
}
