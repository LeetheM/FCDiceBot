﻿using System;
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
    public class TableInfo : ChatBotCommand
    {
        public TableInfo()
        {
            Name = "tableinfo";
            RequireBotAdmin = false;
            RequireChannelAdmin = true;
            RequireChannel = false;
            LockCategory = CommandLockCategory.SavedTables;
        }

        public override void Run(BotMain bot, BotCommandController commandController, string[] rawTerms, string[] terms, string characterName, string channel, UserGeneratedCommand command)
        {
            ChannelSettings thisChannel = null;
            if(commandController.MessageCameFromChannel(channel))
                thisChannel = bot.GetChannelSettings(channel);

            bool fromChannel = commandController.MessageCameFromChannel(channel);

            if (!fromChannel || (thisChannel != null && thisChannel.AllowTableInfo))
            {
                string tableName = commandController.GetTableNameFromCommandTerms(terms);

                SavedRollTable infoTable = Utils.GetTableFromId(bot.SavedTables, tableName);

                string sendMessage = "Table \'" + tableName + "\' not found.";
                if (infoTable != null)
                {
                    sendMessage = "Table id [b]" + infoTable.TableId + "[/b] created by [user]" + infoTable.OriginCharacter + "[/user]";

                    if (infoTable.Table != null)
                    {
                        string tabledesc = infoTable.Table.Description + "\n";
                        sendMessage += "\n\n Name: [b]" + infoTable.Table.Name + "[/b]\n " + tabledesc + " Roll Die: d" + infoTable.Table.DieSides + " Roll Bonus: " + infoTable.Table.RollBonus + " Only Show Result Description?: " + infoTable.Table.OnlyShowResultDescription;
                        
                        //if variables
                        List<List<DiceFunctions.TableRollTrigger>> triggersStart = infoTable.Table.TableEntries.Select(a => a.Triggers).ToList();
                        List<string> variablesFound = new List<string>();
                        if(triggersStart != null)
                        {
                            foreach (List<DiceFunctions.TableRollTrigger> triggeru in triggersStart)
                            {
                                if (triggeru == null)
                                    continue;

                                foreach (DiceFunctions.TableRollTrigger trigg in triggeru)
                                {
                                    if(!string.IsNullOrEmpty(trigg.Command))
                                    {
                                        if (trigg.Command.Contains("#x"))
                                            variablesFound.Add("x");
                                        if (trigg.Command.Contains("#y"))
                                            variablesFound.Add("y");
                                        if (trigg.Command.Contains("#z"))
                                            variablesFound.Add("z");
                                    }
                                }
                            }
                        }//end if triggersstart != null
                        
                        if (variablesFound != null && variablesFound.Count() > 0)
                        {
                            var distinctVars = variablesFound.Distinct();
                            sendMessage += "\n[b]Variables Expected: [/b]" + string.Join(", ", distinctVars); 
                        }

                        sendMessage += "\n" + infoTable.Table.GetTableEntryList();

                    }
                    else
                    {
                        sendMessage += "\n (Rolltable contents not found)";
                    }
                }

                if (fromChannel)
                {
                    bot.SendMessageInChannel(sendMessage, channel);
                }
                else
                    bot.SendPrivateMessage(sendMessage, characterName);
            }
            else
            {
                if(fromChannel)
                    bot.SendMessageInChannel(Name + " is currently not allowed in this channel under " + Utils.GetCharacterUserTags(DiceBot.DiceBotCharacter) + "'s settings for this channel.", channel);
            }
            
        }
    }
}
