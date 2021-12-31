using System;
using System.Collections.Generic;
using System.Text;

namespace FChatDicebot.DiceFunctions
{
    public abstract class CardCollection
    {
        public string Id;
        protected List<DeckCard> Cards;
        protected string CollectionName = "";

        public override string ToString()
        {
            string rtnString = "";
            if (Cards == null || Cards.Count == 0)
                return CollectionName + " empty";

            foreach (DeckCard d in Cards)
            {
                if (!string.IsNullOrEmpty(rtnString))
                {
                    rtnString += ", ";
                }
                rtnString += d.ToString();
            }

            return rtnString;
        }

        public string ToString(bool showHandSize)
        {
            string rtnString = "";
            if (Cards == null || Cards.Count == 0)
                return CollectionName + " empty";

            if (showHandSize && CollectionName == BotMain.HandCollectionName)
            {
                string cardsS = "";
                if (Cards.Count > 1 || Cards.Count == 0)
                    cardsS = "s";

                rtnString += " " + Cards.Count + " card" + cardsS;
            }
            else
            {
                foreach (DeckCard d in Cards)
                {
                    if (!string.IsNullOrEmpty(rtnString))
                    {
                        rtnString += ", ";
                    }
                    rtnString += d.ToString();
                }
            }

            return rtnString;
        }

        public bool Empty()
        {
            return Cards == null || Cards.Count == 0;
        }

    }
}
