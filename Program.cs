using System;
namespace Schach
{
    class Programm
    {
        static void Main()
        {
            bool isWhiteMoving = true;
            
            Board board = new Board();

            while (true)
            {
                board.DrawBoard();

                string color = (isWhiteMoving) ? "White" : "Black";
                Console.Write($"{color} move: ");
                
                string userInput = Console.ReadLine();
                if (board.MovePiece(userInput, isWhiteMoving))
                {
                    isWhiteMoving = !isWhiteMoving;
                }
            }
        }
    }
}

