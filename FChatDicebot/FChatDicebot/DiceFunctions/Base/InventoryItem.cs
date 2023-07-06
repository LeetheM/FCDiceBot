using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//NOTE: currently unused
namespace FChatDicebot.DiceFunctions
{
    public class InventoryItem : IInventoryItem
    {
        protected ItemCategory Category;

        protected double Rarity;//ITEM rarity. enchantment rarity is its own thing.

        protected int GoldValue;

        public Enchantment enchantment; //Rarity; //0-1, 0 being most rare

        protected bool Hidden;

        protected string Name;

        public InventoryItem(string name, ItemCategory category, double rarity, int goldValue )
        {
            Name = name;
            Category = category;
            Rarity = rarity;
            GoldValue = goldValue;
            Hidden = false;
        }

        //????
        public double GetRarity()
        {
            return Rarity;
        }

        public virtual string GetName()
        {
            return Name;
        }

        public ItemCategory GetItemCategory()
        {
            return Category;
        }

        public int GetGoldValue()
        {
            return GoldValue;
        }

        public bool IsHidden()
        {
            return Hidden;
        }

        public string GetRarityString()
        {
            if (enchantment == null)
                return "(no enchantment)";

            string rarity = "common";
            if (enchantment.Rarity > 1)
                rarity = "Common";
            else if (enchantment.Rarity == 1)
                rarity = "[color=green]Uncommon[/color]";
            else if (enchantment.Rarity > .2 && enchantment.Rarity < 1)
                rarity = "[color=blue]Rare[/color]";
            else if (enchantment.Rarity > .05 && enchantment.Rarity <= .2)
                rarity = "[color=purple]Ultra-Rare[/color]";
            else if (enchantment.Rarity <= .05)
                rarity = "[color=red]Mythic[/color]";

            return rarity;
        }
            
        //bool IsWorn();

        public string Activate()
        {
            return "activated " + this.ToString();
        }

        public virtual string ToString()
        {
            return "Inventory Item: " + Category + " " + GetRarityString() + " " + GetGoldValue();
        }
    }
}
