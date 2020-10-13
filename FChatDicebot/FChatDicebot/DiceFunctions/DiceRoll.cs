using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatDicebot.DiceFunctions
{
    public class DiceRoll
    {
        public int DiceRolled;
        public int DiceSides;

        public int Total;
        public List<int> Rolls;
        public List<int> HighlightedIndexes;
        public List<int> BoldedIndexes;

        public bool Error;
        public string ErrorString;

        public bool TextFormat;

        public int KeepHighest;
        public int KeepLowest;
        public int RemoveHighest;
        public int RemoveLowest;

        public bool Explode;

        public DiceRoll()
        {
        }

        public string ResultString()
        {
            if(Error)
            {
                return "ERROR: " + ErrorString;
            }
            if (DiceRolled > 0)
                return "Rolled " + DiceRolled + "d" + DiceSides + " {" + PrintRollsList(Rolls) + "} " + GetConditionsString() + "= [b]" + Total + "[/b]";
            else
                return "[b]" + Total + "[/b]";
        }

        public void Roll(System.Random r)
        {
            if (Error || DiceRolled == 0)
                return;

            try
            {
                Rolls = new List<int>();
                BoldedIndexes = new List<int>();
                HighlightedIndexes = new List<int>();

                int explosionBonusDice = 0;
                for (int i = 0; i < DiceRolled + explosionBonusDice; i++)
                {
                    int rollAmount = r.Next(DiceSides) + 1;
                    Rolls.Add(rollAmount);

                    if(Explode && rollAmount == DiceSides && (explosionBonusDice <= (DiceBot.MaximumDice * 2)))
                    {
                        if(TextFormat)
                        {
                            BoldedIndexes.Add(i);
                        }
                        explosionBonusDice++;
                    }
                }

                bool useKeptRollsTotal = false;
                List<int> keptRolls = new List<int>();

                if(KeepHighest != 0)
                {
                    List<int> highestRollsIndexes = new List<int>();
                    List<int> highestRollsValues = new List<int>();

                    int index = 0;
                    foreach(int j in Rolls)
                    {
                        if(highestRollsIndexes.Count < KeepHighest)
                        {
                            highestRollsIndexes.Add(index);
                            highestRollsValues.Add(j);
                        }
                        else
                        {
                            int min = highestRollsValues.Min();
                            if(min < j)
                            {
                                int indexOfLowValue = highestRollsValues.IndexOf(min);
                                highestRollsIndexes.RemoveAt(indexOfLowValue);
                                highestRollsValues.RemoveAt(indexOfLowValue);
                                highestRollsIndexes.Add(index);
                                highestRollsValues.Add(j);
                            }
                        }
                        index++;
                    }

                    useKeptRollsTotal = true;
                    keptRolls = highestRollsValues;
                    HighlightedIndexes = highestRollsIndexes;
                }
                else if(KeepLowest != 0)
                {
                    List<int> highestRollsIndexes = new List<int>();
                    List<int> highestRollsValues = new List<int>();

                    int index = 0;
                    foreach (int j in Rolls)
                    {
                        if (highestRollsIndexes.Count < KeepLowest)
                        {
                            highestRollsIndexes.Add(index);
                            highestRollsValues.Add(j);
                        }
                        else
                        {
                            int max = highestRollsValues.Max();
                            if (max > j)
                            {
                                int indexOfLowValue = highestRollsValues.IndexOf(max);
                                highestRollsIndexes.RemoveAt(indexOfLowValue);
                                highestRollsValues.RemoveAt(indexOfLowValue);
                                highestRollsIndexes.Add(index);
                                highestRollsValues.Add(j);
                            }
                        }
                        index++;
                    }

                    useKeptRollsTotal = true;
                    keptRolls = highestRollsValues;
                    HighlightedIndexes = highestRollsIndexes;
                }
                else if (RemoveHighest != 0)
                {
                    List<int> highestRollsIndexes = new List<int>();
                    List<int> highestRollsValues = new List<int>();

                    int index = 0;
                    foreach (int j in Rolls)
                    {
                        if (highestRollsIndexes.Count < RemoveHighest)
                        {
                            highestRollsIndexes.Add(index);
                            highestRollsValues.Add(j);
                        }
                        else
                        {
                            int min = highestRollsValues.Min();
                            if (min < j)
                            {
                                int indexOfLowValue = highestRollsValues.IndexOf(min);
                                highestRollsIndexes.RemoveAt(indexOfLowValue);
                                highestRollsValues.RemoveAt(indexOfLowValue);
                                highestRollsIndexes.Add(index);
                                highestRollsValues.Add(j);
                            }
                        }
                        index++;
                    }

                    useKeptRollsTotal = true;

                    keptRolls = Utils.CopyList(Rolls);
                    while (highestRollsValues.Count > 0)
                    {
                        int value = highestRollsValues[0];
                        int indexOfValue = keptRolls.IndexOf(value);
                        keptRolls.RemoveAt(indexOfValue);
                        highestRollsValues.RemoveAt(0);
                    }
                    
                    HighlightedIndexes = highestRollsIndexes;
                }
                else if (RemoveLowest != 0)
                {
                    List<int> highestRollsIndexes = new List<int>();
                    List<int> highestRollsValues = new List<int>();

                    int index = 0;
                    foreach (int j in Rolls)
                    {
                        if (highestRollsIndexes.Count < RemoveLowest)
                        {
                            highestRollsIndexes.Add(index);
                            highestRollsValues.Add(j);
                        }
                        else
                        {
                            int max = highestRollsValues.Max();
                            if (max > j)
                            {
                                int indexOfLowValue = highestRollsValues.IndexOf(max);
                                highestRollsIndexes.RemoveAt(indexOfLowValue);
                                highestRollsValues.RemoveAt(indexOfLowValue);
                                highestRollsIndexes.Add(index);
                                highestRollsValues.Add(j);
                            }
                        }
                        index++;
                    }

                    useKeptRollsTotal = true;
                    keptRolls = Utils.CopyList(Rolls);
                    while (highestRollsValues.Count > 0)
                    {
                        int value = highestRollsValues[0];
                        int indexOfValue = keptRolls.IndexOf(value);
                        keptRolls.RemoveAt(indexOfValue);
                        highestRollsValues.RemoveAt(0);
                    }

                    HighlightedIndexes = highestRollsIndexes;
                }
                
                if (useKeptRollsTotal)
                    Total = keptRolls.Sum();
                else
                    Total = Rolls.Sum();
            }
            catch(Exception exc)
            {
                Error = true;
                ErrorString = "exception occured: " + exc.ToString();
            }
        }

        public string PrintRollsList(List<int> rollsList)
        {
            if (rollsList == null || rollsList.Count == 0)
                return "";

            string rtnString = "";

            int count = 0;
            foreach(int i in rollsList)
            {
                if (rtnString.Length > 0)
                    rtnString += ", ";

                string addition = i.ToString();
                if (HighlightedIndexes != null && HighlightedIndexes.Contains(count))
                {
                    addition = "[color=yellow]" + addition + "[/color]";
                }
                else if (BoldedIndexes != null && BoldedIndexes.Contains(count))
                {
                    addition = "[b]" + addition + "[/b]";
                }

                rtnString += addition;
                count++;
            }

            return rtnString;
        }
        
        public string GetConditionsString()
        {
            string explodeStr = "";
            if (Explode)
                explodeStr = "(exploding) ";
            if (KeepHighest > 0)
                return explodeStr + "(keeping highest " + KeepHighest + ") ";
            if (KeepLowest > 0)
                return explodeStr + "(keeping lowest " + KeepLowest + ") ";
            if (RemoveHighest > 0)
                return explodeStr + "(removing highest " + RemoveHighest + ") ";
            if (RemoveLowest > 0)
                return explodeStr + "(removing lowest " + RemoveLowest + ") ";

            return explodeStr;
        }
    }
}
