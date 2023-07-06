using FChatDicebot.DiceFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatDicebot.SavedData
{
    public class CharacterData
    {
        public string Character;

        public string Channel;
        public bool DiceUnlocked;
        public bool SpecialName;//name from channel

        public List<InventoryItem> Inventory;

        public DateTime LastSlotsSpin;
        public double LastLuckForecastTime;
    }
}
