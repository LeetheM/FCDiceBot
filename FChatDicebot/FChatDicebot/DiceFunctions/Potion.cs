using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatDicebot.DiceFunctions
{
    public class Potion : InventoryItem
    {
        public string potionNoun;
        public string adjective;
        //public Enchantment enchantment;
        public int strength;

        public int printFormat;

        public string flavor;

        public string color;
        public string eicon;

        public Potion(string name, ItemCategory category, double rarity, int goldValue) : base(name, category, rarity, goldValue)
        {
        }

        public override string GetName()
        {
            string returnString = "";
            switch (printFormat)
            {
                case 0:
                case 1:
                    returnString = adjective + " " + color + " " + potionNoun + " of [b]" + enchantment.suffix + "[/b]";
                    break;
                case 2:
                case 3:
                    returnString = adjective + " " + color + " [b]" + enchantment.prefix + "[/b] " + potionNoun;
                    break;
                case 4:
                    returnString = adjective + " [b]" + enchantment.prefix + "[/b] " + potionNoun;
                    break;
                case 5:
                    returnString = color + " [b]" + enchantment.prefix + "[/b] " + potionNoun;
                    break;
                case 6:
                    returnString = color + " " + potionNoun + " of [b]" + enchantment.suffix + "[/b]";
                    break;
                case 7:
                    returnString = adjective + " " + potionNoun + " of [b]" + enchantment.suffix + "[/b]";
                    break;
                default:
                    returnString = adjective + " " + color + " " + potionNoun + " of [b]" + enchantment.suffix + "[/b]";
                    break;
            }

            return returnString;
        }

        public override string ToString()
        {
            if (enchantment == null)
                return "(invalid potion)";

            string str = "";
            for (int i = 0; i < strength; i++)
                str += "☆";
            string strengthString = " ( " + str + " strength )";
            switch (strength)
            {
                case 1:
                case 2:
                    break;
                case 3:
                    strengthString = "[color=yellow]" + strengthString + "[/color]";
                    break;
                case 4:
                    strengthString = "[color=orange]" + strengthString + "[/color]";
                    break;
                case 5:
                    strengthString = "[color=red]" + strengthString + "[/color]";
                    break;
            }
            string explanationString = " \n[sub]" + enchantment.explanation + "[/sub]";

            string rarity = GetRarityString();
            
            string flavorString = string.IsNullOrEmpty(flavor) ? "" : ( "\n[sub]" + flavor.Substring(0, 1).ToUpper() + flavor.Substring(1) + " flavored. " + rarity + "[/sub]" );
            string eiconString = "[eicon]" + eicon + "[/eicon]\n";

            string potionName = GetName();
            string returnString = potionName + strengthString + explanationString + flavorString;// "";
            
            returnString = returnString.Substring(0, 1).ToUpper() + returnString.Substring(1);
            returnString = eiconString + returnString;
            return returnString;
        }

    }
}
