namespace Schach;

public class Game
{
    public bool IsWhiteMoving { get; set; } = true;

    public void GameLoop(Grid grid)
    {
        // bevor das Spiel anfängt, prüfen, ob schon ein Checkmate ist
        if (Rules.IsCheckmate(grid, IsWhiteMoving) || Rules.IsCheckmate(grid, !IsWhiteMoving))
        {
            Board.DrawBoard(grid);
            Console.Write("Checkmate");
            return;
        }
        
        // bevor das Spiel anfängt, prüfen, ob schon ein Checkmate ist
        if (Rules.IsStalemate(grid, IsWhiteMoving) || Rules.IsStalemate(grid, !IsWhiteMoving))
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

            if (Move.InputMove(grid, userInput, IsWhiteMoving))
            { 
                IsWhiteMoving = !IsWhiteMoving;
                if (Rules.IsCheckmate(grid, IsWhiteMoving))
                {
                    Board.DrawBoard(grid);
                    Console.Write("Checkmate");
                    break;
                }

                if (Rules.IsStalemate(grid, IsWhiteMoving))
                {
                    Board.DrawBoard(grid);
                    Console.Write("Stalemate");
                    break;
                }
            }
        }
    }
}