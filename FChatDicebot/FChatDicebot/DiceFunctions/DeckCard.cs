using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatDicebot.DiceFunctions
{
    public class DeckCard
    {
        public int suit;
        public int number;
        public bool joker;
        public string specialName;
        public string cardState;
        public string description;

        public override string ToString()
        {
            string cardStateString = string.IsNullOrEmpty(cardState)? "" : (" (" + cardState + ")" );
            if(!string.IsNullOrEmpty(specialName))
            {
                return specialName + cardStateString;
            }

            string suitString = "";
            string numString = "";
            string colorString = "";
            string closeColorString = "[/color]";

            switch (suit)
            {
                case 0:
                    colorString = "[color=red]";
                    suitString = "♥";
                    break;
                case 1:
                    colorString = "[color=red]";
                    suitString = "♦";
                    break;
                case 2:
                    colorString = "[color=gray]";
                    suitString = "♣";
                    break;
                case 3:
                    colorString = "[color=gray]";
                    suitString = "♠";
                    break;
                case 4://red for uno
                    colorString = "[color=red]";
                    suitString = "R";
                    break;
                case 5:
                    colorString = "[color=blue]";
                    suitString = "B";
                    break;
                case 6:
                    colorString = "[color=yellow]";
                    suitString = "Y";
                    break;
                case 7:
                    colorString = "[color=green]";
                    suitString = "G";
                    break;
                case 8://y for uno
                    colorString = "[color=red]"; // ✝ sword: latin cross
                    suitString = "✝";
                    break;
                case 9:
                    colorString = "[color=blue]"; //Ų cup: latin capital U with Ogonek “🍵”
                    suitString = "🍵";
                    break;
                case 10:
                    colorString = "[color=yellow]"; //✪ pentacle  ⛤⍟  “✪”
                    suitString = "✪";
                    break;
                case 11:
                    colorString = "[color=green]"; // ƪ wand: latin letter reversed Esh loop
                    suitString = "ƪ";
                    break;
            }
            switch (number)
            {
                case 0:
                    if (!string.IsNullOrEmpty(suitString))
                        numString = "0";
                    break;
                case 1:
                    if (suit < 4 || suit >= 8)
                        numString = "A";
                    else if (suit >= 4 && suit < 8)
                        numString = "1";
                    break;
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                    numString = number.ToString();
                    break;
                case 11:
                    if (suit < 4 || suit >= 8)
                        numString = "J";
                    else if (suit >= 4 && suit < 8)
                        numString = "[Draw 2]";
                    break;
                case 12:
                    if (suit < 4)
                        numString = "Q";
                    else if (suit >= 4 && suit < 8)
                        numString = "[Skip]";
                    break;
                case 13:
                    if (suit < 4)
                        numString = "K";
                    else if (suit >= 4 && suit < 8)
                        numString = "[Reverse]";
                    break;
                case 14:
                    if (suit >=8)
                        numString = "P";
                    break;
                default:
                    numString = number.ToString();
                    break;
            }
            if (joker)
            {
                numString = "Joker";
                suitString = "";
            }

            return colorString + numString + suitString + closeColorString + cardStateString;
        }

        public string FullDescription()
        {
            string nameString = ToString();
            string descriptionString = ": " + description;
            if (string.IsNullOrEmpty(description))
                descriptionString = "";
            return "[b]" + nameString + "[/b]" + descriptionString;
        }

        public bool Equals(DeckCard d)
        {
            return suit == d.suit && number == d.number && specialName == d.specialName && joker == d.joker;
        }
    }
}
