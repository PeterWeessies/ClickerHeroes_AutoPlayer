using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace clickerheroes.autoplayer
{
    /// <summary>
    /// Represents an upgrade available to a hero
    /// </summary>
    public class Upgrade
    {
        public string Name { get; set; }

        public int Level { get; set; }

        public double Cost { get; set; }

        public double HeroDamageMultiplier { get; set; }

        public double ClickDamageMultiplier { get; set; }

        public double DpsToClickMultiplier { get; set; }

        public double AllDPSMultiplier { get; set; }

        public double CriticalChanceIncreas { get; set; }

        public double CriticalMultiplierIncrease { get; set; }

        public double GoldMultiplier { get; set; }

        public bool UnlocksSkill { get; set; }
        
        public Upgrade(string name, int level, double cost, double heroDmgMultiplier = 1, double clickDmgMultiplier = 1, double dpsToClick = 0, double allDpsMultiplier = 1, bool skill = false, double criticalChanceIncrease = 0, double criticalMultiplierIncrease = 0, double goldMultiplier = 1)
        {
            Name = name;
            Level = level;
            Cost = cost;
            HeroDamageMultiplier = heroDmgMultiplier;
            ClickDamageMultiplier = clickDmgMultiplier;
            DpsToClickMultiplier = dpsToClick;
            AllDPSMultiplier = allDpsMultiplier;
            UnlocksSkill = skill;
            CriticalChanceIncreas = criticalChanceIncrease;
            CriticalMultiplierIncrease = criticalMultiplierIncrease;
            GoldMultiplier = goldMultiplier;
        }
    }
    /// <summary>
    /// Represents a hero
    /// </summary>
    public class Hero
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name;

        /// <summary>
        /// Cost for first level, without any Dogcog levels
        /// </summary>
        public double Basecost;

        /// <summary>
        /// Damage done at level 1, ignoring all bonuses
        /// </summary>
        public double BaseDamage;

        /// <summary>
        /// The width of the hero's name on screen, as a proportion to the total play screen width
        /// </summary>
        public double Namewidth;

        /// <summary>
        /// An array with each upgrade.
        /// </summary>
        public Upgrade[] Upgrades;

        public Hero(string name, double basecost, double baseDmg, double namewidth, Upgrade[] upgrades)
        {
            Name = name;
            Basecost = basecost;
            BaseDamage = baseDmg;
            Namewidth = namewidth;
            Upgrades = upgrades;
        }

        /// <summary>
        /// Special case Cid
        /// </summary>
        private static double[] cidEarlyLevels = { 0, 5, 11, 17, 26, 37, 51, 67, 86, 108, 132, 161, 194, 232, 277, 325, 380, 439, 502 };

        /// <summary>
        /// Gets the cost to level a hero from one level to another level.
        /// </summary>
        /// <param name="tolevel"></param>
        /// <param name="fromlevel"></param>
        /// <returns></returns>
        public double GetCostToLevel(int tolevel, int fromlevel)
        {
            // oh god it's cid
            if (Basecost == 5.0f)
            {
                if (fromlevel > 15 && tolevel > 15)
                {
                    return GameEngine.GetHeroDiscount() * ((Constants.SumZeroToNOnePointSevenToPower(tolevel - 1) - Constants.SumZeroToNOnePointSevenToPower(fromlevel - 1)) * 20);
                }
                else if (tolevel > 15)
                {
                    return GameEngine.GetHeroDiscount() * ((Constants.SumZeroToNOnePointSevenToPower(tolevel - 1)) * 20 - cidEarlyLevels[fromlevel]);
                }
                else
                {
                    return GameEngine.GetHeroDiscount() * (cidEarlyLevels[tolevel] - cidEarlyLevels[fromlevel]);
                }
            }

            return (Constants.SumZeroToNOnePointSevenToPower(tolevel - 1) - Constants.SumZeroToNOnePointSevenToPower(fromlevel - 1)) * Basecost * GameEngine.GetHeroDiscount();
        }
    };

    /// <summary>
    /// Represents a hero as well as its current level and upgrades
    /// </summary>
    public class HeroStats
    {
        /// <summary>
        /// The hero
        /// </summary>
        public Hero Hero;

        /// <summary>
        /// Its level. This will be -1 if the hero's level cannot be determined (is offscreen)
        /// </summary>
        public int Level;

        /// <summary>
        /// The upgrades it has, as a bitfield. The first bit is 1 if the hero has the 1st upgrade purchased. The second bit is 1 if
        /// the hero has the 2nd upgrade purchased, and so on. If the hero's upgrades cannot be determined, this value is Int16.MinValue (upgrades are offscreen)
        /// </summary>
        public Int16 UpgradeBitfield;

        /// <summary>
        /// The bottom-right point of the hero's name on screen, used to calculate upgrade and buy button offsets.
        /// </summary>
        public Point bottomright;

        /// <summary>
        /// Gets a rectangle which encloses a hero's upgrade
        /// </summary>
        /// <param name="index">The upgrade to get. This is zero-based (first upgrade is 0)</param>
        /// <returns></returns>
        public Rectangle GetUpgradeRect(int index)
        {
            int top, bot, left, right;
            bot = bottomright.Y + GameEngine.GetHeroUpgradeSeperatorHeight() + GameEngine.GetUpgradeHeight() / 2;
            top = bottomright.Y + GameEngine.GetHeroUpgradeSeperatorHeight() - GameEngine.GetUpgradeHeight() / 2;
            left = bottomright.X - GameEngine.GetHeroUpgradeFirstSeperatorWidth() - GameEngine.GetUpgradeWidth() / 2 + index * GameEngine.GetUpgradeWidth();
            right = bottomright.X - GameEngine.GetHeroUpgradeFirstSeperatorWidth() + GameEngine.GetUpgradeWidth() / 2 + index * GameEngine.GetUpgradeWidth();

            return new Rectangle(left, top, right - left, bot - top);
        }

        /// <summary>
        /// Populates the HeroStat's UpgradeBitfield.
        /// </summary>
        /// <param name="lb"></param>
        public void CalculateUpgrades(LockBitmap lb)
        {
            UpgradeBitfield = 0;
            for (int i = 0; i < 7; i++)
            {
                Rectangle r = GetUpgradeRect(i);
                if (r.Bottom > lb.Height)
                {
                    UpgradeBitfield = Int16.MinValue;
                    return;
                }

                double d = OCREngine.GetBlobDensity(lb, r, new Color[] { Color.FromArgb(39, 166, 10), // Normal
                                                                         Color.FromArgb(7, 33, 1), // Bugged out
                                                                       });

                if (d > 0.003)
                {
                    UpgradeBitfield |= (Int16)(1 << i);
                }
            }
        }

        /// <summary>
        /// 1 if the hero has a given upgrade, 0 if it doesn't. -1 if it can't be determined (is offscreen)
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public int HasUpgrade(int i)
        {
            if ((UpgradeBitfield & Int16.MinValue) == Int16.MinValue)
            {
                return -1;
            }

            return (UpgradeBitfield & (Int16)(1 << i)) != 0 ? 1 : 0;
        }

        /// <summary>
        /// Returns a point inside this hero's buy button
        /// </summary>
        /// <param name="p"></param>
        /// <returns>True if successful, false if off-screen</returns>
        public bool GetBuyButton(out Point p)
        {
            p = new Point();
            // Since the buy button is roughly on the same horizontal line as the upgrades, we use that to determine if the buy button is visible
            if ((UpgradeBitfield & Int16.MinValue) == Int16.MinValue)
            {
                return false;
            }

            p.X = bottomright.X - GameEngine.GetBuyHeroHorizontalOffset() + GameEngine.GetHeroesArea().Left;
            p.Y = bottomright.Y + GameEngine.GetHeroUpgradeSeperatorHeight() + GameEngine.GetHeroesArea().Top;
            return true;
        }

        /// <summary>
        /// Returns a point inside one of this hero's upgrade buttons. There is no guarantee that this button is visible on screen
        /// </summary>
        /// <param name="p"></param>
        /// <param name="idex">The upgrade to get -- zero indexed (first upgrade is 0)</param>
        /// <returns></returns>
        public bool GetUpgradeButton(out Point p, int idex)
        {
            p = new Point();
            Rectangle upgradeRect = GetUpgradeRect(idex);
            p.X = upgradeRect.Left + upgradeRect.Width / 2 + GameEngine.GetHeroesArea().Left;
            p.Y = upgradeRect.Top + upgradeRect.Height / 2 + GameEngine.GetHeroesArea().Top;
            return true;
        }
    }

    /// <summary>
    /// Represents all heroes that are visible on screen
    /// </summary>
    public class ParsedHeroes
    {
        public List<HeroStats> HeroStats;

        /// <summary>
        /// The first hero visible
        /// </summary>
        public int FirstHeroIndex;

        /// <summary>
        /// The last hero visible
        /// </summary>
        public int LastHeroIndex;
    }

    /// <summary>
    /// Contains static methods to obtain various states of the game, primarily current money and current heroes.
    /// </summary>
    class GameEngine
    {
        public static IntPtr WindowHandle;
        /// <summary>
        ///  The heroes!
        /// </summary>
        static public Hero[] HeroList = {
            new Hero("Cid, the Helpful Adventurer", 5, 0, 0.1904, new Upgrade[] { 
                new Upgrade("Big Clicks", 10, 100, clickDmgMultiplier: 2), 
                new Upgrade("Clickstorm", 25, 250, skill: true), 
                new Upgrade("Huge Clicks", 50, 1000, clickDmgMultiplier: 2), 
                new Upgrade("Massive Clicks", 75, 8000, clickDmgMultiplier: 2),
                new Upgrade("Titanic Clicks", 100, 80000, clickDmgMultiplier: 2.5), 
                new Upgrade("Colossal Clicks", 125, 4E5, clickDmgMultiplier: 3), 
                new Upgrade("Monumental Clicks", 150, 4E6, clickDmgMultiplier: 3.5)}),
            new Hero("Treebeast", 50, 5, 0.0664, new Upgrade[] { 
                new Upgrade("Fertilizer", 10, 500, heroDmgMultiplier: 2),
                new Upgrade("Thorns", 25, 1250, heroDmgMultiplier: 2), 
                new Upgrade("Megastick", 50, 5000, heroDmgMultiplier: 2), 
                new Upgrade("Ultrastick", 75, 40000, heroDmgMultiplier: 2), 
                new Upgrade("Lacquer", 100, 4E5, dpsToClick: 0.005) }),
            new Hero("Ivan, the Drunken Brawler", 250, 22, 0.1807, new Upgrade[] { 
                new Upgrade("Hard Cider", 10, 2500, heroDmgMultiplier: 2), 
                new Upgrade("Pint of Ale", 25, 6250, heroDmgMultiplier: 2), 
                new Upgrade("Pitcher", 50, 25000, heroDmgMultiplier: 2), 
                new Upgrade("Powersurge", 75, 2E5, skill: true), 
                new Upgrade("Embalming Fluid", 100, 2E6, dpsToClick: 0.005), 
                new Upgrade("Pint of Pig's Whiskey", 125, 1E7, heroDmgMultiplier: 2.5) }),
            new Hero("Brittany, Beach Princess", 1000, 74, 0.1630, new Upgrade[] { 
                new Upgrade("Combat Makeup", 10, 10000, heroDmgMultiplier: 2), 
                new Upgrade("Brand Name Equipment", 25, 25000, heroDmgMultiplier: 2), 
                new Upgrade("Elixir of Deditzification", 50, 1E5, heroDmgMultiplier: 2), 
                new Upgrade("Vegan Meat", 75, 8E5, heroDmgMultiplier: 2.5) }),
            new Hero("The Wandering Fisherman", 4000, 245, 0.1780, new Upgrade[] { 
                new Upgrade("Spear Training", 10, 40000, heroDmgMultiplier: 2), 
                new Upgrade("Crab Net", 25, 1E5, heroDmgMultiplier: 2), 
                new Upgrade("Whetstone", 50, 4E5, heroDmgMultiplier: 2), 
                new Upgrade("Fish Cooking", 75, 3.2E6, allDpsMultiplier: 1.25), 
                new Upgrade("State of the Art Fishing Gear", 100, 3.2E7, dpsToClick: 0.005) }),
            new Hero("Betty Clicker", 20000, 976, 0.0859, new Upgrade[] { 
                new Upgrade("Wilderburr Dumplings", 10, 2E5, allDpsMultiplier: 1.2), 
                new Upgrade("Braised Flamingogo", 25, 5E5, allDpsMultiplier: 1.2), 
                new Upgrade("Truffed Tollgre with Bloop Reduction", 50, 2E6, allDpsMultiplier: 1.2), 
                new Upgrade("Foomgus Risotto", 75, 1.6E7, allDpsMultiplier: 1.2), 
                new Upgrade("Wolrd Famous Cookbook", 100, 1.6E8, dpsToClick: 0.005) }),
            new Hero("The Masked Samurai", 100000, 3725, 0.1399, new Upgrade[] { 
                new Upgrade("Jutsu I", 10, 1E6, heroDmgMultiplier: 2), 
                new Upgrade("Jutsu II", 25, 2.5E6, heroDmgMultiplier: 2), 
                new Upgrade("Jutsu III", 50, 1E7, heroDmgMultiplier: 2), 
                new Upgrade("Jutsu IV", 75, 8E7, heroDmgMultiplier: 2.5) }),
            new Hero("Leon", 400000, 10859, 0.0301, new Upgrade[] { 
                new Upgrade("Courage Tonic", 10, 4E6, heroDmgMultiplier: 2), 
                new Upgrade("Stronger Claws", 25, 1E7, heroDmgMultiplier: 2), 
                new Upgrade("Lionheart Potion", 50, 4E7, heroDmgMultiplier: 2), 
                new Upgrade("Lion's Roar", 75, 3.2E8, allDpsMultiplier: 1.25) }),
            new Hero("The Great Forest Seer", 2500000, 47143, 0.1444, new Upgrade[] { 
                new Upgrade("Forest Creatures", 10, 2.5E7, heroDmgMultiplier: 2), 
                new Upgrade("Insight", 25, 6.25E7, heroDmgMultiplier: 2), 
                new Upgrade("Dark Lore", 50, 2.5E8, heroDmgMultiplier: 2), 
                new Upgrade("Swarm", 75, 2E9, heroDmgMultiplier: 2) }),
            new Hero("Alexa, Assassin", 15000000, 186000, 0.1045, new Upgrade[] {
                new Upgrade("Critical Strike", 10, 1.5E8, criticalChanceIncrease: 0.03), 
                new Upgrade("Clairvoyance", 25, 3.75E8, heroDmgMultiplier: 2.25), 
                new Upgrade("Poisoned Blades", 50, 1.5E9, heroDmgMultiplier: 2.25), 
                new Upgrade("Invisible Strikes", 75, 1.2E10, criticalMultiplierIncrease: 5), 
                new Upgrade("Lucky Strikes", 100, 1.2E11, skill: true) }),
            new Hero("Natalia, Ice Apprentice", 100000000, 782000, 0.1550, new Upgrade[] { 
                new Upgrade("Magic 101", 10, 1E9, heroDmgMultiplier: 2), 
                new Upgrade("Below Zero", 25, 2.5E9, heroDmgMultiplier: 2), 
                new Upgrade("Frozen Warfare", 50, 1E10, heroDmgMultiplier: 2), 
                new Upgrade("The Book of Frost", 75, 8E10, heroDmgMultiplier: 2.5) }),
            new Hero("Mercedes, Duchess of Blades", 800000000, 3721000, 0.1966, new Upgrade[] { 
                new Upgrade("Mithril Edge", 10, 8E9, heroDmgMultiplier: 2), 
                new Upgrade("Enchanted Blade", 25, 2E10, heroDmgMultiplier: 2), 
                new Upgrade("QuickBlade", 50, 8E10, heroDmgMultiplier: 2), 
                new Upgrade("Blessed Sword", 75, 6.4E11, heroDmgMultiplier: 2.5), 
                new Upgrade("Art of Swordfighting", 100, 6.4E12, dpsToClick: 0.005) }),
            new Hero("Bobby, Bounty Hunter", 6.5E9, 17010000, 0.1515, new Upgrade[] { 
                new Upgrade("Impressive Moves", 10, 6.5E10, heroDmgMultiplier: 2), 
                new Upgrade("Acrobatic Jetpack", 25, 1.625E11, heroDmgMultiplier: 2), 
                new Upgrade("Jetpack Dance", 50, 6.5E11, heroDmgMultiplier: 2), 
                new Upgrade("Whirling Skyblade", 75, 5.2E12, heroDmgMultiplier: 2.5), 
                new Upgrade("Sweeping Strikes", 100, 5.2E13, criticalChanceIncrease: 0.03) }),
            new Hero("Broyle Lindeoven, Fire Mage", 5.0E10, 69064000, 0.1949, new Upgrade[] { 
                new Upgrade("Roast Monsters", 10, 5E11, allDpsMultiplier: 1.25), 
                new Upgrade("Combustible Air", 25, 1.25E12, heroDmgMultiplier: 2), 
                new Upgrade("Inner Fire", 50, 5E12, heroDmgMultiplier: 2), 
                new Upgrade("The Floor is Lava", 75, 4E13, heroDmgMultiplier: 2.5), 
                new Upgrade("Metal Detector", 100, 4E14, skill: true) }),
            new Hero("Sir George II, King's Guard", 4.50E11, 4.6E8, 0.1807, new Upgrade[] { 
                new Upgrade("Abandoned Regret", 10, 4.5E12, heroDmgMultiplier: 2), 
                new Upgrade("Offensive Strategies", 25, 1.125E13, heroDmgMultiplier: 2), 
                new Upgrade("Combet Strategy", 50, 4.5E13, heroDmgMultiplier: 2), 
                new Upgrade("Burning Blade", 75, 3.6E14, heroDmgMultiplier: 2.5), 
                new Upgrade("King's Pardon", 100, 3.6E15, dpsToClick: 0.005) }),
            new Hero("King Midas", 4E12, 3.017E9, 0.0753, new Upgrade[] { 
                new Upgrade("Bag of Holding", 10, 4E13, goldMultiplier: 1.25), 
                new Upgrade("Heart of Gold", 25, 1E14, goldMultiplier: 1.25), 
                new Upgrade("Touch of Gold", 50, 4E14, goldMultiplier: 1.25), 
                new Upgrade("Golden Dimension", 75, 3.2E15, goldMultiplier: 1.5), 
                new Upgrade("Golden Clicks", 100, 3.2E16, skill: true), 
                new Upgrade("Gold Blade", 125, 1.6E17, criticalChanceIncrease: 0.03) }),
            new Hero("Referi Jerator, Ice Wizard", 3.6E13, 2.0009E10, 0.1727, new Upgrade[] { 
                new Upgrade("Defrosting", 10, 3.6E14, heroDmgMultiplier: 2), 
                new Upgrade("Headbashing", 25, 9E14, heroDmgMultiplier: 2), 
                new Upgrade("Iceberg Rain", 50, 3.6E15, heroDmgMultiplier: 2), 
                new Upgrade("Glacier Storm", 75, 2.88E16, heroDmgMultiplier: 2.5), 
                new Upgrade("Icy Touch", 125, 2.88E17, criticalMultiplierIncrease: 3) }),
            new Hero("Abaddon", 3.2E14, 1.31E11, 0.0602, new Upgrade[] { 
                new Upgrade("Rise of the Dead", 10, 3.2E15, heroDmgMultiplier: 2.25), 
                new Upgrade("Curse of the Dark God", 25, 8E15, heroDmgMultiplier: 2.25), 
                new Upgrade("Epidemic Evil", 50, 3.2E16, heroDmgMultiplier: 2.25), 
                new Upgrade("The Dark Ritual", 75, 2.560E17, skill: true) }),
            new Hero("Ma Zhu", 2.7E15, 8.14E11, 0.0478, new Upgrade[] { 
                new Upgrade("Heaven's Hand", 10, 2.7E16, heroDmgMultiplier: 2), 
                new Upgrade("Plasma Arc", 25, 6.75E16, heroDmgMultiplier: 2), 
                new Upgrade("Ancient Wrath", 50, 2.7E17, heroDmgMultiplier: 2), 
                new Upgrade("Pet Dragon", 75, 2.16E18, heroDmgMultiplier: 2.5) }),
            new Hero("Amenhotep", 2.4E16, 5.335E12, 0.0788, new Upgrade[] { 
                new Upgrade("Smite", 10, 2.4E17, heroDmgMultiplier: 2), 
                new Upgrade("Genesis Research", 25, 6E17, allDpsMultiplier: 1.2), 
                new Upgrade("Prepare the Rebeginning", 50, 2.4E18, allDpsMultiplier: 1.2), 
                new Upgrade("ASCENSION", 150, 1.92E19, skill: true) }),
            new Hero("Beastlord", 3.0E17, 4.9143E13, 0.0629, new Upgrade[] { 
                new Upgrade("Eye in the Sky", 10, 3E18, heroDmgMultiplier: 2), 
                new Upgrade("Critters", 25, 7.5E18, heroDmgMultiplier: 2), 
                new Upgrade("Beastmode", 50, 3E19, heroDmgMultiplier: 2), 
                new Upgrade("Sacrificial Lamb's Blood", 75, 2.4E20, allDpsMultiplier: 1.1), 
                new Upgrade("Super Clicks", 100, 2.4E21, skill: true) }),
            new Hero("Athena, Goddess of War", 9.0E18, 1.086E15, 0.1639, new Upgrade[] { 
                new Upgrade("Hand-to-Head Combat", 10, 9E19, heroDmgMultiplier: 2), 
                new Upgrade("Warscream", 25, 2.25E20, heroDmgMultiplier: 2), 
                new Upgrade("Bloodlust", 50, 9E20, heroDmgMultiplier: 2), 
                new Upgrade("Boiling Blood", 100, 7.2E21, heroDmgMultiplier: 2) }),
            new Hero("Aphrodite, Goddess of Love", 3.5E20, 3.1124E16, 0.1895, new Upgrade[] { 
                new Upgrade("Lasso of Love", 10, 3.5E21, heroDmgMultiplier: 2), 
                new Upgrade("Love Potion", 25, 8.75E21, heroDmgMultiplier: 2), 
                new Upgrade("Love Hurts", 50, 3.5E22, heroDmgMultiplier: 2), 
                new Upgrade("Energize", 100, 2.8E23, skill: true), 
                new Upgrade("Kiss of Death", 125, 2.8E24, heroDmgMultiplier: 2) }),
            new Hero("Shinatobe, Wind Deity", 1.4E22, 9.17E17, 0.1541, new Upgrade[] { 
                new Upgrade("Dancing Blades", 10, 1.4E23, heroDmgMultiplier: 2), 
                new Upgrade("Annoying Winds", 25, 3.5E23, allDpsMultiplier: 1.1), 
                new Upgrade("Bladestorm", 50, 1.4E24, heroDmgMultiplier: 2), 
                new Upgrade("Eye of the Storm", 75, 1.12E25, heroDmgMultiplier: 2), 
                new Upgrade("Reload", 100, 1.12E26, skill: true) }),
            new Hero("Grant, the General", 4.199E24, 2.02E20, 0.1267, new Upgrade[] { 
                new Upgrade("Red Whip", 10, 4.199E25, allDpsMultiplier: 1.25), 
                new Upgrade("Art of War", 25, 1.049E26, heroDmgMultiplier: 2), 
                new Upgrade("Battle Plan", 50, 4.199E26, allDpsMultiplier: 1.25), 
                new Upgrade("Top of the Line Gear", 75, 3.359E27, heroDmgMultiplier: 2) }),
            new Hero("Frostleaf", 2.1E27, 7.4698E22, 0.0585, new Upgrade[] { 
                new Upgrade("Ice Age", 10, 2.1E28, heroDmgMultiplier: 2), 
                new Upgrade("Book of Winter", 25, 5.249E28, heroDmgMultiplier: 2), 
                new Upgrade("Frozen Stare", 50, 2.099E29, allDpsMultiplier: 1.25), 
                new Upgrade("Frigid Enchant", 75, 1.679E30, dpsToClick: 0.005) }),
            new Hero("Dread Knight", 1.000E40, 1.31E30, 0.0895, new Upgrade[] { 
                new Upgrade("Lost Soul", 10, 1E41, heroDmgMultiplier: 2), 
                new Upgrade("Soul Catcher", 25, 2.5E41, heroDmgMultiplier: 2), 
                new Upgrade("Raging Bull", 50, 1E42, heroDmgMultiplier: 2), 
                new Upgrade("Dark Soul", 100, 8E42, heroDmgMultiplier: 2.5) }),
            new Hero("Atlas", 1.000E55, 9.65E40, 0.0324, new Upgrade[] { 
                new Upgrade("Resurrection", 10, 1E56, heroDmgMultiplier: 2), 
                new Upgrade("Band of Brothers", 25, 2.5E56, heroDmgMultiplier: 2), 
                new Upgrade("Medic", 50, 1.0E57, heroDmgMultiplier: 2), 
                new Upgrade("Fully Charged", 100, 8.0E57, heroDmgMultiplier: 2.5) }),
            new Hero("Terra", 1.000E70, 7.113E55, 0.0324, new Upgrade[] { 
                new Upgrade("Interference", 10, 1E71, heroDmgMultiplier: 2), 
                new Upgrade("Surveillance", 25, 2.5E71, heroDmgMultiplier: 2), 
                new Upgrade("Camouflage", 50, 1.0E72, heroDmgMultiplier: 2), 
                new Upgrade("Revive", 100, 8E72, heroDmgMultiplier: 2.5) }),
            new Hero("Phtalo", 1.0E85, 5.24E70, 0.0506, new Upgrade[] { 
                new Upgrade("Pesticide", 10, 1E86, heroDmgMultiplier: 2), 
                new Upgrade("Rejuvenating Seeds", 25, 2.5E86, heroDmgMultiplier: 2), 
                new Upgrade("Green Scroll", 50, 1E87, heroDmgMultiplier: 2), 
                new Upgrade("Split Earth", 100, 8E87, heroDmgMultiplier: 2.5) }),
            new Hero("Orentchya Gladeye, Didensy Banana", 1.0E100, 3.861E83, 0.1850, new Upgrade[] { 
                new Upgrade("Travel Supplies", 10, 1E101, heroDmgMultiplier: 2), 
                new Upgrade("Portal", 25, 2.5E101, heroDmgMultiplier: 2), 
                new Upgrade("Travel Potion", 50, 1E102, heroDmgMultiplier: 2), 
                new Upgrade("Traveling Sword", 100, 8E102, heroDmgMultiplier: 2.5) }),
            new Hero("Lilin", 1.0E115, 2.845E96, 0.0275, new Upgrade[] { 
                new Upgrade("Heart Juice", 10, 1.0E116, heroDmgMultiplier: 2), 
                new Upgrade("Luscious Lips", 25, 2.5E116, heroDmgMultiplier: 2), 
                new Upgrade("Lover's Quarrel", 50, 1E117, heroDmgMultiplier: 2), 
                new Upgrade("Love at First Sight", 100, 8E117, heroDmgMultiplier: 2.5) }),
            new Hero("Cadmia", 1.0E130, 2.096E109, 0.0504, new Upgrade[] { 
                new Upgrade("Fighting for Dummies", 10, 1.0E131, heroDmgMultiplier: 2), 
                new Upgrade("Warrior Spirit", 25, 2.5E131, heroDmgMultiplier: 2), 
                new Upgrade("Red Sword", 50, 1E132, heroDmgMultiplier: 2), 
                new Upgrade("Flaming Red Sword", 100, 8E132, heroDmgMultiplier: 2.5) }),
            new Hero("Alabaster", 1.0E145, 1.544E122, 0.0662, new Upgrade[] { 
                new Upgrade("Meditation", 10, 1.0E146, heroDmgMultiplier: 2), 
                new Upgrade("Travel Boots", 25, 2.5E146, heroDmgMultiplier: 2), 
                new Upgrade("Peacekeeper", 50, 1E147, heroDmgMultiplier: 2), 
                new Upgrade("Blinding Light", 10, 8E147, heroDmgMultiplier: 2.5) }),
            new Hero("Astrea", 1.0E160, 1.137E135, 0.0515, new Upgrade[] { 
                new Upgrade("Pro-Aging", 10, 1.0E161, heroDmgMultiplier: 2), 
                new Upgrade("Slice and Dice", 25, 2.5E161, heroDmgMultiplier: 2), 
                new Upgrade("Time Travel", 50, 1E162, heroDmgMultiplier: 2), 
                new Upgrade("Alter time", 100, 8E162, heroDmgMultiplier: 2.5) }),
        };

        #region ScreenOffsets
        /// <summary>
        /// The entire screen play area
        /// </summary>
        static private Rectangle PlayableArea;

        /// <summary>
        /// The area which contains the current money
        /// </summary>
        static private Rectangle MoneyArea;

        /// <summary>
        /// The area which contains all visible heroes
        /// </summary>
        static private Rectangle HeroesArea;

        /// <summary>
        /// The point to click to do monster damage
        /// </summary>
        static private Point ClickArea;

        /// <summary>
        /// When looking at lines in the heroes area, this value is used as a cutoff
        /// to determine if two consecutive lines are Hero-Level or Level-Hero
        /// </summary>
        static private int HeroSeperatorHeight;

        /// <summary>
        /// The height of a hero's upgrade box
        /// </summary>
        static private int UpgradeHeight;

        /// <summary>
        /// The width of a hero's upgrade box
        /// </summary>
        static private int UpgradeWidth;

        /// <summary>
        /// The vertical distance between a hero's name (bottom-right corner) and its upgrade area
        /// </summary>
        static private int HeroUpgradeSeperatorHeight;

        /// <summary>
        /// The horizontal distance between a hero's name (bottom-right corner) and its first upgrade
        /// </summary>
        static private int HeroUpgradeFirstSeperatorWidth;

        /// <summary>
        /// The scroll up button
        /// </summary>
        static private Point ScrollUpButton;

        /// <summary>
        /// The scroll down button
        /// </summary>
        static private Point ScrollDownButton;

        /// <summary>
        /// The horizontal distance between a hero's name (bottom-right corner) and the "buy" button
        /// </summary>
        static private int BuyHeroHorizontalOffset;

        /// <summary>
        /// All possible points that a candy can spawn
        /// </summary>
        static private Point[] Candies;

        /// <summary>
        /// The height of a candy's hitbox
        /// </summary>
        static private int CandyHeight;

        /// <summary>
        /// The width of a candy's hitbox
        /// </summary>
        static private int CandyWidth;

        /// <summary>
        /// The location of the "okay" button when ascending
        /// </summary>
        static private Point AscendButton;

        /// <summary>
        /// The location of the Buy Available Upgrades button
        /// </summary>
        static private Point BuyAllButton;

        /// <summary>
        /// The point to click to focus the browser instead of flash
        /// </summary>
        static private Point FocusBrowser;

        /// <summary>
        /// The location of the start button
        /// </summary>
        static private Point StartButton;

        /// <summary>
        /// The location of the close start screen
        /// </summary>
        static private Point CloseStartSceenButton;

        /// <summary>
        /// The hero cost multiplier, which can be between 0.5 and 1.0 depending on Dogcog level.
        /// </summary>
        static double HeroDiscount = 1.0;

        /// <summary>
        /// The location of the progress/farm mode button
        /// </summary>
        static private Point ProgressButton;

        /// <summary>
        /// The location of the option button
        /// </summary>
        static private Point OptionButton;

        /// <summary>
        /// The location of the save button
        /// </summary>
        static private Point SaveButton;

        /// <summary>
        /// The location of the close save screen
        /// </summary>
        static private Point CloseSaveScreenButton;

        /// <summary>
        /// The location of the close option screen
        /// </summary>
        static private Point CloseOptionScreenButton;

        /// <summary>
        /// The location of the Relic Tab Button
        /// </summary>
        static private Point RelicTabButton;

        /// <summary>
        /// The location of the Hero Tab Button
        /// </summary>
        static private Point HeroTabButton;

        /// <summary>
        /// The location of the Salvage Junk Pile Button
        /// </summary>
        static private Point SalvageJunkPileButton;

        /// <summary>
        /// The location of the Salvage Junk Pile Yes Button
        /// </summary>
        static private Point SalvageJunkPileYesButton;

        /// <summary>
        /// The location of the move zone left button
        /// </summary>
        static private Point MoveZoneLeftButton;

        /// <summary>
        /// The location of the move zone right button
        /// </summary>
        static private Point MoveZoneRightButton;
        #endregion

        #region GameEngineGetters
        public static Rectangle GetPlayableArea()
        {
            return PlayableArea;
        }

        public static Rectangle GetMoneyArea()
        {
            return MoneyArea;
        }

        public static Rectangle GetHeroesArea()
        {
            return HeroesArea;
        }

        public static Point GetClickArea()
        {
            return ClickArea;
        }

        public static int GetUpgradeHeight()
        {
            return UpgradeHeight;
        }

        public static int GetUpgradeWidth()
        {
            return UpgradeWidth;
        }

        public static int GetHeroUpgradeSeperatorHeight()
        {
            return HeroUpgradeSeperatorHeight;
        }

        public static int GetHeroUpgradeFirstSeperatorWidth()
        {
            return HeroUpgradeFirstSeperatorWidth;
        }

        public static Point GetScrollbarUpPoint()
        {
            return ScrollUpButton;
        }

        public static Point GetScrollbarDownPoint()
        {
            return ScrollDownButton;
        }

        public static int GetBuyHeroHorizontalOffset()
        {
            return BuyHeroHorizontalOffset;
        }

        public static Point GetProgressButton()
        {
            return ProgressButton;
        }

        public static Point GetAscendButton()
        {
            return AscendButton;
        }

        public static Point GetBuyAllButton()
        {
            return BuyAllButton;
        }

        public static Point GetFocusBrowser()
        {
            return FocusBrowser;
        }

        public static Point GetStartButton()
        {
            return StartButton;
        }

        public static Point GetCloseStartScreenButton()
        {
            return CloseStartSceenButton;
        }

        public static int GetCandyWidth()
        {
            return CandyWidth;
        }

        public static int GetCandyHeight()
        {
            return CandyHeight;
        }

        public static double GetHeroDiscount()
        {
            return HeroDiscount;
        }

        public static Point[] GetCandyButtons()
        {
            return Candies;
        }

        public static Point GetOptionButton()
        {
            return OptionButton;
        }

        public static Point GetSaveButton()
        {
            return SaveButton;
        }

        public static Point GetCloseSaveScreenButton()
        {
            return CloseSaveScreenButton;
        }

        public static Point GetCloseOptionScreenButton()
        {
            return CloseOptionScreenButton;
        }

        public static Point GetRelicTabButton()
        {
            return RelicTabButton;
        }

        public static Point GetHeroTabButton()
        {
            return HeroTabButton;
        }

        public static Point GetSalvageJunkPileButton()
        {
            return SalvageJunkPileButton;
        }

        public static Point GetSalvageJunkPileYesButton()
        {
            return SalvageJunkPileYesButton;
        }

        public static Point GetMoveZoneLeftButton()
        {
            return MoveZoneLeftButton;
        }

        public static Point GetMoveZoneRightButtion()
        {
            return MoveZoneRightButton;
        }
        #endregion

        /// <summary>
        /// Defines the game play area, and calculates all other offsets from that
        /// </summary>
        /// <param name="playableArea"></param>
        public static void SetPlayableArea(Rectangle playableArea) 
        {
            PlayableArea = playableArea;

            // Calculate all other coordinates
            ClickArea.X = (int)(PlayableArea.Width * 0.745 + PlayableArea.Left);
            ClickArea.Y = (int)(PlayableArea.Height * 0.508 + PlayableArea.Top);

            MoneyArea.X = (int)(PlayableArea.Width * 0.153 + PlayableArea.Left);
            MoneyArea.Y = (int)(PlayableArea.Height * 0.040 + PlayableArea.Top);
            MoneyArea.Height = (int)(PlayableArea.Height * 0.090 + PlayableArea.Top) - MoneyArea.Y;
            MoneyArea.Width = (int)(PlayableArea.Width * 0.346 + PlayableArea.Left) - MoneyArea.X;

            HeroesArea.X = (int)(PlayableArea.Width * 0.153 + PlayableArea.Left);
            HeroesArea.Y = (int)(PlayableArea.Height * 0.285 + PlayableArea.Top);
            HeroesArea.Height = (int)(PlayableArea.Height * 0.989 + PlayableArea.Top) - HeroesArea.Y;
            HeroesArea.Width = (int)(PlayableArea.Width * 0.382 + PlayableArea.Left) - HeroesArea.X;

            HeroSeperatorHeight = (int)(0.0839 * PlayableArea.Height);

            UpgradeHeight = (int)(0.0475 * PlayableArea.Height);
            UpgradeWidth = (int)(0.03216 * PlayableArea.Width);

            HeroUpgradeSeperatorHeight = (int)(0.07625 * PlayableArea.Height);
            HeroUpgradeFirstSeperatorWidth = (int)(0.204 * PlayableArea.Width);

            ScrollUpButton.X = (int)(PlayableArea.Width * 0.4814 + PlayableArea.Left);
            ScrollUpButton.Y = (int)(PlayableArea.Height * 0.2978 + PlayableArea.Top);

            ScrollDownButton.X = ScrollUpButton.X;
            ScrollDownButton.Y = (int)(PlayableArea.Height * 0.9756 + PlayableArea.Top);

            BuyHeroHorizontalOffset = (int)(PlayableArea.Width * 0.29114);

            Candies = new Point[6];

            Candies[0].X = (int)(PlayableArea.Width * 0.6590 + PlayableArea.Left);
            Candies[0].Y = (int)(PlayableArea.Height * 0.5457 + PlayableArea.Top);

            // validated
            Candies[1].X = (int)(PlayableArea.Width * 0.8776 + PlayableArea.Left);
            Candies[1].Y = (int)(PlayableArea.Height * 0.6591 + PlayableArea.Top);

            // validated
            Candies[2].X = (int)(PlayableArea.Width * 0.4569 + PlayableArea.Left);
            Candies[2].Y = (int)(PlayableArea.Height * 0.7116 + PlayableArea.Top);

            Candies[3].X = (int)(PlayableArea.Width * 0.7573 + PlayableArea.Left);
            Candies[3].Y = (int)(PlayableArea.Height * 0.7516 + PlayableArea.Top);

            Candies[4].X = (int)(PlayableArea.Width * 0.9167 + PlayableArea.Left);
            Candies[4].Y = (int)(PlayableArea.Height * 0.6467 + PlayableArea.Top);

            // validated
            Candies[5].X = (int)(PlayableArea.Width * 0.6508 + PlayableArea.Left);
            Candies[5].Y = (int)(PlayableArea.Height * 0.6248 + PlayableArea.Top);

            CandyWidth = (int)(PlayableArea.Width * 0.03516);
            CandyHeight = (int)(PlayableArea.Height * 0.05);

            ProgressButton.X = (int)(PlayableArea.Width * 0.9799 + PlayableArea.Left);
            ProgressButton.Y = (int)(PlayableArea.Height * 0.3899 + PlayableArea.Top);

            AscendButton.X = (int)(PlayableArea.Width * 0.4344 + PlayableArea.Left);
            AscendButton.Y = (int)(PlayableArea.Height * 0.6654 + PlayableArea.Top);

            BuyAllButton.X = (int)(PlayableArea.Width * 0.2706 + PlayableArea.Left);
            BuyAllButton.Y = (int)(PlayableArea.Height * 0.8645 + PlayableArea.Top);

            FocusBrowser.X = (int)(PlayableArea.Left - 10);
            FocusBrowser.Y = (int)(PlayableArea.Top - 10);

            StartButton.X = (int)(PlayableArea.Width * 0.5 + PlayableArea.Left);
            StartButton.Y = (int)(PlayableArea.Height * 0.4286 + PlayableArea.Top);

            CloseStartSceenButton.X = (int)(PlayableArea.Width * 0.824 + PlayableArea.Left);
            CloseStartSceenButton.Y = (int)(PlayableArea.Height * 0.12 + PlayableArea.Top);

            OptionButton.X = (int)(PlayableArea.Width * 0.9799 + PlayableArea.Left);
            OptionButton.Y = (int)(PlayableArea.Height * 0.0313 + PlayableArea.Top);

            SaveButton.X = (int)(PlayableArea.Width * 0.2874 + PlayableArea.Left);
            SaveButton.Y = (int)(PlayableArea.Height * 0.1270 + PlayableArea.Top);

            CloseSaveScreenButton.X = (int)(PlayableArea.Width * 0.4403 + PlayableArea.Left);
            CloseSaveScreenButton.Y = (int)(PlayableArea.Height * 0.4702 + PlayableArea.Top);

            CloseOptionScreenButton.X = (int)(PlayableArea.Width * 0.7922 + PlayableArea.Left);
            CloseOptionScreenButton.Y = (int)(PlayableArea.Height * 0.0470 + PlayableArea.Top);

            RelicTabButton.X = (int)(PlayableArea.Width * 0.3298 + PlayableArea.Left);
            RelicTabButton.Y = (int)(PlayableArea.Height * 0.1614 + PlayableArea.Top);

            HeroTabButton.X = (int)(PlayableArea.Width * 0.0344 + PlayableArea.Left);
            HeroTabButton.Y = (int)(PlayableArea.Height * 0.1614 + PlayableArea.Top);

            SalvageJunkPileButton.X = (int)(PlayableArea.Width * 0.2460 + PlayableArea.Left);
            SalvageJunkPileButton.Y = (int)(PlayableArea.Height * 0.6865 + PlayableArea.Top);

            SalvageJunkPileYesButton.X = (int)(PlayableArea.Width * 0.4321 + PlayableArea.Left);
            SalvageJunkPileYesButton.Y = (int)(PlayableArea.Height * 0.6317 + PlayableArea.Top);

            MoveZoneLeftButton.X = (int)(PlayableArea.Width * 0.6907 + PlayableArea.Left);
            MoveZoneLeftButton.Y = (int)(PlayableArea.Height * 0.0568 + PlayableArea.Top);

            MoveZoneRightButton.X = (int)(PlayableArea.Width * 0.8044 + PlayableArea.Left);
            MoveZoneRightButton.Y = (int)(PlayableArea.Height * 0.0568 + PlayableArea.Top);

            //Check for Steam window
            //Background functionality works with Steam window
            //Still seems to be a few issues - if you move the window after detecting it,
            //it throws an OutOfMemory error
            WindowHandle = Imports.FindWindow(null, "Clicker Heroes");
            /*
             * Not sure how to make this effective, while it would be nice to be able to have a chrome
             * window in the background, not sure how to make it work effectivily
             * Somewhat works but pulls the window to the foreground anytime an event is sent
            //Check for Chrome tab if no steam window
            //It finds the window correctly, you can still freely use the mouse but it still
            //takes focus away from everything else
            if(WindowHandle == IntPtr.Zero)
            {
                WindowHandle = FindChromeTab.ChromeWindow();
            }
             * */
        }

        /// <summary>
        /// Vallidates the currently set playable area
        /// </summary>
        public static bool ValidatePlayableArea()
        {
            if (GetMoney() >= 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the current discount level (from Dogcog level). 1.0 for no dogcog, 0.5 for max dogcog level.
        /// </summary>
        /// <param name="discount"></param>
        public static void SetHeroDiscount(double discount)
        {
            HeroDiscount = discount;
        }

        /* Intended to return the point of the currently active candy, if there is one. Doesn't work as intended though, buggy.
        public static bool GetActiveCandy(out Point p)
        {
            p = new Point();
            foreach(Point candyCenter in Candies) {
                Rectangle c = new Rectangle(candyCenter.X - CandyWidth / 2, candyCenter.Y - CandyHeight / 2, CandyWidth, CandyHeight);

                using (Bitmap bitmap = GetImage(c))
                {
                    if (OCREngine.GetBlobDensity(bitmap, new Rectangle(0, 0, bitmap.Width - 1, bitmap.Height - 1), new Color[] {
                        // sandwich colors
                        Color.FromArgb(255, 170, 43, 0),
                        Color.FromArgb(255, 172, 54, 0),
                        Color.FromArgb(255, 202, 38, 10),

                        // pi colors
                        Color.FromArgb(255, 230, 143, 28)
                    }) > 0.0) {
                        p.X = candyCenter.X;
                        p.Y = candyCenter.Y;
                        return true;
                    }
                }
            }

            return false;
        }*/

        /// <summary>
        /// True if progress mode is on
        /// </summary>
        /// <returns></returns>
        public static bool IsProgressModeOn()
        {
            Rectangle c = new Rectangle(ProgressButton.X - CandyWidth / 2, ProgressButton.Y - CandyHeight / 2, CandyWidth, CandyHeight);
            using (Bitmap bitmap = GetImage(c))
            {
                if (OCREngine.GetBlobDensity(bitmap, new Rectangle(0, 0, bitmap.Width- 1, bitmap.Height - 1), new Color[] {
                        Color.FromArgb(255, 255, 0, 0)
                        // put pumpkin pie thing here
                    }) > 0.0)
                {
                    return false;
                }

                return true;
            }
        }

        //From neilmcguire on github; https://github.com/neilmcguire/ClickerHeroes_AutoPlayer/blob/Steam/GameEngine.cs
        //Used with background window support
        public static Bitmap GetImage(Rectangle rect)
        {
            if (Properties.Settings.Default.backgroundWindow && WindowHandle != IntPtr.Zero)
            {
                Imports.RECT rc;
                Imports.GetWindowRect(WindowHandle, out rc);

                Bitmap bmp = new Bitmap(rc.Width, rc.Height, PixelFormat.Format32bppArgb);
                Graphics gfxBmp = Graphics.FromImage(bmp);
                IntPtr hdcBitmap = gfxBmp.GetHdc();
                bool succeeded = Imports.PrintWindow(WindowHandle, hdcBitmap, 0);
                gfxBmp.ReleaseHdc(hdcBitmap);
                if (!succeeded)
                {
                    gfxBmp.FillRectangle(new SolidBrush(Color.Gray), new Rectangle(Point.Empty, bmp.Size));
                }
                IntPtr hRgn = Imports.CreateRectRgn(0, 0, 0, 0);
                Imports.GetWindowRgn(WindowHandle, hRgn);
                Region region = Region.FromHrgn(hRgn);
                if (!region.IsEmpty(gfxBmp))
                {
                    gfxBmp.ExcludeClip(region);
                    gfxBmp.Clear(Color.Transparent);
                }
                gfxBmp.Dispose();

                var point = new Imports.POINT();
                Imports.ScreenToClient(WindowHandle, ref point);

                var windowPos = new Imports.RECT();
                Imports.GetWindowRect(WindowHandle, out windowPos);
                var origin = new Imports.POINT();
                Imports.ClientToScreen(WindowHandle, ref origin);

                rect.Offset(point.X, point.Y);
                rect.Offset(-windowPos.Left, -windowPos.Top);
                rect.Offset(origin.X, origin.Y);

                var img = bmp.Clone(rect, PixelFormat.Format32bppArgb);
                return img;
            }
            else
            {
                var bmp = new Bitmap(rect.Width, rect.Height);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(new Point(rect.Left, rect.Top), Point.Empty, rect.Size);
                }
                return bmp;
            }
        }

        /// <summary>
        /// Tries to get the current amount of money from the screen. Is slow.
        /// </summary>
        /// <returns></returns>
        public static double GetMoney()
        {
            Size s = MoneyArea.Size;
            double money = -1;

            using (Bitmap bitmap = GetImage(MoneyArea))
            {
                IEnumerable<Line> lines = OCREngine.OCRBitmap(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), new Color[] {
                    Color.FromArgb(255, 254, 254, 254),
                    Color.FromArgb(255, 254, 254, 253)
                });

                using (LockBitmap lb = new LockBitmap(bitmap))
                {
                    if (lines.Count() != 0)
                    {
                        Rectangle playableArea = GameEngine.GetPlayableArea();
                        lines.First().DoOcr(lb, playableArea.Height * playableArea.Width);
                        try
                        {
                            money = Convert.ToDouble(lines.First().OcrString);
                        }
                        catch (Exception)
                        {
                            // ignore
                        }
                    }
                }

                return money;
            }
        }

        /// <summary>
        /// Tries to parse all heroes on screen. Is null if there is crap on the screen preventing the heros from being parsed. Is very slow.
        /// </summary>
        /// <returns></returns>
        public static ParsedHeroes GetHeroes()
        {
            Size s = HeroesArea.Size;

            using (Bitmap bitmap = GetImage(HeroesArea))
            {
                List<Line> lines = OCREngine.OCRBitmap(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), new Color[] {
                        Color.FromArgb(255, 254, 254, 254),
                        Color.FromArgb(255, 254, 254, 253),
                        Color.FromArgb(255, 102, 51, 204), // purple for gilded heroes
                    });

                using (LockBitmap lb = new LockBitmap(bitmap))
                {
                    ParsedHeroes ph = GameEngine.ParseHeroes(lines, lb);
                    if (ph == null)
                    {
                        return null;
                    }

                    foreach (HeroStats hs in ph.HeroStats)
                    {
                        hs.CalculateUpgrades(lb);
                    }

                    return ph;
                }
            }
        }

        /// <summary>
        /// Helper method for GetHeroes
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="lb"></param>
        /// <returns></returns>
        private static ParsedHeroes ParseHeroes(List<Line> lines, LockBitmap lb)
        {
            ParsedHeroes parsedHeroes = new ParsedHeroes();

            if (lines.Count() == 0)
            {
                return null;
            } else if (lines.Count() <= 3)
            {
                // it's cid it's cid it's cid
                // Note: Cid is special because of 2 reasons: when she appears it's the only time in the game
                // there can be less than 3 heroes on screen. Second, she doesn't have a number to represent DPS,
                // so we need to special case some stuff to make OCR work.
                parsedHeroes.HeroStats = new List<HeroStats>();
                parsedHeroes.FirstHeroIndex = 0;
                parsedHeroes.LastHeroIndex = 0;
                HeroStats cid = new HeroStats();
                cid.Hero = GameEngine.HeroList[0];
                cid.Level = 0;
                cid.bottomright.X = lines[0].GetBoundingRectangle().Right;
                cid.bottomright.Y = lines[0].GetBoundingRectangle().Bottom;
                parsedHeroes.HeroStats.Add(cid);

                return parsedHeroes;
            }

            List<Line> widths = new List<Line>();
            List<Line> levels = new List<Line>();
            int i = -1;
            bool rectSkipped = false;
            if (lines[1].GetBoundingRectangle().Top - lines[0].GetBoundingRectangle().Top > HeroSeperatorHeight)
            {
                i = 1;
                rectSkipped = true;
            } else {
                i = 0;
            }

            for (; i < lines.Count(); i+=2)
            {
                widths.Add(lines[i]);
                if (i + 1 < lines.Count())
                {
                    levels.Add(lines[i + 1]);
                }
            }

            // If less than 6 lines -- it's Cid
            if (lines.Count() < 6)
            {
                parsedHeroes.FirstHeroIndex = 0;
                parsedHeroes.LastHeroIndex = lines.Count() / 2;
            }
            else
            {
                int smallestIndex = -1;
                int smallestScore = int.MaxValue;

                for (int j = 0; j + widths.Count() <= HeroList.Count(); j++)
                {
                    int thisScore = 0;
                    for (int k = 0; k < widths.Count(); k++)
                    {
                        thisScore += Math.Abs((int)(HeroList[j + k].Namewidth * PlayableArea.Width - widths[k].GetBoundingRectangle().Width));
                    }
                    if (thisScore < smallestScore)
                    {
                        smallestScore = thisScore;
                        smallestIndex = j;
                    }
                }

                parsedHeroes.FirstHeroIndex = smallestIndex;
                parsedHeroes.LastHeroIndex = smallestIndex + widths.Count() - 1;
            }

            List<HeroStats> heroStats = new List<HeroStats>();
            if (parsedHeroes.FirstHeroIndex == 1 && rectSkipped)
            {
                // oh god, is it cid? why is it always cid
                if (lines[0].GetBoundingRectangle().Left < lb.Width / 2)
                {
                    HeroStats stats = new HeroStats();
                    stats.Hero = HeroList[0];
                    stats.Level = 0;
                    stats.bottomright = new Point(lines[0].GetBoundingRectangle().Right, lines[0].GetBoundingRectangle().Bottom);
                    heroStats.Add(stats);
                    parsedHeroes.FirstHeroIndex = 0;
                }
            }

            for (int j = 0; j < widths.Count(); j++)
            {
                HeroStats stats = new HeroStats();
                stats.Hero = HeroList[parsedHeroes.FirstHeroIndex + j];
                stats.bottomright = new Point(widths[j].GetBoundingRectangle().Right, widths[j].GetBoundingRectangle().Bottom);

                if (j < levels.Count())
                {
                    levels[j].DoOcr(lb, PlayableArea.Width * PlayableArea.Height, false, lb.Width / 2, 3 /* lvl */);
                    if (!int.TryParse(levels[j].OcrString, out stats.Level))
                    {
                        stats.Level = 0;
                    }
                }
                else
                {
                    stats.Level = -1;
                }

                heroStats.Add(stats);
            }
            parsedHeroes.HeroStats = heroStats;
            return parsedHeroes;
        }


        public static void KeyDown(uint keyCode)
        {
            if (WindowHandle != IntPtr.Zero)
            {
                Imports.PostMessage(WindowHandle, Imports.WM_KEYDOWN, (IntPtr)keyCode, IntPtr.Zero);
            }
            else
            {
                Imports.keybd_event((byte)keyCode, 0, 0, 0);
            }
        }

        public static void KeyUp(uint keyCode)
        {
            if (WindowHandle != IntPtr.Zero)
            {
                Imports.PostMessage(WindowHandle, Imports.WM_KEYUP, (IntPtr)keyCode, IntPtr.Zero);
            }
            else
            {
                Imports.keybd_event((byte)keyCode, 0, (int)Imports.KEYEVENTF_KEYUP, 0);
            }
        }

        public static void KeyPress(uint keyCode)
        {
            KeyDown(keyCode);
            KeyUp(keyCode);
        }

        public static void DoClick(Point p)
        {
            if (WindowHandle != IntPtr.Zero)
            {
                var pt = new Imports.POINT() { X = p.X, Y = p.Y };
                Imports.ScreenToClient(WindowHandle, ref pt);
                int coordinates = pt.X | (pt.Y << 16);
                Imports.PostMessage(WindowHandle, Imports.WM_LBUTTONDOWN, (IntPtr)0x1, (IntPtr)coordinates);
                System.Threading.Thread.Sleep(10);
                Imports.PostMessage(WindowHandle, Imports.WM_LBUTTONUP, (IntPtr)0x1, (IntPtr)coordinates);
            }
            else
            {
                Cursor.Position = p;
                Imports.mouse_event(Imports.MOUSEEVENTF_LEFTDOWN | Imports.MOUSEEVENTF_LEFTUP, (uint)p.X, (uint)p.Y, 0, 0);
            }
        }
    }
}
