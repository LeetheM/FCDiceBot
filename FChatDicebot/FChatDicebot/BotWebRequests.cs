using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FChatDicebot.Model;
using System.Net;
using Newtonsoft.Json;
using System.Threading;

namespace FChatDicebot
{
    public class BotWebRequests
    {
        public const string AccountName = "AccountName";
        public const string AccountPassword = "AccountPassword";
        public const string CharacterName = "Dice Bot";
        public const string CName = "Dice Bot";
        public const string CVersion = "1.01";

        private string GetApiTicketUrl = "https://f-list.net/json/getApiTicket.php";

        private bool awaitingReturn = false;

        public GetApiTicketResponse ApiTicketResult = null;

        public void LoginToServerPublic()
        {
            Thread t = new Thread(() => LoginToServer(false, false));
            t.IsBackground = true;
            t.Start();
        }

        private void LoginToServer(bool friendsList, bool bookmarksList)
        {

            GetApiTicketRequest requestObject = new GetApiTicketRequest()
            {
                password = AccountPassword,
                account = AccountName,
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
                    }

                    string saaaaaa = rtnValue + " ";
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine("WebException on LoginToServer Call: " + ex.Message);
                Console.WriteLine("Retrying...");
                LoginToServer(friendsList, bookmarksList);
            }
        }
    }
}
