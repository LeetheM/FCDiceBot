using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatDicebot.SavedData
{
    public class ChannelSettings
    {
        public string Name;

        public bool UseEicons; //todo: create feature and apply 
        public bool GreetNewUsers;
        public bool AllowTableRolls;
        public bool AllowCustomTableRolls;
        public bool AllowTableInfo;
        public bool AllowChips;
        public bool AllowGames;
        public ChipsClearanceLevel ChipsClearance;
        public bool StartWith500Chips;
        public bool UseVcAccountForChips;
        public bool StartupChannel;

        //todo: these settings are currently inactive
        public bool OnlyChannelOpsCanUseAnyBotCommands;
        public bool OnlyChannelOpsCanUseDeckControls;
        public bool OnlyChannelOpsCanUseTableCommands;

        public bool Essential;
    }

    public enum ChipsClearanceLevel
    {
        NONE,
        ChannelOp,
        DicebotAdmin
    }
}
