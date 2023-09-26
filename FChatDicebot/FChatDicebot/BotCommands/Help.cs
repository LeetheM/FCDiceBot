using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FChatDicebot.BotCommands.Base;

namespace FChatDicebot.BotCommands
{
    public class Help : ChatBotCommand
    {
        public Help()
        {
            Name = "help";
            RequireBotAdmin = false;
            RequireChannelAdmin = false;
            RequireChannel = false;
            LockCategory = CommandLockCategory.NONE;
        }

        public override void Run(BotMain bot, BotCommandController commandController, string[] rawTerms, string[] terms, string characterName, string channel, UserGeneratedCommand command)
        {
            string messageText = "[b]General Commands:[/b] [i]Can be sent to Dice Bot as a DM (Does not require channel)[/i]\n" +
                    "!roll, !fitd, !coinflip, !fen, !joinchannel, !leavethischannel, !botinfo, !uptime, !help, !rachel\n" +
                    "!savetable, !savetablesimple, !deletetable, !tableinfo, !mytables, !showtables, !savecustomdeck, !savecustomdecksimple, !showdecks, !slotsinfo,\n" +
                    "!deletepotion, !showpotions, !tablejson, !deckjson\n" +
                    "[i]Requires channel[/i]\n" +
                    "!drawcard, !resetdeck, !shuffledeck, !shufflediscardintodeck, !endhand, !showhand, !showcardpiles, !movecard\n" +
                    "!discardcard, !takefromdiscard, !deckinfo, !playcard, !takefromplay, !discardfromplay, !playfromdiscard, !hidecard, !revealcard\n" +
                    "!register, !addchips, !showchips, !givechips, !bet, !claimpot, !removepile, !takechips, !redeemchips, !removechips\n" +
                    "!rolltable, !slots, !slotsinfo, !work, !showprofile, !potionjson\n" +
                    "!generatepotion, !generatepotioninfo, !revealpotion, !droppotion, !savepotion,\n" +
                    "!joingame, !leavegame, !cancelgame, !startgame, !gamestatus, !gamecommand, !gc, !g, !showgames\n" +
                    "[b]Channel Op only Commands:[/b]\n" +
                    "!setstartingchannel, !updatesetting, !viewsettings, !removefromgame, !addtogame\n" +
                    "!savetable, !savetablesimple, !deletetable, !tableinfo, !mytables, !showtables, !savecustomdeck, !savecustomdecksimple\n" +
                    "For full information on commands, see the profile [user]Dice Bot[/user].";

            if(Utils.IsCharacterAdmin(bot.AccountSettings.AdminCharacters, command.characterName))
            {
                messageText += "\n[b]Admin only Commands [/b](no channel req except testviewdeck and testops)\n" +
                    "!TestChar, !TestOps, !TestViewDeck,\n!SendToChannel, !SendAllChannels, !SetStatus, !GenerateChipsCode, !UpdateSettingAll, !TestExtractPotions, " +
                    "!RemoveOldData, !TestSlotRolls, !TestVcConnection, !ForceGiveChips, !TestSetHand, !testresetcooldowns, !testkiss, !unlockdice\n" +
                    "!RestrictChips, !removeallpiles";
            }

            if (!commandController.MessageCameFromChannel(channel))
            {
                bot.SendPrivateMessage(messageText + "\nMost of [user]Dice Bot[/user]'s functions require it to be in a [b]channel[/b]. You can invite it to a private channel and then use [b]!joinchannel[/b] to test commands.", characterName);
            }
            else
            {
                bot.SendMessageInChannel(messageText, channel);
            }
        }
    }
}
