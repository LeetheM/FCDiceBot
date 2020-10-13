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
    }
}
