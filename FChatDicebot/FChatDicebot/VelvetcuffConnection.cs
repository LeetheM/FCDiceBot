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
    public class VelvetcuffConnection
    {
        private BotWebRequests WebRequests;
        private AccountSettings AccountSettings;

        private DateTime OriginDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        public const double MaximumSecondsForVcToken = 60 * 60 * 8; //8 hour maximum seconds
        public const double MaximumSecondsForVcTransaction = 60 * 60 * 8; //8 hour, was 24 hour maximum seconds in 1.32c
        public double LastTokenRetrieved = 0;

        public const int TimeoutVcRequests = 60000; //1 minute max for all retries
        public const int TimeIntervalCheckOrdersSeconds = 30;

        public string CurrentOAuthVcKey = "";
        public string CurrentPaymentId = "";

        List<CheckAttempt> CheckThreads;

        public VelvetcuffConnection(BotWebRequests botWebRequests, AccountSettings accountSettings)
        {
            WebRequests = botWebRequests;
            AccountSettings = accountSettings;
            CheckThreads = new List<CheckAttempt>();
        }
        public static DateTime ConvertFromSecondsTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(timestamp);
        }

        public static double ConvertToSecondsTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }

        public void CheckVelvetcuffOrders(BotMain bot)
        {
            double currentTime = ConvertToSecondsTimestamp(DateTime.UtcNow);
            foreach(VcChipOrder order in bot.DiceBot.VcChipOrders)
            {
                if((order.CheckedCount < 10 && order.LastCheckedTime + TimeIntervalCheckOrdersSeconds < currentTime) ||
                    (order.LastCheckedTime + (TimeIntervalCheckOrdersSeconds * 10) < currentTime))//make a callback for an order when it returns a status
                {
                    CheckAttempt existing = CheckThreads.FirstOrDefault(ab => ab.OrderId == order.TransactionId);

                    if(existing != null)
                    {
                        if(order.LastCheckedTime + TimeoutVcRequests < currentTime)
                        {
                            if(existing.CheckThread != null)
                            {
                                existing.CheckThread.Abort();
                            }
                            existing.CheckThread = null;
                            CheckThreads.RemoveAll(a => a.OrderId == order.TransactionId);
                        }

                    }
                    else
                    {
                        Thread t = new Thread(() => CheckVcTransactionAndAct(order, bot, order.Created + MaximumSecondsForVcTransaction < currentTime));
                        t.IsBackground = true;
                        t.Start();

                        CheckThreads.Add(new CheckAttempt()
                        {
                            CheckThread = t,
                            StartTime = currentTime,
                            OrderId = order.TransactionId
                        });

                        order.LastCheckedTime = currentTime;
                        order.CheckedCount++;
                    }

                }
            }
            int beforeCount = bot.DiceBot.VcChipOrders.Count;
            bot.DiceBot.VcChipOrders.RemoveAll(a => a.OrderStatus >= 1);

            int afterCount = bot.DiceBot.VcChipOrders.Count;
            if(beforeCount != afterCount)
            {
                bot.BotCommandController.SaveVcChipOrdersToDisk();
            }
        }

        public bool NeedNewVelvetucuffOauthToken()
        {
            if (string.IsNullOrEmpty(CurrentOAuthVcKey))
                return true;

            double currentSeconds = ConvertToSecondsTimestamp(DateTime.UtcNow);
            if(currentSeconds > LastTokenRetrieved + MaximumSecondsForVcToken)
            {
                return true;
            }
            return false;
        }

        public bool GetNewVelvetcuffOauthToken()
        {
            CurrentOAuthVcKey = null;
            WebRequests.LoginToVCPublic(AccountSettings);

            WebRequests.VcOauthResult = null;
            WebRequests.AbandonWebRequestVc = false;

            int timeout = TimeoutVcRequests;
            while (WebRequests.VcOauthResult == null)
            {
                timeout -= BotMain.TickTimeMiliseconds;
                Thread.Sleep(BotMain.TickTimeMiliseconds);
                if (BotMain.TickTimeMiliseconds <= 0)
                {
                    WebRequests.AbandonWebRequestVc = true;
                    Console.WriteLine("Timeout on acquiring new LoginToVC Oauth token");
                    return false;
                }
            }

            LastTokenRetrieved = ConvertToSecondsTimestamp(DateTime.UtcNow);
            if(BotMain._debug)
                Console.WriteLine("VcOauthResult token acquired: " + WebRequests.VcOauthResult.access_token);
            CurrentOAuthVcKey = WebRequests.VcOauthResult.access_token;
            return true;
        }

        public bool CreateNewVcTransaction(int transactionAmount, string targetChar, string sourceChar, bool sendingMoney, string customMessage)
        {
            CurrentPaymentId = null;
            if (NeedNewVelvetucuffOauthToken())
            {
                bool gotToken = GetNewVelvetcuffOauthToken();
                if(!gotToken)
                {
                    return false;
                }
            }

            WebRequests.VcCreateTransactionResponse = null;
            WebRequests.AbandonVCCreatePayment = false;
            WebRequests.VCCreatePaymentHttp(CurrentOAuthVcKey, transactionAmount, targetChar, sourceChar, sendingMoney, customMessage);// 1000, "Aelith Blanchette", "Dice Bot", false, "Ordering 1000 Dicebot Chips for Casino");// .LoginToVCPublic(AccountSettings);

            int timeout = TimeoutVcRequests;
            while (WebRequests.VcCreateTransactionResponse == null)
            {
                timeout -= BotMain.TickTimeMiliseconds;
                Thread.Sleep(BotMain.TickTimeMiliseconds);
                if (timeout <= 0)
                {
                    Console.WriteLine("Timeout on VCCreatePaymentHttp");
                    WebRequests.AbandonVCCreatePayment = true;
                    return false;
                }
            }

            if (BotMain._debug)
                Console.WriteLine("VCCreatePaymentHttp created: " + WebRequests.VcCreateTransactionResponse.id);
            CurrentPaymentId = WebRequests.VcCreateTransactionResponse.id;
            return true;
        }

        public bool CheckVcTransaction(string paymentId, out int currentPaymentStatus)
        {
            currentPaymentStatus = -1;
            if (NeedNewVelvetucuffOauthToken())
            {
                bool gotToken = GetNewVelvetcuffOauthToken();
                if(!gotToken)
                {
                    return false;
                }
            }

            WebRequests.AbandonVCCheckPayment = false;
            VCTransactionResponse response = WebRequests.VCCheckPaymentHttp(CurrentOAuthVcKey, paymentId);

            if (BotMain._debug)
                Console.WriteLine("VCCheckPaymentHttp checked: " + (response == null ? "null" : response.id + " " + response.status) );
            currentPaymentStatus = response == null? -1 : response.status;
            return true;
        }

        public void CheckVcTransactionAndAct(VcChipOrder order, BotMain bot, bool delete)
        {
            int thisStatus = -1;
            CheckVcTransaction(order.TransactionId, out thisStatus);

            order.OrderStatus = thisStatus;

            switch (thisStatus)
            {
                case -1: //no response
                case 0: //sent
                    break;
                case 1: //accepted
                    string addedChips = bot.DiceBot.AddChips(order.Character, order.ChannelId, order.Chips, false);
                    bot.BotCommandController.SaveChipsToDisk("VelvetcuffConnection CheckVcTransaction");
                    bot.SendMessageInChannel("Chips invoice for " + Utils.GetCharacterUserTags(order.Character) + " has been paid.\n" + addedChips, order.ChannelId);
                    break;
                case 2: //declied
                    bot.SendMessageInChannel("Chips invoice for " + Utils.GetCharacterUserTags(order.Character) + " has been declined.", order.ChannelId);
                    delete = true;
                    break;
                case 3: //cancelled
                    bot.SendMessageInChannel("Chips invoice for " + Utils.GetCharacterUserTags(order.Character) + " has been cancelled.", order.ChannelId);
                    delete = true;
                    break;
            }
            if(delete)
            {
                DeleteVcTransaction(order.TransactionId);
            }

            var tss = CheckThreads.FirstOrDefault(b => b.OrderId == order.TransactionId);
            CheckThreads.Remove(tss);
            tss.CheckThread.Abort();
        }

        public bool DeleteVcTransaction(string transactionId)
        {
            CurrentPaymentId = null;
            if (NeedNewVelvetucuffOauthToken())
            {
                bool gotToken = GetNewVelvetcuffOauthToken();
                if (!gotToken)
                {
                    return false;
                }
            }

            WebRequests.CompletedDeletion = false;
            WebRequests.AbandonDeleteVcTransaction = false;
            WebRequests.VCDeletePaymentHttp(CurrentOAuthVcKey, transactionId);

            int timeout = TimeoutVcRequests;
            while (WebRequests.CompletedDeletion == false)
            {
                timeout -= BotMain.TickTimeMiliseconds;
                Thread.Sleep(BotMain.TickTimeMiliseconds);
                if (timeout <= 0)
                {
                    Console.WriteLine("Timeout on DeleteVcTransaction");
                    WebRequests.AbandonDeleteVcTransaction = true;
                    return false;
                }
            }

            if (BotMain._debug)
                Console.WriteLine("DeleteVcTransaction complete: " + transactionId);
            return true;
        }
    }

    public class CheckAttempt
    {
        public string OrderId;
        public double StartTime;
        public Thread CheckThread;
    }
}
