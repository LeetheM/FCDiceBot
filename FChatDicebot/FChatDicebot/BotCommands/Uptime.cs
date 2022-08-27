﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FChatDicebot.BotCommands.Base;

namespace FChatDicebot.BotCommands
{
    public class Uptime : ChatBotCommand
    {
        public Uptime()
        {
            Name = "uptime";
            RequireBotAdmin = false;
            RequireChannelAdmin = false;
            RequireChannel = true;
            LockCategory = CommandLockCategory.NONE;
        }

        public override void Run(BotMain bot, BotCommandController commandController, string[] rawTerms, string[] terms, string characterName, string channel, UserGeneratedCommand command)
        {
            int channelsNumber = bot.ChannelsJoined.Count();
            TimeSpan onlineTime = DateTime.UtcNow - bot.LoginTime;
            bot.SendMessageInChannel("Dicebot has been online for " + Utils.GetTimeSpanPrint(onlineTime), channel);
        }
    }
}