using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using WebSocketSharp;
using FChatDicebot.Model;
using Newtonsoft.Json;
using FChatDicebot.DiceFunctions;
using FChatDicebot.SavedData;
using System.IO;

namespace FChatDicebot
{
    public class BotMain
    {
        //TODO: auto reconnect on disconnect
        public string CurrentApiKey = "";
        public const string FListChatUri = "wss://chat.f-list.net/chat2";
        public const bool _debug = false;
        public const string Version = "1.03";

        public const string FileFolder = "C:\\BotData\\DiceBot";
        public const string AccountSettingsFileName = "account_settings.txt";
        public const string StartingChannelsFileName = "starting_channels.txt";
        public const string ChannelSettingsFileName = "channel_settings.txt";

        public BotWebRequests WebRequests;
        public BotMessageQueue MessageQueue;
        public DiceBot DiceBot;
        public AccountSettings AccountSettings;
        private List<StartingChannel> StartingChannels;

        public TimeSpan Uptime;
        public TimeSpan LastCheckin;
        public DateTime LoginTime;
        public int TickTimeMiliseconds = 200;
        public int MinimumTimeBetweenMessages = 1200; //minimum 1 second between messages for FList bot rules
        public TimeSpan CheckinInterval = new TimeSpan(0, 0, 20, 0, 0);
        List<string> ChannelsJoined = new List<string>();

        public void Run()
        {
            LoadAccountSettings();
            LoadStartingChannels();

            MessageQueue = new BotMessageQueue();
            WebRequests = new BotWebRequests();
            DiceBot = new DiceFunctions.DiceBot();

            System.Threading.Thread th = new Thread(RunLoop);
            th.IsBackground = true;
            th.Start();
        }

        private void LoadAccountSettings()
        {
            string path = Utils.GetTotalFileName( FileFolder , AccountSettingsFileName);
            try
            {
                if (!Directory.Exists(FileFolder))
                {
                    Directory.CreateDirectory(FileFolder);
                }

                if (File.Exists(path))
                {
                    string fileText = File.ReadAllText(path, Encoding.ASCII);

                    if(_debug)
                        Console.WriteLine("read " + path);

                    AccountSettings = JsonConvert.DeserializeObject<AccountSettings>(fileText);

                    if (_debug)
                        Console.WriteLine("loaded AccountSettings successfully.");
                }
                else
                {
                    Console.WriteLine("AccountSettings file does not exist.");
                }
            }
            catch (System.Exception exc)
            {
                Console.WriteLine("Exception: Failed to load AccountSettings for " + path + "\n" + exc.ToString());
            }
        }

        private void LoadStartingChannels()
        {
            string path = FileFolder + "\\" + StartingChannelsFileName;
            try
            {
                if (!Directory.Exists(FileFolder))
                {
                    Directory.CreateDirectory(FileFolder);
                }

                if (File.Exists(path))
                {
                    string fileText = File.ReadAllText(path, Encoding.ASCII);

                    if (_debug)
                        Console.WriteLine("read " + path);

                    StartingChannels = JsonConvert.DeserializeObject<List<StartingChannel>>(fileText);

                    if (_debug)
                        Console.WriteLine("loaded AccountSettings successfully.");
                }
                else
                {
                    Console.WriteLine("AccountSettings file does not exist.");
                }
            }
            catch (System.Exception exc)
            {
                Console.WriteLine("Exception: Failed to load AccountSettings for " + path + "\n" + exc.ToString());
            }
        }

