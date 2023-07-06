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
                        sendMessage += "\n\n Name: [b]" + infoTable.Table.Name + "[/b]\n " + tabledesc + " Roll Die: d" + infoTable.Table.DieSides + " Roll Bonus: " + infoTable.Table.RollBonus;
                        
                        //if variables
                        List<List<DiceFunctions.TableRollTrigger>> triggersStart = infoTable.Table.TableEntries.Select(a => a.Triggers).ToList();//.Select(q => q.)
                        List<string> variablesFound = new List<string>();
                        if(triggersStart != null)
                        {
                            foreach (List<DiceFunctions.TableRollTrigger> triggeru in triggersStart)
                            {
                                if (triggeru == null)
                                    continue;

                                foreach (DiceFunctions.TableRollTrigger trigg in triggeru)
                                {
                                    if (!string.IsNullOrEmpty(trigg.VariableRollBonus))
                                        variablesFound.Add(trigg.VariableRollBonus.ToLower());
                                    if (!string.IsNullOrEmpty(trigg.TableId))
                                    {
                                        int currentIndex = 0;
                                        int safety = 100;
                                        while (currentIndex < trigg.TableId.Length - 1 && safety > 0)
                                        {
                                            safety--;
                                            int pound = trigg.TableId.IndexOf('#', currentIndex);
                                            if (pound >= 0 && pound < trigg.TableId.Length - 2)
                                            {
                                                variablesFound.Add(trigg.TableId[pound + 1].ToString().ToLower());
                                                currentIndex = pound + 1;
                                            }
                                            else
                                            {
                                                currentIndex = int.MaxValue;
                                            }
                                        }
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
                    bot.SendMessageInChannel(Name + " is currently not allowed in this channel under " + Utils.GetCharacterUserTags("Dice Bot") + "'s settings for this channel.", channel);
            }
            
        }
    }
}
