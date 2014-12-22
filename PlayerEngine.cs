using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace clickerheroes.autoplayer
{
    /// <summary>
    /// Buttons which may be pressed down when the mouse is clicked
    /// </summary>
    enum Modifiers
    {
        NONE = 0,
        CTRL,
        SHIFT,
        Z
    }

    /// <summary>
    /// An action (click) to be taken, defined as a point on screen and any key modifiers
    /// </summary>
    class Action
    {
        public Point p;
        public Modifiers modifiers;

        public Action(Point pt, Modifiers mod)
        {
            p = pt;
            modifiers = mod;
        }
    }

    /// <summary>
    /// A special task, which will ascend. While ascending, the CTRL-SHIFT-D shutoff will be temporarily disabled.
    /// See documentation for Task
    /// </summary>
    class AscendTask : Task
    {
        public AscendTask() : base(-1, -1, -1, false)
        {
        }
    }

     /// <summary>
    /// A special task, which will buy all upgrades
    /// </summary>
    class BuyAllTask : Task
    {
        public BuyAllTask() : base(-1, -1, -1, false)
        {
        }
    }

    /// <summary>
    /// Due to latency issues, the player may periodically over-level or under-level heroes (will happen when system is having
    /// perf issues and doesn't update the game or player fast enough). Over-leveling is generally not an issue -- it is just a waste
    /// of some money. However under-leveling can be very serious, if many levels are skipped. A verify task is a special type of task
    /// that will wait for 5 seconds and validate that the correct level and upgrades are bought, to prevent under-leveling.
    /// 
    /// See documentation for Task
    /// </summary>
    class VerifyTask : Task
    {
        public int verifyCount;
        public VerifyTask(int heroIndex, int level, int upgrade, bool wait = false)
            : base(heroIndex, level, upgrade, wait)
        {
            verifyCount = 0;
        }
    }

    /// <summary>
    /// A task represents something for the game to do. Each task consists of a specified hero, a level to raise that hero to,
    /// and some number of upgrades to purchase for that hero.
    /// </summary>
    class Task
    {
        /// <summary>
        /// The index of the hero to level/upgrade
        /// </summary>
        public int HeroIndex;

        /// <summary>
        /// The level to bring the hero to. If the hero is already at or above this level, nothing will happen
        /// </summary>
        public int Level;

        /// <summary>
        /// The highest level upgrade to puchase (everything lower level will also be purchased). Set to -1 if
        /// trying to level only, with no upgrade.
        /// </summary>
        public int Upgrade;

        /// <summary>
        /// If true, the auto-player will wait until the exact level can be reached before leveling anything at all
        /// </summary>
        public bool Wait;

        public Task(int heroIndex, int level, int upgrade, bool wait = false) {
            HeroIndex = heroIndex;
            Level = level;
            Upgrade = upgrade;
            Wait = wait;
        }
    }

    /// <summary>
    /// This is the main driving class of the player. It uses GameEngine to read the current game state and queues up
    /// actions for the auto-clicker to perform.
    /// </summary>
    class PlayerEngine
    {
        private static List<Task> Tasks = new List<Task>();

        public static string ParseTasklist(string s)
        {
            string[] tasks = s.Split(new char[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
            foreach (string str in tasks)
            {
                if (str.Trim().StartsWith("//"))
                {
                    continue;
                }

                if (str.Trim().Equals("Ascend"))
                {
                    Tasks.Add(new Task(19, 150, -1, false));
                    Tasks.Add(new AscendTask());
                    continue;
                }

                if (str.Trim().Equals("BuyAllUpgrades"))
                {
                    Tasks.Add(new BuyAllTask());
                    continue;
                }

                string[] args = str.Split(',');
                if (args.Count() != 3 && args.Count() != 4 && args.Count() != 5)
                {
                    return string.Format("Wrong number of args for task, expected 3, 4, or 5: {0}", str);
                }

                int hindex;
                int level;
                int upgrades;
                bool wait;
                bool verify;
                if (!int.TryParse(args[0], out hindex))
                {
                    return string.Format("Unrecognized hero index {0} for task {1}", args[0], str);
                }

                if (!int.TryParse(args[1], out level))
                {
                    return string.Format("Unrecognized hero level {0} for task {1}", args[1], str);
                }

                if (!int.TryParse(args[2], out upgrades))
                {
                    return string.Format("Unrecognized hero upgrades {0} for task {1}", args[2], str);
                }

                if (args.Count() < 4 || string.Equals(args[3].Trim(), "false", StringComparison.InvariantCultureIgnoreCase)) {
                    wait = false;
                }
                else if (string.Equals(args[3].Trim(), "true", StringComparison.InvariantCultureIgnoreCase))
                {
                    wait = true;
                } else {
                    return string.Format("Expected \"false\" or \"true\" for wait parameter of task: {0}", str);
                }

                if (args.Count() < 5 || string.Equals(args[4].Trim(), "false", StringComparison.InvariantCultureIgnoreCase))
                {
                    verify = false;
                }
                else if (string.Equals(args[4].Trim(), "true", StringComparison.InvariantCultureIgnoreCase))
                {
                    verify = true;
                } else {
                    return string.Format("Expected \"false\" or \"true\" for verify parameter of task: {0}", str);
                }

                if (verify)
                {
                    Tasks.Add(new VerifyTask(hindex, level, upgrades, wait));
                } else {
                    Tasks.Add(new Task(hindex, level, upgrades, wait));
                }
            }

            return null;
        }

        /// <summary>
        /// This is a shared queue across reader and writer threads that queues up any non-monster click actions to perform
        /// </summary>
        static Queue<Action> SpecialActionQueue = new Queue<Action>();

        /// <summary>
        /// Mutex to protect access to the special action queue
        /// </summary>
        static Mutex SpecialActionQueueMutex = new Mutex();

        /// <summary>
        /// For perf reasons, we also use a shared int across reader/writer threads, which is updated using Interlocked operations
        /// </summary>
        static int SpecialActionQueueHasValues = 0;

        /// <summary>
        /// True if the auto-clicker thread should be active. Updated using interlocked operations
        /// </summary>
        static int ThreadActive = 0;

        /// <summary>
        /// Sets the auto-clicker thread state
        /// </summary>
        /// <param name="active"></param>
        public static void SetThreadActive(int active)
        {
            Interlocked.Exchange(ref ThreadActive, active);
        }

        /// <summary>
        /// Adds an action to the auto-clicker queue
        /// </summary>
        /// <param name="a">action</param>
        /// <param name="count">The number of times for the action to be added</param>
        public static void AddAction(Action a, int count = 1)
        {
            if (SpecialActionQueueMutex.WaitOne())
            {
                for (int i = 0; i < count; i++)
                {
                    SpecialActionQueue.Enqueue(a);
                }
            }
            Interlocked.Increment(ref SpecialActionQueueHasValues);
            SpecialActionQueueMutex.ReleaseMutex();
        }

        /// <summary>
        /// Tries to level a given hero to a desired level, if there is enough money.
        /// </summary>
        /// <param name="heroIndex">The hero to level</param>
        /// <param name="desiredLevel">The level desired</param>
        /// <param name="currentMoney">The current amount of money</param>
        /// <returns>True if and only if the hero is currently already at that level</returns>
        public static bool TryLevelHero(ParsedHeroes ph, int heroIndex, int desiredLevel, double currentMoney, bool wait)
        {
            if (ph.FirstHeroIndex > heroIndex)
            {
                AddAction(new Action(GameEngine.GetScrollbarUpPoint(), 0), 3);
                return false;
            }
            else if (ph.LastHeroIndex < heroIndex)
            {
                AddAction(new Action(GameEngine.GetScrollbarDownPoint(), 0), 3);
                return false;
            }

            int adjustedIndex = heroIndex - ph.FirstHeroIndex;
            HeroStats hs = ph.HeroStats[adjustedIndex];
            if (hs.Level == -1)
            {
                AddAction(new Action(GameEngine.GetScrollbarDownPoint(), 0), 3);
                return false;
            }

            int heroLevel = hs.Level;
            if (heroLevel >= desiredLevel)
            {
                return true;
            }

            if (wait && hs.Hero.GetCostToLevel(desiredLevel, heroLevel) > currentMoney)
            {
                return false;
            }

            Point pt;
            if (hs.GetBuyButton(out pt))
            {
                while (hs.Hero.GetCostToLevel(heroLevel + 1, heroLevel) < currentMoney && desiredLevel > heroLevel)
                {

                    if (desiredLevel - heroLevel >= 100 && hs.Hero.GetCostToLevel(heroLevel + 100, heroLevel) < currentMoney)
                    {
                        AddAction(new Action(pt, Modifiers.CTRL));
                        currentMoney -= hs.Hero.GetCostToLevel(heroLevel + 100, heroLevel);
                        heroLevel += 100;
                    }
                    else if (desiredLevel - heroLevel >= 25 && hs.Hero.GetCostToLevel(heroLevel + 25, heroLevel) < currentMoney)
                    {
                        AddAction(new Action(pt, Modifiers.Z));
                        currentMoney -= hs.Hero.GetCostToLevel(heroLevel + 25, heroLevel);
                        heroLevel += 25;
                    }
                    else if (desiredLevel - heroLevel >= 10 && hs.Hero.GetCostToLevel(heroLevel + 10, heroLevel) < currentMoney)
                    {
                        AddAction(new Action(pt, Modifiers.SHIFT));
                        currentMoney -= hs.Hero.GetCostToLevel(heroLevel + 10, heroLevel);
                        heroLevel += 10;
                    }
                    else
                    {
                        AddAction(new Action(pt, 0));
                        currentMoney -= hs.Hero.GetCostToLevel(heroLevel + 1, heroLevel);
                        heroLevel++;
                    }
                }

                return false;
            }
            else
            {
                AddAction(new Action(GameEngine.GetScrollbarDownPoint(), 0), 3);
                return false;
            }
        }

        /// <summary>
        /// Tries to buy a given hero upgrade, if there is enough money.
        /// </summary>
        /// <param name="heroIndex"></param>
        /// <param name="desiredUpgrade"></param>
        /// <param name="currentMoney"></param>
        /// <returns></returns>
        public static bool TryUpgradeHero(ParsedHeroes ph, int heroIndex, int desiredUpgrade, double currentMoney)
        {
            if (ph.FirstHeroIndex > heroIndex)
            {
                AddAction(new Action(GameEngine.GetScrollbarUpPoint(), 0), 3);
                return false;
            }
            else if (ph.LastHeroIndex < heroIndex)
            {
                AddAction(new Action(GameEngine.GetScrollbarDownPoint(), 0), 3);
                return false;
            }

            int adjustedIndex = heroIndex - ph.FirstHeroIndex;
            HeroStats hs = ph.HeroStats[adjustedIndex];
            int upgradeStatus = hs.HasUpgrade(desiredUpgrade);
            if (upgradeStatus == 1)
            {
                return true;
            }
            else if (upgradeStatus == -1)
            {
                AddAction(new Action(GameEngine.GetScrollbarDownPoint(), 0), 3);
                return false;
            }
            else
            {
                Point upgradeButton;
                if (hs.Hero.UpgradeCosts[desiredUpgrade] < currentMoney && hs.GetUpgradeButton(out upgradeButton, desiredUpgrade))
                {
                    AddAction(new Action(upgradeButton, 0));
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Main method of the auto-clicking thread
        /// </summary>
        public static void ClickThread()
        {
            while (true)
            {
                Thread.Sleep(20);
                if (ThreadActive == 0)
                {
                    continue;
                }

                // fast check
                if (SpecialActionQueueHasValues != 0)
                {
                    if (SpecialActionQueueMutex.WaitOne())
                    {
                        while (SpecialActionQueue.Count() != 0)
                        {
                            Action nextAction = SpecialActionQueue.Dequeue();
                            // modifiers
                            Cursor.Position = nextAction.p;

                            switch (nextAction.modifiers)
                            {
                                case Modifiers.CTRL:
                                    Imports.keybd_event((byte)Imports.VK_CONTROL, 0, 0, 0);
                                    break;
                                case Modifiers.SHIFT:
                                    Imports.keybd_event((byte)Imports.VK_SHIFT, 0, 0, 0);
                                    break;
                                case Modifiers.Z:
                                    Imports.keybd_event((byte)Imports.VK_Z, 0, 0, 0);
                                    break;
                                default:
                                    break;
                            }

                            Imports.mouse_event(Imports.MOUSEEVENTF_LEFTDOWN | Imports.MOUSEEVENTF_LEFTUP, (uint)nextAction.p.X, (uint)nextAction.p.Y, 0, 0);

                            switch (nextAction.modifiers)
                            {
                                case Modifiers.CTRL:
                                    Imports.keybd_event((byte)Imports.VK_CONTROL, 0, (int)Imports.KEYEVENTF_KEYUP, 0);
                                    break;
                                case Modifiers.SHIFT:
                                    Imports.keybd_event((byte)Imports.VK_SHIFT, 0, (int)Imports.KEYEVENTF_KEYUP, 0);
                                    break;
                                case Modifiers.Z:
                                    Imports.keybd_event((byte)Imports.VK_Z, 0, (int)Imports.KEYEVENTF_KEYUP, 0);
                                    break;
                                default:
                                    break;
                            }

                            Thread.Sleep(20);
                        }
                        SpecialActionQueueHasValues = 0;
                    }
                    SpecialActionQueueMutex.ReleaseMutex();
                }
                else
                {
                    Cursor.Position = GameEngine.GetClickArea();

                    Imports.mouse_event(Imports.MOUSEEVENTF_LEFTDOWN | Imports.MOUSEEVENTF_LEFTUP, (uint)GameEngine.GetClickArea().X, (uint)GameEngine.GetClickArea().Y, 0, 0);
                }
            }
        }

        /// <summary>
        /// Sends a keypress
        /// </summary>
        /// <param name="keycode"></param>
        public static void PressKey(uint keycode)
        {
            Imports.keybd_event((byte)keycode, 0, 0, 0);
            Imports.keybd_event((byte)keycode, 0, (int)Imports.KEYEVENTF_KEYUP, 0);
        }

        private static int nextTaskToPerform = 0;

        /// <summary>
        /// The main function which drives autoplaying. It will iterate through the task list sequentially,
        /// performing each task as necessary. When it reaches the Ascend task, it will restart from the beginning.
        /// </summary>
        /// <param name="ph">The parsed heroes obtained from the GameEngine</param>
        /// <param name="curMoney">The current money</param>
        /// <returns>A string giving a human-readable representation of the next task</returns>
        public static string TryNextTask(ParsedHeroes ph, double curMoney)
        {
            if (nextTaskToPerform >= Tasks.Count())
            {
                return string.Empty;
            }

            Task nextTask = Tasks[nextTaskToPerform];

            // Iris -- after ascending, we may potentially jump many levels into the game, but we don't have any money to buy any heroes.
            // To get around this, we do what human players do. We click candies when (and only when) we're on the very first task (aka
            // immediately after we've ascended).
            if (nextTaskToPerform == 0)
            {
                Point[] pts = GameEngine.GetCandyButtons();
                foreach (Point p in pts)
                {
                    AddAction(new Action(p, Modifiers.NONE));
                }
            }

            if (nextTask is AscendTask)
            {
                Ascend(ph, curMoney);
                nextTaskToPerform = 0;
                return "Ascending";
            }

            if (nextTask is BuyAllTask)
            {
                BuyAllUpgrades();
                nextTaskToPerform++;
                return "Buying all upgrades";
            }

            VerifyTask vt = nextTask as VerifyTask;

            string retStr = string.Empty;
            if (!TryLevelHero(ph, nextTask.HeroIndex, nextTask.Level, curMoney, nextTask.Wait))
            {
                if (vt != null)
                {
                    vt.verifyCount = 0;
                }
                retStr = string.Format("Level {0} to {1}", GameEngine.HeroList[nextTask.HeroIndex].Name, nextTask.Level);
                return retStr;
            }

            if (nextTask.Upgrade != -1)
            {
                for (int i = 0; i <= nextTask.Upgrade; i++)
                {
                    if (!TryUpgradeHero(ph, nextTask.HeroIndex, i, curMoney))
                    {
                        if (vt != null)
                        {
                            vt.verifyCount = 0;
                        }
                        retStr = string.Format("Get upgrade {0} for {1}", nextTask.Upgrade + 1, GameEngine.HeroList[nextTask.HeroIndex].Name);
                        return retStr;
                    }
                }
            }

            if (vt != null)
            {
                vt.verifyCount++;
                if (vt.verifyCount == 5)
                {
                    nextTaskToPerform++;
                    vt.verifyCount = 0;
                    return TryNextTask(ph, curMoney);
                }
                return string.Format("Verify that {0} is properly leveled and upgraded", GameEngine.HeroList[nextTask.HeroIndex].Name);
            }

            nextTaskToPerform++;
            return TryNextTask(ph, curMoney);
        }

        /// <summary>
        /// Don't leave this method until after we've ascended
        /// </summary>
        /// <param name="ph"></param>
        /// <param name="curMoney"></param>
        public static void Ascend(ParsedHeroes ph, double curMoney)
        {
            Point AscendButton = GameEngine.GetAscendButton();

            // use candy height and width cuz it's close enough
            int candyHeight = GameEngine.GetCandyHeight();
            int candyWidth = GameEngine.GetCandyWidth();
            Rectangle c = new Rectangle(AscendButton.X - candyWidth / 2, AscendButton.Y - candyHeight / 2, candyWidth, candyHeight);

            while (true)
            {
                using (Bitmap bitmap = new Bitmap(c.Width, c.Height))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(new Point(c.Left, c.Top), Point.Empty, c.Size);
                    }

                    if (OCREngine.GetBlobDensity(bitmap, new Rectangle(0, 0, bitmap.Width - 1, bitmap.Height - 1), new Color[] {
                        Color.FromArgb(68, 215, 35)
                    }) > 0.10)
                    {
                        AddAction(new Action(AscendButton, 0));
                        Thread.Sleep(1000);
                        return;
                    }
                }

                TryUpgradeHero(ph, 19, 3, curMoney);
                Thread.Sleep(1000);
                ph = GameEngine.GetHeroes();
                curMoney = GameEngine.GetMoney();
            }
        }

        /// <summary>
        /// Buy All upgrades
        /// </summary>
        public static void BuyAllUpgrades()
        {
            AddAction(new Action(GameEngine.GetScrollbarUpPoint(), 0), 6);
            AddAction(new Action(GameEngine.GetScrollbarDownPoint(), 0), 100);
            AddAction(new Action(GameEngine.GetBuyAllButton(), 0), 3);
        }

    }
}