        public void RunLoop()
        {
            if(_debug)
                Console.WriteLine("Runloop started");

            GetNewApiTicket();

            Uptime = new TimeSpan();
            LastCheckin = new TimeSpan();

            int LastMessageSent = 0;

            using (var ws = new WebSocket(FListChatUri))
            {
                BotMessage firstIdnRequest = GetNewIDNRequest();

                ws.OnOpen += (sender, e) =>
                    ws.Send(firstIdnRequest.PrintedCommand());

                ws.OnMessage += (sender, e) =>
                    OnMessage(e.Data);

                ws.OnError += (sender, e) =>
                    Console.WriteLine("Websocket Error: " + e.Message);

                ws.OnClose += (sender, e) =>
                    Console.WriteLine("WebSocket Close ({0}) ({1})", e.Code, e.Reason);

                ws.Origin = "http://localhost:4649";

                ws.EnableRedirection = true;

                ws.Connect();

                LoginTime = DateTime.UtcNow;

                //required to wait for the server to stop sending startup messages, otherwise overflow occurs and ws closes
                Thread.Sleep(5000);

                foreach(StartingChannel channel in StartingChannels)
                {
                    JoinChannel(channel.Code);
                }
                
                Thread.Sleep(1000);

                StartingChannel testChannel = StartingChannels.FirstOrDefault(a => a.Essential == true);
                if(testChannel != null)
                    SendMessageInChannel("Dicebot Online", testChannel.Code);

                SetStatus(STAStatus.Online, "[color=yellow]Dice Bot v" + Version + " Online![/color] Check out the [user]Dice Bot[/user] profile for a list of commands. You can add Dicebot to your channel with !joinchannel [[i]channel invite code paste[/i]]");

                while(true)
                {
                    LastMessageSent += TickTimeMiliseconds;

                    if(MessageQueue.HasMessages())
                    {
                        BotMessage nextMessagePeek = MessageQueue.Peek();

                        if (!Utils.BotMessageIsChatMessage(nextMessagePeek) || LastMessageSent >= MinimumTimeBetweenMessages)
                        {
                            BotMessage nextMessageToSend = MessageQueue.GetNextMessage();

                            if (nextMessageToSend != null)
                            {
                                Console.WriteLine("sending: " + nextMessageToSend.PrintedCommand());
                                ws.Send(nextMessageToSend.PrintedCommand());
                            }
                            if (nextMessageToSend.messageType == BotMessageFactory.MSG)
                            {
                                LastMessageSent = 0;
                            }
                        }
                    }

                    Uptime.Add(new TimeSpan(0, 0, 0, 0, TickTimeMiliseconds));

                    PerformCheckinIfNecessary(ref Uptime, ref LastCheckin);

                    Thread.Sleep(TickTimeMiliseconds);
                }
            }
        }

        private void GetNewApiTicket()
        {
            CurrentApiKey = null;
            WebRequests.LoginToServerPublic(AccountSettings);

            int timeout = 100000;
            while (WebRequests.ApiTicketResult == null)
            {
                timeout -= TickTimeMiliseconds;
                Thread.Sleep(TickTimeMiliseconds);
                if (TickTimeMiliseconds <= 0)
                {
                    Console.WriteLine("Timeout on acquiring new API ticket");
                    break;
                }
            }

            Console.WriteLine("Ticket acquired: " + WebRequests.ApiTicketResult.ticket);
            CurrentApiKey = WebRequests.ApiTicketResult.ticket;
        }

        private BotMessage GetNewIDNRequest()
        {
            var idn = new IDNclient()
            {
                account = AccountSettings.AccountName,
                ticket = CurrentApiKey,
                character = AccountSettings.CharacterName,
                cname = AccountSettings.CName,
                cversion = Version,
                method = "ticket"
            };
            return new BotMessage() { MessageDataFormat = idn, messageType = "IDN" };
        }

        private void PerformCheckinIfNecessary(ref TimeSpan uptime, ref TimeSpan lastCheckin)
        {
            if (Uptime - LastCheckin > CheckinInterval)
            {
                GetNewApiTicket();
                MessageQueue.AddMessage(GetNewIDNRequest());
                LastCheckin += (Uptime - LastCheckin);
            }
        }

