namespace Schach;

public abstract class Pieces
{
    protected Pieces(bool isWhite)
    {
        IsWhite = isWhite;
    }
    
    public enum MoveType
    {
        Invalid,
        Promotion,
        Castling,
        Normal,
        EnPassant
    }
    
    public bool IsWhite { get; }
    public abstract char GetIcon();

    public abstract Pieces Copy();
    public abstract MoveType DetermineMoveType(Grid grid, Grid.Tile fromTile, Grid.Tile toTile);

    public abstract IEnumerable<Grid.Tile> FieldsOnPath(Grid grid, Grid.Tile fromTile, Grid.Tile toTile);

    public IEnumerable<Grid.Tile> AllPossibleMoves(Grid grid, Grid.Tile fromTile)
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Grid.Tile toTile = new Grid.Tile(row, col);
                
                if (fromTile != toTile &&
                    grid[fromTile].DetermineMoveType(grid, fromTile, toTile) != MoveType.Invalid)
                {
                    yield return toTile;
                }
            }
        }
    }

    protected bool LoopThrough(Grid grid, Grid.Tile fromTile, Grid.Tile toTile, int dirRow, int dirCol)
    {
        int row = fromTile.Row += dirRow;
        int col = fromTile.Col += dirCol;
        
        for (; row is >= 0 and < 8 && col is >= 0 and < 8; row += dirRow, col += dirCol)
        {
            Grid.Tile runnerTile = new Grid.Tile(row, col);
            // wenn die Runner es bis zum Ziel geschafft haben -> true
            if (runnerTile == toTile)
            {
                return true;
            }

            // wenn ein Feld auf dem Weg nicht leer ist -> false
            if (grid[runnerTile] is not Empty)
            {
                return false;
            }
        }
        
        return false;
    }

    // TODO: das alles schöner machen das momentan schiach
    protected bool IsFieldOnPath(Grid grid, Grid.Tile fromTile, Grid.Tile targetTile, Grid.Tile toTile, int dirRow, int dirCol)
    {
        int lowerBoundRow = (fromTile.Row < toTile.Row) ? toTile.Row : -1;
        int upperBoundRow = (fromTile.Row < toTile.Row) ? 8 : toTile.Row;
        int lowerBoundCol = (fromTile.Col < toTile.Col) ? toTile.Col : -1;
        int upperBoundCol = (fromTile.Col < toTile.Col) ? 8 : toTile.Col;

        int row = fromTile.Row;
        int col = fromTile.Col;
        
        for (; row > lowerBoundRow && row < upperBoundRow && col > lowerBoundCol && col < upperBoundCol; 
             row += dirRow, col += dirCol)
        {
            Grid.Tile runnerTile = new Grid.Tile(row, col);
            if (runnerTile == targetTile)
            {
                return true;
            }
        }

        return false;
    }
}

public class Pawn : Pieces
{
    public Pawn(bool isWhite) : base(isWhite)
    {
        IsEnPassantable = false;
    }

    public override Pieces Copy()
    {
        return new Pawn(IsWhite) { IsEnPassantable = IsEnPassantable };
    }
    
    public bool IsEnPassantable { get; set; }

    private bool IsPromotable(int row)
    {
        int end = (IsWhite) ? 0 : 7;
        return row == end;
    }

    public override IEnumerable<Grid.Tile> FieldsOnPath(Grid grid, Grid.Tile fromTile, Grid.Tile toTile)
    {
        yield return new Grid.Tile(0, 0);
    }
    
    public override MoveType DetermineMoveType(Grid grid,Grid.Tile fromTile, Grid.Tile toTile)
    {
        IsEnPassantable = false;
        
        int start = (IsWhite) ? 6 : 1;
        int dir = (IsWhite) ? 1 : -1;

        int moveTwoField = fromTile.Row - 2 * dir;
        int moveOneField = fromTile.Row - 1 * dir;

        int diffCol = toTile.Col - fromTile.Col;

        // Diagonal nehmen -> nur 1 nach links oder rechts, es muss eine Figur dort stehen, es muss das nächste Feld sein
        if (Math.Abs(diffCol) == 1 && grid[toTile] is not Empty && moveOneField == toTile.Row)
        {
            if (IsPromotable(toTile.Row)) return MoveType.Promotion;
            return MoveType.Normal;
        }
        
        // En Passant -> nur 1 nach links oder rechts, muss das nächste Feld sein, das Piece daneben muss ein Bauer sein
        if (Math.Abs(diffCol) == 1 && moveOneField == toTile.Row && grid[fromTile.Row, fromTile.Col + diffCol] is Pawn besidePawn)
        {
            // wenn das Piece daneben EnPassantable is dann bam
            if (besidePawn.IsEnPassantable)
            {
                return MoveType.EnPassant;
            }
        }
        
        // Pawn first move 2 fields
        if (fromTile.Row == start && moveTwoField == toTile.Row && fromTile.Col == toTile.Col)
        {
            // check if nextField and next-nextField is free
            if (grid[moveOneField, fromTile.Col] is Empty && grid[moveTwoField, fromTile.Col] is Empty)
            {
                IsEnPassantable = true;
                return MoveType.Normal;
            }
        }
        
        // Pawn normal move
        if (moveOneField == toTile.Row && fromTile.Col == toTile.Col)
        {
            // check if nextField is free
            if (grid[moveOneField, fromTile.Col] is Empty)
            {
                if (IsPromotable(toTile.Row)) return MoveType.Promotion;
                return MoveType.Normal;
            }
        }
        
        return MoveType.Invalid;
    }
    public override char GetIcon()
    {
        if (IsWhite) return '♟';
        else return '♙';
    }
}

