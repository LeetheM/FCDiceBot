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
    public class JoinGame : ChatBotCommand
    {
        public JoinGame()
        {
            Name = "joingame";
            RequireBotAdmin = false;
            RequireChannelAdmin = false;
            RequireChannel = true;
            LockCategory = CommandLockCategory.ChannelScores;
        }

        public override void Run(BotMain bot, BotCommandController commandController, string[] rawTerms, string[] terms, string characterName, string channel, UserGeneratedCommand command)
        {
            AddPlayerToGame.Run(bot, commandController, rawTerms, terms, characterName, channel, command, characterName, Name);
            //ChannelSettings thisChannel = bot.GetChannelSettings(channel);

            //if (thisChannel.AllowGames)
            //{
            //    string messageString = "";
            //    int ante = Utils.GetNumberFromInputs(terms);
            //    if (ante < 0)
            //        ante = 0;

            //    if(ante > 0 && !thisChannel.AllowChips)
            //    {
            //        messageString =  Name + " [b]with chips[/b] is currently not allowed in this channel under " + Utils.GetCharacterUserTags("Dice Bot") + "'s settings for this channel.";
            //    }
            //    else
            //    {
            //        IGame gametype = commandController.GetGameTypeForCommand(bot.DiceBot, channel, terms, out messageString);

            //        if(gametype != null)
            //        {
            //            GameSession sesh = bot.DiceBot.GetGameSession(channel, gametype);

            //            if (sesh != null)
            //            {
            //                ChipPile characterChips = bot.DiceBot.GetChipPile(characterName, channel);
                                
            //                if (!gametype.AllowAnte() && ante > 0)
            //                {
            //                    messageString = sesh.CurrentGame.GetGameName() + " cannot be played with ante. Try joining the game without an ante amount.";
            //                }
            //                else if (sesh.Players.Contains(characterName))
            //                {
            //                    messageString = Utils.GetCharacterUserTags(characterName) + " is already in " + sesh.CurrentGame.GetGameName() + ".";
            //                }
            //                else if (sesh.Players.Count >= sesh.CurrentGame.GetMaxPlayers())
            //                {
            //                    messageString = Utils.GetCharacterUserTags(characterName) + " cannot join " + sesh.CurrentGame.GetGameName() + " because it is already at the [b]maximum amount of players[/b].";
            //                }
            //                else if (ante > 0 && characterChips.Chips < ante)
            //                {
            //                    messageString = Utils.GetCharacterUserTags(characterName) + " cannot make a bet for " + ante + " because they do not have enough chips. [i](" + characterChips.Chips + " chips owned)[/i]";
            //                }
            //                else
            //                {
            //                    string timerString = "";

            //                    double secondsRemain = bot.DiceBot.GetSecondsRemainingOnCountdownTimer(channel, gametype.GetGameName());

            //                    if (secondsRemain > 0)
            //                        timerString = "[i] (game can begin in " + secondsRemain.ToString("N3") + " seconds)[/i]";

            //                    bool anteProblem = false;
            //                    if(gametype.UsesFlatAnte())
            //                    {
            //                        bool anteJustSet = false;
            //                        if (!sesh.AnteSet) //leave as separate if
            //                        {
            //                            sesh.AnteSet = true;
            //                            anteJustSet = true;
            //                            sesh.Ante = ante > 0 ? ante : 0;
            //                        }

            //                        if (anteJustSet && ante > 0 && characterChips.Chips < sesh.Ante)
            //                        {
            //                            messageString = Utils.GetCharacterUserTags(characterName) + " does not have [b]" + ante + " chips[/b] to start a game with an ante this high. [i](" + characterChips.Chips + " chips owned)[/i]";
            //                            bot.DiceBot.RemoveGameSession(channel, sesh.CurrentGame);
            //                            anteProblem = true;
            //                        }
            //                        else if (sesh.AnteSet && ante > 0 && sesh.Ante != ante)
            //                        {
            //                            messageString = "The ante for " + sesh.CurrentGame.GetGameName() + " has already been set to something else. Use [b]!joingame " + sesh.CurrentGame.GetGameName() + "[/b] without an ante to match it.";
            //                            anteProblem = true;
            //                        }
            //                        else if (characterChips.Chips < sesh.Ante)
            //                        {
            //                            messageString = Utils.GetCharacterUserTags(characterName) + " cannot join " + sesh.CurrentGame.GetGameName() + " because they have less than the ante amount of [b]" + sesh.Ante + " chips.[/b] [i](" + characterChips.Chips + " chips owned)[/i]";
            //                            anteProblem = true;
            //                        }
            //                    }

            //                    if(!anteProblem)
            //                    {
            //                        string errorString = "";
            //                        bool successfullyAddedGameData = gametype.AddGameDataForPlayerJoin(characterName, sesh, bot, terms, ante, out errorString);

            //                        if(!successfullyAddedGameData)
            //                        {
            //                            messageString = errorString;
            //                        }
            //                        else
            //                        {
            //                            messageString = bot.DiceBot.JoinGame(characterName, channel, gametype);
            //                            messageString += "\n" + sesh.Players.Count + " / " + sesh.CurrentGame.GetMaxPlayers() + " players ready.";

            //                            if (!string.IsNullOrEmpty(timerString))
            //                            {
            //                                messageString += timerString;
            //                            }
            //                            else if (sesh.HasEnoughPlayersToStart())
            //                            {
            //                                messageString += "[b] (Ready to start!)[/b]";
            //                            }
            //                        }
            //                    }
            //                }//end too many players or player already joined
            //            }
            //            else
            //            {
            //                messageString = "Error: Game session for " + gametype.GetGameName() + " not found or created.";
            //            }//end game session null
            //        }//end if gametype != null 
            //    }

            //    bot.SendMessageInChannel(messageString, channel);
            //}
            //else
            //{
            //    bot.SendMessageInChannel(Name + " is currently not allowed in this channel under " + Utils.GetCharacterUserTags("Dice Bot") + "'s settings for this channel.", channel);
            //}
        }
    }
}
