using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clickerheroes.autoplayer
{
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
        /// The width of the hero's name on screen, as a proportion to the total play screen width
        /// </summary>
        public double Namewidth;

        /// <summary>
        /// An array with the costs of each upgrade.
        /// </summary>
        public double[] UpgradeCosts;

        public Hero(string name, double basecost, double namewidth, double[] upgradeCosts)
        {
            Name = name;
            Basecost = basecost;
            Namewidth = namewidth;
            UpgradeCosts = upgradeCosts;
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

                double d = OCREngine.GetBlobDensity(lb, r, new Color[] { Color.FromArgb(39, 166, 10) });
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
        /// <summary>
        ///  The heroes!
        /// </summary>
        static public Hero[] HeroList = {
            new Hero("Cid, the Helpful Adventurer", 5, 0.1904, new double[] { 100, 250, 1000, 8000, 80000, 4E5, 4E6 }),
            new Hero("Treebeast", 50, 0.0664, new double[] { 500, 1250, 5000, 40000, 4E5 }),
            new Hero("Ivan, the Drunken Brawler", 250, 0.1807, new double[] { 2500, 6250, 25000, 2E5, 2E6, 1E7 }),
            new Hero("Brittany, Beach Princess", 1000, 0.1630, new double[] { 10000, 25000, 1E5, 8E5 }),
            new Hero("The Wandering Fisherman", 4000, 0.1780, new double[] { 40000, 1E5, 4E5, 3.2E6, 3.2E7 }),
            new Hero("Betty Clicker", 20000, 0.0859, new double[] { 2E5, 5E5, 2E6, 1.6E7, 1.6E8 }),
            new Hero("The Masked Samurai", 100000, 0.1399, new double[] { 1E6, 2.5E6, 1E7, 8E7 }),
            new Hero("Leon", 400000, 0.0301, new double[] { 4E6, 1E7, 4E7, 3.2E8 }),
            new Hero("The Great Forest Seer", 2500000, 0.1444, new double[] { 2.5E7, 6.25E7, 2.5E8, 2E9 }),
            new Hero("Alexa, Assassin", 15000000, 0.1045, new double[] {1.5E8, 3.75E8, 1.5E9, 1.2E10, 1.2E11 }),
            new Hero("Natalia, Ice Apprentice", 100000000, 0.1550, new double[] { 1E9, 2.5E9, 1E10, 8E10 }),
            new Hero("Mercedes, Duchess of Blades", 800000000, 0.1966, new double[] { 8E9, 2E10, 8E10, 6.4E11, 6.4E12 }),
            new Hero("Bobby, Bounty Hunter", 6.5E9, 0.1515, new double[] { 6.5E10, 1.625E11, 6.5E11, 5.2E12, 5.2E13 }),
            new Hero("Broyle Lindeoven, Fire Mage", 5.0E10, 0.1949, new double[] { 5E11, 1.25E12, 5E12, 4E13, 4E14 }),
            new Hero("Sir George II, King's Guard", 4.50E11, 0.1807, new double[] { 4.5E12, 1.125E13, 4.5E13, 3.6E14, 3.6E15 }),
            new Hero("King Midas", 4E12, 0.0753, new double[] { 4E13, 1E14, 4E14, 3.2E15, 3.2E16, 1.6E17 }),
            new Hero("Referi Jerator, Ice Wizard", 3.6E13, 0.1727, new double[] { 3.6E14, 9E14, 3.6E15, 2.88E16, 2.88E17 }),
            new Hero("Abaddon", 3.2E14, 0.0602, new double[] { 3.2E15, 8E15, 3.2E16, 2.560E17 }),
            new Hero("Ma Zhu", 2.7E15, 0.0478, new double[] { 2.7E16, 6.75E16, 2.7E17, 2.16E18 }),
            new Hero("Amenhotep", 2.4E16, 0.0788, new double[] { 2.4E17, 6E17, 2.4E18, 1.92E19 }),
            new Hero("Beastlord", 3.0E17, 0.0629, new double[] { 3E18, 7.5E18, 3E19, 2.4E20, 2.4E21 }),
            new Hero("Athena, Goddess of War", 9.0E18, 0.1639, new double[] { 9E19, 2.25E20, 9E20, 7.2E21 }),
            new Hero("Aphrodite, Goddess of Love", 3.5E20, 0.1895, new double[] { 3.5E21, 8.75E21, 3.5E22, 2.8E23, 2.8E24 }),
            new Hero("Shinatobe, Wind Deity", 1.4E22, 0.1541, new double[] { 1.4E23, 3.5E23, 1.4E24, 1.12E25, 1.12E26 }),
            new Hero("Grant, the General", 4.199E24, 0.1267, new double[] { 4.199E25, 1.049E26, 4.199E26, 3.359E27 }),
            new Hero("Frostleaf", 2.1E27, 0.0585, new double[] { 2.1E28, 5.249E28, 2.099E29, 1.679E30 }),
            new Hero("Dread Knight", 1.000E40, 0.0895, new double[] { 1E41, 2.5E41, 1E42, 8E42 }),
            new Hero("Atlas", 1.000E55, 0.0324, new double[] { 1E56, 2.5E56, 1.0E57, 8.0E57 }),
            new Hero("Terra", 1.000E70, 0.0324, new double[] { 1E71, 2.5E71, 1.0E72, 8E72 }),
            new Hero("Phtalo", 1.0E85, 0.0506, new double[] { 1E86, 2.5E86, 1E87, 8E87 }),
            new Hero("Orentchya Gladeye, Didensy Banana", 1.0E100, 0.1850, new double[] { 1E101, 2.5E101, 1E102, 8E102 }),
            new Hero("Lilin", 1.0E115, 0.0275, new double[] { 1.0E116, 2.5E116, 1E117, 8E117 }),
            new Hero("Cadmia", 1.0E130, 0.0504, new double[] { 1.0E131, 2.5E131, 1E132, 8E132 }),
            new Hero("Alabaster", 1.0E145, 0.0662, new double[] { 1.0E146, 2.5E146, 1E147, 8E147 }),
            new Hero("Astrea", 1.0E160, 0.0515, new double[] { 1.0E161, 2.5E161, 1E162, 8E162 }),
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
        /// The hero cost multiplier, which can be between 0.5 and 1.0 depending on Dogcog level.
        /// </summary>
        static double HeroDiscount = 1.0;

        /// <summary>
        /// The location of the progress/farm mode button
        /// </summary>
        static private Point ProgressButton;
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
        #endregion

        /// <summary>
        /// Defines the game play area, and calculates all other offsets from that
        /// </summary>
        /// <param name="playableArea"></param>
        public static void SetPlayableArea(Rectangle playableArea) {
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
            ProgressButton.Y = (int)(PlayableArea.Height * 0.3308 + PlayableArea.Top);

            AscendButton.X = (int)(PlayableArea.Width * 0.4344 + PlayableArea.Left);
            AscendButton.Y = (int)(PlayableArea.Height * 0.6654 + PlayableArea.Top);

            BuyAllButton.X = (int)(PlayableArea.Width * 0.4344 + PlayableArea.Left);
            BuyAllButton.Y = (int)(PlayableArea.Height * 0.6654 + PlayableArea.Top);
        }

        /// <summary>
        /// Vallidates the currently set playable area
        /// </summary>
        public static bool ValidatePlayableArea()
        {
            if (GetMoney() > 0)
            {
                return true;
            }
            else if (GameEngine.GetHeroes() != null)
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

                using (Bitmap bitmap = new Bitmap(c.Width, c.Height))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(new Point(c.Left, c.Top), Point.Empty, c.Size);
                    }

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
            using (Bitmap bitmap = new Bitmap(c.Width, c.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(new Point(c.Left, c.Top), Point.Empty, c.Size);
                }

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

        /// <summary>
        /// Tries to get the current amount of money from the screen. Is slow.
        /// </summary>
        /// <returns></returns>
        public static double GetMoney()
        {
            Size s = MoneyArea.Size;
            double money = 0;

            using (Bitmap bitmap = new Bitmap(s.Width, s.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(new Point(MoneyArea.Left, MoneyArea.Top), Point.Empty, s);
                }

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

                return money / 1000;
            }
        }

        /// <summary>
        /// Tries to parse all heroes on screen. Is null if there is crap on the screen preventing the heros from being parsed. Is very slow.
        /// </summary>
        /// <returns></returns>
        public static ParsedHeroes GetHeroes()
        {
            Size s = HeroesArea.Size;

            using (Bitmap bitmap = new Bitmap(s.Width, s.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(new Point(HeroesArea.Left, HeroesArea.Top), Point.Empty, s);
                }

                using (LockBitmap lb = new LockBitmap(bitmap))
                {
                    List<Line> lines = OCREngine.OCRBitmap(lb, new Rectangle(0, 0, bitmap.Width, bitmap.Height), new Color[] {
                        Color.FromArgb(255, 254, 254, 254),
                        Color.FromArgb(255, 254, 254, 253),
                        Color.FromArgb(255, 102, 51, 204), // purple for gilded heroes
                    });

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
    }
}
