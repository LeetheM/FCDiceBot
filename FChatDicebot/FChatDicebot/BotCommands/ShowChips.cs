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
    public class ShowChips : ChatBotCommand
    {
        public ShowChips()
        {
            Name = "showchips";
            RequireBotAdmin = false;
            RequireChannelAdmin = false;
            RequireChannel = true;
            LockCategory = CommandLockCategory.ChannelScores;
        }

        public override void Run(BotMain bot, BotCommandController commandController, string[] rawTerms, string[] terms, string characterName, string channel, UserGeneratedCommand command)
        {
            ChannelSettings thisChannel = bot.GetChannelSettings(channel);

            if (thisChannel.AllowChips)
            {
                bool all = false;
                bool pot = false;
                bool secret = false;

                if (terms != null && terms.Length >= 1 && terms.Contains("all"))
                    all = true;

                if (terms != null && terms.Length >= 1 && terms.Contains("pot"))
                    pot = true;

                if (terms != null && terms.Length >= 1 && terms.Contains("secret"))
                    secret = true;

                string messageString = "";
                if (all)
                {
                    messageString = bot.DiceBot.ListAllChipPiles(channel);
                }
                else
                {
                    if (pot)
                        messageString = bot.DiceBot.DisplayChipPile(channel, DiceBot.PotPlayerAlias);
                    else
                        messageString = bot.DiceBot.DisplayChipPile(channel, characterName);
                }

                if(all || secret)
                {
                    bot.SendPrivateMessage(messageString, characterName);
                }
                else
                {
                    bot.SendMessageInChannel(messageString, channel);
                }

            }
            else
            {
                bot.SendMessageInChannel(Name + " is currently not allowed in this channel under " + Utils.GetCharacterUserTags("Dice Bot") + "'s settings for this channel.", channel);
            }
        }
    }
}
