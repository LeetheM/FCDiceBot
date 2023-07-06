using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FChatDicebot.BotCommands.Base;
using FChatDicebot.SavedData;
using Newtonsoft.Json;

namespace FChatDicebot.BotCommands
{
    public class ForceGiveChips : ChatBotCommand
    {
        public ForceGiveChips()
        {
            Name = "forcegivechips";
            RequireBotAdmin = true;
            RequireChannelAdmin = false;
            RequireChannel = true;
            LockCategory = CommandLockCategory.ChannelScores;
        }

        public override void Run(BotMain bot, BotCommandController commandController, string[] rawTerms, string[] terms, string characterName, string channel, UserGeneratedCommand command)
        {
            ChannelSettings thisChannel = bot.GetChannelSettings(channel);

            if (thisChannel.AllowChips)
            {
                string messageString = "";
                if (terms.Length < 2)
                {
                    messageString = "Error: This command requires a number (first) and a user name (second).";
                }
                else
                {
                    bool all = false;
                    int giveAmount = Utils.GetNumberFromInputs(rawTerms);

                    string[] rawTermsMost = new string[rawTerms.Length - 1];

                    for (int i = 1; i < rawTerms.Length; i++)
                    {
                        rawTermsMost[i - 1] = rawTerms[i];
                    }

                    string targetUserName = Utils.GetFullStringOfInputs(rawTermsMost).Trim();

                    if (giveAmount <= 0 && !all)
                    {
                        messageString = "Error: You must input a number to give an amount of chips.";
                    }
                    else
                    {
                        messageString = bot.DiceBot.GiveChips(characterName, targetUserName, channel, giveAmount, all, true);

                        commandController.SaveChipsToDisk("ForceGiveChips");
                    }
                }

                bot.SendMessageInChannel(messageString, channel);
            }
            else
            {
                bot.SendMessageInChannel(Name + " is currently not allowed in this channel under " + Utils.GetCharacterUserTags("Dice Bot") + "'s settings for this channel.", channel);
            }
        }
    }
}
