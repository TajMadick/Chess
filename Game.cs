namespace Schach;

public class Game
{
    public bool IsWhiteMoving { get; set; } = true;

    public void GameLoop(Grid grid)
    {
        // bevor das Spiel anfängt, prüfen, ob schon ein Checkmate ist
        // gecheckt wird die jeweils andere Farbe, die dran ist
        if (Rules.IsCheckmate(grid, isWhiteChecking:!IsWhiteMoving, IsWhiteMoving))
        {
            Board.DrawBoard(grid);
            Console.Write("Checkmate");
            return;
        }
        
        // bevor das Spiel anfängt, prüfen, ob schon ein Stalemate ist
        // gecheckt wird die jeweils andere Farbe, die dran ist
        if (Rules.IsStalemate(grid, isWhiteChecking:IsWhiteMoving))
        {
            Board.DrawBoard(grid);
            Console.Write("Stalemate");
            return;
        }
        
        while (true)
        {
            Board.DrawBoard(grid);

            string color = (IsWhiteMoving) ? "White" : "Black";
            Console.Write($"{color} move: ");
                
            string? userInput = Console.ReadLine();
            if (userInput is null) continue;
            if (userInput == "esc")
            {
                break;
            }
            
            if (!Validation.PassesSanityChecks(grid, userInput, IsWhiteMoving)) continue;
            Utils.CalculateCoordinates(userInput, out Grid.Tile fromTile, out Grid.Tile toTile);


            if (Move.InputMove(grid, fromTile, toTile, IsWhiteMoving))
            { 
                IsWhiteMoving = !IsWhiteMoving;
                if (Rules.IsCheckmate(grid, IsWhiteMoving, IsWhiteMoving))
                {
                    Board.DrawBoard(grid);
                    Console.Write("Checkmate");
                    break;
                }

                if (Rules.IsStalemate(grid, isWhiteChecking:IsWhiteMoving))
                {
                    Board.DrawBoard(grid);
                    Console.Write("Stalemate");
                    break;
                }
            }
        }
    }
}