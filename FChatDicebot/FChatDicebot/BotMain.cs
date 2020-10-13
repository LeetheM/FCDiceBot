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

namespace FChatDicebot
{
    public class BotMain
    {
        //TODO: auto reconnect on disconnect
        public string CurrentApiKey = "";
        public const string FListChatUri = "wss://chat.f-list.net/chat2";
        public const bool _debug = false;

        public BotWebRequests WebRequests;
        public BotMessageQueue MessageQueue;
        public DiceBot DiceBot;
        public TimeSpan Uptime;
        public TimeSpan LastCheckin;
        public int TickTimeMiliseconds = 200;
        public int MinimumTimeBetweenMessages = 1200; //minimum 1 second between messages for FList bot rules
        public TimeSpan CheckinInterval = new TimeSpan(0, 0, 20, 0, 0);


        public void Run()
        {
            MessageQueue = new BotMessageQueue();
            WebRequests = new BotWebRequests();
            DiceBot = new DiceFunctions.DiceBot();

            System.Threading.Thread th = new Thread(RunLoop);
            th.IsBackground = true;
            th.Start();
        }

        public void RunLoop()
        {
            if(_debug)
                Console.WriteLine("Runloop started");

            GetNewApiTicket();

            string testChannel = "adh-fb744504ecce0909b18b";

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

                //required to wait for the server to stop sending startup messages, otherwise overflow occurs and ws closes
                Thread.Sleep(5000);

                //TODO: load list of startup channels to join
                JoinChannel(testChannel);

                Thread.Sleep(1000);

                SendMessageInChannel("Dicebot Online", testChannel);
                SetStatus(STAStatus.Online, "[color=yellow]Dice Bot v" + BotWebRequests.CVersion + " Online![/color] Check out the [user]Dice Bot[/user] profile for a list of commands. You can add Dicebot to your channel with !joinchannel [[i]channel invite code paste[/i]]");

                while(true)
                {
                    LastMessageSent += TickTimeMiliseconds;

                    if(MessageQueue.HasMessages())
                    {
                        BotMessage nextMessagePeek = MessageQueue.Peek();

                        if (nextMessagePeek.messageType != BotMessageFactory.MSG || LastMessageSent >= MinimumTimeBetweenMessages)
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
            WebRequests.LoginToServerPublic();

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
                account = BotWebRequests.AccountName,
                ticket = CurrentApiKey,
                character = BotWebRequests.CharacterName,
                cname = BotWebRequests.CName,
                cversion = BotWebRequests.CVersion,
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

                            string messageOutput = "[i]Card" + cardsS + " drawn:[/i] " + DiceBot.DrawCards(numberDrawn, jokers, deckDraw, channel, characterName);
                            if (secretDraw)
                            {
                                SendPrivateMessage(messageOutput, characterName);
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
                            DiceBot.ResetDeck(jokers, channel);
                            SendMessageInChannel("[i]Channel deck reset." + (jokers ? " (contains jokers)" : "") + "[/i]", channel);
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
                            DiceBot.ShuffleDeck(DiceBot.random, channel, fullShuffle);
                            SendMessageInChannel("[i]Channel deck shuffled. " + (fullShuffle ? "Hands emptied." : "") + "[/i]", channel);
                        }
                    }
                    break;
                case "endhand":
                    {
                        if (!string.IsNullOrEmpty(channel))
                        {
                            DiceBot.EndHand(channel);
                            SendMessageInChannel("[i]All hands have been emptied.[/i]", channel);
                        }
                    }
                    break;
                case "showhand":
                    {
                        if (!string.IsNullOrEmpty(channel))
                        {
                            Hand h = DiceBot.GetHand(channel, characterName);
                            SendMessageInChannel("[i]Showing [user]" + characterName + "[/user]'s hand: [/i]" + h.ToString(), channel);
                        }
                    }
                    break;
                case "decklist":
                    {
                        if (!string.IsNullOrEmpty(channel))
                        {
                            if (DiceBot.IsCharacterAdmin(characterName))
                            {
                                Deck a = DiceBot.GetDeck(channel);

                                SendMessageInChannel("[i]Channel deck contents: [/i]" + a.ToString(), channel);
                            }
                            else
                            {
                                SendMessageInChannel("You do not have authorization to complete this command.", channel);
                            }
                        }
                    }
                    break;
                case "botinfo":
                    {
                        if (!string.IsNullOrEmpty(channel))
                            SendMessageInChannel("Dice Bot was developed by [user]Darkness Syndra[/user] on 10/12/2020"
                                + "\nversion " + DiceBot.Version
                                + "\nfor a list of commands, see the profile [user]Dice Bot[/user].", channel);
                    }
                    break;
                case "help":
                    {
                        if (!string.IsNullOrEmpty(channel))
                            SendMessageInChannel("Commands:\n!roll\n!drawcard\n!resetdeck\n!shuffledeck\n!endhand\n!showhand\n!botinfo\n!joinchannel\n!help\nFor full information on commands, see the profile [user]Dice Bot[/user].", channel);
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
                case "nope":
                    {
                        if (!string.IsNullOrEmpty(channel))
                            SendMessageInChannel("Yep.", channel);
                    }
                    break;
            }
        }
    }
}
