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
    public class LeaveGame : ChatBotCommand
    {
        public LeaveGame()
        {
            Name = "leavegame";
            RequireBotAdmin = false;
            RequireChannelAdmin = false;
            RequireChannel = true;
            LockCategory = CommandLockCategory.ChannelScores;
        }

        public override void Run(BotMain bot, BotCommandController commandController, string[] rawTerms, string[] terms, string characterName, string channel, UserGeneratedCommand command)
        {
            ChannelSettings thisChannel = bot.GetChannelSettings(channel);

            if (thisChannel.AllowGames)
            {
                string messageString = "";

                IGame gametype = commandController.GetGameTypeFromCommandTerms(bot.DiceBot, terms);

                if(gametype == null)
                {
                    //TODO: make this code somewhere else - it's repeated in many game commands
                    //check game sessions and see if this channel has a session for anything
                    var gamesParticipated = bot.DiceBot.GameSessions.Where(a => a.ChannelId == channel && a.Players.Contains(characterName));
                    if(gamesParticipated.Count() == 0)
                    {
                        messageString = "Error: Game type not found.";
                    }
                    else if(gamesParticipated.Count() > 1)
                    {
                        messageString = "Error: You must specify a game type if you are in more than one game.";
                    }
                    else if (gamesParticipated.Count() == 1)
                    {
                        GameSession sesh = gamesParticipated.First();
                        gametype = sesh.CurrentGame;
                    }
                }
                if(gametype != null) 
                {
                    GameSession sesh = bot.DiceBot.GetGameSession(channel, gametype, false);
                    if (sesh != null)
                    {
                        if (!sesh.Players.Contains(characterName))
                        {
                            messageString = Utils.GetCharacterUserTags(characterName) + " has not joined " + sesh.CurrentGame.GetGameName() + ".";
                        }
                        else
                        {
                            messageString = bot.DiceBot.LeaveGame(characterName, channel, gametype);
                            
                            messageString += "\n" + sesh.Players.Count + " / " + sesh.CurrentGame.GetMaxPlayers() + " players ready.";
                                
                            if(sesh.CurrentGame.GetType() == typeof(Roulette))
                            {
                                sesh.RemoveRouletteBet(characterName);
                            }

                            if(sesh.Players.Count >= sesh.CurrentGame.GetMinPlayers())
                            {
                                messageString += "[b] (Ready to start!)[/b]";
                            }
                        }
                    }
                    else
                    {
                        messageString = "Error: Game session for " + gametype.GetGameName() + " not found or created.";
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
