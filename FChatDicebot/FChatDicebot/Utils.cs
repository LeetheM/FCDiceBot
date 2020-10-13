using FChatDicebot.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FChatDicebot
{
    public class Utils
    {
        public static WebRequest CreateWebRequest(string url, System.Object saveReq, string method = "POST")// System.Object saveReq, string method)
        {
            WebRequest request = WebRequest.Create(url);

            request.Timeout = 20000;
            request.Method = method;

            if (method == "POST")
            {
                Console.WriteLine("method POST");
                request.ContentType = "application/json";

                string jsonString = "";
                try
                {
                    jsonString = JsonConvert.SerializeObject(saveReq);
                }
                catch (Exception exc)
                {
                    Console.WriteLine("error occured on jsonconvert: " + exc.ToString());
                }
                byte[] bytes = Encoding.ASCII.GetBytes(jsonString);

                Stream os = null;
                try
                {
                    request.ContentLength = bytes.Length;
                    os = request.GetRequestStream();
                    os.Write(bytes, 0, bytes.Length);
                    os.Close();
                    Console.WriteLine("wrote stream " + jsonString);
                }
                catch (WebException ex)
                {
                    Console.WriteLine("error occred on content write " + ex.ToString());
                }
            }


            return request;
        }


        public static string PrintList(string[] stringArray)
        {
            if (stringArray == null || stringArray.Length == 0)
                return "";

            string rtnString = "";

            foreach (string i in stringArray)
            {
                if (rtnString.Length > 0)
                    rtnString += ", ";
                rtnString += i.ToString();
            }

            return rtnString;
        }

        public static string PrintList(char[] charArray)
        {
            if (charArray == null || charArray.Length == 0)
                return "";

            string rtnString = "";

            foreach (char i in charArray)
            {
                if (rtnString.Length > 0)
                    rtnString += ", ";
                rtnString += i.ToString();
            }

            return rtnString;
        }

        public static string PrintList(List<int> intList)
        {
            if (intList == null || intList.Count == 0)
                return "";

            string rtnString = "";

            foreach (int i in intList)
            {
                if (rtnString.Length > 0)
                    rtnString += ", ";
                rtnString += i.ToString();
            }

            return rtnString;
        }

        public static List<int> CopyList(List<int> input)
        {
            if (input == null)
                return null;

            List<int> rtnList = new List<int>();
            if (input.Count == 0)
                return rtnList;

            foreach (int i in input)
            {
                rtnList.Add(i);
            }

            return rtnList;
        }

        public static string[] LowercaseStrings(string[] inputs)
        {
            if (inputs == null)
                return null;

            string[] rtnArray = new String[inputs.Length];
            for (int i = 0; i < inputs.Length; i++)
            {
                rtnArray[i] = inputs[i].ToLower();
            }
            return rtnArray;
        }

        public static int GetNumberFromInputs(string[] inputs)
        {
            int returnInt = -1;
            if (inputs == null || inputs.Length == 0)
                return returnInt;

            foreach (string s in inputs)
            {
                int.TryParse(s, out returnInt);
                if (returnInt > 0)
                {
                    break;
                }
            }

            return returnInt;
        }

        public static string CombineStringArray(string[] input)
        {
            if (input == null)
                return null;
            if (input.Length == 0)
                return "";

            string returnString = "";
            foreach (string s in input)
            {
                returnString += s;
            }
            return returnString;
        }

        public static string GetChannelIdFromInputs(string[] inputs)
        {
            string returnString = "";
            if (inputs == null || inputs.Length == 0)
                return returnString;

            string combinedInputs = CombineStringArray(inputs);

            if (combinedInputs.Contains("[/session]"))
            {
                string combined1 = combinedInputs.Replace("[session=", "").Replace("[/session]", "");

                int startIndex = combined1.IndexOf(']') + 1;
                if (startIndex + 24 >= combined1.Length)
                {
                    string s = combined1.Substring(startIndex, 24);

                    returnString = s;
                }
            }

            return returnString;
        }
    }
}
