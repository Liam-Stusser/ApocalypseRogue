# Apocalypse Rogue 2D

This repository contains all the scripts used in my 2D game **Apocalypse Rogue**, which you can play on Unity Play:
[https://play.unity.com/en/games/55e599ed-1472-4426-907d-ff265e23646f/apocalypserogue](https://play.unity.com/en/games/55e599ed-1472-4426-907d-ff265e23646f/apocalypserogue)

---

## Gameplay

### Basics

* Uses **WASD** for movement and **Spacebar** to attack walls or enemies.
* You have a **food counter** at the bottom which acts as your health — if it hits 0, you lose.
* The goal is to **reach the Exit sign** at the far end of the map.
* **Walls** block your path, and **undead creatures** hunt you down.

---

### Food

* Food spawns randomly depending on difficulty.
* **4 food types:**

  * *Small:* Soda, Apple (+10 each)
  * *Medium:* Dead Fish (+20)
  * *Large:* Cooked Chicken (+30)
* Taking damage, moving, or bumping into walls **reduces food**, so plan your moves carefully.

---

### Walls

* Spawn randomly, covering parts of the board based on difficulty.
* Bumping into a wall **reduces its health**, slightly **stuns** you, and advances the turn counter.
* You can also **attack** walls directly with your weapon.

---

### Enemies

* **3 types:** Weak Zombies, Tough Zombies, and Mummies.
* They **actively hunt** you down and can even **break walls** to reach you.
* Sometimes they get lazy and just stand there.
* Stats:

  * Weak Zombie → 1 dmg / 4 HP
  * Tough Zombie → 2 dmg / 6 HP
  * Mummy → 10 dmg / 3 HP

---

### Difficulty

* **Level 1** is mostly fixed except for food and enemy spawn locations.
* Difficulty scales as you progress:

  * *Easy:* Levels 1–10
  * *Medium:* Levels 11–20
  * *Hard:* Levels 21+
* Maps get **larger**, enemies become **stronger**, and **better food** spawns.

---

## Making the game:

  This game follows a tutorial on Unity Learn which starts off pretty simple and is not too challenging (if you have already made some projects on Unity), and walks you through making all the basic components of the game
then leaves polishing up to you. It's very important to note though that the tutorial is based on grid movement and essentially moving your piece one square at a time like moving a chess piece then the enemy goes, and 
the whole game is built/structred around this idea. Where I went wrong is I decided I wanted movement to be fluid and based on holding the key down then using floats and Time.DeltaTime to smooth out the movement.
This implementation breaks sooo many core features the tutorial is based on and made this project ALOT harder than it needed to be for me, once I got to around the walls and enemies part of the tutorial I eventually
just stopped following it and went free form, which brings us here.

---

# Movement

I first knew I was going to have problems when in the movement part of the tutorial after I had implemented my "smoother" movement it said we are now going to create a turn system, which increments when the player presses
and releases the move key, then shifts the player to that cell. This then calls a hasMoved boolean in the tutorial so the player does not keep moving on one key press, which is the exact opposite of my "smooth"
movement based system. My whole movement is based on holding the key down and running around happy as can be, so I had to get creative and add a way to detect when the player changes cells to increment my turn system,
which brought me to my first deviation from the tutorial. You can see in my PlayerMovement script we keep track of the players current cell then check to see if currentCell does not equal our newCell, if it changed we
increment turn counter with a Tick() (more on the Tick() system later) then update currentCell, simple enough.

---

# Walls

Adding food, UI elements, Cell objects, all went pretty alright nothing too bad there but then we get into walls which is based, as you might have gussed... on movement. Since the players movement was always updating in my
version, rather than just move player to the center of this cell. I had an issue with the player touching a wall once and that wall taking 1 damage, at 60 frames a second haha. Now I had to implement a whole new way to make
sure the walls can only take 1 damage each turn while the player can still move "smoothly". My new idea was to make a stun feature which stuns the player and knocks them back a little bit, which then introduced more bugs,
which is the theme of this whole project. To first detect a wall I needed to essentially check what cell the player wanted to move in and see if it was a wall, if you look in playerController you will notice a targetCell
variable in the movePlayer function. Then if it is a wall we call the stun method of player in the wallObject script and "bounce" the player back. Worked great at first, until the walls started knocking the player 
out of bounds (OOB) essentially breaking the game. Now I had to go and fix this bug which was a nightmare and had its own set of issues/bugs but I finally got it all working perfectly and felt like nothing could stop me.

---

# Enemies

Well I had no idea what I was in for as I moved onto the enemy part I slowly started to realize that I screwed myself. About 1 hour into the tutorial for enemies I threw the tutorial out the window and spen the next I think
week or little over a week just making/mostly debugging the enemy script. Rather than use Unitys built in AI tools for pathing, and obsticles I was excited to use my Data Structures and Algorithims knowledge I learned
and finally put it to the test. I was actually excited to finally use Dijkstra's algorithim since I was essentially working with an adjacency matrix graph, so I busted out my notebook read through my notes and slowly
realized Dijkstra is for WEIGHTED graphs. Which meant I just had to use a classic greedy BFS search to hunt the player down and did end up actually being my favorite part of this whole project as I got to use quite a few
data structures to make this pathing algorithim. A BFS search involves using a Queue (in graphs at least), then a hashset to store visited nodes, and finally a hashmap to store the route you are building to the player, and
then building the path by reversing the order of your hashmap in a new array.

---

#Turn System

With my pathing algorithim finished I just now had to account for the enemy potentially getting stuck or boxed in by walls, so I added another findPlayer method which builds a path including tiles that are walls, then in
the enemies turn we check to see if he can move, if not we find the nearest wall and attack it, otherwise theres an error and we just tell the enemy to stand there. Which brings me to the turn system, once I got to adding 
multiple enemies I realized (after alot of debugging and trying different things) that the tutorials Tick() system was just not going to work. So my last about 3 days involved overhauling the turn system to have the enemies
store an internal count so each individual enemy on the board can move, attack, or be confused by a wall, while still having the player be able to walk around and do his stuff. This essentially meant I had to refactor the
TurnManager, GameManager, PlayerController, and EnemyOnject just loads of fun.

---

In the end I got it all done, maybe a few little bugs here and there with some audio components but I happy with where it is so enjoy the game.
