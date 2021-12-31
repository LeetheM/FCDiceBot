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
            }
            switch (number)
            {
                case 1:
                    numString = "A";
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
                    numString = "J";
                    break;
                case 12:
                    numString = "Q";
                    break;
                case 13:
                    numString = "K";
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
    }
}
