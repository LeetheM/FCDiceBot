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
            ChannelSettings thisChannel = bot.GetChannelSettings(channel);

            if (thisChannel.AllowGames)
            {
                string messageString = "";
                int ante = Utils.GetNumberFromInputs(terms);
                if (ante < 0)
                    ante = 0;

                if(ante > 0 && !thisChannel.AllowChips)
                {
                    messageString =  Name + " [b]with chips[/b] is currently not allowed in this channel under " + Utils.GetCharacterUserTags("Dice Bot") + "'s settings for this channel.";
                }
                else
                {
                    IGame gametype = commandController.GetGameTypeFromCommandTerms(bot.DiceBot, terms);

                    if(gametype == null)
                    {
                        //check game sessions and see if this channel has a session for anything
                        var gamesPresent = bot.DiceBot.GameSessions.Where(a => a.ChannelId == channel);
                        if(gamesPresent.Count() == 0)
                        {
                            messageString = "Error: No game type specified. [i]You must create a game session by specifying the game type as the first player.[/i]";
                        }
                        else if(gamesPresent.Count() > 1)
                        {
                            messageString = "Error: You must specify a game type if more than one game session exists in the channel.";
                        }
                        else if (gamesPresent.Count() == 1)
                        {
                            GameSession sesh = gamesPresent.First();
                            gametype = sesh.CurrentGame;
                        }
                    }
                    if(gametype != null)
                    {
                        if (!gametype.AllowAnte() && ante > 0)
                        {
                            messageString = Name + " cannot be played with ante. Try joining the game without an ante amount.";
                        }
                        else
                        {
                            GameSession sesh = bot.DiceBot.GetGameSession(channel, gametype);

                            if (sesh != null)
                            {
                                ChipPile characterChips = bot.DiceBot.GetChipPile(characterName, channel);

                                if (sesh.Players.Contains(characterName))
                                {
                                    messageString = Utils.GetCharacterUserTags(characterName) + " is already in " + sesh.CurrentGame.GetGameName() + ".";
                                }
                                else if (sesh.Players.Count > sesh.CurrentGame.GetMaxPlayers())
                                {
                                    messageString = Utils.GetCharacterUserTags(characterName) + " cannot join " + sesh.CurrentGame.GetGameName() + " because it is already at the [b]maximum amount of players[/b].";
                                }
                                else if (ante > 0 && characterChips.Chips < ante)
                                {
                                    messageString = Utils.GetCharacterUserTags(characterName) + " cannot make a bet for " + ante + " because they do not have enough chips.";
                                }
                                else
                                {
                                    //TODO: change game join logic so that each game uses its own method to verify and add players
                                    if (gametype.GetType() == typeof(Roulette))
                                    {
                                        #region gametype roulette
                                        RouletteBet betType = RouletteBet.NONE;
                                        int betNumber = -1;

                                        if (terms.Contains("red"))
                                            betType = RouletteBet.Red;
                                        else if (terms.Contains("black"))
                                            betType = RouletteBet.Black;
                                        else if (terms.Contains("even"))
                                            betType = RouletteBet.Even;
                                        else if (terms.Contains("odd"))
                                            betType = RouletteBet.Odd;
                                        else if (terms.Contains("first12"))
                                            betType = RouletteBet.First12;
                                        else if (terms.Contains("second12"))
                                            betType = RouletteBet.Second12;
                                        else if (terms.Contains("third12"))
                                            betType = RouletteBet.Third12;
                                        else if (terms.Contains("firsthalf"))
                                            betType = RouletteBet.OneToEighteen;
                                        else if (terms.Contains("secondhalf"))
                                            betType = RouletteBet.NineteenToThirtySix;
                                        else if (terms.Contains("#1"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 1;
                                        }
                                        else if (terms.Contains("#2"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 2;
                                        }
                                        else if (terms.Contains("#3"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 3;
                                        }
                                        else if (terms.Contains("#4"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 4;
                                        }
                                        else if (terms.Contains("#5"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 5;
                                        }
                                        else if (terms.Contains("#6"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 6;
                                        }
                                        else if (terms.Contains("#7"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 7;
                                        }
                                        else if (terms.Contains("#8"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 8;
                                        }
                                        else if (terms.Contains("#9"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 9;
                                        }
                                        else if (terms.Contains("#10"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 10;
                                        }
                                        else if (terms.Contains("#11"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 11;
                                        }
                                        else if (terms.Contains("#12"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 12;
                                        }
                                        else if (terms.Contains("#13"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 13;
                                        }
                                        else if (terms.Contains("#14"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 14;
                                        }
                                        else if (terms.Contains("#15"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 15;
                                        }
                                        else if (terms.Contains("#16"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 16;
                                        }
                                        else if (terms.Contains("#17"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 17;
                                        }
                                        else if (terms.Contains("#18"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 18;
                                        }
                                        else if (terms.Contains("#19"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 19;
                                        }
                                        else if (terms.Contains("#20"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 20;
                                        }
                                        else if (terms.Contains("#21"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 21;
                                        }
                                        else if (terms.Contains("#22"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 22;
                                        }
                                        else if (terms.Contains("#23"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 23;
                                        }
                                        else if (terms.Contains("#24"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 24;
                                        }
                                        else if (terms.Contains("#25"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 25;
                                        }
                                        else if (terms.Contains("#26"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 26;
                                        }
                                        else if (terms.Contains("#27"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 27;
                                        }
                                        else if (terms.Contains("#28"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 28;
                                        }
                                        else if (terms.Contains("#29"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 29;
                                        }
                                        else if (terms.Contains("#30"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 30;
                                        }
                                        else if (terms.Contains("#31"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 31;
                                        }
                                        else if (terms.Contains("#32"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 32;
                                        }
                                        else if (terms.Contains("#33"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 33;
                                        }
                                        else if (terms.Contains("#34"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 34;
                                        }
                                        else if (terms.Contains("#35"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 35;
                                        }
                                        else if (terms.Contains("#36"))
                                        {
                                            betType = RouletteBet.SpecificNumber;
                                            betNumber = 36;
                                        }

                                        if (betType == RouletteBet.NONE)
                                        {
                                            messageString = "No bet type was found in the [b]!joingame " + sesh.CurrentGame.GetGameName() + "[/b] command. Try adding a bet amount and a bet type.";
                                        }
                                        else if (ante <= 0)
                                        {
                                            messageString = "No bet amount was found in the [b]!joingame " + sesh.CurrentGame.GetGameName() + "[/b] command. Try adding a bet amount and a bet type.";
                                        }
                                        else
                                        {
                                            RouletteBetData betData = new RouletteBetData()
                                            {
                                                bet = betType,
                                                specificNumberBet = betNumber,
                                                characterName = characterName,
                                                amount = ante
                                            };

                                            string timerString = "";

                                            double secondsRemain = bot.DiceBot.GetSecondsRemainingOnCountdownTimer(channel, gametype.GetGameName());

                                            if (secondsRemain > 0)
                                                timerString = "[i] (game can begin in " + secondsRemain.ToString("N3") + " seconds)[/i]";

                                            string addDataString = bot.DiceBot.AddGameData(channel, gametype, betData);

                                            if (addDataString != "success")
                                            {
                                                messageString = "Error: Failed to add bet data to game session for " + gametype.GetGameName() + ". Game session terminated.";
                                                bot.DiceBot.CancelGame(channel, gametype);
                                            }
                                            else
                                            {
                                                messageString = bot.DiceBot.JoinGame(characterName, channel, gametype);


                                                messageString += "\n" + sesh.Players.Count + " / " + sesh.CurrentGame.GetMaxPlayers() + " players ready.";

                                                if (!string.IsNullOrEmpty(timerString))
                                                {
                                                    messageString += timerString;
                                                }
                                                else if (sesh.Players.Count >= sesh.CurrentGame.GetMinPlayers())
                                                {
                                                    messageString += "[b] (Ready to start!)[/b]";
                                                }
                                            }
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        bool anteJustSet = false;
                                        if (!sesh.AnteSet) //leave as separate if
                                        {
                                            sesh.AnteSet = true;
                                            anteJustSet = true;
                                            sesh.Ante = ante > 0 ? ante : 0;
                                        }

                                        if (anteJustSet && ante > 0 && characterChips.Chips < sesh.Ante)
                                        {
                                            messageString = Utils.GetCharacterUserTags(characterName) + " does not have [b]" + ante + " chips[/b] to start a game with an ante this high.";
                                            bot.DiceBot.RemoveGameSession(channel, sesh.CurrentGame);
                                        }
                                        else if (sesh.AnteSet && ante > 0 && sesh.Ante != ante)
                                        {
                                            messageString = "The ante for " + sesh.CurrentGame.GetGameName() + " has already been set to something else. Use [b]!joingame " + sesh.CurrentGame.GetGameName() + "[/b] without an ante.";
                                        }
                                        else if (characterChips.Chips < sesh.Ante)
                                        {
                                            messageString = Utils.GetCharacterUserTags(characterName) + " cannot join " + sesh.CurrentGame.GetGameName() + " because they have less than the ante amount of [b]" + sesh.Ante + " chips.[/b]";
                                        }
                                        else if (sesh.Players.Contains(characterName))
                                        {
                                            messageString = Utils.GetCharacterUserTags(characterName) + " is already in " + sesh.CurrentGame.GetGameName() + ".";
                                        }
                                        else if (sesh.Players.Count > sesh.CurrentGame.GetMaxPlayers())
                                        {
                                            messageString = Utils.GetCharacterUserTags(characterName) + " cannot join " + sesh.CurrentGame.GetGameName() + " because it is already at the [b]maximum amount of players[/b].";
                                        }
                                        else
                                        {
                                            messageString = bot.DiceBot.JoinGame(characterName, channel, gametype);
                                            messageString += "\n" + sesh.Players.Count + " / " + sesh.CurrentGame.GetMaxPlayers() + " players ready.";

                                            if (sesh.Players.Count >= sesh.CurrentGame.GetMinPlayers())
                                            {
                                                messageString += "[b] (Ready to start!)[/b]";
                                            }
                                        }
                                    }

                                }//end too many players or player already joined
                            }
                            else
                            {
                                messageString = "Error: Game session for " + gametype.GetGameName() + " not found or created.";
                            }//end game session null
                        }
                    }//end if gametype != null 
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
