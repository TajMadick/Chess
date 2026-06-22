using System;

namespace Schach
{
    static class Programm
    {
        static void Main()
        {
            bool isWhiteMoving = true;
            Board board = new Board(ref isWhiteMoving);

            while (true)
            {
                board.DrawBoard();

                string color = (isWhiteMoving) ? "White" : "Black";
                Console.Write($"{color} move: ");
                
                string? userInput = Console.ReadLine();
                if (userInput is null) continue;
                if (userInput == "esc")
                {
                    break;
                }
                
                if (Move.InputMove(board, userInput, isWhiteMoving))
                {
                    isWhiteMoving = !isWhiteMoving;
                    
                    if (Move.IsCheckmatePostMove(board, isWhiteMoving))
                    {
                        board.DrawBoard();
                        Console.Write("Checkmate");
                        break;
                    }
                }
            }
        }
    }
}

