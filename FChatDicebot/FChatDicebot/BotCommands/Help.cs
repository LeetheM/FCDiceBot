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
            string messageText = "[b]General Commands:[/b]\n!roll, !fitd" +
                    "\n!drawcard, !resetdeck, !shuffledeck, !endhand, !showhand, !discardcard, !takefromdiscard, !deckinfo, !playcard, !takefromplay, !discardfromplay, !playfromdiscard\n" +
                    "!register, !addchips, !showchips, !givechips, !bet, !claimpot, !removepile, !takechips, !redeemchips, !removechips\n" +
                    "!joinchannel, !leavethischannel\n!botinfo, !uptime, !help\n" +
                    "!rolltable, !savetable, !deletetable, !tableinfo\n" +
                    "!joingame, !leavegame, !cancelgame, !startgame, !gamestatus, !gamecommand, !gc\n" +
                    "[b]Channel Op only Commands:[/b]\n" +
                    "!removeallpiles, !setstartingchannel, !updatesetting, !viewsettings\n" +
                    "For full information on commands, see the profile [user]Dice Bot[/user].";
            if(string.IsNullOrEmpty(channel))
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
