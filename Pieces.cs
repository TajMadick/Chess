namespace Schach;

internal interface ISlidingPieces
{
    IEnumerable<Grid.Tile> FieldsOnPath(Grid grid, Grid.Tile fromTile, Grid.Tile toTile);
}
public abstract class Pieces(bool isWhite)
{
    public bool IsWhite { get; } = isWhite;

    public abstract char GetIcon();
    public abstract Pieces Copy();
    public abstract Types.MoveType DetermineMoveType(Grid grid, Grid.Tile fromTile, Grid.Tile toTile);
    public IEnumerable<Grid.Tile> AllMoves(Grid grid, Grid.Tile fromTile)
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Grid.Tile toTile = new Grid.Tile(row, col);
                
                if (fromTile != toTile &&
                    grid[fromTile].DetermineMoveType(grid, fromTile, toTile) != Types.MoveType.Invalid)
                {
                    yield return toTile;
                }
            }
        }
    }
    
    protected Grid.Tile DetermineDirection(Grid.Tile fromTile, Grid.Tile toTile)
    {
        int diffCol = Math.Abs(toTile.Col - fromTile.Col);
        int diffRow = Math.Abs(toTile.Row - fromTile.Row);

        Grid.Tile direction = new Grid.Tile();
        
        // läuft auf Cols -> Horizontal
        if (fromTile.Row == toTile.Row)
        {
            direction.Col = (toTile.Col - fromTile.Col > 0) ? 1 : -1;
        }
        
        // läuft auf Rows -> Vertikal
        if (fromTile.Col == toTile.Col)
        {
            direction.Row = (toTile.Row - fromTile.Row > 0) ? 1 : -1;
        }

        // läuft diagonal
        if (diffCol == diffRow)
        {
            direction.Col = (toTile.Col - fromTile.Col > 0) ? 1 : -1;
            direction.Row = (toTile.Row - fromTile.Row > 0) ? 1 : -1;
        }

        return direction;
    }
    protected bool LoopThrough(Grid grid, Grid.Tile fromTile, Grid.Tile toTile, Grid.Tile direction)
    {
        // gleich mal eins in die Richtung gehen damit man nicht auf selben Feld startet
        int row = fromTile.Row += direction.Row;
        int col = fromTile.Col += direction.Col;
        
        for (; row is >= 0 and < 8 && col is >= 0 and < 8; row += direction.Row, col += direction.Col)
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
    
    // Todo: kann man safe noch besser machen
    protected bool IsFieldOnPath(Grid.Tile fromTile, Grid.Tile targetTile, Grid.Tile toTile, Grid.Tile direction)
    {
        int lowerBoundRow = (fromTile.Row < toTile.Row) ? toTile.Row : -1;
        int upperBoundRow = (fromTile.Row < toTile.Row) ? 8 : toTile.Row;
        int lowerBoundCol = (fromTile.Col < toTile.Col) ? toTile.Col : -1;
        int upperBoundCol = (fromTile.Col < toTile.Col) ? 8 : toTile.Col;

        int row = fromTile.Row;
        int col = fromTile.Col;
        
        for (; row > lowerBoundRow && row < upperBoundRow && col > lowerBoundCol && col < upperBoundCol; 
             row += direction.Row, col += direction.Col)
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

public class Pawn(bool isWhite) : Pieces(isWhite)
{
    public override Pieces Copy()
    {
        return new Pawn(IsWhite) { IsEnPassantable = IsEnPassantable };
    }
    public bool IsEnPassantable { get; set; } = false;
    private bool IsPromotable(int row) => row == (IsWhite ? 0 : 7);
    public override Types.MoveType DetermineMoveType(Grid grid,Grid.Tile fromTile, Grid.Tile toTile)
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
            if (IsPromotable(toTile.Row)) return Types.MoveType.Promotion;
            return Types.MoveType.Normal;
        }
        
        // En Passant -> nur 1 nach links oder rechts, muss das nächste Feld sein, das Piece daneben muss ein Bauer sein
        if (Math.Abs(diffCol) == 1 && moveOneField == toTile.Row && grid[fromTile.Row, fromTile.Col + diffCol] is Pawn besidePawn)
        {
            // wenn das Piece daneben EnPassantable is dann bam
            if (besidePawn.IsEnPassantable)
            {
                return Types.MoveType.EnPassant;
            }
        }
        
        // Pawn first move 2 fields
        if (fromTile.Row == start && moveTwoField == toTile.Row && fromTile.Col == toTile.Col)
        {
            // check if nextField and next-nextField is free
            if (grid[moveOneField, fromTile.Col] is Empty && grid[moveTwoField, fromTile.Col] is Empty)
            {
                IsEnPassantable = true;
                return Types.MoveType.Normal;
            }
        }
        
        // Pawn normal move
        if (moveOneField == toTile.Row && fromTile.Col == toTile.Col)
        {
            // check if nextField is free
            if (grid[moveOneField, fromTile.Col] is Empty)
            {
                if (IsPromotable(toTile.Row)) return Types.MoveType.Promotion;
                return Types.MoveType.Normal;
            }
        }
        
        return Types.MoveType.Invalid;
    }
    public override char GetIcon() => (IsWhite) ? '♟' : '♙';
}

