using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatDicebot.DiceFunctions
{
    public class Hand : CardCollection
    {
        public string Character;

        public Hand()
        {
            ResetHand();
            CollectionName = "hand";
        }

        public void ResetHand()
        {
            Cards = new List<DeckCard>();
        }

        public void AddCard(DeckCard d)
        {
            Cards.Add(d);
        }

        public int HandPokerValue()
        {
            return 0;
        }

        public DeckCard GetCardAtIndex(int i)
        {
            if (i < Cards.Count && i >= 0)
                return Cards[i];
            else
                return null;
        }

        public int CardsCount()
        {
            return Cards.Count();
        }
    }
}
