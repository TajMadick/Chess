namespace Schach;
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
}

public class Pawn(bool isWhite) : Pieces(isWhite)
{
    public override Pieces Copy()
    {
        return new Pawn(IsWhite) { IsEnPassantable = IsEnPassantable, EnPassantExpiresIn = EnPassantExpiresIn };
    }
    public bool IsEnPassantable { get; set; } = false;
    public int EnPassantExpiresIn { get; set; } = 0;
    private bool IsPromotable(int row) => row == (IsWhite ? 0 : 7);
    public override Types.MoveType DetermineMoveType(Grid grid, Grid.Tile fromTile, Grid.Tile toTile)
    {
        int start = (IsWhite) ? 6 : 1;
        int dir = (IsWhite) ? 1 : -1;

        int moveTwoField = fromTile.Row - 2 * dir;
        int moveOneField = fromTile.Row - 1 * dir;

        int diffCol = toTile.Col - fromTile.Col;
        
        // diagonal nehmen
        if (Math.Abs(diffCol) == 1 && moveOneField == toTile.Row)
        {
            // es steht ganz normal eine Figur da
            if (grid[toTile] is not Empty)
            {
                if (IsPromotable(toTile.Row)) return Types.MoveType.Promotion;
                return Types.MoveType.Normal;
            }

            // der Bauer daneben ist EnPassantable
            if (grid[fromTile.Row, fromTile.Col + diffCol] is Pawn { IsEnPassantable: true })
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
                return Types.MoveType.DoubleStepPawn;
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

public class Empty() : Pieces(false)
{
    public override Pieces Copy() => new Empty();
    public override Types.MoveType DetermineMoveType(Grid grid, Grid.Tile fromTile, Grid.Tile toTile) => Types.MoveType.Invalid;
    public override char GetIcon() => ' ';
}