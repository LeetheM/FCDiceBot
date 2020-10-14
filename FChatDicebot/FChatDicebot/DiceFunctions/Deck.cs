using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatDicebot.DiceFunctions
{
    public class Deck : CardCollection
    {
        public DeckType DeckType;
        protected int DeckPosition = 0;

        public Deck(DeckType type)
        {
            DeckType = type;
            CollectionName = "deck";
        }

        public void FillDeck(bool addJokers)
        {
            Cards = new List<DeckCard>();

            switch(DeckType)
            {
                case DiceFunctions.DeckType.Playing:
                    {
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
                    break;
                case DiceFunctions.DeckType.Tarot:
                    {
                        Cards.Add(new DeckCard() { specialName = "The World" });
                        Cards.Add(new DeckCard() { specialName = "Judgement" });
                        Cards.Add(new DeckCard() { specialName = "The Sun" });
                        Cards.Add(new DeckCard() { specialName = "The Moon" });
                        Cards.Add(new DeckCard() { specialName = "The Star" });
                        Cards.Add(new DeckCard() { specialName = "The Tower" });
                        Cards.Add(new DeckCard() { specialName = "The Devil" });
                        Cards.Add(new DeckCard() { specialName = "Temperance" });
                        Cards.Add(new DeckCard() { specialName = "Death" });
                        Cards.Add(new DeckCard() { specialName = "The Hanged Man" });
                        Cards.Add(new DeckCard() { specialName = "Justice" });
                        Cards.Add(new DeckCard() { specialName = "The Wheel of Fortune" });
                        Cards.Add(new DeckCard() { specialName = "The Hermit" });
                        Cards.Add(new DeckCard() { specialName = "Strength" });
                        Cards.Add(new DeckCard() { specialName = "The Chariot" });
                        Cards.Add(new DeckCard() { specialName = "The Lovers" });
                        Cards.Add(new DeckCard() { specialName = "The Hierophant" });
                        Cards.Add(new DeckCard() { specialName = "The Emperor" });
                        Cards.Add(new DeckCard() { specialName = "The Empress" });
                        Cards.Add(new DeckCard() { specialName = "The High Priestess" });
                        Cards.Add(new DeckCard() { specialName = "The Magician" });
                        Cards.Add(new DeckCard() { specialName = "The Fool" });


                        Cards.Add(new DeckCard() { specialName = "Ace of Wands" });
                        Cards.Add(new DeckCard() { specialName = "Two of Wands" });
                        Cards.Add(new DeckCard() { specialName = "Three of Wands" });
                        Cards.Add(new DeckCard() { specialName = "Four of Wands" });
                        Cards.Add(new DeckCard() { specialName = "Five of Wands" });
                        Cards.Add(new DeckCard() { specialName = "Six of Wands" });
                        Cards.Add(new DeckCard() { specialName = "Seven of Wands" });
                        Cards.Add(new DeckCard() { specialName = "Eight of Wands" });
                        Cards.Add(new DeckCard() { specialName = "Nine of Wands" });
                        Cards.Add(new DeckCard() { specialName = "Ten of Wands" });
                        Cards.Add(new DeckCard() { specialName = "Page of Wands" });
                        Cards.Add(new DeckCard() { specialName = "Queen of Wands" });
                        Cards.Add(new DeckCard() { specialName = "King of Wands" });
                        Cards.Add(new DeckCard() { specialName = "Knight of Wands" });

                        Cards.Add(new DeckCard() { specialName = "Ace of Cups" });
                        Cards.Add(new DeckCard() { specialName = "Two of Cups" });
                        Cards.Add(new DeckCard() { specialName = "Three of Cups" });
                        Cards.Add(new DeckCard() { specialName = "Four of Cups" });
                        Cards.Add(new DeckCard() { specialName = "Five of Cups" });
                        Cards.Add(new DeckCard() { specialName = "Six of Cups" });
                        Cards.Add(new DeckCard() { specialName = "Seven of Cups" });
                        Cards.Add(new DeckCard() { specialName = "Eight of Cups" });
                        Cards.Add(new DeckCard() { specialName = "Nine of Cups" });
                        Cards.Add(new DeckCard() { specialName = "Ten of Cups" });
                        Cards.Add(new DeckCard() { specialName = "Page of Cups" });
                        Cards.Add(new DeckCard() { specialName = "Queen of Cups" });
                        Cards.Add(new DeckCard() { specialName = "King of Cups" });
                        Cards.Add(new DeckCard() { specialName = "Knight of Cups" });

                        Cards.Add(new DeckCard() { specialName = "Ace of Swords" });
                        Cards.Add(new DeckCard() { specialName = "Two of Swords" });
                        Cards.Add(new DeckCard() { specialName = "Three of Swords" });
                        Cards.Add(new DeckCard() { specialName = "Four of Swords" });
                        Cards.Add(new DeckCard() { specialName = "Five of Swords" });
                        Cards.Add(new DeckCard() { specialName = "Six of Swords" });
                        Cards.Add(new DeckCard() { specialName = "Seven of Swords" });
                        Cards.Add(new DeckCard() { specialName = "Eight of Swords" });
                        Cards.Add(new DeckCard() { specialName = "Nine of Swords" });
                        Cards.Add(new DeckCard() { specialName = "Ten of Swords" });
                        Cards.Add(new DeckCard() { specialName = "Page of Swords" });
                        Cards.Add(new DeckCard() { specialName = "Queen of Swords" });
                        Cards.Add(new DeckCard() { specialName = "King of Swords" });
                        Cards.Add(new DeckCard() { specialName = "Knight of Swords" });

                        Cards.Add(new DeckCard() { specialName = "Ace of Pentacles" });
                        Cards.Add(new DeckCard() { specialName = "Two of Pentacles" });
                        Cards.Add(new DeckCard() { specialName = "Three of Pentacles" });
                        Cards.Add(new DeckCard() { specialName = "Four of Pentacles" });
                        Cards.Add(new DeckCard() { specialName = "Five of Pentacles" });
                        Cards.Add(new DeckCard() { specialName = "Six of Pentacles" });
                        Cards.Add(new DeckCard() { specialName = "Seven of Pentacles" });
                        Cards.Add(new DeckCard() { specialName = "Eight of Pentacles" });
                        Cards.Add(new DeckCard() { specialName = "Nine of Pentacles" });
                        Cards.Add(new DeckCard() { specialName = "Ten of Pentacles" });
                        Cards.Add(new DeckCard() { specialName = "Page of Pentacles" });
                        Cards.Add(new DeckCard() { specialName = "Queen of Pentacles" });
                        Cards.Add(new DeckCard() { specialName = "King of Pentacles" });
                        Cards.Add(new DeckCard() { specialName = "Knight of Pentacles" });
                    }
                    break;
                case DeckType.ManyThings:
                    {
                        Cards.Add(new DeckCard() { specialName = "Balance" });
                        Cards.Add(new DeckCard() { specialName = "Comet" });
                        Cards.Add(new DeckCard() { specialName = "Donjon" });
                        Cards.Add(new DeckCard() { specialName = "Euryale" });
                        Cards.Add(new DeckCard() { specialName = "The Fates" });
                        Cards.Add(new DeckCard() { specialName = "Flames" });
                        Cards.Add(new DeckCard() { specialName = "Fool" });
                        Cards.Add(new DeckCard() { specialName = "Gem" });
                        Cards.Add(new DeckCard() { specialName = "Idiot" });
                        Cards.Add(new DeckCard() { specialName = "Jester" });
                        Cards.Add(new DeckCard() { specialName = "Key" });
                        Cards.Add(new DeckCard() { specialName = "Knight" });
                        Cards.Add(new DeckCard() { specialName = "Moon" });
                        Cards.Add(new DeckCard() { specialName = "Rogue" });
                        Cards.Add(new DeckCard() { specialName = "Ruin" });
                        Cards.Add(new DeckCard() { specialName = "Skull" });
                        Cards.Add(new DeckCard() { specialName = "Star" });
                        Cards.Add(new DeckCard() { specialName = "Sun" });
                        Cards.Add(new DeckCard() { specialName = "Talons" });
                        Cards.Add(new DeckCard() { specialName = "Throne" });
                        Cards.Add(new DeckCard() { specialName = "Vizier" });
                        Cards.Add(new DeckCard() { specialName = "The Void" });
                    }
                    break;
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

        public int GetCardsRemaining()
        {
            return Cards.Count() - DeckPosition;
        }

        public int GetTotalCards()
        {
            return Cards.Count();
        }

        public bool ContainsJokers()
        {
            return DeckType == DeckType.Playing && Cards.Count() > 52;
        }
    }

    public enum DeckType
    {
        Playing,
        Tarot,
        ManyThings
    }
}