public class Knight(bool isWhite) : Pieces(isWhite)
{
    public override Pieces Copy()
    {
        return new Knight(IsWhite);
    }
    public override Types.MoveType DetermineMoveType(Grid grid, Grid.Tile fromTile, Grid.Tile toTile)
    {
        int diffCol = Math.Abs(toTile.Col - fromTile.Col);
        int diffRow = Math.Abs(toTile.Row - fromTile.Row);

        if (diffCol == 1 && diffRow == 2 ||
            diffCol == 2 && diffRow == 1)
        {
            return Types.MoveType.Normal;
        }
        
        return Types.MoveType.Invalid;
    }

    public override char GetIcon() =>(IsWhite) ? '♞' : '♘';
}

public class Bishop(bool isWhite) : Pieces(isWhite), ISlidingPieces
{
    public override Pieces Copy()
    {
        return new Bishop(IsWhite);
    }
    
    public override Types.MoveType DetermineMoveType(Grid grid, Grid.Tile fromTile, Grid.Tile toTile)
    {
        Grid.Tile direction = DetermineDirection(fromTile, toTile);
        
        if (LoopThrough(grid, fromTile, toTile, direction))
        {
            return Types.MoveType.Normal;
        }
        else
        {
            return Types.MoveType.Invalid;
        }
    }
    public IEnumerable<Grid.Tile> FieldsOnPath(Grid grid, Grid.Tile fromTile, Grid.Tile toTile)
    {
        Grid.Tile direction = DetermineDirection(fromTile, toTile);
        
        foreach (Grid.Tile tile in grid[fromTile].AllMoves(grid, fromTile))
        {
            if (IsFieldOnPath(fromTile, tile, toTile, direction))
            {
                yield return tile;
            }
        }
    }

    public override char GetIcon() => (IsWhite) ? '♝' : '♗';
}

public class Rook(bool isWhite) : Pieces(isWhite), ISlidingPieces
{
    public override Pieces Copy()
    {
        return new Rook(IsWhite) {HasMoved = HasMoved};
    }
    
    public bool HasMoved { get; set; } = true;

    public override Types.MoveType DetermineMoveType(Grid grid, Grid.Tile fromTile, Grid.Tile toTile)
    {
        Grid.Tile direction = DetermineDirection(fromTile, toTile);

        if (LoopThrough(grid, fromTile, toTile, direction))
        {
            return Types.MoveType.Normal;
        }
        else
        {
            return Types.MoveType.Invalid;
        }
    }
    
    public IEnumerable<Grid.Tile> FieldsOnPath(Grid grid, Grid.Tile fromTile, Grid.Tile toTile)
    {
        Grid.Tile direction = DetermineDirection(fromTile, toTile);
        
        foreach (Grid.Tile tile in grid[fromTile].AllMoves(grid, fromTile))
        {
            if (IsFieldOnPath(fromTile, tile, toTile, direction))
            {
                yield return tile;
            }
        }
    }
    public override char GetIcon() => (IsWhite) ? '♜' : '♖';
}

public class Queen(bool isWhite) : Pieces(isWhite), ISlidingPieces
{
    public override Pieces Copy()
    {
        return new Queen(IsWhite);
    }

    public override Types.MoveType DetermineMoveType(Grid grid, Grid.Tile fromTile, Grid.Tile toTile)
    {
        Grid.Tile direction = DetermineDirection(fromTile, toTile);

        if (LoopThrough(grid, fromTile, toTile, direction))
        {
            return Types.MoveType.Normal;
        }
        else
        {
            return Types.MoveType.Invalid;
        }
    }
    
    // Todo: Schiach besser machen
    public IEnumerable<Grid.Tile> FieldsOnPath(Grid grid, Grid.Tile fromTile, Grid.Tile toTile)
    {
        Grid.Tile direction = DetermineDirection(fromTile, toTile);
        
        foreach (Grid.Tile tile in grid[fromTile].AllMoves(grid, fromTile))
        {
            if (IsFieldOnPath(fromTile, tile, toTile, direction))
            {
                yield return tile;
            }
        }
    }

    public override char GetIcon() => (IsWhite) ? '♛' :'♕';
}

public class King(bool isWhite) : Pieces(isWhite)
{
    public override Pieces Copy()
    {
        return new King(IsWhite) {HasMoved = HasMoved};
    }
    
    public bool HasMoved { get; set; } = false;

    public override Types.MoveType DetermineMoveType(Grid grid, Grid.Tile fromTile, Grid.Tile toTile)
    {
        int diffCol = toTile.Col - fromTile.Col;
        int diffRow = toTile.Row - fromTile.Row;

        if (Math.Abs(diffCol) <= 1 && Math.Abs(diffRow) <= 1)
        {
            return Types.MoveType.Normal;
        }
        
        // soll nur horizontale direction herausfinden deshalb row = 0
        Grid.Tile direction = DetermineDirection(fromTile, toTile);
        direction.Row = 0;
        
        int start = (IsWhite) ? 7 : 0;
        int rookCol = (diffCol > 0) ? 7 : 0;
        
        if (!HasMoved && grid[start, rookCol] is Rook { HasMoved: false })
        {
            if (LoopThrough(grid, fromTile, toTile, direction))
            {
                return Types.MoveType.Castling;
            }
        }
        
        return Types.MoveType.Invalid;
    }
    public override char GetIcon() => (IsWhite) ? '♚' : '♔';
}

public class Empty() : Pieces(false)
{
    public override Pieces Copy() => new Empty();
    public override Types.MoveType DetermineMoveType(Grid grid, Grid.Tile fromTile, Grid.Tile toTile) => Types.MoveType.Invalid;
    public override char GetIcon() => ' ';
}