using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatDicebot.DiceFunctions
{
    public class Deck : CardCollection
    {
        protected int DeckPosition = 0;

        public Deck()
        {
            CollectionName = "deck";
        }

        public void FillDeck(bool addJokers)
        {
            Cards = new List<DeckCard>();

            for (int suit = 0; suit < 4; suit++)
            {
                for (int num = 1; num < 14; num++)
                {
                    Cards.Add(new DeckCard() { number = num, suit = suit });
                }
            }

            if (addJokers)
            {
                Cards.Add(new DeckCard() { joker = true, suit = 0 });
                Cards.Add(new DeckCard() { joker = true, suit = 2 });
            }
        }

        public void ShuffleFullDeck(System.Random r)
        {
            if (Cards == null || Cards.Count == 0)
                return;

            List<int> newPositions = new List<int>();
            for (int i = 0; i < Cards.Count; i++)
            {
                int swapPos = r.Next(Cards.Count);
                SwapPositions(i, swapPos);
            }

            DeckPosition = 0;
        }

        public void ShuffleRemainingDeck(System.Random r)
        {
            if (Cards == null || Cards.Count == 0)
                return;

            List<int> newPositions = new List<int>();

            int remainingCards = Cards.Count - DeckPosition;

            for (int i = 0; i < remainingCards; i++)
            {
                int swapPos = r.Next(remainingCards);
                SwapPositions(DeckPosition + i, DeckPosition + swapPos);
            }
        }

        public void SwapPositions(int pos, int pos2)
        {
            DeckCard dc = Cards[pos];
            Cards[pos] = Cards[pos2];
            Cards[pos2] = dc;
        }

        public DeckCard DrawCard()
        {
            if (Cards == null || DeckPosition >= Cards.Count)
                return null;

            DeckCard returnCard = Cards[DeckPosition];
            DeckPosition++;

            return returnCard;
        }
    }
}
