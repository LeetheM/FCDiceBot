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
    public class TakeChips : ChatBotCommand
    {
        public TakeChips()
        {
            Name = "takechips";
            RequireBotAdmin = false;
            RequireChannelAdmin = false;
            RequireChannel = true;
            LockCategory = CommandLockCategory.ChannelScores;
        }

        public override void Run(BotMain bot, BotCommandController commandController, string[] rawTerms, string[] terms, string characterName, string channel, UserGeneratedCommand command)
        {
            ChannelSettings thisChannel = bot.GetChannelSettings(channel);

            bool characterIsAdmin = Utils.IsCharacterAdmin(bot.AccountSettings.AdminCharacters, characterName);

            if (thisChannel.ChipsClearance == ChipsClearanceLevel.ChannelOp && command.ops == null)
            {
                bot.RequestChannelOpListAndQueueFurtherRequest(command);
            }
            else if ((thisChannel.ChipsClearance == ChipsClearanceLevel.DicebotAdmin && !characterIsAdmin) ||
                (thisChannel.ChipsClearance == ChipsClearanceLevel.ChannelOp && !command.ops.Contains(characterName) && !characterIsAdmin))
            {
                bot.SendMessageInChannel(Utils.GetCharacterUserTags(characterName) + " cannot perform [" + Name + "] under the current chip settings for this channel.", channel);
            }
            else if (thisChannel.AllowChips)
            {
                string messageString = "";
                if (terms.Length < 2)
                {
                    messageString = "Error: This command requires a number (first) and a user name (second).";
                }
                else
                {
                    bool all = false;
                    int giveAmount = Utils.GetNumberFromInputs(terms);

                    string[] rawTermsMost = new string[rawTerms.Length - 1];

                    for (int i = 1; i < rawTerms.Length; i++)
                    {
                        rawTermsMost[i - 1] = rawTerms[i];
                    }

                    string targetUserName = Utils.GetFullStringOfInputs(rawTermsMost).Trim();

                    if (giveAmount <= 0 && !all)
                    {
                        messageString = "Error: You must input a number to take an amount of chips.";
                    }
                    else
                    {
                        messageString = bot.DiceBot.TakeChips(characterName, targetUserName, channel, giveAmount, all);

                        commandController.SaveChipsToDisk("TakeChips");
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
