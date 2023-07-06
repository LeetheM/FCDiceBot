using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FChatDicebot.Model;
using System.Net;
using Newtonsoft.Json;
using System.Threading;
using FChatDicebot.SavedData;
using System.IO;

namespace FChatDicebot
{
    public class BotWebRequests
    {
        private string GetApiTicketUrl = "https://f-list.net/json/getApiTicket.php";

        private bool awaitingReturn = false;

        public GetApiTicketResponse ApiTicketResult = null;
        public PostOAuthResponse VcOauthResult = null;
        public VCTransactionResponse VcCreateTransactionResponse = null;

        public bool CompletedDeletion = false;

        public bool AbandonWebRequestVc = false;
        public bool AbandonWebRequestFList = false;
        public bool AbandonVCCheckPayment = false;
        public bool AbandonVCCreatePayment = false;
        public bool AbandonDeleteVcTransaction = false;
        
        public void LoginToServerPublic(AccountSettings accountSettings)
        {
            Thread t = new Thread(() => LoginToServer(accountSettings, false, false));
            t.IsBackground = true;
            t.Start();
        }

        private void LoginToServer(AccountSettings accountSettings, bool friendsList, bool bookmarksList)
        {

            GetApiTicketRequest requestObject = new GetApiTicketRequest()
            {
                password = accountSettings.AccountPassword,
                account = accountSettings.AccountName,
                no_bookmarks = bookmarksList? "false" : "true",
                no_characters = "true",
                no_friends = friendsList ? "false" : "true"
            };

            string webserver = GetApiTicketUrl;
            //parameters are what is actually used to get the api ticket rather than the post body content
            string parameters = "?account=" + requestObject.account + "&password=" + requestObject.password + "&no_characters=" + 
                requestObject.no_characters + "&no_friends=" + requestObject.no_friends + "&no_bookmarks=" + 
                requestObject.no_bookmarks;

            WebRequest request = Utils.CreateWebRequest(webserver + parameters, requestObject);
            WebResponse webResponse = null;

            try
            {
                webResponse = (HttpWebResponse)request.GetResponse();

                if (webResponse != null)
                {
                    System.IO.StreamReader sr = new System.IO.StreamReader(webResponse.GetResponseStream());
                    string rtnValue = sr.ReadToEnd();

                    if (BotMain._debug)
                    {
                        Console.WriteLine("LoginToServer return message: " + rtnValue);
                        Utils.AddToLog("LoginToServer return message: " + rtnValue, null);
                    }

                    GetApiTicketResponse rpt = JsonConvert.DeserializeObject<GetApiTicketResponse>(rtnValue);
                    awaitingReturn = false;

                    if (rpt != null)
                    {
                        ApiTicketResult = rpt;
                    }
                    else
                    {
                        Console.WriteLine("Error: failed to parse result into GetApiTicketResponse");
                        Utils.AddToLog("Error: failed to parse result into GetApiTicketResponse", null);
                    }

                    string saaaaaa = rtnValue + " ";
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine("WebException on LoginToServer Call: " + ex.Message);
                if (!AbandonWebRequestFList)
                {
                    Thread.Sleep(2000);
                    Console.WriteLine("Retrying...");

                    Utils.AddToLog("WebException on LoginToServer Call: " + ex.Message, accountSettings);
                    LoginToServer(accountSettings, friendsList, bookmarksList);
                }
            }
        }


        public void LoginToVCPublic(AccountSettings accountSettings)
        {
            Thread t = new Thread(() => GetVCAuthTicket(accountSettings));
            t.IsBackground = true;
            t.Start();
        }

        private void GetVCAuthTicket(AccountSettings accountSettings)
        {
            string webserver = "https://api.velvetcuff.me/connect/token";

            string requestString = string.Format("&grant_type=password&client_id=MyDommeWallet_App&username={0}&password={1}", accountSettings.VcAccountName, accountSettings.VcAccountPassword);
            
            WebRequest request = Utils.CreateWebRequest(webserver, requestString, "POST", true);
            WebResponse webResponse = null;

            try
            {
                webResponse = (HttpWebResponse)request.GetResponse();

                if (webResponse != null)
                {
                    System.IO.StreamReader sr = new System.IO.StreamReader(webResponse.GetResponseStream());
                    string rtnValue = sr.ReadToEnd();

                    PostOAuthResponse rpt = JsonConvert.DeserializeObject<PostOAuthResponse>(rtnValue);
                    awaitingReturn = false;

                    if (rpt != null)
                    {
                        VcOauthResult = rpt;
                    }
                    else
                    {
                        Console.WriteLine("Error: failed to parse result into PostOAuthResponse");
                    }

                    string saaaaaa = rtnValue + " ";
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine("WebException on GetVCAuthTicket Call: " + ex.Message, accountSettings);
                if (!AbandonWebRequestVc)
                {
                    Thread.Sleep(2000);
                    Console.WriteLine("Retrying...");
                    GetVCAuthTicket(accountSettings);
                }
            }
        }

        public void VCCreatePaymentHttp(string oAuthToken, int transactionAmount, string targetChar, string sourceChar, bool sendingMoney, string customMessage)
        {
            if(BotMain._debug)
                Console.WriteLine("VCCreatePaymentHttp start");

            if (string.IsNullOrEmpty(oAuthToken))
            {
                Console.WriteLine(oAuthToken + " oAuthToken was empty");
                return;
            }

            VCTransactionRequest requestObject = new VCTransactionRequest()
            {
                initiatingCharacterId = sourceChar,
                targetedCharacterId = targetChar,
                isSending = sendingMoney,
                transactionType = 0,
                customMessage = customMessage,
                amount = transactionAmount,
                chatLogs = null,
                containsExtremeContent = false,
                isSimulation = false,
                areLogsAnonymous = true,
                lineItems = new List<LineItem>() { new LineItem() {
                 action = 409,
                 amount = transactionAmount, 
                 creationTime = DateTime.Now,
                 id = 0,
                 isMine = true}
                },
                isLoserAnonymous = false,
                isWinnerAnonymous = false,
                canUseSavings = false,
                //isTaxFree = false,
                isTaxFree = true,
                sendFromSavings = false,
                sendToSavings = false,
                sendSavingsToWallet = false,
                sendNotification = true
                //do not include:: externalAppActionId, externalAppId, linkedEventId
            };

            // Create a new 'HttpWebRequest' Object to the mentioned URL.
            HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.velvetcuff.me/api/vc/transactions");

            myHttpWebRequest.Headers.Add("Authorization", "Bearer " + oAuthToken);
            myHttpWebRequest.Method = "POST";

            myHttpWebRequest.ContentType = "application/json";


            string jsonString = "";
            try
            {
                jsonString = JsonConvert.SerializeObject(requestObject);
            }
            catch (Exception exc)
            {
                Console.WriteLine("error occured on jsonconvert: " + exc.ToString());
            }

            byte[] bytes = Encoding.ASCII.GetBytes(jsonString);

            Stream os = null;
            try
            {
                myHttpWebRequest.ContentLength = bytes.Length;
                os = myHttpWebRequest.GetRequestStream();
                os.Write(bytes, 0, bytes.Length);
                os.Close();
                Console.WriteLine("wrote stream " + jsonString);
            }
            catch (WebException ex)
            {
                Console.WriteLine("error occred on content write " + ex.ToString());
            }

            try
            {
                // Assign the response object of 'HttpWebRequest' to a 'HttpWebResponse' variable.
                HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();

                if (myHttpWebResponse != null)
                {
                    System.IO.StreamReader sr = new System.IO.StreamReader(myHttpWebResponse.GetResponseStream());
                    string rtnValue = sr.ReadToEnd();

                    VCTransactionResponse rpt = JsonConvert.DeserializeObject<VCTransactionResponse>(rtnValue);
                    awaitingReturn = false;

                    if (rpt != null)
                    {
                        VcCreateTransactionResponse = rpt;
                    }
                    else
                    {
                        Console.WriteLine("Error: failed to parse result into VCTransactionResponse");
                    }

                    string saaaaaa = rtnValue + " ";
                    sr.Close();
                }
                myHttpWebResponse.Close();
            }
            catch (WebException ex)
            {
                Console.WriteLine("WebException on VCCreatePaymentHttp Call: " + ex.Message);
                if (!AbandonVCCreatePayment)
                {
                    Thread.Sleep(2000);
                    Console.WriteLine("Retrying...");
                    VCCreatePaymentHttp(oAuthToken, transactionAmount, targetChar, sourceChar, sendingMoney, customMessage);
                }
            }

        }

        public VCTransactionResponse VCCheckPaymentHttp(string oAuthToken, string transactionId)
        {
            VCTransactionResponse response = null;
            if(BotMain._debug)
                Console.WriteLine("VCCheckPaymentHttp start");

            if (string.IsNullOrEmpty(oAuthToken))
            {
                Console.WriteLine(oAuthToken + " oAuthToken was empty");
                return response;
            }
            if (string.IsNullOrEmpty(transactionId))
            {
                Console.WriteLine(transactionId + " transactionId was empty");
                return response;
            }


            // Create a new 'HttpWebRequest' Object to the mentioned URL.
            HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.velvetcuff.me/api/vc/transactions/" + transactionId);

            myHttpWebRequest.Headers.Add("Authorization", "Bearer " + oAuthToken);
            myHttpWebRequest.Method = "GET";

            try
            {
                // Assign the response object of 'HttpWebRequest' to a 'HttpWebResponse' variable.
                HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();

                if (myHttpWebResponse != null)
                {
                    System.IO.StreamReader sr = new System.IO.StreamReader(myHttpWebResponse.GetResponseStream());
                    string rtnValue = sr.ReadToEnd();

                    VCTransactionResponse rpt = JsonConvert.DeserializeObject<VCTransactionResponse>(rtnValue);
                    awaitingReturn = false;

                    if (rpt != null)
                    {
                        response = rpt;
                    }
                    else
                    {
                        Console.WriteLine("Error: failed to parse result into PostOAuthResponse");
                    }

                    string saaaaaa = rtnValue + " ";
                    sr.Close();
                }
                myHttpWebResponse.Close();
            }
            catch (WebException ex)
            {
                Console.WriteLine("WebException on VCCheckPaymentHttp Call: " + ex.Message);
                if (!AbandonVCCheckPayment)
                {
                    Thread.Sleep(2000);
                    Console.WriteLine("Retrying...");
                    VCCheckPaymentHttp(oAuthToken, transactionId);
                }
            }
            return response;
        }

        public void VCDeletePaymentHttp(string oAuthToken, string transactionId)
        {
            if(BotMain._debug)
                Console.WriteLine("VCCheckPaymentHttp start");
            if (string.IsNullOrEmpty(oAuthToken))
            {
                Console.WriteLine(oAuthToken + " oAuthToken was empty");
                return;
            }
            if (string.IsNullOrEmpty(transactionId))
            {
                Console.WriteLine(transactionId + " transactionId was empty");
                return;
            }


            // Create a new 'HttpWebRequest' Object to the mentioned URL.
            HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.velvetcuff.me/api/vc/transactions/" + transactionId);

            myHttpWebRequest.Headers.Add("Authorization", "Bearer " + oAuthToken);
            myHttpWebRequest.Method = "DELETE";

            try
            {
                // Assign the response object of 'HttpWebRequest' to a 'HttpWebResponse' variable.
                HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();

                if (myHttpWebResponse != null)
                {
                    System.IO.StreamReader sr = new System.IO.StreamReader(myHttpWebResponse.GetResponseStream());
                    string rtnValue = sr.ReadToEnd();

                    awaitingReturn = false;

                    if (rtnValue != null)
                    {
                        CompletedDeletion = true;
                    }
                    else
                    {
                        Console.WriteLine("Error: failed to parse result into VCDeletePaymentHttp");
                    }

                    string saaaaaa = rtnValue + " ";
                    sr.Close();
                }
                myHttpWebResponse.Close();
            }
            catch (WebException ex)
            {
                Console.WriteLine("WebException on VCCheckPaymentHttp Call: " + ex.Message);
                if (!AbandonDeleteVcTransaction)
                {
                    Thread.Sleep(2000);
                    Console.WriteLine("Retrying...");
                    VCDeletePaymentHttp(oAuthToken, transactionId);
                }
            }
        }
    }
}
