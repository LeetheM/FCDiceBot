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
    public class GameCommand : ChatBotCommand
    {
        public GameCommand()
        {
            Name = "gamecommand";
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

                if (terms.Length < 1)
                {
                    messageString = "Error: This command requires a game name.";
                }
                else
                {
                    IGame gametype = commandController.GetGameTypeFromCommandTerms(bot.DiceBot, terms);

                    if (gametype == null)
                    {
                        //check game sessions and see if this channel has a session for anything
                        var gamesPresent = bot.DiceBot.GameSessions.Where(a => a.ChannelId == channel);
                        if (gamesPresent.Count() == 0)
                        {
                            messageString = "Error: No game sessions are active in this channel to cancel.";
                        }
                        else if (gamesPresent.Count() > 1)
                        {
                            messageString = "Error: You must specify a game type if more than one game session exists in the channel.";
                        }
                        else if (gamesPresent.Count() == 1)
                        {
                            GameSession sesh = gamesPresent.First();
                            gametype = sesh.CurrentGame;
                        }
                    }

                    if(gametype == null)
                    {
                        messageString = "Error: Game type not found.";
                    }
                    else
                    {
                        GameSession sesh = bot.DiceBot.GetGameSession(channel, gametype, false);

                        if (sesh != null)
                        {
                            messageString = bot.DiceBot.IssueGameCommand(characterName, channel, sesh, terms);
                        }
                        else
                        {
                            messageString = "Game session for " + gametype.GetGameName() + " not found or created.";
                        }
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
