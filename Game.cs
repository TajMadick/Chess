namespace Schach;

public class Game
{
    public void GameLoop(Board board)
    {
        Console.WriteLine(Test.PossiblePositions(board, 3, board.IsWhiteMoving, true));
        Console.ReadKey();

        
        // bevor das Spiel anfängt, prüfen, ob schon ein Checkmate ist
        // gecheckt wird die jeweils andere Farbe, die dran ist
        if (Rules.IsCheckmate(board, isWhiteChecking:!board.IsWhiteMoving, board.IsWhiteMoving))
        {
            board.DrawBoard();
            Console.Write("Checkmate");
            return;
        }
        
        // bevor das Spiel anfängt, prüfen, ob schon ein Stalemate ist
        // gecheckt wird die jeweils andere Farbe, die dran ist
        if (Rules.IsStalemate(board, isWhiteChecking:board.IsWhiteMoving))
        {
            board.DrawBoard();
            Console.Write("Stalemate");
            return;
        }
        
        while (true)
        {
            board.DrawBoard();

            string color = (board.IsWhiteMoving) ? "White" : "Black";
            Console.Write($"{color} move: ");
                
            string? userInput = Console.ReadLine();
            if (userInput is null) continue;
            if (userInput == "esc")
            {
                break;
            }
            
            if (!Validation.PassesSanityChecks(board.Grid, userInput, board.IsWhiteMoving)) continue;
            Utils.CalculateCoordinates(userInput, out Grid.Tile fromTile, out Grid.Tile toTile);


            if (board.MakeMove(fromTile, toTile))
            { 
                board.SwitchColorToMove();
                if (Rules.IsCheckmate(board, isWhiteChecking:board.IsWhiteMoving, board.IsWhiteMoving))
                {
                    board.DrawBoard();
                    Console.Write("Checkmate");
                    break;
                }

                if (Rules.IsStalemate(board, isWhiteChecking:board.IsWhiteMoving))
                {
                    board.DrawBoard();
                    Console.Write("Stalemate");
                    break;
                }

                Rules.EnPassantExpires(board.Grid);
            }
        }
    }
}