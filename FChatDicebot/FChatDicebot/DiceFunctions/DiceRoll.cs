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
        public List<int> CrossoutIndexes;
        public List<int> BoldedIndexes;

        public bool Error;
        public string ErrorString;

        public bool TextFormat;

        public int KeepHighest;
        public int KeepLowest;
        public int RemoveHighest;
        public int RemoveLowest;
        public int RerollNumber;
        public int CountOver;
        public int CountUnder;

        public bool Explode;
        public int ExplodeThreshold;

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
                CrossoutIndexes = new List<int>();
                HighlightedIndexes = new List<int>();

                int explosionBonusDice = 0;
                int rerollBonusDice = 0;
                bool currentDieIsReroll = false;

                for (int i = 0; (i < DiceRolled + explosionBonusDice + rerollBonusDice && i < DiceBot.MaximumRolls); i++)
                {
                    int rollAmount = r.Next(DiceSides) + 1;
                    Rolls.Add(rollAmount);

                    if(RerollNumber != 0)
                    {
                        if(RerollNumber == rollAmount && !currentDieIsReroll)
                        {
                            CrossoutIndexes.Add(i);
                            rerollBonusDice++;
                            currentDieIsReroll = true;
                        }
                        else
                        {
                            currentDieIsReroll = false;
                        }
                    }

                    if(Explode)
                    {
                        if((ExplodeThreshold == 0 && rollAmount == DiceSides) ||
                            (ExplodeThreshold > 1 && rollAmount >= ExplodeThreshold)
                            && (explosionBonusDice <= (DiceBot.MaximumDice * 2)))
                        {
                            if(TextFormat)
                            {
                                BoldedIndexes.Add(i);
                            }
                            explosionBonusDice++;
                        }
                    }
                }

                bool useKeptRollsTotal = false;
                List<int> keptRolls = new List<int>();

                if(RerollNumber != 0)
                {
                    for (int i = 0; i < Rolls.Count; i++ )
                    {
                        if (CrossoutIndexes.Contains(i))
                        {
                        }
                        else
                        {
                            keptRolls.Add(Rolls[i]);
                        }
                    }

                    useKeptRollsTotal = true;
                }
                else if(CountOver != 0)
                {
                    List<int> countedRollsIndexes = new List<int>();
                    for (int i = 0; i < Rolls.Count; i++)
                    {
                        if (Rolls[i] > CountOver)
                        {
                            countedRollsIndexes.Add(i);
                            keptRolls.Add(1);
                        }
                    }

                    useKeptRollsTotal = true;
                    HighlightedIndexes = countedRollsIndexes;
                }
                else if (CountUnder != 0)
                {
                    List<int> countedRollsIndexes = new List<int>();

                    for (int i = 0; i < Rolls.Count; i++)
                    {
                        if (Rolls[i] < CountUnder)
                        {
                            countedRollsIndexes.Add(i);
                            keptRolls.Add(1);
                        }
                    }

                    useKeptRollsTotal = true;
                    HighlightedIndexes = countedRollsIndexes;
                }
                else if(KeepHighest != 0)
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
                if(CrossoutIndexes != null && CrossoutIndexes.Contains(count))
                {
                    addition = "[s]" + addition + "[/s]";
                }

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

            if(Rolls.Count >= DiceBot.MaximumRolls)
            {
                explodeStr += "(Maximum roll count reached.)";
            }

            if (Explode)
            {
                string explosionNumberPrint = (ExplodeThreshold > 0 ? ExplodeThreshold + "+" : DiceSides.ToString());
                explodeStr += "(exploding on " + explosionNumberPrint + ") ";
            }
            if (RerollNumber > 0)
                return explodeStr + "(re-rolling once on " + RerollNumber + ") ";
            if (CountOver > 0)
                return explodeStr + "(# of rolls over " + CountOver + ") ";
            if (CountUnder > 0)
                return explodeStr + "(# of rolls under " + CountUnder + ") ";
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