public class Knight : Pieces
{
    public Knight(bool isWhite) : base(isWhite) { }
    
    public override Pieces Copy()
    {
        return new Knight(IsWhite);
    }
    
    public override IEnumerable<Grid.Tile> FieldsOnPath(Grid grid, Grid.Tile fromTile, Grid.Tile toTile)
    {
        yield return new Grid.Tile(0, 0);}

    public override MoveType DetermineMoveType(Grid grid, Grid.Tile fromTile, Grid.Tile toTile)
    {
        int diffCol = Math.Abs(toTile.Col - fromTile.Col);
        int diffRow = Math.Abs(toTile.Row - fromTile.Row);

        if (diffCol == 1 && diffRow == 2 ||
            diffCol == 2 && diffRow == 1)
        {
            return MoveType.Normal;
        }
        
        return MoveType.Invalid;
    }

    public override char GetIcon()
    {
        if (IsWhite) return '♞';
        else return '♘';
    }
}

public class Bishop : Pieces
{
    public Bishop(bool isWhite) : base(isWhite) { }

    public override Pieces Copy()
    {
        return new Bishop(IsWhite);
    }
    
    public override MoveType DetermineMoveType(Grid grid, Grid.Tile fromTile, Grid.Tile toTile)
    {
        int dirCol = (toTile.Col - fromTile.Col > 0) ? 1 : -1;
        int dirRow = (toTile.Row - fromTile.Row > 0) ? 1 : -1;
        
        if (LoopThrough(grid, fromTile, toTile, dirRow, dirCol))
        {
            return MoveType.Normal;
        }
        else
        {
            return MoveType.Invalid;
        }
    }

    public override IEnumerable<Grid.Tile> FieldsOnPath(Grid grid, Grid.Tile fromTile, Grid.Tile toTile)
    {
        int dirCol = (toTile.Col - fromTile.Col > 0) ? 1 : -1;
        int dirRow = (toTile.Row - fromTile.Row > 0) ? 1 : -1;
        
        foreach (Grid.Tile tile in grid[fromTile].AllPossibleMoves(grid, fromTile))
        {
            if (IsFieldOnPath(grid, fromTile, tile, toTile, dirRow, dirCol))
            {
                yield return tile;
            }
        }
    }

    public override char GetIcon()
    {
        if (IsWhite) return '♝';
        else return '♗';
    }
}

public class Rook : Pieces
{
    public Rook(bool isWhite) : base(isWhite)
    {
        HasMoved = true;
    }
    
    public override Pieces Copy()
    {
        return new Rook(IsWhite) {HasMoved = HasMoved};
    }
    
    public bool HasMoved { get; set; }

    public override MoveType DetermineMoveType(Grid grid, Grid.Tile fromTile, Grid.Tile toTile)
    {
        int dirCol = 0, dirRow = 0;
        
        // Rook läuft auf Cols -> Horizontal
        if (fromTile.Row == toTile.Row)
        {
            dirCol = (toTile.Col - fromTile.Col > 0) ? 1 : -1;

        }
        
        // Rook läuft auf Rows -> Vertikal
        if (fromTile.Col == toTile.Col)
        {
            dirRow = (toTile.Row - fromTile.Row > 0) ? 1 : -1;
        }

        if (LoopThrough(grid, fromTile, toTile, dirRow, dirCol))
        {
            return MoveType.Normal;
        }
        else
        {
            return MoveType.Invalid;
        }
    }
    
    public override IEnumerable<Grid.Tile> FieldsOnPath(Grid grid, Grid.Tile fromTile, Grid.Tile toTile)
    {
        int dirCol = 0, dirRow = 0;
        
        // Rook läuft auf Cols -> Horizontal
        if (fromTile.Row == toTile.Row)
        {
            dirCol = (toTile.Col - fromTile.Col > 0) ? 1 : -1;

        }
        
        // Rook läuft auf Rows -> Vertikal
        if (fromTile.Col == toTile.Col)
        {
            dirRow = (toTile.Row - fromTile.Row > 0) ? 1 : -1;
        }
        
        foreach (Grid.Tile tile in grid[fromTile].AllPossibleMoves(grid, fromTile))
        {
            if (IsFieldOnPath(grid, fromTile, tile, toTile, dirRow, dirCol))
            {
                yield return tile;
            }
        }
    }
    public override char GetIcon()
    {
        if (IsWhite) return '♜';
        else return '♖';
    }
}

