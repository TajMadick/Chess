namespace Schach;

public class Board
{
    public bool IsWhiteMoving { get; set; } = true;
    public Grid Grid = new Grid();
    private Stack<Move> allMoves = new Stack<Move>();
    
    /*
    private Pieces lastCapturedPiece;
    private Grid.Tile lastToTile;       // lastToTile sind die Koordinaten von dem lastCapturedPiece
    private Grid.Tile lastFromTile;
    */

    public Board(string fen)
    {
        FenParser(fen);
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
                Console.Write($"[{Grid[row, col].GetIcon()} ]");
            }
            Console.Write($" {8-row} ");
            Console.WriteLine();
        }
        Console.WriteLine("    a   b   c   d   e   f   g   h");
    }

    public bool MakeMove(Grid.Tile fromTile, Grid.Tile toTile)
    {
        Move newMove = Grid[fromTile].DetermineMoveType(Grid, fromTile, toTile) switch
        {
            Types.MoveType.Normal => new NormalMove(Grid, fromTile, toTile),
            Types.MoveType.Castling => new CastlingMove(Grid, fromTile, toTile),
            Types.MoveType.DoubleStepPawn => new DoubleStepPawnMove(Grid, fromTile, toTile),
            Types.MoveType.EnPassant => new EnPassantMove(Grid, fromTile, toTile),
            Types.MoveType.Promotion => new PromotionMove(Grid, fromTile, toTile),
            _ => new InvalidMove(Grid, fromTile, toTile)
        };

        if (!newMove.TryDoMove(Grid, IsWhiteMoving))
        {
            return false;
        }
        allMoves.Push(newMove);
        SwitchColorToMove();
        return true;
    }

    public void UnmakeMove()
    {
        Move lastMove = allMoves.Pop();
        lastMove.UndoMove(Grid);
        SwitchColorToMove();
    }

    public void SwitchColorToMove()
    {
        IsWhiteMoving = !IsWhiteMoving;
    }
    private void FenParser(string fen)
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
                        Grid[row, col] = new Empty();
                        number--;
                        col++;
                    }
                }
                else
                {
                    // wenn der Buchstabe groß ist, ist die Figur weiß
                    bool isWhite = char.IsUpper(character);
                    // schauen welcher Buchstabe unabhängig ob groß oder klein
                    Grid[row, col] = char.ToLower(character) switch
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
        

        if (parts[1] == "w") IsWhiteMoving = true;
        else if (parts[1] == "b") IsWhiteMoving = false;
        
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
            
            if (tile.IsValid() && Grid[tile.Row, tile.Col] is Rook rook)
            {
                rook.HasMoved = false;
            }
        }
        

        if (parts[3] != "-")
        {
            int col = parts[3][0] - 'a';
            if (parts[3][1] == '3')
            {
                if (Grid[4, col] is Pawn pawn)
                {
                    pawn.IsEnPassantable = true;
                }
            }
            if (parts[3][1] == '6')
            {
                if (Grid[3, col] is Pawn pawn)
                {
                    pawn.IsEnPassantable = true;
                }
            }
        }
    }
}