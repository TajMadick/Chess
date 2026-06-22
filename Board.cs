namespace Schach;

public static class Board
{
    public static void DrawBoard(Grid grid)
    {
        Console.Clear();
        Console.WriteLine("    a   b   c   d   e   f   g   h");
        for (int row = 0; row < 8; row++)
        {
            Console.Write($" {8-row} ");
            for (int col = 0; col < 8; col++)
            {
                Console.Write($"[{grid[row, col].GetIcon()} ]");
            }
            Console.Write($" {8-row} ");
            Console.WriteLine();
        }
        Console.WriteLine("    a   b   c   d   e   f   g   h");
    }
    public static void FenParser(string fen, Grid grid, Game game)
    {
        string[] parts = fen.Split(' ');
        string[] rows = parts[0].Split('/');
        
        for (int row = 0; row < 8; row++)
        {
            int col = 0;
            // Spalte kann Buchstabe oder Zahl sein
            foreach (char character in rows[row])
            {
                // Bei Zahl n müssen n Empty Spaces eingefügt werden
                if (char.IsNumber(character))
                {
                    int number = character - '0';
                    while (number > 0)
                    {
                        grid[row, col] = new Empty();
                        number--;
                        col++;
                    }
                }
                else
                {
                    // wenn der Buchstabe groß ist, ist die Figur weiß
                    bool isWhite = char.IsUpper(character);
                    // schauen welcher Buchstabe unabhängig ob groß oder klein
                    grid[row, col] = char.ToLower(character) switch
                    {
                        'r' => new Rook(isWhite),
                        'n' => new Knight(isWhite),
                        'b' => new Bishop(isWhite),
                        'q' => new Queen(isWhite),
                        'k' => new King(isWhite),
                        'p' => new Pawn(isWhite),
                        _ => new Empty()
                    };
                    
                    col++;
                }
            }
        }
        

        if (parts[1] == "w") game.IsWhiteMoving = true;
        else if (parts[1] == "b") game.IsWhiteMoving = false;
        
        foreach (char character in parts[2])
        {
            Grid.Tile tile = character switch
            {
                'K' => new Grid.Tile(7, 7),
                'Q' => new Grid.Tile(7, 0),
                'k' => new Grid.Tile(0, 7),
                'q' => new Grid.Tile(0, 0),
                _ => new Grid.Tile(-1,-1)
            };
            
            if (tile.IsValid() && grid[tile.Row, tile.Col] is Rook rook)
            {
                rook.HasMoved = false;
            }
        }
        

        if (parts[3] != "-")
        {
            int col = parts[3][0] - 'a';
            if (parts[3][1] == '3')
            {
                if (grid[4, col] is Pawn pawn)
                {
                    pawn.IsEnPassantable = true;
                }
            }
            if (parts[3][1] == '6')
            {
                if (grid[3, col] is Pawn pawn)
                {
                    pawn.IsEnPassantable = true;
                }
            }
        }
    }
}