public class Queen : Pieces
{
    public Queen(bool isWhite) : base(isWhite) { }

    public override Pieces Copy()
    {
        return new Queen(IsWhite);
    }

    public override MoveType DetermineMoveType(Grid grid, Grid.Tile fromTile, Grid.Tile toTile)
    {
        int diffCol = Math.Abs(toTile.Col - fromTile.Col);
        int diffRow = Math.Abs(toTile.Row - fromTile.Row);
        
        int dirCol = 0, dirRow = 0;
        
        // Queen läuft auf Cols -> Horizontal
        if (fromTile.Row == toTile.Row)
        {
            dirCol = (toTile.Col - fromTile.Col > 0) ? 1 : -1;
        }
        
        // Queen läuft auf Rows -> Vertikal
        if (fromTile.Col == toTile.Col)
        {
            dirRow = (toTile.Row - fromTile.Row > 0) ? 1 : -1;
        }

        // Queen läuft diagonal
        if (diffCol == diffRow)
        {
            dirCol = (toTile.Col - fromTile.Col > 0) ? 1 : -1;
            dirRow = (toTile.Row - fromTile.Row > 0) ? 1 : -1;
        }

        if (LoopThrough(grid, fromTile, toTile, dirRow, dirCol))
        {
            return MoveType.Normal;
        }
        else
        {
            return MoveType.Invalid;
        }
    }
    
    public override IEnumerable<Grid.Tile> FieldsOnPath(Grid grid, Grid.Tile fromTile, Grid.Tile toTile)
    {
        int diffCol = Math.Abs(toTile.Col - fromTile.Col);
        int diffRow = Math.Abs(toTile.Row - fromTile.Row);
        
        int dirCol = 0, dirRow = 0;
        
        // Queen läuft auf Cols -> Horizontal
        if (fromTile.Row == toTile.Row)
        {
            dirCol = (toTile.Col - fromTile.Col > 0) ? 1 : -1;
        }
        
        // Queen läuft auf Rows -> Vertikal
        if (fromTile.Col == toTile.Col)
        {
            dirRow = (toTile.Row - fromTile.Row > 0) ? 1 : -1;
        }

        // Queen läuft diagonal
        if (diffCol == diffRow)
        {
            dirCol = (toTile.Col - fromTile.Col > 0) ? 1 : -1;
            dirRow = (toTile.Row - fromTile.Row > 0) ? 1 : -1;
        }
        
        foreach (Grid.Tile tile in grid[fromTile].AllPossibleMoves(grid, fromTile))
        {
            if (IsFieldOnPath(grid, fromTile, tile, toTile, dirRow, dirCol))
            {
                yield return tile;
            }
        }
    }

    public override char GetIcon()
    {
        if (IsWhite) return '♛';
        else return '♕';
    }
}

public class King : Pieces
{
    public King(bool isWhite) : base(isWhite)
    {
        HasMoved = false;
    }
    
    public override Pieces Copy()
    {
        return new King(IsWhite) {HasMoved = HasMoved};
    }
    
    public bool HasMoved { get; set; }
    
    public override IEnumerable<Grid.Tile> FieldsOnPath(Grid grid, Grid.Tile fromTile, Grid.Tile toTile)
    {
        yield return new Grid.Tile(0, 0);}

    public override MoveType DetermineMoveType(Grid grid, Grid.Tile fromTile, Grid.Tile toTile)
    {
        int diffCol = toTile.Col - fromTile.Col;
        int diffRow = toTile.Row - fromTile.Row;
        
        int start = (IsWhite) ? 7 : 0;

        int rookCol = (diffCol > 0) ? 7 : 0;
        int dirCol = (diffCol > 0) ? 1 : -1;

        if (Math.Abs(diffCol) <= 1 && Math.Abs(diffRow) <= 1)
        {
            return MoveType.Normal;
        }
        
        if (!HasMoved && grid[start, rookCol] is Rook { HasMoved: false })
        {
            if (LoopThrough(grid, fromTile, toTile, 0, dirCol))
            {
                return MoveType.Castling;
            }
        }
        
        return MoveType.Invalid;
    }
    public override char GetIcon()
    {
        if (IsWhite) return '♚';
        else return '♔';
    }
}

public class Empty : Pieces
{
    public Empty() : base(false) { }
    
    public override Pieces Copy()
    {
        return new Empty();
    }
    
    public override IEnumerable<Grid.Tile> FieldsOnPath(Grid grid, Grid.Tile fromTile, Grid.Tile toTile)
    {
        yield return new Grid.Tile(0, 0);}
    public override MoveType DetermineMoveType(Grid grid, Grid.Tile fromTile, Grid.Tile toTile) => MoveType.Invalid;
    public override char GetIcon() => ' ';
}