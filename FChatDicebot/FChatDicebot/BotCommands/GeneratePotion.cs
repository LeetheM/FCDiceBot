using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FChatDicebot.BotCommands.Base;
using FChatDicebot.SavedData;
using Newtonsoft.Json;
using FChatDicebot.DiceFunctions;

namespace FChatDicebot.BotCommands
{
    public class GeneratePotion : ChatBotCommand
    {
        public GeneratePotion()
        {
            Name = "generatepotion";
            RequireBotAdmin = false;
            RequireChannelAdmin = false;
            RequireChannel = true;
            LockCategory = CommandLockCategory.CharacterInventories;
        }

        public override void Run(BotMain bot, BotCommandController commandController, string[] rawTerms, string[] terms, string characterName, string channel, UserGeneratedCommand command)
        {

            string privateMessage = "";
            string rollResult = bot.DiceBot.GeneratePotion(terms, characterName, channel, out privateMessage);

            bot.SendMessageInChannel(rollResult, channel);
            if(!string.IsNullOrEmpty(privateMessage))
            {
                bot.SendPrivateMessage(privateMessage, characterName);
            }
        }
    }
}
