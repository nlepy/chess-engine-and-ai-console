using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

///--------------------------------To do list--------------------------------
/// HIGH PRIORITY
/// - Move tree creation is slow. fix. avoid creating children for pieces which cant move
/// LOW PRIORITY
/// - Allow other pieces to be moved during a check, but only to save the king piece. (simulate turn in game after check is detected, find every move that works to remove check when checkcheck is run again and add to list.)
/// - Add extra moves.
/// -------------------------------------------------------------------------
namespace Chess
{
    public static class ExtensionMethods
    {
        // Deep clone
        public static T DeepClone<T>(this T a)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, a);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }
    }//used to deep copy objects. copied from stackoverflow (https://stackoverflow.com/questions/129389/how-do-you-do-a-deep-copy-of-an-object-in-net)
    [Serializable]
    class Piece
    {
        bool _team; //true = black, false = white
        bool _isDead; //self explanatory
        char _symbol; //char to display on board
        int _moveCounter; //times a piece has moved
        int _points; //points for killing piece

        public bool team { get => _team; set => _team = value; }
        public bool isDead { get => _isDead; set => _isDead = value; }
        public char symbol { get => _symbol; set => _symbol = value; }
        public int moveCounter { get => _moveCounter; set => _moveCounter = value; }
        public int points { get => _points; set => _points = value; }

        public Piece(bool team)
        {
            symbol = 'P';
            points = 10;
            this.team = team;
            isDead = false;
        }
        public Piece()
        {
            symbol = 'P';
            isDead = true;
        }
        public char GetSymbol()
        {
            if (isDead) return 'X';
            else return symbol;
        } //what to print when printing board
        public virtual List<(int, int)> GetMovementList(int x, int y, Piece[,] board)
        {
            List<(int, int)> lineOfSight = new List<(int, int)>();
            for (int boardY = 0; boardY < 8; boardY++)
            {
                for (int boardX = 0; boardX < 8; boardX++)
                {
                    if (boardY == y && boardX == x)
                    {
                        lineOfSight.Add((boardX, boardY));
                    } //position of piece
                    else if (!team) //for bottom
                    {
                        if ((boardY == y + 1 && boardX == x) && board[boardX, boardY].isDead)
                        {
                            lineOfSight.Add((boardX, boardY));
                        } //1 forward if no piece in front
                        else if (boardY - 1 == y && (boardX + 1 == x || boardX - 1 == x) && board[boardX, boardY].team != team)
                        {
                            lineOfSight.Add((boardX, boardY));
                        } //if there is an enemy diagonal forward (kill)
                        else if ((boardY == y + 2 && boardX == x) && board[boardX, boardY].isDead && board[x, y].moveCounter == 0)
                        {
                            lineOfSight.Add((boardX, boardY));
                        } //if you have not moved a pawn it can move 2 forward
                    }
                    else if (team) //for top
                    {
                        if ((boardY == y - 1 && boardX == x) && board[boardX, boardY].isDead)
                        {
                            lineOfSight.Add((boardX, boardY));
                        } //1 forward if no piece in front
                        else if (boardY + 1 == y && (boardX + 1 == x || boardX - 1 == x) && !board[boardX, boardY].team && !board[boardX, boardY].isDead)
                        {
                            lineOfSight.Add((boardX, boardY));
                        } //if there is a living enemy diagonal forward (kill)
                        else if ((boardY == y - 2 && boardX == x) && board[boardX, boardY].isDead && board[x, y].moveCounter == 0)
                        {
                            lineOfSight.Add((boardX, boardY));
                        } //if you have not moved a pawn it can move 2 forward
                    }
                }
            }
            return lineOfSight;
        }
        public virtual List<(int, int)> GetKillMovementList(int x, int y, Piece[,] board)
        {
            List<(int, int)> lineOfSight = new List<(int, int)>();
            if(team)
            {
                lineOfSight.Add((x + 1, y - 1));
                lineOfSight.Add((x - 1, y - 1));
            }//top
            else
            {
                lineOfSight.Add((x + 1, y + 1));
                lineOfSight.Add((x - 1, y + 1));
            }//bottom
            return lineOfSight;
        }
    }
    [Serializable]
    class Rook : Piece
    {
        public Rook(bool team) : base(team)
        {
            symbol = 'R';
            points = 50;
        }
        public Rook() : base()
        {
            symbol = 'R';
        }
        public override List<(int, int)> GetMovementList(int x, int y, Piece[,] board)
        {
            List<(int, int)> lineOfSight = new List<(int, int)>();
            bool notEnd = true;
            for (int i = 0; notEnd; i++) //check positive x path
            {
                try
                {
                    if (i == 0) //if piece
                    {
                        lineOfSight.Add((x, y));
                    }
                    else if (board[x + i, y].team != team && !board[x + i, y].isDead) //if enemy
                    {
                        lineOfSight.Add((x + i, y));
                        notEnd = false;
                    }
                    else if (board[x + i, y].team == team && !board[x + i, y].isDead) //if friendly
                    {
                        notEnd = false;
                    }
                    else lineOfSight.Add((x + i, y));
                }
                catch { notEnd = false; }
            }
            notEnd = true;

            for (int i = 0; notEnd; i++) //check negative x path
            {
                try
                {
                    if (i == 0) //if piece
                    {
                        lineOfSight.Add((x, y));
                    }
                    else if (board[x - i, y].team != team && !board[x - i, y].isDead) //if enemy
                    {
                        lineOfSight.Add((x - i, y));
                        notEnd = false;
                    }
                    else if (board[x - i, y].team == team && !board[x - i, y].isDead) //if friendly
                    {
                        notEnd = false;
                    }
                    else lineOfSight.Add((x - i, y));
                }
                catch { notEnd = false; }
            }
            notEnd = true;

            for (int i = 0; notEnd; i++) //check positive y path
            {
                try
                {
                    if (i == 0) //if piece
                    {
                        lineOfSight.Add((x, y));
                    }
                    else if (board[x, y + i].team != team && !board[x, y + i].isDead) //if enemy
                    {
                        lineOfSight.Add((x, y + i));
                        notEnd = false;
                    }
                    else if (board[x, y + i].team == team && !board[x, y + i].isDead) //if friendly
                    {
                        notEnd = false;
                    }
                    else lineOfSight.Add((x, y + i));
                }
                catch { notEnd = false; }
            }
            notEnd = true;

            for (int i = 0; notEnd; i++) //check negative y path
            {
                try
                {
                    if (i == 0) //if piece
                    {
                        lineOfSight.Add((x, y));
                    }
                    else if (board[x, y - i].team != team && !board[x, y - i].isDead) //if enemy
                    {
                        lineOfSight.Add((x, y - i));
                        notEnd = false;
                    }
                    else if (board[x, y - i].team == team && !board[x, y - i].isDead) //if friendly
                    {
                        notEnd = false;
                    }
                    else lineOfSight.Add((x, y - i));
                }
                catch { notEnd = false; }
            }
            return lineOfSight;
        }
        public override List<(int, int)> GetKillMovementList(int x, int y, Piece[,] board)
        {
            List<(int, int)> lineOfSight = new List<(int, int)>();
            bool notEnd = true;
            for (int i = 1; notEnd; i++) //check positive x path
            {
                try
                {
                    if (!board[x + i, y].isDead) //if piece
                    {
                        lineOfSight.Add((x + i, y));
                        notEnd = false;
                    }
                    else lineOfSight.Add((x + i, y));
                }
                catch { notEnd = false; }
            }
            notEnd = true;

            for (int i = 1; notEnd; i++) //check negative x path
            {
                try
                {
                    if (!board[x - i, y].isDead) //if piece
                    {
                        lineOfSight.Add((x - i, y));
                        notEnd = false;
                    }
                    else lineOfSight.Add((x - i, y));
                }
                catch { notEnd = false; }
            }
            notEnd = true;

            for (int i = 1; notEnd; i++) //check positive y path
            {
                try
                {
                    if (!board[x, y + i].isDead) //if piece
                    {
                        lineOfSight.Add((x, y + i));
                        notEnd = false;
                    }
                    else lineOfSight.Add((x, y + i));
                }
                catch { notEnd = false; }
            }
            notEnd = true;

            for (int i = 1; notEnd; i++) //check negative y path
            {
                try
                {
                    
                    if (!board[x, y - i].isDead) //if piece
                    {
                        lineOfSight.Add((x, y - i));
                        notEnd = false;
                    }
                    else lineOfSight.Add((x, y - i));
                }
                catch { notEnd = false; }
            }
            return lineOfSight;
        }
    }
    [Serializable]
    class Knight : Piece
    {
        public Knight(bool team) : base(team)
        {
            symbol = 'K';
            points = 30;
        }
        public Knight() : base()
        {
            symbol = 'K';
        }
        public override List<(int, int)> GetMovementList(int x, int y, Piece[,] board)
        {
            List<(int, int)> lineOfSight = new List<(int, int)>();
            (int, int)[] knightCoords = { (x + 2, y + 1), (x + 2, y - 1), (x - 2, y + 1), (x - 2, y - 1), (x + 1, y + 2), (x + 1, y - 2), (x - 1, y + 2), (x - 1, y - 2) };
            lineOfSight.Add((x, y));

            foreach ((int, int) coord in knightCoords)
            {
                try
                {
                    if (board[coord.Item1, coord.Item2].isDead) //if empty
                    {
                        lineOfSight.Add(coord);
                    }
                    else if (board[coord.Item1, coord.Item2].team != team && !board[coord.Item1, coord.Item2].isDead) //if enemy
                    {
                        lineOfSight.Add(coord);
                    }
                }
                catch { }
            }
            return lineOfSight;
        }
        public override List<(int, int)> GetKillMovementList(int x, int y, Piece[,] board)
        {
            List<(int, int)> lineOfSight = new List<(int, int)>();
            (int, int)[] knightCoords = { (x + 2, y + 1), (x + 2, y - 1), (x - 2, y + 1), (x - 2, y - 1), (x + 1, y + 2), (x + 1, y - 2), (x - 1, y + 2), (x - 1, y - 2) };

            foreach ((int, int) coord in knightCoords)
            {
                try
                {
                    lineOfSight.Add(coord);
                }
                catch { }
            }
            return lineOfSight;
        }
    }
    [Serializable]
    class Bishop : Piece
    {
        public Bishop(bool team) : base(team)
        {
            symbol = 'B';
            points = 30;
        }
        public Bishop() : base()
        {
            symbol = 'B';
        }
        public override List<(int, int)> GetMovementList(int x, int y, Piece[,] board)
        {
            List<(int, int)> lineOfSight = new List<(int, int)>();
            bool notEnd = true;
            for (int i = 0; notEnd; i++) //check positive positive path
            {
                try
                {
                    if (i == 0) //if piece
                    {
                        lineOfSight.Add((x, y));
                    }
                    else if (board[x + i, y + i].team != team && !board[x + i, y + i].isDead) //if enemy
                    {
                        lineOfSight.Add((x + i, y + i));
                        notEnd = false;
                    }
                    else if (board[x + i, y + i].team == team && !board[x + i, y + i].isDead) //if friendly
                    {
                        notEnd = false;
                    }
                    else lineOfSight.Add((x + i, y + i));
                }
                catch { notEnd = false; }
            }
            notEnd = true;

            for (int i = 0; notEnd; i++) //check positive negative path
            {
                try
                {
                    if (i == 0) //if piece
                    {
                        lineOfSight.Add((x, y));
                    }
                    else if (board[x + i, y - i].team != team && !board[x + i, y - i].isDead) //if enemy
                    {
                        lineOfSight.Add((x + i, y - i));
                        notEnd = false;
                    }
                    else if (board[x + i, y - i].team == team && !board[x + i, y - i].isDead) //if friendly
                    {
                        notEnd = false;
                    }
                    else lineOfSight.Add((x + i, y - i));
                }
                catch { notEnd = false; }
            }
            notEnd = true;

            for (int i = 0; notEnd; i++) //check negative negative path
            {
                try
                {
                    if (i == 0) //if piece
                    {
                        lineOfSight.Add((x, y));
                    }
                    else if (board[x - i, y - i].team != team && !board[x - i, y - i].isDead) //if enemy
                    {
                        lineOfSight.Add((x - i, y - i));
                        notEnd = false;
                    }
                    else if (board[x - i, y - i].team == team && !board[x - i, y - i].isDead) //if friendly
                    {
                        notEnd = false;
                    }
                    else lineOfSight.Add((x - i, y - i));
                }
                catch { notEnd = false; }
            }
            notEnd = true;

            for (int i = 0; notEnd; i++) //check negative positive path
            {
                try
                {
                    if (i == 0) //if piece
                    {
                        lineOfSight.Add((x, y));
                    }
                    else if (board[x - i, y + i].team != team && !board[x - i, y + i].isDead) //if enemy
                    {
                        lineOfSight.Add((x - i, y + i));
                        notEnd = false;
                    }
                    else if (board[x - i, y + i].team == team && !board[x - i, y + i].isDead) //if friendly
                    {
                        notEnd = false;
                    }
                    else lineOfSight.Add((x - i, y + i));
                }
                catch { notEnd = false; }
            }
            return lineOfSight;
        }
        public override List<(int, int)> GetKillMovementList(int x, int y, Piece[,] board)
        {
            List<(int, int)> lineOfSight = new List<(int, int)>();
            bool notEnd = true;
            for (int i = 1; notEnd; i++) //check positive positive path
            {
                try
                {
                    if (!board[x + i, y + i].isDead) //if piece
                    {
                        lineOfSight.Add((x + i, y + i));
                        notEnd = false;
                    }
                    else lineOfSight.Add((x + i, y + i));
                }
                catch { notEnd = false; }
            }
            notEnd = true;

            for (int i = 1; notEnd; i++) //check positive negative path
            {
                try
                {
                    if (!board[x + i, y - i].isDead) //if piece
                    {
                        lineOfSight.Add((x + i, y - i));
                        notEnd = false;
                    }
                    else lineOfSight.Add((x + i, y - i));
                }
                catch { notEnd = false; }
            }
            notEnd = true;

            for (int i = 1; notEnd; i++) //check negative negative path
            {
                try
                {
                    if (!board[x - i, y - i].isDead) //if piece
                    {
                        lineOfSight.Add((x - i, y - i));
                        notEnd = false;
                    }
                    else lineOfSight.Add((x - i, y - i));
                }
                catch { notEnd = false; }
            }
            notEnd = true;

            for (int i = 1; notEnd; i++) //check negative positive path
            {
                try
                {
                    if (!board[x - i, y + i].isDead) //if piece
                    {
                        lineOfSight.Add((x - i, y + i));
                        notEnd = false;
                    }
                    else lineOfSight.Add((x - i, y + i));
                }
                catch { notEnd = false; }
            }
            return lineOfSight;
        }
    }
    [Serializable]
    class Queen : Piece
    {
        public Queen(bool team) : base(team)
        {
            symbol = 'Q';
            points = 90;
        }
        public Queen() : base()
        {
            symbol = 'Q';
        }
        public override List<(int, int)> GetMovementList(int x, int y, Piece[,] board)
        {
            Piece temp = board[x, y]; //save queen piece at position
            board[x, y] = new Rook(board[x, y].team); //create a rook on queen position
            List<(int, int)> lineOfSight = board[x, y].GetMovementList(x, y, board); //get line of sight for rook
            board[x, y] = new Bishop(board[x, y].team); //create a bishop on rook position
            lineOfSight.AddRange(board[x, y].GetMovementList(x, y, board)); //add line of sight of bishop to line of sight for rook
            board[x, y] = temp; //replace bishop with old queen

            return lineOfSight;
        }
        public override List<(int, int)> GetKillMovementList(int x, int y, Piece[,] board)
        {
            Piece temp = board[x, y];
            board[x, y] = new Rook(board[x, y].team); //create a rook on queen position
            List<(int, int)> lineOfSight = board[x, y].GetKillMovementList(x, y, board); //get line of sight for rook
            board[x, y] = new Bishop(board[x, y].team); //create a bishop on rook position
            lineOfSight.AddRange(board[x, y].GetKillMovementList(x, y, board)); //add line of sight of bishop to line of sight for rook
            board[x, y] = temp;

            return lineOfSight;
        }
    }
    [Serializable]
    class King : Piece
    {
        bool _hasCastled = false;
        bool _inCheck = false;

        public bool hasCastled { get => _hasCastled; set => _hasCastled = value; }
        public bool inCheck { get => _inCheck; set => _inCheck = value; }

        public King(bool team) : base(team)
        {
            symbol = '#';
            points = 900;
        }
        public King() : base()
        {
            symbol = '#';
        }
        public bool CheckCheck(int x, int y, Piece[,] board)
        {
            if (GetEnemyKillMoveList(board).Contains((x, y))) //if enemy piece is able to move onto position of king
            {
                inCheck = true;

                int piececount = 0;
                foreach (Piece p in board)
                {
                    if (p.team == team && !p.isDead) piececount++;
                }

                var temp = new List<(int,int)> { (x, y) };

                //-----debug-----
//                Console.WriteLine("\nThe {1} king is in check at {0} and can move here:", (x, y), board[x, y].team);
//                foreach ((int,int) coord in GetMovementList(x, y, board).Except(temp))
//                {
//                    Console.WriteLine(coord);
//                }
//                System.Threading.Thread.Sleep(5000);//wait 5 seconds and continue
//                Console.ReadLine();//allow user to advance when they want
                //---------------

                if (GetMovementList(x, y, board).Any() == false || (piececount == 1 && GetMovementList(x, y, board).Except(temp).Any() == false)) //if king cannot move OR (if king is last piece AND king movelist only contains xy)
                {
                    return true;

                }//if king cannot move anywhere (checkmate) - FIX FOR CASES WHERE PLAYER IS NOT IN CHECK BUT HAS TO MOVE KING (LAST PIECE)

                //Console.WriteLine("Checkmate not detected");
                //Console.ReadLine();
            }
            else inCheck = false; //if not in check reset inCheck attribute to false
            return false;
        }//check detection, will return true if king is in checkmate.
        private List<(int, int)> GetEnemyKillMoveList(Piece[,] board)
        {
            //Console.WriteLine("Starting");
            List<(int, int)> lineOfSight = new List<(int, int)>();
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    try
                    {
                        if (!board[x, y].isDead && board[x, y].team != team) //if enemy piece
                        {
//                            Console.WriteLine("getting list for ({0},{1})",x,y);
                            lineOfSight.AddRange(board[x, y].GetKillMovementList(x, y, board));
                        }
                    }
                    catch { }
                }
            }
            //Console.WriteLine("Done");
            return lineOfSight;
        }//get movement list for every enemy piece (only useful in King class)
        public override List<(int, int)> GetMovementList(int x, int y, Piece[,] board)
        {
            List<(int, int)> lineOfSight = new List<(int, int)>();
            List<(int, int)> enemyLineOfSight = GetEnemyKillMoveList(board);
            (int, int)[] kingCoords = { (x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1), (x + 1, y + 1), (x - 1, y + 1), (x + 1, y - 1), (x - 1, y - 1) };

            if (!enemyLineOfSight.Contains((x,y))) lineOfSight.Add((x, y));

            foreach ((int, int) coord in kingCoords)
            {
                try
                {
                    if (!enemyLineOfSight.Contains(coord)) //if not in enemy line of sight
                    {
                        if (board[coord.Item1, coord.Item2].isDead) //if empty
                        {
                            lineOfSight.Add(coord);
                        }
                        else if (board[coord.Item1, coord.Item2].team != team && !board[coord.Item1, coord.Item2].isDead) //if enemy
                        {
                            lineOfSight.Add(coord);
                        }
                    }
                }
                catch { }
            }
            return lineOfSight;
        }//king not allowed to move itself into check
        public override List<(int, int)> GetKillMovementList(int x, int y, Piece[,] board)
        {
            List<(int, int)> lineOfSight = new List<(int, int)>();
            (int, int)[] kingCoords = { (x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1), (x + 1, y + 1), (x - 1, y + 1), (x + 1, y - 1), (x - 1, y - 1) };

            foreach ((int, int) coord in kingCoords)
            {
                try
                {
                    lineOfSight.Add(coord);
                }
                catch { }
            }
            return lineOfSight;
        }//old version of GetMovementList (used in check detection)
    }
    class Game
    {
        static Random gen = new Random();
        static Piece[,] _board = new Piece[8, 8];
        static bool _turn;
        static int _handx;
        static int _handy;
        static bool _noWinner;
        static List<(int, int)> _lineOfSight = new List<(int, int)>();
        static bool _trueAI;
        static bool _falseAI;
        static int _simSpeed;
        static (int, int) _trueking;
        static (int, int) _falseking;
        static int _turncounter;
        static int _lastcaptureturn;
        static (int, int) _lastposdata;
        static (int, int) _lastmovedata;
        static Piece _lastpospiece;
        static Piece _lastmovepiece;

        static bool debugBoard = false;
        public static bool turn { get => _turn; set => _turn = value; }
        public static Piece[,] board { get => _board; set => _board = value; }
        public static int handX { get => _handx; set => _handx = value; }
        public static int handY { get => _handy; set => _handy = value; }
        public static List<(int, int)> lineOfSight { get => _lineOfSight; set => _lineOfSight = value; }
        public static bool noWinner { get => _noWinner; set => _noWinner = value; }
        public static bool trueAI { get => _trueAI; set => _trueAI = value; }
        public static bool falseAI { get => _falseAI; set => _falseAI = value; }
        public static int simSpeed { get => _simSpeed; set => _simSpeed = value; }
        public static (int, int) trueking { get => _trueking; set => _trueking = value; }
        public static (int, int) falseking { get => _falseking; set => _falseking = value; }
        public static int turncounter { get => _turncounter; set => _turncounter = value; }
        public static int lastcaptureturn { get => _lastcaptureturn; set => _lastcaptureturn = value; }
        public static (int, int) lastposdata { get => _lastposdata; set => _lastposdata = value; }
        public static (int, int) lastmovedata { get => _lastmovedata; set => _lastmovedata = value; }
        internal static Piece lastpospiece { get => _lastpospiece; set => _lastpospiece = value; }
        internal static Piece lastmovepiece { get => _lastmovepiece; set => _lastmovepiece = value; }
        internal static List<((int, int), List<(int, int)>)> saveKingMoves = new List<((int, int), List<(int, int)>)>();

        public Game()
        {
            turncounter = 0;
            lastcaptureturn = 0;
            Console.WriteLine("How many AI? (0-2):");
            string input = "-1";
            while (int.Parse(input) < 0 || int.Parse(input) > 2)
            {
                try
                {
                    input = Console.ReadLine();
                    if (int.Parse(input) < 0 || int.Parse(input) > 2) Console.WriteLine("Invalid input. Enter a value between 0 and 2.");
                }
                catch
                {
                    input = "-1";
                    Console.WriteLine("Invalid input. Enter a value between 0 and 2.");
                }
            }
            if (int.Parse(input) == 2)
            {
                trueAI = true;
                falseAI = true;
//                Console.WriteLine("Enter sim speed (1-10)");
//              string simSpeedString = "0";
//                while (int.Parse(simSpeedString) > 10 || int.Parse(simSpeedString) < 1)
//                {
//                    try
//                    {
//                        simSpeedString = Console.ReadLine();
//                        if (int.Parse(simSpeedString) > 10 || int.Parse(simSpeedString) < 1) Console.WriteLine("Invalid input. Enter a value from 1 to 10.");
//                    }
//                    catch
//                    {
//                        simSpeedString = "0";
//                        Console.WriteLine("Invalid input. Enter a value from 1 to 10.");
//                    }
//                }
//                simSpeed = int.Parse(simSpeedString);
            }
            else if (int.Parse(input) == 1)
            {
                falseAI = true;
                trueAI = false;
            }
            else
            {
                falseAI = false;
                trueAI = false;
            }
            ResetBoard();
            Console.Clear();
        }
        public void Run()
        {
            saveKingMoves.Clear();
            //check for check(mate)
            (trueking, falseking) = FindKings();
            if (((King)board[trueking.Item1, trueking.Item2]).CheckCheck(trueking.Item1, trueking.Item2, board) || ((King)board[falseking.Item1, falseking.Item2]).CheckCheck(falseking.Item1, falseking.Item2, board) || turncounter - lastcaptureturn >= 100)
            {
                noWinner = false;
            }

            if (noWinner)
            
            {
                //if (turn == false && (falseAI && !trueAI)) //if it is an AI's turn in a COM VS PLAYER game
                //{
                //    //grid is not printed
                //}
                //else
                {
                    Console.Clear();
                    PrintBoard();
                    if (((King)board[trueking.Item1, trueking.Item2]).inCheck || ((King)board[falseking.Item1, falseking.Item2]).inCheck)
                    {
                        Console.Write("\nKing is in check!");
                    }
                    Console.Write("\nPlayer {0}'s turn ({1}). ", Convert.ToInt32(!turn) + 1, turn);
                }
                if ((turn == true && trueAI == true) || (turn == false && falseAI == true)) //if AI
                {
                    //if (falseAI && trueAI) System.Threading.Thread.Sleep(2000 / simSpeed);

                    //DoHighestMove();
                    DoMinimaxMove(3); //minimax move ----------------------------------------------------------------------------------------------------------------------------------------!

                    Run();
                }
                else //if PLAYER
                {
                    PlayerTurn();
                    Run();
                }//if player
            }
            else
            {
                Console.Clear();
                PrintBoard();
                if (turncounter - lastcaptureturn >= 100) Console.WriteLine("\nDraw.\n");
                else Console.WriteLine("\nPlayer {0} wins!\n", Convert.ToInt32(turn) + 1);
            }//if win condition reached
        }
        static bool ParseCommand(string input)
        {
            string p1 = (Regex.Replace(input.Split()[0], @"[^A-z0-9]*", "")).ToLower();
            string p2 = input.Substring(input.IndexOf(" ") + 1);
            if (p1 == "pick")
            {
                var (x, y) = ParseCoordinates(p2);
                if (x != handX || y != handY)
                {
                    return Pick(x, y);
                }
                else return false;
            }
            else if (p1 == "info")
            {
                var (x, y) = ParseCoordinates(p2);
                try
                {
                    var type = board[x, y].GetType();
                    Console.WriteLine("Position: {0}\nType: {1}\nTimes moved: {2}", p2, type, board[x, y].moveCounter);
                    System.Threading.Thread.Sleep(5000);
                    PlayerTurn();
                    return true;
                }
                catch { return false; }
            }
            else if (p1 == "save")
            {
                return true;
            }
            else if (p1 == "exit")
            {
                return true;
            }
            else
            {
                Console.WriteLine("Invalid command.\nValid commands: pick, info, save, load");
                return false;
            }
        }
        static (int, int) ParseCoordinates(string input)
        {
            Regex coordinates = new Regex(@"[A-z][0-9]");
            if (coordinates.IsMatch(input))
            {
                //Console.WriteLine("(" + (int)((input[0]) - 97) + ", " + ((int)(char.GetNumericValue(input[1])) - 1) + ")");
                return ((int)((input[0]) - 97), ((int)char.GetNumericValue(input[1])) - 1);
            }
            else Console.WriteLine("Invalid syntax. Correct syntax: pick <letter><number> e.g. pick b4"); return (-1, -1);
        }
        static string ConvertCoordinates(int x, int y)
        {
            return ((char)(x + 97)).ToString() + (y + 1).ToString();
        }
        static void ResetBoard()
        {
            noWinner = true;
            handX = -1;
            handY = -1;
            if (gen.Next(2) == 1) turn = true;
            else turn = false;

            if (!debugBoard)
            {
                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        if (y == 1) board[x, y] = new Piece(false); //create false pawns
                        else if (y == 6) board[x, y] = new Piece(true); //create true pawns
                        else if (y == 7 || y == 0)
                        {
                            if (x == 0 || x == 7) //create rooks
                            {
                                if (y == 7) board[x, y] = new Rook(true);
                                else board[x, y] = new Rook(false);
                            }
                            if (x == 1 || x == 6) //create knights
                            {
                                if (y == 7) board[x, y] = new Knight(true);
                                else board[x, y] = new Knight(false);
                            }
                            if (x == 2 || x == 5) //create bishops
                            {
                                if (y == 7) board[x, y] = new Bishop(true);
                                else board[x, y] = new Bishop(false);
                            }
                            if (x == 3) //create king or queen
                            {
                                if (y == 7) board[x, y] = new Queen(true);
                                else board[x, y] = new King(false);
                            }
                            if (x == 4)
                            {
                                if (y == 7) board[x, y] = new King(true);
                                else board[x, y] = new Queen(false);
                            }
                        }
                        else board[x, y] = new Piece();
                    }
                }
            }
            else
            {
                board = new Piece[8, 8] { {new Piece(), new Piece(), new Queen(false), new Piece(), new Rook(false), new Piece(), new Piece(false), new Rook(false) },
                { new Piece(), new Piece(), new Piece(), new Piece(), new Piece(), new Piece(), new Piece(), new Piece() },
                { new Piece(), new Piece(), new Piece(), new Piece(), new Piece(), new Piece(), new Piece(), new Piece() },
                { new Piece(), new Piece(), new Piece(), new Piece(false), new Piece(), new King(false), new Piece(), new Piece() },
            { new Piece(), new Piece(), new Piece(), new Piece(true), new Piece(), new Piece(), new Piece(true), new Piece() },
            { new Piece(), new King(true), new Piece(), new Piece(), new Piece(), new Piece(), new Piece(), new Piece() },
            { new Piece(), new Piece(), new Queen(true), new Piece(), new Piece(), new Rook(true), new Piece(), new Piece() },
            { new Piece(), new Piece(), new Piece(), new Piece(), new Piece(), new Piece(true), new Piece(), new Piece() },};
            }
        }
        static void PrintBoard()
        {
            Console.WriteLine("Turn {0}, {1} turns until draw.",turncounter+1,100-(turncounter-lastcaptureturn));
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    if (y - 1 >= 0 && x != 8)
                    {
                        if (x == handX && y - 1 == handY) Console.ForegroundColor = ConsoleColor.Cyan; //if picked position
                        else if (lineOfSight.Contains((x, y - 1)) && board[x, y - 1].team == !turn && !board[x, y - 1].isDead) Console.ForegroundColor = ConsoleColor.Red; //if enemy in line of sight
                        else if (lineOfSight.Contains((x, y - 1))) Console.ForegroundColor = ConsoleColor.Yellow; //if can move there
                        else
                        {
                            if ((falseAI && trueAI) || falseAI)//if AI game
                            {
                                if (board[x, y - 1].team && board[x, y - 1].isDead == false) Console.ForegroundColor = ConsoleColor.Green; //if true
                                else if (!board[x, y - 1].team && board[x, y - 1].isDead == false) Console.ForegroundColor = ConsoleColor.DarkRed; //if false
                            }
                            else
                            {
                                if (turn == board[x, y - 1].team && board[x, y - 1].isDead == false) Console.ForegroundColor = ConsoleColor.Green; //if friendly
                                else if (turn == !board[x, y - 1].team && board[x, y - 1].isDead == false) Console.ForegroundColor = ConsoleColor.DarkRed; //if enemy
                            }
                        }
                    }

                    if (x == 8)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        if (y != 0) Console.Write(y + " \n");
                        else Console.Write("\n");
                    }
                    else if (y == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write((char)(x + 97) + " ");
                    }
                    else
                    {
                        Console.Write(board[x, y - 1].GetSymbol() + " ");
                    }
                    Console.ResetColor();
                }
            }
            //show last move
            if (turncounter != 0)
            {
                Console.Write("\nPlayer {0} ({2}) moved {1} (", Convert.ToInt32(turn) + 1, ConvertCoordinates(lastposdata.Item1, lastposdata.Item2), !turn);

                if ((falseAI && trueAI) || falseAI)//if AI game
                {
                    if (lastpospiece.team && lastpospiece.isDead == false) Console.ForegroundColor = ConsoleColor.Green; //if true
                    else if (!lastpospiece.team && lastpospiece.isDead == false) Console.ForegroundColor = ConsoleColor.DarkRed; //if false
                }
                else
                {
                    if (turn == !lastpospiece.team && lastpospiece.isDead == false) Console.ForegroundColor = ConsoleColor.Green; //if friendly
                    else if (turn == lastpospiece.team && lastpospiece.isDead == false) Console.ForegroundColor = ConsoleColor.DarkRed; //if enemy
                }
                Console.Write(lastpospiece.GetSymbol());
                Console.ResetColor();

                Console.Write(") to {0} (", ConvertCoordinates(lastmovedata.Item1, lastmovedata.Item2));
                if ((falseAI && trueAI) || falseAI)//if AI game
                {
                    if (lastmovepiece.team && lastmovepiece.isDead == false) Console.ForegroundColor = ConsoleColor.Green; //if true
                    else if (!lastmovepiece.team && lastmovepiece.isDead == false) Console.ForegroundColor = ConsoleColor.DarkRed; //if false
                }
                else
                {
                    if (turn == !lastmovepiece.team && lastmovepiece.isDead == false) Console.ForegroundColor = ConsoleColor.Green; //if friendly
                    else if (turn == lastmovepiece.team && lastmovepiece.isDead == false) Console.ForegroundColor = ConsoleColor.DarkRed; //if enemy
                }
                Console.Write(lastmovepiece.GetSymbol());
                Console.ResetColor();

                Console.Write(").");
            }
        }
        static void PrintBoardDebug(Piece[,] brd)
        {
            Console.WriteLine("Turn {0}, {1} turns until draw.", turncounter + 1, 100 - (turncounter - lastcaptureturn));
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    if (y - 1 >= 0 && x != 8)
                    {
                        if (x == handX && y - 1 == handY) Console.ForegroundColor = ConsoleColor.Cyan; //if picked position
                        else if (lineOfSight.Contains((x, y - 1)) && brd[x, y - 1].team == !turn && !brd[x, y - 1].isDead) Console.ForegroundColor = ConsoleColor.Red; //if enemy in line of sight
                        else if (lineOfSight.Contains((x, y - 1))) Console.ForegroundColor = ConsoleColor.Yellow; //if can move there
                        else
                        {
                            if (falseAI && trueAI)//if AI game
                            {
                                if (brd[x, y - 1].team && brd[x, y - 1].isDead == false) Console.ForegroundColor = ConsoleColor.Green; //if true
                                else if (!brd[x, y - 1].team && brd[x, y - 1].isDead == false) Console.ForegroundColor = ConsoleColor.DarkRed; //if false
                            }
                            else
                            {
                                if (turn == brd[x, y - 1].team && brd[x, y - 1].isDead == false) Console.ForegroundColor = ConsoleColor.Green; //if friendly
                                else if (turn == !brd[x, y - 1].team && brd[x, y - 1].isDead == false) Console.ForegroundColor = ConsoleColor.DarkRed; //if enemy
                            }
                        }
                    }

                    if (x == 8)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        if (y != 0) Console.Write(y + " \n");
                        else Console.Write("\n");
                    }
                    else if (y == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write((char)(x + 97) + " ");
                    }
                    else
                    {
                        Console.Write(brd[x, y - 1].GetSymbol() + " ");
                    }
                    Console.ResetColor();
                }
            }
        }
        static void SetHand(int x, int y)
        {
            handX = x;
            handY = y;
            lineOfSight = board[handX, handY].GetMovementList(handX, handY, board);
        }
        static bool Pick(int x, int y)
        {
            if (isValidPick(x, y, turn, board))
            {
                SetHand(x, y);
                Console.Clear();
                PrintBoard();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("\n" + board[handX, handY].GetSymbol());
                Console.ResetColor();
                Console.WriteLine(" selected");
                Console.WriteLine("Enter coordinates to place piece");
                var (placex, placey) = ParseCoordinates(Console.ReadLine());
                while (Place(placex, placey) != true)
                {
                    (placex, placey) = ParseCoordinates(Console.ReadLine());
                }
                return true;
            }
            else
            {
                Console.WriteLine("Invalid coordinate " + ConvertCoordinates(x, y));
                return false;
            }
        }
        static bool Place(int x, int y)
        {
            if (lineOfSight.Contains((x, y)) || (x,y) == (handX,handY)) //check if possible move
            {
                if (board[handX, handY].GetType() == typeof(Piece) || !board[x, y].isDead) //if pawn is moved or there is a capture
                {
                    lastcaptureturn = turncounter;
                }

                if (x != handX || y != handY)
                {
                    //Piece temp = board[handX, handY];
                    board[handX,handY].moveCounter++;
                    //if (board[x, y].isDead)
                    //{
                    //    board[handX, handY] = board[x, y];
                    //    board[x, y] = temp;
                    //}
                    //else
                    {
                        //board[x, y] = temp;
                        lastpospiece = board[handX, handY];
                        lastmovepiece = board[x, y];
                        board[x, y] = board[handX, handY];
                        board[handX, handY] = new Piece();
                    }
                    turn = !turn;
                    turncounter++; //if the move is not just back to the original position of the piece...
                }
                lastposdata = (handX, handY);
                lastmovedata = (x, y);
                (handX, handY) = (-1, -1);
                lineOfSight.Clear();
                return true;
            }
            else
            {
                Console.WriteLine("Invalid move of {0} to {1}. Enter new coordinates.", ConvertCoordinates(handX, handY), ConvertCoordinates(x, y));
                return false;
            }
        }
        static bool isValidPick(int x, int y, bool team, Piece[,] table)
        {
            if (saveKingMoves.Any())
            {
                //if the king needs to be saved
                if (saveKingMoves.Any(i => i.Item1 == (x, y)))
                {
                    //if the pick is in the savekingmoves list return true
                    return true;
                }
                else return false; //if it isn't return false
            }
            else
            {
                if (x < 8 && x >= 0 && y >= 0 && y < 8 && table[x, y].isDead == false && table[x, y].team == team) return true;
                else return false;
            }
        }
        static void DoRandomMove()
        {
            List<((int, int), List<(int, int)>)> possibleMoves = new List<((int, int), List<(int, int)>)>(); //List of the coordinates, and every possible move

            King tempking = new King(true);
            if (turn) tempking = (King)board[trueking.Item1, trueking.Item2];
            else tempking = (King)board[falseking.Item1, falseking.Item2];
            (int, int) movexy;
            if (tempking.inCheck)
            {
                if (turn) SetHand(trueking.Item1, trueking.Item2);
                else SetHand(falseking.Item1, falseking.Item2);
                var moves = board[handX, handY].GetMovementList(handX,handY,board);
                movexy = moves[gen.Next(moves.Count)];
            }//if king is in check - forces king to be moved
            else
            {
                for (int yy = 0; yy < 8; yy++)
                {
                    for (int xx = 0; xx < 8; xx++)
                    {
                        if (isValidPick(xx, yy, turn, board))
                        {
                            List<(int, int)> filteredMoves = board[xx, yy].GetMovementList(xx, yy, board).Where(i => i.Item1 != xx || i.Item2 != yy).ToList(); //list of all moves
                            if (filteredMoves.Count > 0)
                            {
                                possibleMoves.Add(((xx, yy), filteredMoves));
                                //Console.WriteLine("{0} added to list of moveable pieces",ConvertCoordinates(xx, yy));
                            }
                        }
                    }
                }
                var ((x, y), moves) = possibleMoves[gen.Next(possibleMoves.Count)];
                SetHand(x, y);
//                moves.Remove((x,y));//Prevents AI from not moving
                //Console.WriteLine("Coordinates {0} selected", ConvertCoordinates(x, y));
                //Console.ReadLine();
                movexy = moves[gen.Next(moves.Count)];
            }//if king is not in check
            //Console.WriteLine("Moving {0} to {1}",ConvertCoordinates(x, y), ConvertCoordinates(movexy.Item1, movexy.Item2));
            //Console.ReadLine();
            Place(movexy.Item1, movexy.Item2);
        }//does a random move
        static void DoHighestMove()
        {
            List<((int, int), (int, int), int)> possibleMoves = new List<((int, int), (int, int), int)>(); //List of the coordinates, the highest scoring move from this position, and the score of this move
            //King tempking = new King(true);
            //if (turn) tempking = (King)board[trueking.Item1, trueking.Item2];
            //else tempking = (King)board[falseking.Item1, falseking.Item2];
            if (((King)board[trueking.Item1, trueking.Item2]).inCheck || ((King)board[falseking.Item1, falseking.Item2]).inCheck)
            {
                int x;
                int y;
                if (turn)
                {
                    x = trueking.Item1;
                    y = trueking.Item2;
                }
                else
                {
                    x = falseking.Item1;
                    y = falseking.Item2;
                }

                List<(int, int)> filteredMoves = board[x, y].GetMovementList(x, y, board).Where(i => i.Item1 != x || i.Item2 != y).ToList(); //list of all moves
                if (filteredMoves.Count > 0)
                {
                    int highest = -1000; //highest score
                    var bestmove = (0, 0); //move for this score
                    foreach ((int, int) move in filteredMoves) //check every possible move
                    {
                        if (GetPoints(board, move.Item1, move.Item2) >= highest) //if this move scores higher
                        {
                            bestmove = (move.Item1, move.Item2); //set this as the best move
                            highest = GetPoints(board, move.Item1, move.Item2); //set the score of this move
                        }
                    }
                    possibleMoves.Add(((x, y), bestmove, highest)); //add to list
                }
            }//if king in check - forces king to be moved
            else
            {
                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        if (isValidPick(x, y, turn, board))
                        {
                            List<(int, int)> filteredMoves = board[x, y].GetMovementList(x, y, board).Where(i => i.Item1 != x || i.Item2 != y).ToList(); //list of all moves
                            if (filteredMoves.Count > 0)
                            {
                                int highest = -1000; //highest score
                                var bestmove = (0, 0); //move for this score
                                foreach ((int, int) move in filteredMoves) //check every possible move
                                {
                                    if (GetPoints(board, move.Item1, move.Item2) >= highest) //if this move scores higher
                                    {
                                        bestmove = (move.Item1, move.Item2); //set this as the best move
                                        highest = GetPoints(board, move.Item1, move.Item2); //set the score of this move
                                    }
                                }
                                possibleMoves.Add(((x, y), bestmove, highest)); //add to list
                                                                                //Console.WriteLine("{0} added to list of moveable pieces",ConvertCoordinates(xx, yy));
                            }
                        }
                    }
                }
            }//king not in check
            int highest2 = -1000; //highest possible score
            var piececoords = (0, 0); //coords for the piece to be moved
            var mainmove = (0, 0); //move for this score
            foreach (var (xy, bestmove, score) in possibleMoves) //check every possible best move
            {
                if (score > highest2) //if score from this move is higher
                {
                    mainmove = bestmove; //set main move
                    piececoords = xy; //set coords of piece to be moved
                    highest2 = score; //set score for this move
                }
            }
            if (highest2 == 0) DoRandomMove(); //if no scoring move is found a random move is executed.
            else //if highest scoring move has been found move piece.
            {
                SetHand(piececoords.Item1, piececoords.Item2);
                Place(mainmove.Item1, mainmove.Item2);
            }
        }//does a move with the highest point value
        static (int,Node) Minimax(Node node, int depth, int alpha, int beta, bool maximisingPlayer)
        {
            Node bestmove = null;
            int value = 0;
            if (depth == 0 || node.GameIsOver()) //if leaf node, or game is over
            {
                return (node.BoardEval(),null); //return current score
            }

            if (maximisingPlayer) //if maximising player
            {
                //set maxEval to lowest possible score
                value = Int32.MinValue;
                //create a variable to store the maxEval move
                //check every child of current node
                foreach (Node child in node.GetChildren())
                {
                    //call minimax on the child
                    var (evalscore,eval) = Minimax(child, depth - 1, alpha, beta, false);
                    //get the score of the node returned by minimax
                    if (evalscore > value)
                    {
                        //change maxEval to the returned node
                        value = evalscore;
                        bestmove = child;
                        //Console.WriteLine("maxEval set to {0} at depth {1}",evalscore,depth);
                    }
                    //alpha-beta pruning
                    alpha = Math.Max(alpha, evalscore);
                    if (beta <= alpha)
                        break;
                }
            }
            else //if minimising player
            {
                //Console.WriteLine("minimising player at depth {0}", depth);
                //set maxEval to lowest possible score
                value = Int32.MaxValue;
                //create a variable to store the maxEval move
                //check every child of current node
                foreach (Node child in node.GetChildren())
                {
                    //call minimax on the child
                    var (evalscore, eval) = Minimax(child, depth - 1, alpha, beta, true);
                    //get the score of the node returned by minimax
                    if (evalscore < value)
                    {
                        //change minEval to the returned node
                        value = evalscore;
                        bestmove = child;
                        //Console.WriteLine("minEval set to {0} at depth {1}", evalscore,depth);
                    }
                    //alpha-beta pruning
                    beta = Math.Min(beta, evalscore);
                    if (beta <= alpha)
                        break;
                }
            }

            //Console.WriteLine("returning bestmove (depth {0}", depth);
            return (value,bestmove);
        }//minimax algorithm
        static void DoMinimaxMove(int depth)
        {
            Console.WriteLine("\n\nAI is thinking...");
            ////create a tree with all the possible moves:
            //
            //create root node (current position)
            //Console.WriteLine("\n[DEBUG]"); //DEBUG
            //int movescounter = 0; //DEBUG
            //Stopwatch stopWatch = new Stopwatch(); //DEBUG
            //stopWatch.Start(); //DEBUG
            Node root = new Node(board);
            //Console.WriteLine("Current board eval = {0}", root.BoardEval());
            //Console.WriteLine("Generating move tree..."); //DEBUG
            //Console.WriteLine("Root");
            //PrintBoardDebug(root.boardData);
            //Console.WriteLine("Root node created"); //DEBUG
            //create children of root node (all moves from current position)
            //get all moves on board for maximising player
            var possibleMoves = GetAllMoves(board, turn);
            foreach (((int, int), List<(int, int)>) movelist in possibleMoves)
            {
                foreach ((int,int) move in movelist.Item2)
                {
                    Piece[,] board1 = board.DeepClone(); //deep copy of main board
                    //do move on board part 1 (copy piece to move location)
                    board1[move.Item1, move.Item2] = board1[movelist.Item1.Item1, movelist.Item1.Item2];
                    //do move on board part 2 (remove original piece)
                    board1[movelist.Item1.Item1, movelist.Item1.Item2] = new Piece();
                    board1[move.Item1, move.Item2].moveCounter++;

                    //create a node for every possible move
                    Node child = new Node(movelist.Item1,move,board1.DeepClone()); //depth 1
                    //Console.WriteLine("Child");
                    //PrintBoardDebug(child.boardData);
                    //movescounter++;

                    if (depth >= 2)
                    {
                        //get all moves on board1 for minimising player
                        var possibleMoves1 = GetAllMoves(board1, !turn);
                        foreach (((int, int), List<(int, int)>) movelist1 in possibleMoves1)
                        {
                            foreach ((int, int) move1 in movelist1.Item2)
                            {
                                Piece[,] board2 = board1.DeepClone(); //deep copy of board
                                //do move on board part 1 (copy piece to move location)
                                board2[move1.Item1, move1.Item2] = board2[movelist1.Item1.Item1, movelist1.Item1.Item2];
                                //do move on board part 2 (remove original piece)
                                board2[movelist1.Item1.Item1, movelist1.Item1.Item2] = new Piece();
                                board2[move1.Item1, move1.Item2].moveCounter++;

                                //create a node for every possible move
                                Node child1 = new Node(movelist1.Item1, move1, board2.DeepClone());
                                //Console.WriteLine("Child1");
                                //PrintBoardDebug(child1.boardData);
                                //movescounter++;

                                if (depth == 3)
                                {
                                    //get all moves on board2 for maximising player
                                    var possibleMoves2 = GetAllMoves(board2, turn);
                                    foreach (((int, int), List<(int, int)>) movelist2 in possibleMoves2)
                                    {
                                        foreach ((int, int) move2 in movelist2.Item2)
                                        {
                                            Piece[,] board3 = board2.DeepClone(); //deep copy of board
                                            //do move on board part 1 (copy piece to move location)
                                            board3[move2.Item1, move2.Item2] = board3[movelist2.Item1.Item1, movelist2.Item1.Item2];
                                            //do move on board part 2 (remove original piece)
                                            board3[movelist2.Item1.Item1, movelist2.Item1.Item2] = new Piece();
                                            board3[move2.Item1, move2.Item2].moveCounter++;

                                            //create a node for every possible move
                                            Node child2 = new Node(movelist2.Item1, move2, board3.DeepClone()); //depth 3
                                            //Console.WriteLine("Child2"); //DEBUG
                                            //PrintBoardDebug(child2.boardData);
                                            //Console.ReadLine();
                                            //movescounter++;
                                            child1.AddChild(child2);
                                            //Console.WriteLine("Child2 added to Child1"); //DEBUG
                                        }
                                    }
                                }
                                child.AddChild(child1);
                              //Console.WriteLine("Child1 added to Child"); //DEBUG
                            }
                        }
                    }
                    root.AddChild(child);
                    //Console.WriteLine("Child added to root"); //DEBUG
                }
            }//move tree loop
            //DEBUG
            //stopWatch.Stop();
            //Console.WriteLine("Done! {0}ms elapsed", stopWatch.ElapsedMilliseconds.ToString());
            //Console.WriteLine(movescounter + " nodes created. Calling minimax (depth {0})...",depth);
            //DEBUG
            //
            ////call the minimax algorithm:
            //stopWatch.Start(); //DEBUG
            var (besteval, best) = Minimax(root, depth, Int32.MinValue, Int32.MaxValue, turn);
            //DEBUG
            //stopWatch.Stop();
            //Console.WriteLine("Done! {0}ms elapsed", stopWatch.ElapsedMilliseconds.ToString());
            //stopWatch.Reset();
            if (best.isRoot) Console.WriteLine("ERROR: MINIMAX RETURNED ROOT NODE");
            if (!isValidPick(best.posData.Item1, best.posData.Item2, turn, board)) Console.WriteLine("ERROR: INVALID MOVE");
            //Console.WriteLine("Minimax returned best move as: {0} to {1}.  eval = {2}", ConvertCoordinates(best.posData.Item1,best.posData.Item2), ConvertCoordinates(best.moveData.Item1,best.moveData.Item2), besteval);
            //Console.WriteLine("Moving piece...");
            //System.Threading.Thread.Sleep(5000);
            //Console.ReadLine();
            SetHand(best.posData.Item1, best.posData.Item2);
            Place(best.moveData.Item1, best.moveData.Item2);
            //DEBUG
        }
        static int GetPoints(Piece[,] table, int x, int y)
        {
            if (table[x, y].team != turn) return board[x, y].points;
            else return 0;
        }//get points for DoHighestMove
        static ((int,int),(int,int)) FindKings()
        {
            var truekingcoords = (4,7);
            var falsekingcoords = (3,0);

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    try
                    {
                        if(board[x,y].GetType() == typeof(King) && !board[x,y].isDead)
                        {
                            if (board[x, y].team)
                            {
                                truekingcoords = (x, y); 
                                //Console.WriteLine("true king found at ({0},{1})", x, y);
                            }
                            else
                            {
                                falsekingcoords = (x, y);
                                //Console.WriteLine("false king found at ({0},{1})", x, y);
                            }
                        }
                    }
                    catch { }
                }
            }

            //Console.ReadLine();
            return (truekingcoords, falsekingcoords);
        }
        static List<((int, int), List<(int, int)>)> GetAllMoves(Piece[,] board, bool team)
        {
            var allMoves = new List<((int, int), List<(int, int)>)>(); //List of the coordinates of the moveable pieces, and every possible move for each piece
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    //check every possible place on the board
                    if (isValidPick(x, y, team,board)) //if this is a valid piece to move
                    {
                        var pieceMoves = board[x, y].GetMovementList(x, y, board); //create list of all moves of this piece

                        bool wasRemoved = true;
                        do
                        {
                            wasRemoved = pieceMoves.Remove((x, y));
                        } while (wasRemoved);

                        if (pieceMoves.Count > 0) //if moves were found
                        {
                            allMoves.Add(((x, y), pieceMoves)); //add the position of this piece and all its moves to the possibleMoves list
                            //Console.WriteLine("{0} added to list of moveable pieces",ConvertCoordinates(x, y));
                        }
                    }
                }
            }
            return allMoves; //return the list
        }
        static void PlayerTurn()
        {
            Console.WriteLine("Enter command:");
            if (((King)board[trueking.Item1, trueking.Item2]).inCheck || ((King)board[falseking.Item1, falseking.Item2]).inCheck)
            {
                var filteredMoves = new List<((int, int), List<(int, int)>)>(); //list of the coordinates of the piece and every possible move of that piece
                var pieceMoves = new List<(int, int)>();
                Console.WriteLine("You must save your king. Valid pieces to move:");
                //find every move which removes the checkmate.
                foreach (var movelist in GetAllMoves(board, turn))
                {
                    foreach (var move in movelist.Item2)
                    {
                        //do move on board:
                        //do move on board part 1 (copy piece to move location)
                        var tempPiece = board[move.Item1, move.Item2];
                        board[move.Item1, move.Item2] = board[movelist.Item1.Item1, movelist.Item1.Item2];
                        //do move on board part 2 (remove original piece)
                        board[movelist.Item1.Item1, movelist.Item1.Item2] = new Piece();
                        //check if check/checkmate still.
                        try
                        {
                            //Console.WriteLine("checking move of {0} to {1}",ConvertCoordinates(movelist.Item1.Item1, movelist.Item1.Item2),ConvertCoordinates(move.Item1,move.Item2));
                            if (board[trueking.Item1, trueking.Item2].GetType() != typeof(King) || board[falseking.Item1, falseking.Item2].GetType() != typeof(King))
                            {
                                //Console.WriteLine("king has moved, finding king");
                                (trueking,falseking) = FindKings(); //if a king has moved find it again
                            }
                            ((King)board[trueking.Item1, trueking.Item2]).CheckCheck(trueking.Item1, trueking.Item2, board);
                            ((King)board[falseking.Item1, falseking.Item2]).CheckCheck(falseking.Item1, falseking.Item2, board);
                            if (!((King)board[trueking.Item1, trueking.Item2]).inCheck && !((King)board[falseking.Item1, falseking.Item2]).inCheck) //if the check is gone
                            {
                                pieceMoves.Add(move); //add move to list
                                //Console.WriteLine("adding a move");
                            }
                        }
                        catch (Exception e) { Console.WriteLine("ERROR: " + e.Message); }
                        //undo move
                        board[movelist.Item1.Item1, movelist.Item1.Item2] = board[move.Item1, move.Item2];
                        board[move.Item1, move.Item2] = tempPiece;
                    }
                    if (pieceMoves.Any())
                    {
                        filteredMoves.Add((movelist.Item1, pieceMoves)); //add the coordinates of the piece to the list of moveable pieces, and all the possible moves of that piece.
                        pieceMoves.Clear();
                    }
                    saveKingMoves = filteredMoves;
                }
                foreach(var Piece in filteredMoves)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("{0}", board[Piece.Item1.Item1, Piece.Item1.Item2].GetSymbol());
                    Console.ResetColor();
                    Console.Write(" at {0}\n", ConvertCoordinates(Piece.Item1.Item1, Piece.Item1.Item2));
                }
            }//if king is in check
            bool temp = true;
            do
            {
                if (ParseCommand(Console.ReadLine()))
                {
                    temp = false;
                }
            } while (temp); //loop until valid command
        }
    }
    class Node
    {
        Piece[,] _boardData;
        (int, int) _moveData;
        (int, int) _posData;
        List<Node> _children;
        bool _isRoot;
        public Node(Piece[,] boardData)
        {
            this.boardData = boardData;
            children = new List<Node>();
            isRoot = true;
        }//for root node
        public Node((int,int) posData, (int,int) moveData, Piece[,] boardData)
        {
            this.posData = posData;
            this.moveData = moveData;
            this.boardData = boardData;
            children = new List<Node>();
            isRoot = false;
        }
        public (int, int) moveData { get => _moveData; set => _moveData = value; }
        internal Piece[,] boardData { get => _boardData; set => _boardData = value; }
        internal List<Node> children { get => _children; set => _children = value; }
        public (int, int) posData { get => _posData; set => _posData = value; }
        public bool isRoot { get => _isRoot; set => _isRoot = value; }

        public void AddChild((int,int) posData, (int,int) moveData, Piece[,] boardData)
        {
            children.Add(new Node(posData, moveData, boardData));
        }
        public void AddChild(Node child)
        {
            children.Add(child);
        }
        public List<Node> GetChildren()
        {
            var childlist = new List<Node>();
            foreach (Node child in children)
            {
                childlist.Add(child);
            }
            return childlist;
        }
        public int BoardEval()
        {
            int total = 0;
            foreach (Piece square in boardData)
            {
                if(!square.isDead)
                {
                    if (square.team)
                    {
                        total += square.points; //true pieces are worth positive point values
                    }
                    else
                    {
                        total -= square.points; //false pieces are worth negative point values
                    }
                }
            }
            //Console.WriteLine("boardeval returned " + total);
            return total;
        }
        public bool GameIsOver() 
        {
            bool trueking = false; 
            bool falseking = false;

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    try
                    {
                        if (boardData[x, y].GetType() == typeof(King) && !boardData[x, y].isDead)
                        {
                            if (boardData[x, y].team)
                            {
                                trueking = true;
                            }
                            else
                            {
                                falseking = true;
                            }
                        }
                    }
                    catch { }
                }
            }

            if (trueking && falseking) return false;
            else return true;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game();
            game.Run();
            Console.WriteLine("[RUN STATE EXITED] Press enter to play a new game, or 'exit' to exit");
            string input = Console.ReadLine().ToLower();
            if (input != "exit")
            {
                Console.Clear();
                game = new Game();
                game.Run();
            }
        }
    }
}
