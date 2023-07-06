using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatDicebot.DiceFunctions
{
    public class PotionGenerator
    {
        public Random Random;

        //Notes about potion rarity for printout::
        //public const double Common = 2, Uncommon = 1, Rare = .33, UltraRare = .02; //but these are more of a range...
        //if (enchantment.Rarity > 1)
        //    rarity = "Common";
        //else if (enchantment.Rarity == 1)
        //    rarity = "[color=green]Uncommon[/color]";
        //else if (enchantment.Rarity > .2 && enchantment.Rarity < 1)
        //    rarity = "[color=blue]Rare[/color]";
        //else if (enchantment.Rarity > .05 && enchantment.Rarity <= .2)
        //    rarity = "[color=purple]Ultra-Rare[/color]";
        //else if (enchantment.Rarity <= .05)

        public PotionGenerator(Random r)
        {
            Random = r;
        }

        private void LoadPotionGenerationInfo()
        {
            //TODO: come up with way for channels to add their own potions for just that channel... save in channel info?
            //not used RN (allows load of info from disk instead of from file)
            LoadPotionGenerationFromDisk(BotMain.FileFolder, BotMain.PotionGenerationFileName);
        }

        private void LoadPotionGenerationFromDisk(string fileFolder, string fileName)
        {
            string path = Utils.GetTotalFileName(fileFolder, fileName);
            try
            {
                BotMain.VerifyDirectoryExists();

                if (File.Exists(path))
                {
                    string fileText = File.ReadAllText(path, Encoding.ASCII);

                    Console.WriteLine("Chip piles file text " + fileText == null ? "null" : fileText.Take(100) + "...");
                    PotionGenerationInfo generationInfo = JsonConvert.DeserializeObject<PotionGenerationInfo>(fileText);
                    
                    AllColors = generationInfo.AllColors;
                    AllAdjectives = generationInfo.AllAdjectives;
                    AllPotionNouns = generationInfo.AllPotionNouns;
                    AllEicons = generationInfo.AllEicons;
                    AllFlavors = generationInfo.AllFlavors;
                    AllOrigins = generationInfo.AllOrigins;
                    AllWords = generationInfo.AllWords;
                    AllEnchantments = generationInfo.AllEnchantments;

                    if (BotMain._debug)
                        Console.WriteLine("loaded PotionGenerationInfo successfully.");
                }
                else
                {
                    Console.WriteLine("PotionGenerationInfo file does not exist.");
                }
            }
            catch (System.Exception exc)
            {
                Console.WriteLine("Exception: Failed to load PotionGenerationInfo for " + path + "\n" + exc.ToString());
            }
        }

        private List<string> AllColors = new List<string>() { 
            "white", "pink", "blue", "gold", "silver", 
            "green", "purple", "orange", "red", "brown", 
            "tan", "teal", "peach", "yellow", "seafoam", 
            "rainbow", "bronze", "purple", "hazel", "lilac", 
            "black", "grail mud", "pearl", "lavender", "fuschia", 
            "olive" };

        private List<string> AllAdjectives = new List<string>() {
            "swirling","glowing","bland-looking","spotted","bright",
            "suspicious","clear","colorful","rainbow-streaked","clumpy",
            "muddy","stale","fizzing","fresh","lovely",
            "gleaming","fragile","unstable","old","new",
            "gorgeous","fancy","elegant","inexpensive","rich",
            "light","odd","odorous","long","short",
            "small","open","smoky","striped","tasty",
            "huge","greasy"
        };

        private List<string> AllPotionNouns = new List<string>() {
            "potion","potion","potion","potion","potion",
            "vial","vial","elixir","elixir","flask","flask",
            //"mug","shot glass","glass","pudding",
            "oil","tonic","serum"
        };

        private List<string> AllEicons = new List<string>() {
            "potion1","potion2","potion3","potion4","potion5",
            "potion6","potion7","potion8","potion9","potion10",
            "potion11","potion12","potion13","potion14",
            "potionred1","potionred2","potionred3","potionred4","potionpink1",
            "potiongreen1","potiongreen2","potiongreen3","potiongreen4","potionpink2",
            "potionpurple1","potionpurple2","potionorange1","potionorange2","potionorange3",
            "potionblue1","potionblue2","potionvase1","potionvase2"
        };

        private List<string> AllFlavors = new List<string>() {
            "chocolate","strawberry","steak","banana","apple",
            "cotton candy","cola","milk","sprite","orange",
            "coffee","pumpkin","maple","vanilla","tofu",
            "pumpkin spice","candy cane","caramel apple","caramel corn","nutmeg",
            "pecan pie","eggnog","turkey","deviled eggs","sweet potato",
            "chicken","bubblegum","mead","honey","cheesecake",
            "pineapple","grapefruit","grape","raisin","mud",
            "boogers","saliva"
        };

        private List<string> AllOrigins = new List<string>() {
            "Potion generated!","Please take this potion.","Potion created.","I brewed this for you.","You'll want this one.",
            "Use with caution.","I think the formula was right.","This one is a favorite.","I'm not sure where this one came from.","Not another one of these...",
            "Potions are easy.","Maybe one day I'll run out of these.","Hopefully this helps.","You can thank me later.","A potion for all seasons.",
            "Don't tell anyone you got it here.","Another freebie, eh?","Potion ready~", "Potion here!","Tell me how this one feels."
        };

        private List<string> AllWords = new List<string>() {
            "idol","that","I","and","servant","king","bottle"
        };

        private List<Enchantment> AllEnchantments = new List<Enchantment>() {
            new Enchantment(true, "Bloat", "Bloating", "You balloon in size, increasing dramatically in weight. It's all fat, too.", 0.5),
            new Enchantment(true, "Deep Sleep", "Deep Slumber", "You instantly fall asleep. The slumber is deep enough that it takes quite a lot to wake you up too, but if you're physically damaged you'll awaken.", 0.5),
            new Enchantment(true, "Clumsy", "Clumsiness", "You lose almost all of your physical coordination. Just walking around is about as good as you can do without messing up somehow.", 0.5),
            
            //common here:
            new Enchantment(false, "Cleansing", "Cleansing", "Removes all potion effects", 2),
            new Enchantment(false, "Taste Flavoring", "Taste Flavor", "Everything you consume tastes like [Flavor].", 2),
            new Enchantment(false, "Skin Coloring", "Skin Color", "Changes the color of your skin to [Color].", 2),
            
            //additional here:
            new Enchantment(false, "Strength", "Strength", "Your physical and magical strength is magnified to a huge level!"),
            new Enchantment(false, "Forgetful", "Forgetfulness", "You find the simplest things hard to remember, and you might as well not even try for about anything longer than a few words."),
            

        };

        private List<Enchantment> CommonEnchantments = new List<Enchantment>() {
            new Enchantment(false, "Cleansing", "Cleansing", "Removes all potion effects", 2),
            new Enchantment(false, "Taste Flavoring", "Taste Flavor", "Everything you consume tastes like [Flavor].", 2),
            new Enchantment(false, "Skin Coloring", "Skin Color", "Changes the color of your skin to [Color].", 2),
        };

        public List<Enchantment> GetAllEnchantments()
        {
            return AllEnchantments;
        }

        private Potion GeneratePotionBase()
        {
            Potion potion = new Potion("potion", ItemCategory.Potion, 1, 1);

            potion.strength = GetRandomStrength(Random);
            potion.printFormat = Random.Next(8);
            potion.color = GetRandomColor(Random);
            potion.adjective = GetRandomAdjective(Random);
            potion.potionNoun = GetRandomPotionNoun(Random);

            potion.flavor = GetRandomFlavor(Random);
            potion.eicon = GetRandomEicon(Random);

            return potion;
        }

        public Potion GeneratePotionWithSpecificEffect(bool allowFlavor, bool allowLewd, bool requireLewd, string potionEffectSearch)
        {
            Potion potion = GeneratePotionBase();

            Enchantment specific = GetSpecificEnchantment(potionEffectSearch);

            potion.enchantment = specific;

            return potion;
        }

        public Enchantment GetSpecificEnchantment(string potionEffectSearch)
        {
            return AllEnchantments.FirstOrDefault(a => a.prefix.ToLower() == potionEffectSearch || a.suffix.ToLower() == potionEffectSearch);
        }

        public Potion GeneratePotion(bool allowFlavor, bool allowLewd, bool requireLewd)
        {
            Potion potion = GeneratePotionBase();

            potion.enchantment = GetRandomEnchantment(Random, allowFlavor, allowLewd, requireLewd);

            return potion;
        }

        public string GetPotionGenerationOutputString(Potion p)
        {
            if (p == null)
                return "Error: Potion was null.";
            String potionString = p.ToString();
            String returnString = SubstituteWords(potionString);
            if (p.enchantment == null)
                return returnString;
            returnString = GetRandomOrigin(Random) + " " + returnString;
            return returnString;
        }


        private int GetRandomStrength(System.Random Random)
        {
            double seed = Random.NextDouble();
            int strength = 1;
            if (seed <= .3)
                strength = 1; //30%
            else if (seed <= .6)
                strength = 2; //30%
            else if (seed <= .8)
                strength = 3; //20%
            else if (seed <= .93)
                strength = 4; //13%
            else
                strength = 5; //7%
            return strength;
        }

        public string SubstituteWords(string input)
        {
            string randomWord = GetRandomWord(Random);
            string randomColor = GetRandomColor(Random);
            string randomColor2 = GetRandomColor(Random);
            string randomFlavor = GetRandomFlavor(Random);

            string returnString = input.Replace("[Color]", randomColor)
                    .Replace("[Color2]", randomColor2)
                    .Replace("[Flavor]", randomFlavor)
                    .Replace("[Word]", randomWord);
            if(input.Contains("[Word2]"))
            {
                string randomWord2 = GetRandomWord(Random);
                string randomWord3 = GetRandomWord(Random);
                string randomWord4 = GetRandomWord(Random);
                string randomWord5 = GetRandomWord(Random);
                string randomWord6 = GetRandomWord(Random);
                string randomWord7 = GetRandomWord(Random);
                string randomWord8 = GetRandomWord(Random);
                string randomWord9 = GetRandomWord(Random);
                string randomWord10 = GetRandomWord(Random);
                returnString = returnString.Replace("[Word2]", randomWord2).Replace("[Word3]", randomWord3).Replace("[Word4]", randomWord4)
                    .Replace("[Word5]", randomWord5).Replace("[Word6]", randomWord6).Replace("[Word7]", randomWord7).Replace("[Word8]", randomWord8)
                    .Replace("[Word9]", randomWord9).Replace("[Word10]", randomWord10);
            }

            return returnString;
        }

        public string GetRandomColor(Random r)
        {
            return AllColors[r.Next(AllColors.Count)];
        }

        public string GetRandomAdjective(Random r)
        {
            return AllAdjectives[r.Next(AllAdjectives.Count)];
        }

        public string GetRandomEicon(Random r)
        {
            return AllEicons[r.Next(AllEicons.Count)];
        }

        public string GetRandomPotionNoun(Random r)
        {
            return AllPotionNouns[r.Next(AllPotionNouns.Count)];
        }

        public string GetRandomFlavor(Random r)
        {
            return AllFlavors[r.Next(AllFlavors.Count)];
        }

        public string GetRandomWord(Random r)
        {
            return AllWords[r.Next(AllWords.Count)];
        }

        public string GetRandomOrigin(Random r)
        {
            return AllOrigins[r.Next(AllOrigins.Count)];
        }

        public Enchantment GetRandomEnchantment(Random r, bool allowFlavor, bool allowLewd, bool requireLewd)
        {
            Enchantment first = GetRandomEnchantment(r, allowLewd, requireLewd);
            int safety = 0;

            while (!allowFlavor && first.suffix.ToLower().Contains("flavor"))
            {
                safety++;
                first = GetRandomEnchantment(r, allowLewd, requireLewd);
            }

            return first;
        }

        public Enchantment GetRandomEnchantment(Random r, bool allowKinky, bool requireKinky)
        {
            double seed = r.NextDouble();

            List<Enchantment> relevantCommonEnchantments = CommonEnchantments;
            List<Enchantment> relevantAllEnchantments = AllEnchantments;
            if (requireKinky)
            {
                relevantCommonEnchantments = relevantCommonEnchantments.Where(ba => ba.kinky).ToList();
                relevantAllEnchantments = relevantAllEnchantments.Where(ba => ba.kinky).ToList();
            }
            if (!allowKinky)
            {
                relevantCommonEnchantments = relevantCommonEnchantments.Where(ba => !ba.kinky).ToList();
                relevantAllEnchantments = relevantAllEnchantments.Where(ba => !ba.kinky).ToList();
            }

            if (seed <= .33) //was .40
            {
                return relevantCommonEnchantments[r.Next(relevantCommonEnchantments.Count)];
            }
            else
            {
                Enchantment xxx = relevantAllEnchantments[r.Next(relevantAllEnchantments.Count)];

                while (xxx.Rarity < 1)
                {
                    double roll = r.NextDouble();
                    if (roll <= xxx.Rarity)
                        break;

                    xxx = relevantAllEnchantments[r.Next(relevantAllEnchantments.Count)];
                }

                return xxx;
            }
        }
    }


    public class PotionGenerationInfo
    {

        public List<string> AllColors;
        public List<string> AllAdjectives;
        public List<string> AllPotionNouns;
        public List<string> AllEicons;
        public List<string> AllFlavors;
        public List<string> AllOrigins;
        public List<string> AllWords;
        public List<Enchantment> AllEnchantments;

    }
}