        public void OnMessage(string data)
        {
            string[] pieces = data.Split(' ');
            if(pieces == null || pieces.Length < 1)
            {
                Console.WriteLine("Error OnMessage, data did not parse correctly.");
                return;
            }
            string messageType = pieces[0];

            string trimmedChatCommand = data.Substring(3).Trim();

            switch(messageType)
            {
                case "NLN": //a user connected
                case "STA": //status change for character
                case "FLN":
                    break;
                case "ICH": //initial channel users
                case "LIS": //all online characters, gender, status
                    break;
                case "COL": //sent in response to join channel
                    COLserver colinfo = JsonConvert.DeserializeObject<COLserver>(trimmedChatCommand);
                    ChannelsJoined.Add(colinfo.channel);
                    break;
                case "JCH": //someone joined a channel the bot is in
                    JCHserver jchInfo = JsonConvert.DeserializeObject<JCHserver>(trimmedChatCommand);
                    //TODO: greeting option for new channel users
                    break;
                case "LCH"://someone left a channel the bot is in
                    break;
                case "PIN": //ping from server. reply with ping
                    MessageQueue.AddMessage(new BotMessage() { MessageDataFormat = null, messageType = "PIN" });
                    break;
                case "PRI": //private message from a user
                    PRICmd msgContentPri = JsonConvert.DeserializeObject<PRICmd>(trimmedChatCommand);
                    InterpretChatCommand(new MSGserver() { character = msgContentPri.character, message = msgContentPri.message, channel = null });
                    break;
                case "MSG": //message sent in channel
                    MSGserver msgContent = JsonConvert.DeserializeObject<MSGserver>(trimmedChatCommand);
                    InterpretChatCommand(msgContent);
                    break;
                default:
                    Console.WriteLine("Recieved: " + (data.Length > 100? data.Substring(0, 100) : data));
                    break;
            }

            //if(_debug) //shows all messages recieved from server. very spammy.
            //    Console.WriteLine("Recieved: " + (data.Length > 100 ? data.Substring(0, 100) : data));
        }

        public void InterpretChatCommand(MSGserver messageContent)
        {
            //bot commands in chat all start with '!'
            if(messageContent.message.StartsWith("!"))
            {
                string[] splitSpace = messageContent.message.Split(' ');
                string commandName = splitSpace[0].Substring(1);
                string[] commandTerms = splitSpace.Skip(1).ToArray();
                ParseCommand(commandTerms, commandName, messageContent.channel, messageContent.character);
            }

            if(_debug && !string.IsNullOrEmpty(messageContent.channel))
            {
                MessageQueue.AddMessage(new BotMessage()
                {
                    messageType = BotMessageFactory.MSG,
                    MessageDataFormat = new MSGclient()
                    {
                        channel = messageContent.channel,
                        message = messageContent.channel + " " + messageContent.character + " " + messageContent.message
                    }
                });
            }
        }

        public void SendMessageInChannel(string message, string channel)
        {
            MessageQueue.AddMessage(BotMessageFactory.NewMessage(BotMessageFactory.MSG, new MSGclient() { channel = channel, message = message }));
        }

        public void SendPrivateMessage(string message, string recipient)
        {
            MessageQueue.AddMessage(BotMessageFactory.NewMessage(BotMessageFactory.PRI, new PRIclient() { recipient = recipient, message = message }));
        }

        public void JoinChannel(string channel)
        {
            MessageQueue.AddMessage(BotMessageFactory.NewMessage(BotMessageFactory.JCH, new JCHclient() { channel = channel }));
        }

        public void LeaveChannel(string channel)
        {
            MessageQueue.AddMessage(BotMessageFactory.NewMessage(BotMessageFactory.LCH, new LCHclient() { channel = channel }));
        }

        public void SetStatus(string status, string message)
        {
            MessageQueue.AddMessage(BotMessageFactory.NewMessage(BotMessageFactory.STA, new STAclient() { status = status, statusmsg = message }));
        }

