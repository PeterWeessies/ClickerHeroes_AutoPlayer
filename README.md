# ClickerHeroes AutoPlayer
An autoplayer for the game Clicker Heroes (http://clickerheroes.com). It can autoclick, activate skills, buy heroes and upgrades, and ascend, and start all over.
This app is not tested with any other version of clicker heroes like Kongregate. Turn on show individual hero dps, and always use scientific notation when using this tool.

## Getting started
The first thing to do is setting the location of your clicker heroes game:

1. Go to Setting -> Clicker Heroes Position
2. Press Set
3. Hover your mouse over the highlighted corners, starting with top left, and press spacebar
4. Continue until all corners are set, and you get an confirmation it is saved.

Or if you are running a Windows client (e.g. Steam)

2. Press Detect. We'll ask Windows to find the right area for us.

## Setting the task list
The most important thing of this application is the task list. Using this task list you can specify what you want it to do.
The main command in here is [hero id], [amount], [upgrades], [wait], [verify] for example 27, 100, 3, false, true

| Variable  | Explanation                                                                                                       |
| --------- | ----------------------------------------------------------------------------------------------------------------- |
|[hero id]  | Id of the hero, cid = 0, treebeast = 1, ivan = 2 etc                                                              |
|[amount]   | The minimum amount of levels you want to buy                                                                      |
|[upgrades] | -1 = buy no upgrades, 0 = buy 1st, 1 = buy 1st and 2nd etc                                                        |
|[wait]     | Set to true if you want to wait untill you have enough money to buy all specified levels at once (default: false) |
|[verify]   | Set verify to true if you want to verify the hero's level (will be slower but will prevent under-leveling heroes) |

There are also some special commands you can use:

| Command       | Explanation                                                 |
| ------------- | ----------------------------------------------------------- |
|//             | Starting a line with // you can make a comment              |
|Idle           | Play Idle (this is default) and do not click and use skills |
|Active         | Start playing active, autoclick and use skills              |
|BuyAllUpgrades | Press the buy all upgrades button                           |
|Ascend         | Ascend, click a candy and play idle                         |
|ReloadBrowser  | Reload the browsers game window (F5)                        |

## Other Options
| Option          | Explanation                                                  |
| --------------- | ------------------------------------------------------------ |
|Follow tasklist  | Follow the tasklist (turn of if you only want to autoclick)  |
|Auto skill usage | Automaticly use skills (when active, or tasklist turned off) |
|Auto clicking    | Autoclick monsters (when active, or tasklist turned off)     |
|Log output       | Logs the output, and make screenshots in the logs folder     |
|Dogcog level     | Level of your Dogcog ancient                                 |

## Special Thanks
This project is originally started by chthrowaway3 on reddit thread https://www.reddit.com/r/ClickerHeroes/comments/2otl3d/autoplayer_update/


## Sources
This project was forked from:
https://github.com/PeterWeessies/ClickerHeroes_AutoPlayer

And was updated to implement the changes found on:
https://github.com/Lutzy/CHAutoPlayer

Then I added a couple of features like the caching of hero levels and updating the  styling of the dialog.
