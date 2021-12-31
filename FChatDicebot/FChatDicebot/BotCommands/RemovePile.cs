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
    public class RemovePile : ChatBotCommand
    {
        public RemovePile()
        {
            Name = "removepile";
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
                string messageString = bot.DiceBot.RemoveChipsPile(characterName, channel);

                commandController.SaveChipsToDisk();

                bot.SendMessageInChannel(messageString, channel);
            }
            else
            {
                bot.SendMessageInChannel(Name + " is currently not allowed in this channel under " + Utils.GetCharacterUserTags("Dice Bot") + "'s settings for this channel.", channel);
            }
        }
    }
}