        public void ParseCommand(string[] commandTerms, string commandName, string channel, string characterName)
        {
            string[] terms = Utils.LowercaseStrings(commandTerms);
            switch (commandName.ToLower())
            {
                case "roll":
                    {
                        if (!string.IsNullOrEmpty(channel))
                        {
                            if (_debug)
                                SendMessageInChannel("Command recieved: " + Utils.PrintList(terms), channel);

                            bool debugOverride = true;
                            string finalResult = DiceBot.GetRollResult(terms, debugOverride);

                            SendMessageInChannel(finalResult, channel);

                            if (_debug)
                                Console.WriteLine("Command finished: " + finalResult);
                        }
                    }
                    break;
                case "drawcard":
                    {
                        if (!string.IsNullOrEmpty(channel))
                        {
                            bool jokers = false;
                            bool deckDraw = true;
                            bool secretDraw = false;
                            string characterDrawName = GetCharacterDrawNameFromCommandTerms(characterName, terms);
                            if (terms != null && terms.Length >= 1 && terms.Contains("j"))
                                jokers = true;
                            if (terms != null && terms.Length >= 1 && terms.Contains("nodeck"))
                                deckDraw = false;
                            int numberDrawn = Utils.GetNumberFromInputs(terms);
                            if (terms != null && terms.Length >= 1 && (terms.Contains("s") || terms.Contains("secret")))
                                secretDraw = true;

                            string cardsS = "";
                            if (numberDrawn > 1)
                                cardsS = "s";

                            DeckType deckType = GetDeckTypeFromCommandTerms(terms);

                            string deckTypeString = Utils.GetDeckTypeStringHidePlaying(deckType);
                            string cardDrawingCharacterString = "[user]" + characterDrawName + "[/user]";
                            if (characterDrawName == DiceBot.DealerName)
                                cardDrawingCharacterString = "The dealer";
                            if (characterDrawName == DiceBot.BurnCardsName)
                                cardDrawingCharacterString = "Burned";
                            string messageOutput = "[i]" + cardDrawingCharacterString + ": " + deckTypeString + "Card" + cardsS + " drawn:[/i] " + DiceBot.DrawCards(numberDrawn, jokers, deckDraw, channel, deckType, characterDrawName);
                            if (secretDraw && !(characterDrawName == DiceBot.DealerName || characterDrawName == DiceBot.BurnCardsName))
                            {
                                SendPrivateMessage(messageOutput, characterName);
                            }
                            
                            if(secretDraw)
                            {
                                string newMessageOutput = "[i]" + cardDrawingCharacterString + ": " + deckTypeString + "Card" + cardsS + " drawn: (secret)[/i]";
                                SendMessageInChannel(newMessageOutput, channel);
                            }
                            else
                            {
                                SendMessageInChannel(messageOutput, channel);
                            }

                        }
                    }
                    break;
                case "discardcard":
                    {
                        if (!string.IsNullOrEmpty(channel))
                        {
                            bool all = false;
                            bool redraw = false;
                            bool secretDraw = false;
                            string characterDrawName = GetCharacterDrawNameFromCommandTerms(characterName, terms);
                            if (terms != null && terms.Length >= 1 && terms.Contains("all"))
                                all = true;
                            if (terms != null && terms.Length >= 1 && terms.Contains("redraw"))
                                redraw = true;
                            if (terms != null && terms.Length >= 1 && (terms.Contains("s") || terms.Contains("secret")))
                                secretDraw = true;

                            List<int> discardsTemp = Utils.GetAllNumbersFromInputs(terms);
                            List<int> discards = new List<int>();

                            //decrease all the numbers by 1 to match array indexes, rather than the card position for a player
                            if(discardsTemp.Count > 0)
                            {
                                foreach (int i in discardsTemp)
                                {
                                    discards.Add(i - 1);
                                }
                            }

                            string cardsS = "";
                            if (discards.Count > 1)
                                cardsS = "s";

                            DeckType deckType = GetDeckTypeFromCommandTerms(terms);

                            string deckTypeString = Utils.GetDeckTypeStringHidePlaying(deckType);
                            string cardDrawingCharacterString = "[user]" + characterDrawName + "[/user]";
                            if (characterDrawName == DiceBot.DealerName)
                                cardDrawingCharacterString = "The dealer";
                            if (characterDrawName == DiceBot.BurnCardsName)
                                cardDrawingCharacterString = "Burned";

                            int numberDiscards = 0;
                            string messageOutput = "[i]" + cardDrawingCharacterString + ": " + deckTypeString + "Card" + cardsS + " discarded:[/i] " + DiceBot.DiscardCards(discards, all, channel, deckType, characterDrawName, out numberDiscards);
                            if(redraw)
                            {
                                messageOutput += "\n [i]Redrawn:[/i] " + DiceBot.DrawCards(numberDiscards, false, true, channel, deckType, characterDrawName);
                            }
                                
                            if (secretDraw && !(characterDrawName == DiceBot.DealerName || characterDrawName == DiceBot.BurnCardsName))
                            {
                                SendPrivateMessage(messageOutput, characterName);
                            }
                            
                            if (secretDraw)
                            {
                                string redrawSecretString = redraw ? " (and redrew)" : "";
                                string newMessageOutput = "[i]" + cardDrawingCharacterString + ": " + deckTypeString + "Card" + cardsS + " discarded: (secret)" + redrawSecretString + "[/i] ";
                                SendMessageInChannel(newMessageOutput, channel);
                            }
                            else
                            {
                                SendMessageInChannel(messageOutput, channel);
                            }

                        }
                    }
                    break;
                case "resetdeck":
                    {
                        if (!string.IsNullOrEmpty(channel))
                        {
                            bool jokers = false;
                            if (commandTerms != null && commandTerms.Length >= 1 && terms.Contains("j"))
                                jokers = true;

                            DeckType deckType = GetDeckTypeFromCommandTerms(terms);

                            string deckTypeString = Utils.GetDeckTypeStringHidePlaying(deckType);

                            DiceBot.ResetDeck(jokers, channel, deckType);
                            SendMessageInChannel("[i]" + deckTypeString + "Channel deck reset." + (jokers ? " (contains jokers)" : "") + "[/i]", channel);
                        }
                    }
                    break;
                case "shuffledeck":
                    {
                        if (!string.IsNullOrEmpty(channel))
                        {
                            bool fullShuffle = false;
                            if (commandTerms != null && commandTerms.Length >= 1 && terms.Contains("eh"))
                                fullShuffle = true;

                            DeckType deckType = GetDeckTypeFromCommandTerms(terms);

                            string deckTypeString = Utils.GetDeckTypeStringHidePlaying(deckType);

                            DiceBot.ShuffleDeck(DiceBot.random, channel, deckType, fullShuffle);
                            SendMessageInChannel("[i]" + deckTypeString + "Channel deck shuffled. " + (fullShuffle ? "Hands emptied." : "") + "[/i]", channel);
                        }
                    }
                    break;
                case "endhand":
                    {
                        if (!string.IsNullOrEmpty(channel))
                        {
                            DeckType deckType = GetDeckTypeFromCommandTerms(terms);

                            string deckTypeString = Utils.GetDeckTypeStringHidePlaying(deckType);
                            DiceBot.EndHand(channel, deckType);
                            SendMessageInChannel("[i]" + deckTypeString + "All hands have been emptied.[/i]", channel);
                        }
                    }
                    break;
                case "showhand":
                    {
                        if (!string.IsNullOrEmpty(channel))
                        {
                            string characterDrawName = GetCharacterDrawNameFromCommandTerms(characterName, terms);

                            DeckType deckType = GetDeckTypeFromCommandTerms(terms);

                            string deckTypeString = Utils.GetDeckTypeStringHidePlaying(deckType);
                            Hand h = DiceBot.GetHand(channel, deckType, characterDrawName);

                            string outputString = "[i]" + deckTypeString + "Showing [user]" + characterDrawName + "[/user]'s hand: [/i]" + h.ToString();
                            if(characterDrawName == DiceBot.BurnCardsName)
                                 outputString = "[i]" + deckTypeString + "Showing burned cards: [/i]" + h.ToString();
                            if(characterDrawName == DiceBot.DealerName)
                                 outputString = "[i]" + deckTypeString + "Showing the dealer's hand: [/i]" + h.ToString();

                            SendMessageInChannel(outputString, channel);
                        }
                    }
                    break;
                case "decklist":
                    {
                        if (!string.IsNullOrEmpty(channel))
                        {
                            if (Utils.IsCharacterAdmin(AccountSettings.AdminCharacters, characterName))
                            {
                                DeckType deckType = GetDeckTypeFromCommandTerms(terms);

                                string deckTypeString = Utils.GetDeckTypeStringHidePlaying(deckType);

                                Deck a = DiceBot.GetDeck(channel, deckType);

                                SendMessageInChannel("[i]" + deckTypeString + "Channel deck contents: [/i]" + a.ToString(), channel);
                            }
                            else
                            {
                                SendMessageInChannel("You do not have authorization to complete this command.", channel);
                            }
                        }
                    }
                    break;
                case "deckinfo":
                    {
                        if (!string.IsNullOrEmpty(channel))
                        {
                            DeckType deckType = GetDeckTypeFromCommandTerms(terms);

                            string deckTypeString = Utils.GetDeckTypeStringHidePlaying(deckType);

                            Deck a = DiceBot.GetDeck(channel, deckType);
                            string cardsString = a.GetCardsRemaining() + " / " + a.GetTotalCards();
                            string jokersString = a.ContainsJokers()? " [i](contains jokers)[/i] " : "";
                            SendMessageInChannel("[i]" + deckTypeString + "Channel deck cards remaining: [/i]" + cardsString + jokersString, channel);
                        }
                    }
                    break;
                case "botinfo":
                    {
                        if (!string.IsNullOrEmpty(channel))
                        {
                            int channelsNumber = ChannelsJoined.Count();
                            TimeSpan onlineTime =  DateTime.UtcNow - LoginTime;
                            SendMessageInChannel("Dice Bot was developed by [user]Darkness Syndra[/user] on 10/12/2020"
                                + "\nversion " + Version 
                                + "\ncurrently operating in " + channelsNumber + " channels."
                                + "\nonline for " + Utils.GetTimeSpanPrint(onlineTime)
                                + "\nfor a list of commands, see the profile [user]Dice Bot[/user].", channel);
                        }
                    }
                    break;
                case "uptime":
                    {
                        if (!string.IsNullOrEmpty(channel))
                        {
                            int channelsNumber = ChannelsJoined.Count();
                            TimeSpan onlineTime = DateTime.UtcNow - LoginTime;
                            SendMessageInChannel("Dicebot has been online for " + Utils.GetTimeSpanPrint(onlineTime), channel);
                        }
                    }
                    break;
                case "help":
                    {
                        if (!string.IsNullOrEmpty(channel))
                            SendMessageInChannel("Commands:\n!roll\n!drawcard,!resetdeck,!shuffledeck,!endhand,!showhand,!deckinfo\n!joinchannel\n!botinfo,!uptime,!help\nFor full information on commands, see the profile [user]Dice Bot[/user].", channel);
                    }
                    break;
                case "joinchannel":
                    {
                        string channelId = Utils.GetChannelIdFromInputs(commandTerms);

                        string sendMessage = "Attempting to join channel: " + channelId;

                        JoinChannel(channelId);

                        //send to character if there is no origin channel for this command
                        if (string.IsNullOrEmpty(channel))
                        {
                            SendPrivateMessage(sendMessage, characterName);
                        }
                        else
                        {
                            SendMessageInChannel(sendMessage, channel);
                        }
                    }
                    break;
                case "leavethischannel":
                    {
                        if (!string.IsNullOrEmpty(channel))
                        {
                            SendMessageInChannel("Goodbye~", channel);
                            LeaveChannel(channel);
                        }
                    }
                    break;
                case "setstartingchannel":
                    {
                        if (!string.IsNullOrEmpty(channel))
                        {
                            StartingChannel existing = StartingChannels.FirstOrDefault(a => a.Code == channel.ToLower());

                            string sendMessage = "[b]Added[/b] ";

                            if (existing != null)
                            {
                                sendMessage = "[b]Removed[/b] ";
                                StartingChannels.Remove(existing);
                            }
                            else
                            {
                                StartingChannel newChannel = new StartingChannel()
                                {
                                    Code = channel.ToLower(),
                                    CharacterInvitedName = characterName,
                                    Essential = false,
                                    Name = channel.ToLower()
                                };
                                StartingChannels.Add(newChannel);
                            }

                            Utils.WriteToFileAsData(StartingChannels, Utils.GetTotalFileName(FileFolder, StartingChannelsFileName));

                            SendMessageInChannel(sendMessage, channel);
                        }
                    }
                    break;
                case "nope":
                    {
                        if (!string.IsNullOrEmpty(channel))
                            SendMessageInChannel("Yep.", channel);
                    }
                    break;
            }
        }

        private DeckType GetDeckTypeFromCommandTerms(string[] terms)
        {
            DeckType deckType = DeckType.Playing;
            if (terms != null && terms.Length >= 1 && terms.Contains("tarot"))
                deckType = DeckType.Tarot;
            if (terms != null && terms.Length >= 1 && terms.Contains("manythings"))
                deckType = DeckType.ManyThings;

            return deckType;
        }

        private string GetCharacterDrawNameFromCommandTerms(string characterName, string[] terms)
        {
            string characterDrawName = characterName;
            if (terms != null && terms.Length >= 1 && terms.Contains("dealer"))
                characterDrawName = DiceBot.DealerName;
            if (terms != null && terms.Length >= 1 && terms.Contains("burn"))
                characterDrawName = DiceBot.BurnCardsName;

            return characterDrawName;
        }
    }
}
