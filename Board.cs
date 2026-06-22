namespace Schach;

public class Board
{
    private Pieces[,] grid = new Pieces[8, 8];

    public Pieces this[Tile tile]
    {
        get => grid[tile.Row, tile.Col];
        set => grid[tile.Row, tile.Col] = value;
    }
    
    public Pieces this[int row, int col]
    {
        get => grid[row, col];
        set => grid[row, col] = value;
    }

    public ref Pieces GetRef(int row, int col)
    {
        return ref grid[row, col];
    }
    
    public ref Pieces GetRef(Tile tile)
    {
        return ref grid[tile.Row, tile.Col];
    }
    
    public Board(ref bool isWhiteMoving)
    {
        FENParser("6k1/5ppp/8/8/8/8/5PPP/R5K1 w - - 0 1", ref isWhiteMoving);
    }

    public Board(Board source)
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                grid[row, col] = source.grid[row, col].Copy();
            }
        }
    }

    public struct Tile(int row, int col)
    {
        public int Row = row;
        public int Col = col;
        
        public static bool operator ==(Tile t1, Tile t2)
        {
            return t1.Row == t2.Row && t1.Col == t2.Col;
        }
        
        public static bool operator !=(Tile t1, Tile t2)
        {
            return t1.Row != t2.Row || t1.Col != t2.Col;
        }
    }
    
    public void DrawBoard()
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
    private void FENParser(string fen, ref bool isWhiteMoving)
    {
        string[] parts = fen.Split(' ');
        string[] rows = parts[0].Split('/');

        for (int row = 0; row < 8; row++)
        {
            int col = 0;
            foreach (char character in rows[row])
            {
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
                    bool isWhite = char.IsUpper(character);
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
        
        isWhiteMoving = parts[1] switch
        {
            "w" => true,
            "b" => false,
            _ => false
        };

        
        foreach (char character in parts[2])
        {
            if (character == 'K' && grid[7, 7] is Rook rook1)
            {
                rook1.HasMoved = false;
            }
            if (character == 'Q' && grid[7, 0] is Rook rook2)
            {
                rook2.HasMoved = false;
            }
            if (character == 'k' && grid[0, 7] is Rook rook3)
            {
                rook3.HasMoved = false;
            }
            if (character == 'q' && grid[0, 0] is Rook rook4)
            {
                rook4.HasMoved = false;
            }
        }
        

        if (parts[3] != "-")
        {
            int col = parts[4][0] - 'a';
            if (parts[4][1] == '3')
            {
                if (grid[4, col] is Pawn pawn)
                {
                    pawn.IsEnPassantable = true;
                }
            }
            if (parts[4][1] == '6')
            {
                if (grid[3, col] is Pawn pawn)
                {
                    pawn.IsEnPassantable = true;
                }
            }
        }
    }
}