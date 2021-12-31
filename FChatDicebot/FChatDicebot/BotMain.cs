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
        public string CurrentApiKey = "";
        public const string FListChatUri = "wss://chat.f-list.net/chat2";
        public const bool _debug = false;
        public const bool _returnAllRecievedChatMessagesFromChannels = false;
        public const bool _testVersion = false;
        public const string Version = "1.14b";

        public const string FileFolder = "C:\\BotData\\DiceBot";
        public const string TestFilePrefix = "_test_";
        public const string AccountSettingsFileName = "account_settings.txt";
        public const string SavedTablesFileName = "saved_tables.txt";
        public const string SavedDecksFileName = "saved_decks.txt";
        public const string ChannelSettingsFileName = "channel_settings.txt";
        public const string LogFileName = "outputlog.txt";
        public const string SavedChipsFileName = "saved_chipPiles.txt";
        public const string ChipsCouponsFileName = "coupons_active.txt";
        public const string ChipsCouponsInactiveFileName = "coupons_inactive.txt";

        public const string BotTestingStatus = "Dicebot is currently undergoing [color=yellow]testing[/color]. Performance may be impacted.";
        public const string BotOnlineStatus = "[color=yellow]Dice Bot v" + Version + " Online![/color] Use '!help' for commands and check out the [user]Dice Bot[/user] profile for instructions. You can add Dicebot to your channel with !joinchannel [[i]channel invite code paste[/i]]";

        public const string HandCollectionName = "hand";
        public const string InPlayCollectionName = "cards in play";
        public const string DiscardCollectionName = "discard";

        public BotWebRequests WebRequests;
        public BotMessageQueue MessageQueue;
        public BotCommandController BotCommandController;
        public DiceBot DiceBot;
        public AccountSettings AccountSettings;
        public List<SavedRollTable> SavedTables;
        public List<SavedDeck> SavedDecks;
        public List<ChannelSettings> SavedChannelSettings;
        public List<ChipsCoupon> ChipsCoupons;

        public const int StartingChipsInPile = 500;

        public const int MinimumCharactersTableId = 2;
        public const int MaximumCharactersTableName = 30;
        public const int MaximumCharactersTableDescription = 300;
        public const int MaximumCharactersTableEntryDescription = 200;
        public const int MaximumRollTriggersPerEntry = 4;
        public const int MaximumSavedTablesPerCharacter = 3;
        public const int MaximumSavedEntriesPerTable = 50;
        public const int MaximumCardsInDeck = 200;
        public const int ReconnectTimeMs = 300000; //5 minutes reconnect time
        public const int InitialWaitTime = 5000;

        public const int MaximumCharsInMessage = 11000;

        public TimeSpan Uptime;
        public TimeSpan LastCheckin;
        public DateTime LoginTime;
        public int TickTimeMiliseconds = 200;
        public int MinimumTimeBetweenMessages = 1500; //minimum 1 second between messages for FList bot rules
        public int ReconnectTimer = 0;
        public TimeSpan CheckinInterval = new TimeSpan(0, 0, 20, 0, 0);
        public List<string> ChannelsJoined = new List<string>();
        public List<UserGeneratedCommand> WaitingChannelOpRequests = new List<UserGeneratedCommand>();

        public delegate void BotCommandDelegate();

        public void Run()
        {
            BotCommandController = new BotCommandController(this);

            LoadAccountSettings();

            LoadChannelSettings();

            LoadSavedTables();

            LoadChipsCoupons();

            LoadSavedDecks();

            MessageQueue = new BotMessageQueue();
            WebRequests = new BotWebRequests();
            DiceBot = new DiceFunctions.DiceBot(this);

            System.Threading.Thread th = new Thread(RunLoop);
            th.IsBackground = true;
            th.Start();
        }

        private void LoadAccountSettings()
        {
            string settingsFile = AccountSettingsFileName;
            
            string path = Utils.GetTotalFileName(FileFolder, settingsFile);
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
                        Console.WriteLine("loaded LoadAccountSettings successfully.");
                }
                else
                {
                    Console.WriteLine("LoadAccountSettings file does not exist.");
                }
            }
            catch (System.Exception exc)
            {
                Console.WriteLine("Exception: Failed to load LoadAccountSettings for " + path + "\n" + exc.ToString());
            }
        }

        private void LoadChannelSettings()
        {
            string path = Utils.GetTotalFileName(FileFolder, ChannelSettingsFileName);
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

                    SavedChannelSettings = JsonConvert.DeserializeObject<List<ChannelSettings>>(fileText);

                    if (_debug)
                        Console.WriteLine("loaded LoadChannelSettings successfully.");
                }
                else
                {
                    SavedChannelSettings = new List<ChannelSettings>();
                    //LoadStartingChannels();
                    Console.WriteLine("LoadChannelSettings file does not exist.");
                }
            }
            catch (System.Exception exc)
            {
                Console.WriteLine("Exception: Failed to load LoadChannelSettings for " + path + "\n" + exc.ToString());
            }
        }

        private void LoadSavedTables()
        {
            string path = Utils.GetTotalFileName(FileFolder, SavedTablesFileName);
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

                    SavedTables = JsonConvert.DeserializeObject<List<SavedRollTable>>(fileText);

                    if (_debug)
                        Console.WriteLine("loaded SavedTables successfully.");
                }
                else
                {
                    Console.WriteLine("LoadSavedTables file does not exist.");
                }
            }
            catch (System.Exception exc)
            {
                Console.WriteLine("Exception: Failed to load LoadSavedTables for " + path + "\n" + exc.ToString());
            }
        }

        private void LoadSavedDecks()
        {
            string path = Utils.GetTotalFileName(FileFolder, SavedDecksFileName);
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

                    SavedDecks = JsonConvert.DeserializeObject<List<SavedDeck>>(fileText);

                    if (_debug)
                        Console.WriteLine("loaded SavedDecks successfully.");

                    if (SavedDecks == null)
                        SavedDecks = new List<SavedDeck>();
                }
                else
                {
                    Console.WriteLine("LoadSavedDecks file does not exist.");
                }
            }
            catch (System.Exception exc)
            {
                Console.WriteLine("Exception: Failed to load LoadSavedTables for " + path + "\n" + exc.ToString());
            }
        }

        private void LoadChipsCoupons()
        {
            ChipsCoupons = new List<ChipsCoupon>();

            string path = Utils.GetTotalFileName(FileFolder, ChipsCouponsFileName);
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

                    ChipsCoupons =  JsonConvert.DeserializeObject<List<ChipsCoupon>>(fileText);

                    if (_debug)
                        Console.WriteLine("loaded ChipsCoupons successfully.");
                }
                else
                {
                    Console.WriteLine("ChipsCoupons file does not exist.");
                }
            }
            catch (System.Exception exc)
            {
                Console.WriteLine("Exception: Failed to load ChipsCoupons for " + path + "\n" + exc.ToString());
            }
        }

        private int PinMessageSent = 0;
        public void RunLoop()
        {
            if(_debug)
                Console.WriteLine("Runloop started");

            Uptime = new TimeSpan();
            LastCheckin = new TimeSpan();

            int LastMessageSent = 0;

            using (var ws = new WebSocket(FListChatUri))
            {
                BotMessage firstIdnRequest = GetIdentificationTicket();

                //ws.OnOpen += (sender, e) =>
                //ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.None;

                ws.OnMessage += (sender, e) =>
                    OnMessage(e.Data);

                ws.OnError += (sender, e) =>
                {
                    Console.WriteLine("Websocket Error: " + e.Message);
                    Utils.AddToLog("Websocket Error: " + e.Message, null);
                };

                ws.OnClose += (sender, e) =>
                {
                    Console.WriteLine("WebSocket Close ({0}) ({1})", e.Code, e.Reason);
                    Utils.AddToLog(string.Format("WebSocket Close ({0}) ({1})", e.Code, e.Reason), null);

                    var sslProtocolHack = (System.Security.Authentication.SslProtocols)(SslProtocolsHack.Tls12 | SslProtocolsHack.Tls11 | SslProtocolsHack.Tls);
                    //TlsHandshakeFailure
                    if (e.Code == 1015 && ws.SslConfiguration.EnabledSslProtocols != sslProtocolHack)
                    {
                        Console.WriteLine("activating ssl protocol change");
                        //Utils.AddToLog("activating ssl protocol change", null);
                        ws.SslConfiguration.EnabledSslProtocols = sslProtocolHack;
                        Thread.Sleep(1000);
                        ws.Connect();
                    }
                };


                ws.Origin = "http://localhost:4649";

                ws.EnableRedirection = true;

                PerformStartupClientCommands(ws, firstIdnRequest, false);

                while(true)
                {
                    if (ws.IsAlive)
                    {
                        LastMessageSent += TickTimeMiliseconds;

                        if (MessageQueue.HasMessages())
                        {
                            BotMessage nextMessagePeek = MessageQueue.Peek();

                            if (!Utils.BotMessageIsChatMessage(nextMessagePeek) || LastMessageSent >= MinimumTimeBetweenMessages)
                            {
                                BotMessage nextMessageToSend = MessageQueue.GetNextMessage();

                                if (nextMessageToSend != null)
                                {
                                    if (nextMessageToSend.messageType == "PIN")
                                    {
                                        PinMessageSent++;
                                        if(PinMessageSent % 5 == 0)
                                        {
                                            Console.WriteLine("sending: " + nextMessageToSend.PrintedCommand() + " (#" + PinMessageSent + ")");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("sending: " + nextMessageToSend.PrintedCommand());
                                        Utils.AddToLog("sending: " + nextMessageToSend.PrintedCommand(), nextMessageToSend);
                                    }

                                    ws.Send(nextMessageToSend.PrintedCommand());
                                }
                                if (Utils.BotMessageIsChatMessage(nextMessageToSend))
                                {
                                    LastMessageSent = 0;
                                }
                            }
                        }
                        if (WaitingChannelOpRequests.Count > 0)
                        {
                            if(_testVersion)
                                Console.WriteLine("Channeloprequest found in queue");

                            List<UserGeneratedCommand> finished = new List<UserGeneratedCommand>();
                            foreach(UserGeneratedCommand req in WaitingChannelOpRequests)
                            {
                                if(req.ops != null)
                                {
                                    BotCommandController.RunChatBotCommand(req);
                                    finished.Add(req);
                                }
                            }
                            if (finished.Count > 0)
                            {
                                foreach (UserGeneratedCommand rFin in finished)
                                {
                                    WaitingChannelOpRequests.Remove(rFin);
                                }
                            }
                        }

                        Uptime.Add(new TimeSpan(0, 0, 0, 0, TickTimeMiliseconds));

                        PerformCheckinIfNecessary(ref Uptime, ref LastCheckin);
                    }
                    else
                    {
                        if(ReconnectTimer == 0)
                        {
                            Console.WriteLine("Connection was lost.");
                            Console.WriteLine("Waiting for " + (ReconnectTimeMs / 1000) + " seconds to reconnect.");
                            Utils.AddToLog("Waiting for " + (ReconnectTimeMs / 1000) + " seconds to reconnect.", null);
                        }

                        ReconnectTimer += TickTimeMiliseconds;

                        if (ReconnectTimer > ReconnectTimeMs)
                        {
                            BotMessage reconnectIdnRequest = GetIdentificationTicket();
                            PerformStartupClientCommands(ws, reconnectIdnRequest, true);
                            ReconnectTimer = 0;
                        }
                    }
                    Thread.Sleep(TickTimeMiliseconds);
                }
            }
        }

        private BotMessage GetIdentificationTicket()
        {
            if(GetNewApiTicket())
            {
                return GetNewIDNRequest();
            }
            else
            {
                return null;
            }
        }

        private void PerformStartupClientCommands(WebSocket sock, BotMessage firstIdnRequest, bool reconnect)
        {
            if (firstIdnRequest == null)
                return;

            Console.WriteLine("connecting...");
            sock.Connect();

            if (!sock.IsAlive)
                return;

            sock.Send(firstIdnRequest.PrintedCommand());

            LoginTime = DateTime.UtcNow;

            //required to wait for the server to stop sending startup messages, otherwise overflow occurs and ws closes
            Console.WriteLine("waiting for " + InitialWaitTime + "ms...");
            Thread.Sleep(InitialWaitTime);

            JoinStartingChannels();

            Thread.Sleep(1000);

            string botStatus = BotOnlineStatus;
            if (_testVersion)
                botStatus = BotTestingStatus;

            SetStatus(STAStatus.Online, botStatus);
        }

        private void JoinStartingChannels()
        {
            ChannelsJoined = new List<string>();

            if (SavedChannelSettings.Count > 0)
            {
                foreach (ChannelSettings channel in SavedChannelSettings)
                {
                    if(channel.StartupChannel)
                        JoinChannel(channel.Name);
                }
            }
        }

        private bool GetNewApiTicket()
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
                    return false;
                }
            }

            Console.WriteLine("Ticket acquired: " + WebRequests.ApiTicketResult.ticket);
            CurrentApiKey = WebRequests.ApiTicketResult.ticket;
            return true;
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
            Utils.AddToLog("idn request created", idn);
            Console.WriteLine("idn request created: " + JsonConvert.SerializeObject(idn));

            return new BotMessage() { MessageDataFormat = idn, messageType = "IDN" };
        }

        private void PerformCheckinIfNecessary(ref TimeSpan uptime, ref TimeSpan lastCheckin)
        {
            if (Uptime - LastCheckin > CheckinInterval)
            {
                bool newTicket = GetNewApiTicket();
                Utils.AddToLog("performing checkin, newticket? " + newTicket, AccountSettings);
                Console.WriteLine("performing checkin, newticket? " + newTicket);
                if(newTicket)
                {
                    MessageQueue.AddMessage(GetNewIDNRequest());
                }
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
                case "RLL": //a user generated /roll using FChat's roll
                    break;
                case "ICH": //initial channel users
                case "LIS": //all online characters, gender, status
                    break;
                case "COL": //sent in response to join channel
                    COLserver colinfo = JsonConvert.DeserializeObject<COLserver>(trimmedChatCommand);

                    Console.WriteLine("Recieved: " + (data.Length > 300 ? data.Substring(0, 300) : data));
                    UserGeneratedCommand req = WaitingChannelOpRequests.FirstOrDefault(b => b.channel == colinfo.channel);

                    if(req != null)
                    {
                        Console.WriteLine("channelops returned COL, channel ops returned");
                        req.ops = colinfo.oplist;
                    }
                    else
                    {
                        ChannelsJoined.Add(colinfo.channel);
                    }
                    break;
                case "JCH": //someone joined a channel the bot is in
                    JCHserver jchInfo = JsonConvert.DeserializeObject<JCHserver>(trimmedChatCommand);

                    ChannelSettings s = GetChannelSettings(jchInfo.channel);

                    if(s.GreetNewUsers)
                    {
                        SendMessageInChannel("Welcome to the channel, " + Utils.GetCharacterUserTags(jchInfo.character.identity) + "!", jchInfo.channel);
                    }
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
                    Console.WriteLine("Recieved: " + (data.Length > 300? data.Substring(0, 300) : data));
                    break;
            }

        }

        public void InterpretChatCommand(MSGserver messageContent)
        {
            //bot commands in chat all start with '!'
            if(messageContent.message.StartsWith("!") && messageContent.message.Length > 1)
            {
                string commandName = "";
                string[] commandTerms = SeparateCommandTerms(messageContent.message, out commandName);
                //string[] splitSpace = messageContent.message.Split(' ');
                //string commandName = splitSpace[0].Substring(1).ToLower();
                //string[] commandTerms = splitSpace.Skip(1).ToArray();

                BotCommandController.RunChatBotCommand(new UserGeneratedCommand(){
                    rawTerms = commandTerms, 
                    channel = messageContent.channel,
                    characterName = messageContent.character,
                    commandName = commandName,
                    ops = null,
                    terms = null
                });//commandTerms, commandName, messageContent.character, messageContent.channel);
            }

            if (_returnAllRecievedChatMessagesFromChannels && !string.IsNullOrEmpty(messageContent.channel))
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

        public string[] SeparateCommandTerms(string rawCommandMessage, out string commandName)
        {
            string[] splitSpace = rawCommandMessage.Split(' ');
            commandName = splitSpace[0].Substring(1).ToLower();
            string[] commandTerms = splitSpace.Skip(1).ToArray();
            return commandTerms;
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

        public void RequestChannelOpListAndQueueFurtherRequest(UserGeneratedCommand command)
        {
            WaitingChannelOpRequests.Add(command);
            MessageQueue.AddMessage(BotMessageFactory.NewMessage(BotMessageFactory.COL, new COLclient() { channel = command.channel } ));
        }

        public ChannelSettings GetChannelSettings(string channel)
        {
            ChannelSettings set =  SavedChannelSettings.FirstOrDefault(a => a.Name.ToLower() == channel.ToLower());

            if(set == null)
            {
                set = new ChannelSettings()
                {
                    Name = channel,
                    AllowCustomTableRolls = true,
                    AllowTableRolls = true,
                    AllowTableInfo = true,
                    AllowChips = true,
                    AllowGames = true,
                    ChipsClearance = ChipsClearanceLevel.NONE
                };

                SavedChannelSettings.Add(set);
                Utils.WriteToFileAsData(SavedChannelSettings, Utils.GetTotalFileName(BotMain.FileFolder, BotMain.ChannelSettingsFileName));
            }

            return set;
        }
    }

    public enum SslProtocolsHack
    {
        Tls = 192,
        Tls11 = 768,
        Tls12 = 3072
    }
}
