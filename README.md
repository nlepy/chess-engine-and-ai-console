My A-Level Computer Science coursework from 2019.

It's not very good but it was a fun project to work on.
The code is a mess and I'm not proud of it, but it works well enough and I learned a lot from it.

The UI is completely in console and isn't very user friendly. In future I would like to make an improved version of this project with a GUI.

Not my best work but certainly a good learning experience. This scored 100%, so I'm fairly pleased with it.

## How it works

First, the user is asked "How many AI?". This determines how many AI players will be playing the game. 0-2 AI players are supported, where 0 is a human vs human game, 1 is a human vs AI game and 2 is an AI vs AI game.

Then, the chess board is printed to the console and the user is prompted to enter a command. Coordinates on the board take the form `a1`, `b2`, `c3` etc. The board is printed as follows:

![board example](https://i.postimg.cc/cLvHJQ3q/board-example.png)

<sub> Not the prettiest, I know. </sub>

There are 3 types of commands. They aren't very intuitive and I would like to improve this in future versions. The commands are:

- pick: Allows the user to select any owned piece on the board and "pick it up". Format is `pick <coordinates>`. The board is then re-printed, highlighting the selected piece in blue, and printing any moveable square in yellow, or if an enemy is contained within the line of sight, they are highlighted in a lighter shade of red. An example screenshot of this in the game is shown below:
    
    ![pick example](https://i.postimg.cc/g210Lnyx/pick-example.png)

    If a valid piece is selected, user is prompted to "enter coordinates to place piece". If an invalid piece is selected, the user is informed that the piece is invalid and is re-prompted to enter a command.

    If valid coordinates are entered, the piece is moved to the new coordinates and the board is re-printed. If invalid coordinates are entered, the user is informed that the move is invalid and is re-prompted to enter a command.

    The user can also choose to place the piece back into place. This does not count as a move and the user is re-prompted to enter a command. This is useful if the user accidentally selects the wrong piece, or changes their mind about the move they want to make.

- info: Displays some info about the selected coordinate (its class and times moved). Format is `info <coordinates>`. Not very useful, but I thought it was cool.

- save: Saves the game to a file. The game was supposed to be loadable from the file using the `load` command, however I ran out of time and didn't get around to implementing this. The file is saved in the install directory and is called `SavedGame.bin`, and is overwritten each time the game is saved.

### The algorithm

Here's the meat and potatoes. The Minimax algorithm, commonly used in two player turn-based games such as chess, is used to determine the best possible move for the (maximising) player.

#### **How it works**

A tree will be created with each child being the coordinates of every possible move from the parent’s coordinates. The algorithm will pick the move resulting in the best possible outcome for the player (maximum possible score), assuming that the opponent (minimising player) is also using the best possible moves (minimum possible score). The algorithm starts at the deepest node to the left and, using recursive backtracking, traverses the tree of moves, returning the best possible outcome. 

Alpha-Beta pruning, an optimisation method, can used to reduce the number of searches. This is done by eliminating branches of the tree which would not yield a better score than what has already been discovered. In this way, the time efficiency of the algorithm is improved, and a deeper search can be performed with the same usage of resources. The pseudocode for the algorithm with alpha-beta pruning implemented is shown below:

```csharp
//initial call
Minimax(currentposition, 3, -9999, 9999, true)
```
```csharp
//base alpha-beta pruning example code
func Minimax(position , depth, alpha, beta, maximisingPlayer)
        {
            IF depth = 0 OR GameIsOver
                RETURN position.BoardEval

            IF maximisingPlayer
            {
                maxEval = -9999
                FOR EACH child of position
                {
                    eval = Minimax(child, depth - 1, alpha, beta, false)
                    maxEval = LargestFrom: maxEval, eval
                    alpha = LargestFrom: alpha, eval
                    IF beta <= alpha
                        BREAK
                }
                RETURN maxEval
            }
            ELSE
            {
                minEval = 9999
                FOR EACH child of position
                {
                    eval = Minimax(child, depth - 1, alpha, beta, true)
                    minEval = SmallestFrom: minEval, eval
                    beta = SmallestFrom: beta, eval
                    IF beta <= alpha
                        BREAK
                }
                RETURN minEval
            }
        }
```
The minimax algorithm is called within another method, DoMinimaxMove. The purpose of this method is to first generate a tree of every possible move. This is done by first creating a root node. The root node of the tree is a blank Node of score 0. Next, the algorithm finds every possible move by the maximising player on the current board, creates a new board in which one of these moves has taken place, creates a node (child) for this move, finds all moves on that board, creates another new board in which one of these moves has taken place, creates another node (child1) for this move, finds all moves on that board, creates a new board on which one of these moves has taken place, and finally creates another node (child2) for this move. Each node contains a list, children, which contains every child of the node. The node (child2) is then added as to the child list of the previous node (child1), this node (child1) is added to the child list of the previous node (child), which is then added to the child list of the root. Next, the algorithm calls the minimax with the root node of the tree, and then do the move that is returned by the algorithm. This is done by using two methods, SetHand and Place. SetHand sets the Game class’ hand value to the position of the piece on the board. Place will then place the piece at the position stored in the hand value to the given value. This is done by copying the piece at the position stored in the hand value to the new position, and then creating a new dead piece in the old position of the piece. The moveCounter attribute contained within the Piece class is then incremented by 1.


The Minimax algorithm checks if the game is over every time at the current node when it is called (base case). This is done using a method, GameIsOver, contained within the Node class. This method creates two Boolean values, trueking and falseking, set to false. Then it searches the Node’s boardData for each of the kings, using typeOf(King). If a king of team true is found, trueking is set to true. If a king of team false is found, falseking is set to true. At the end of the method, if both falseking and trueking are true, the method returns false, else it returns true.

The score value of the move is calculated using a method, BoardEval, contained within the Node class. This method evaluates the board contained within the node class. This is done by first creating a variable to store the total score and setting it to 0, then checking every square on the board. If the piece is alive and of team true, a positive score is added to the total score value. If the piece is alive and of team false, a negative score value is added to the total score value. After every piece has been checked, the total score is returned. These score values are contained within the Piece class. The scores for each piece are shown below:

| Class  | Score |
|--------|-------|
| Piece  | 10    |
| Knight | 30    |
| Bishop | 30    |
| Rook   | 50    |
| Queen  | 90    |
| King   | 900   |

The AI was able to play well on its own after the algorithm had been implemented.
I noticed that, due to the nature of the algorithm, when the AI played against each other and the starting player remained the same, the outcome of the game and the board always ended up being identical. Pretty cool right?

# Takeaways

I had some trouble with the move lists during the implementation of the algorithm. This was because, within the DoMinimaxMove method, as the boards were copied to create a move tree, the Piece objects were not copied, instead references to the objects were used. Therefore, if a piece was taken during the board generation, it would be marked as taken on the main board too. This was fixed by using a method to deep copy each Piece, so that each board’s pieces were different objects to those on other boards. I used a method from stackoverflow.com.

I have learned that it is important to plan out your code before writing it. I had not planned my code out very thoroughly before starting this project, therefore when it came to writing certain methods and algorithms it was much harder to manoeuvre the code to work the way I wanted it to. This was especially prevalent regarding manipulation of the board. If I were to rewrite my code, I would have the board contained within a separate object, BoardManager, allowing me to create copies of the board with ease and manipulate them. This would prove especially useful during the move tree creation process and the minimax algorithm, as I would be able to handle all types of moves much more easily. This would have also been useful when adding additional functionality to the game, such as castling or promotion.

The AI sometimes makes some questionable decisions, especially at the start. This is because a minimax search of depth 3 is not enough for every move to be good. This could be improved by adding piece-square tables. These are tables created for each different piece – the game rewards the AI for placing their pieces in strategic positions, e.g. a rook near the middle of the board is better than a rook in the corner of the board. An example of a piece-square table for a pawn is shown below:

![piece square table](https://cdn-media-1.freecodecamp.org/images/1*iG6FUYZpU0_RKlqHnC8XxA.png)

<sub> Taken from https://www.freecodecamp.org/news/simple-chess-ai-step-by-step-1d55a9266977/ </sub>

Honestly, I regret picking chess. I've never played chess in my life before this and it's been done to death. I should have picked some viking/medieval board game instead! Another time perhaps...