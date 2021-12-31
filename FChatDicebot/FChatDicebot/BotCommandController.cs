using FChatDicebot.BotCommands;
using FChatDicebot.BotCommands.Base;
using FChatDicebot.DiceFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FChatDicebot
{
    public class BotCommandController
    {
        private BotMain Bot;

        public List<ChatBotCommand> BotCommands;

        private object SavedTablesFileLock = new object();
        private object SavedChannelsFileLock = new object();
        private object ChannelDecksLock = new object();
        private object ChannelScoresLock = new object();

        public BotCommandController(BotMain sourceBot)
        {
            Bot = sourceBot;
            BotCommands = new List<ChatBotCommand>();

            LoadChatBotCommands();
        }

        private void LoadChatBotCommands()
        {
            Type[] allTypes = Assembly.GetExecutingAssembly().GetTypes();

            foreach(Type thisType in allTypes)
            {
                if (thisType.Namespace == "FChatDicebot.BotCommands")
                {
                    object obj = Activator.CreateInstance(thisType);

                    //dynamic is assumed to be any type you tell it, not validated at compile time. can crash runtime if methods not found.
                    dynamic changedObj = Convert.ChangeType(obj, thisType);

                    if (changedObj.GetType().BaseType == typeof(ChatBotCommand))
                        BotCommands.Add(changedObj);
                }
            }
        }

        public void RunChatBotCommand(UserGeneratedCommand command)
        {
            ChatBotCommand c = BotCommands.FirstOrDefault(a => a.Name == command.commandName);

            if (c == null)
                return;

            string[] terms = Utils.LowercaseStrings(command.rawTerms);
            terms = Utils.FixComparators(terms);
            command.terms = terms;

            object lockObject = GetObjectToLock(c.LockCategory);
            bool characterIsAdmin = Utils.IsCharacterAdmin(Bot.AccountSettings.AdminCharacters, command.characterName);
            
            if (MessageCameFromChannel(command.channel) || !c.RequireChannel)
            {
                if (characterIsAdmin || !c.RequireBotAdmin)
                {
                    if (command.ops == null && c.RequireChannelAdmin && !characterIsAdmin)
                    {
                        Bot.RequestChannelOpListAndQueueFurtherRequest(command);// command.channel, command.characterName, command.rawTerms, terms, command.commandName);
                    }
                    else if ((!c.RequireChannelAdmin) || characterIsAdmin || (command.ops != null && command.ops.Contains(command.characterName)))
                    {
                        if (lockObject != null)
                        {
                            lock (lockObject)
                            {
                                c.Run(Bot, this, command.rawTerms, terms, command.characterName, command.channel, command);
                            }
                        }
                        else
                        {
                            c.Run(Bot, this, command.rawTerms, terms, command.characterName, command.channel, command);
                        }
                    }
                    else
                    {
                        Bot.SendMessageInChannel(Utils.GetCharacterUserTags(command.characterName) + ", you need to be a channel op to use this command (" + command.commandName + ").", command.channel);
                    }
                }
                else
                {
                    Bot.SendMessageInChannel("You do not have authorization to complete this command.", command.channel);
                }
            }
        }

        public bool MessageCameFromChannel(string channel)
        {
            return !string.IsNullOrEmpty(channel);
        }

        public object GetObjectToLock(CommandLockCategory lockType)
        {
            switch(lockType)
            {
                case CommandLockCategory.SavedChannels:
                    return SavedChannelsFileLock;
                case CommandLockCategory.SavedTables:
                    return SavedTablesFileLock;
                case CommandLockCategory.ChannelDecks:
                    return ChannelDecksLock;
                case CommandLockCategory.ChannelScores:
                    return ChannelScoresLock;
            }
            return null;
        }

        public string GetTableNameFromCommandTerms(string[] terms)
        {
            var leftovers = terms.Where(a => a != "nolabel" && !a.Contains('+') && !a.Contains('-'));

            string tableName = leftovers.FirstOrDefault();
            if (tableName != null)
                tableName = tableName.ToLower();

            return tableName;
        }

        public int GetRollModifierFromCommandTerms(string[] terms)
        {
            string modifierTerm = terms.FirstOrDefault(a => a.StartsWith("+") || a.StartsWith("-"));

            int mod = 0;

            if (modifierTerm != null && modifierTerm.Length > 1)
            {
                bool negative = modifierTerm.StartsWith("-");

                modifierTerm = modifierTerm.Substring(1);

                int.TryParse(modifierTerm, out mod);

                if (negative)
                    mod = -1 * mod;
            }

            return mod;
        }

        public DeckType GetDeckTypeFromCommandTerms(string[] terms)
        {
            DeckType deckType = DeckType.Playing;
            if (terms != null && terms.Length >= 1 && terms.Contains("tarot"))
                deckType = DeckType.Tarot;
            if (terms != null && terms.Length >= 1 && terms.Contains("manythings"))
                deckType = DeckType.ManyThings;
            if (terms != null && terms.Length >= 1 && terms.Contains("custom"))
                deckType = DeckType.Custom;

            return deckType;
        }

        public IGame GetGameTypeFromCommandTerms(DiceBot diceBot, string[] terms)
        {
            if (terms == null || terms.Length == 0)
                return null;

            IGame returnGame = null;

            foreach (IGame game in diceBot.PossibleGames)
            { 
                if(terms.Contains(game.GetGameName().ToLower()))
                {
                    returnGame = game;
                    break;
                }
            }
            return returnGame;
        }

        public string GetCharacterDrawNameFromCommandTerms(string characterName, string[] terms)
        {
            string characterDrawName = characterName;
            if (terms != null && terms.Length >= 1 && terms.Contains("dealer"))
                characterDrawName = DiceBot.DealerName;
            if (terms != null && terms.Length >= 1 && terms.Contains("burn"))
                characterDrawName = DiceBot.BurnCardsName;
            if (terms != null && terms.Length >= 1 && terms.Contains("discard"))
                characterDrawName = DiceBot.DiscardName;
            if (terms != null && terms.Length >= 1 && terms.Contains("inplay"))
                characterDrawName = characterName + DiceBot.PlaySuffix;

            return characterDrawName;
        }

        public void SaveChipsToDisk()
        {
            Utils.WriteToFileAsData(Bot.DiceBot.ChipPiles, Utils.GetTotalFileName(BotMain.FileFolder, BotMain.SavedChipsFileName));
        }

        public void SaveCouponsToDisk()
        {
            Utils.WriteToFileAsData(Bot.ChipsCoupons, Utils.GetTotalFileName(BotMain.FileFolder, BotMain.ChipsCouponsFileName));
        }
    }

    public enum CommandLockCategory
    {
        NONE,
        SavedTables,
        SavedChannels,
        ChannelDecks,
        ChannelScores
    }

    public class UserGeneratedCommand
    {
        public string characterName;
        public string channel;
        public string commandName;
        public string[] rawTerms;
        public string[] terms;
        public string[] ops;
    }
}
