namespace Schach;

public abstract class Move(Grid grid, Grid.Tile fromTile, Grid.Tile toTile)
{
    protected readonly Grid.Tile FromTile = fromTile;
    protected readonly Grid.Tile ToTile = toTile;
    protected readonly Pieces OriginalFromPiece = grid[fromTile];
    protected readonly Pieces OriginalToPiece = grid[toTile];
    private bool OriginalHasMoved;

    protected void DoStandardMove(Grid grid)
    {
        grid[ToTile] = OriginalFromPiece;
        grid[FromTile] = new Empty();
        
        if (grid[ToTile] is King king) OriginalHasMoved = king.HasMoved;
        if (grid[ToTile] is Rook rook) OriginalHasMoved = rook.HasMoved;
    }
    protected void UndoStandardMove(Grid grid)
    {
        grid[ToTile] = OriginalToPiece;
        grid[FromTile] = OriginalFromPiece;
        
        if (grid[ToTile] is King king) king.HasMoved = OriginalHasMoved;
        if (grid[ToTile] is Rook rook) rook.HasMoved = OriginalHasMoved;
    }

    protected bool IsKingAttackedAfterMove(Grid grid, bool isWhiteMoving)
    {
        return (grid[fromTile] is King) switch
        {
            true => Rules.IsAttackingField(grid, ToTile, isWhiteMoving),
            false => Rules.IsAttackingField(grid, Rules.FindKing(grid, isWhiteMoving), isWhiteMoving)
        };
    }
    
    public abstract bool TryDoMove(Grid grid, bool isWhiteMoving);
    public abstract void UndoMove(Grid grid);
}

public class NormalMove(Grid grid, Grid.Tile fromTile, Grid.Tile toTile) : Move(grid, fromTile, toTile)
{
    public override bool TryDoMove(Grid grid, bool isWhiteMoving)
    {
        DoStandardMove(grid);
        if (IsKingAttackedAfterMove(grid, isWhiteMoving))
        {
            UndoMove(grid);
            return false;
        }

        return true;
    }
    public override void UndoMove(Grid grid)
    {
        UndoStandardMove(grid);
    }
}
public class CastlingMove : Move
{
    private Rook OriginalRook;
    private Grid.Tile RookFromTile;
    private Grid.Tile RookToTile;

    private int castlingDir;

    public CastlingMove(Grid grid, Grid.Tile fromTile, Grid.Tile toTile) : base(grid, fromTile, toTile)
    {
        int diffCol = toTile.Col - fromTile.Col;
        int rookCol = (diffCol > 0) ? 7 : 0;
        castlingDir = (diffCol > 0) ? 1 : -1;
        
        RookFromTile = new Grid.Tile(fromTile.Row, rookCol);
        RookToTile = new Grid.Tile(fromTile.Row, fromTile.Col + castlingDir);
        OriginalRook = (Rook)grid[RookFromTile];
    }
    private bool IsValid(Grid grid, bool isWhiteMoving)
    {
        // Tiles dazwischen checken, ob in check und mit toCol+CastlingDir auch das Tile wohin King will
        // +dir, weil Schleife geht nur bis zu dem toCol und bricht da ab bevor er prüft mit +CastlingDir geht er eins weiter
        for (int col = FromTile.Col; col != (ToTile.Col + castlingDir); col += castlingDir)
        {
            if (Rules.IsAttackingField(grid, new Grid.Tile(ToTile.Row, col), isWhiteMoving))
            {
                return false;
            }
        }

        return true;
    }
    public override bool TryDoMove(Grid grid, bool isWhiteMoving)
    {
        if (IsValid(grid, isWhiteMoving))
        {
            grid[RookToTile] = OriginalRook;
            grid[RookFromTile] = new Empty();

            grid[ToTile] = OriginalFromPiece;
            grid[FromTile] = new Empty();

            return true;
        }

        return false;
    }

    public override void UndoMove(Grid grid)
    {
        grid[RookToTile] = new Empty();
        grid[RookFromTile] = OriginalRook;

        grid[ToTile] = new Empty();
        grid[FromTile] = OriginalFromPiece;
    }
}
public class DoubleStepPawnMove : Move
{
    private int OriginalExpiration;
    private bool OriginalIsEnPassantable;
    
    public DoubleStepPawnMove(Grid grid, Grid.Tile fromTile, Grid.Tile toTile) : base(grid, fromTile, toTile)
    {
        Pawn pawn = (Pawn)grid[fromTile];
        OriginalIsEnPassantable = pawn.IsEnPassantable;
        OriginalExpiration = pawn.EnPassantExpiresIn;
    }
    public override bool TryDoMove(Grid grid, bool isWhiteMoving)
    {
        DoStandardMove(grid);
        
        Pawn pawn = (Pawn)grid[ToTile];
        pawn.IsEnPassantable = true;
        pawn.EnPassantExpiresIn = 2;

        if (IsKingAttackedAfterMove(grid, isWhiteMoving))
        {
            UndoMove(grid);
            return false;
        }

        return true;
    }

    public override void UndoMove(Grid grid)
    {
        UndoStandardMove(grid);
        Pawn pawn = (Pawn)grid[FromTile];
        pawn.IsEnPassantable = OriginalIsEnPassantable;
        pawn.EnPassantExpiresIn = OriginalExpiration;
    }
}

public class EnPassantMove : Move
{    
    private Pawn OriginalKilledPawn;
    private Grid.Tile KilledPawnTile;
    
    public EnPassantMove(Grid grid, Grid.Tile fromTile, Grid.Tile toTile) : base(grid, fromTile, toTile)
    {
        int diffCol = toTile.Col - fromTile.Col;
        
        KilledPawnTile = new Grid.Tile(fromTile.Row, fromTile.Col + diffCol);
        OriginalKilledPawn = (Pawn)grid[KilledPawnTile];
    }

    public override bool TryDoMove(Grid grid, bool isWhiteMoving)
    {
        DoStandardMove(grid);
        // Bauern daneben schlagen
        grid[KilledPawnTile] = new Empty();
        
        if (IsKingAttackedAfterMove(grid, isWhiteMoving))
        {
            UndoMove(grid);
            return false;
        }

        return true;
    }

    public override void UndoMove(Grid grid)
    {
        UndoStandardMove(grid);
        grid[KilledPawnTile] = OriginalKilledPawn;
    }
}

public class PromotionMove(Grid grid, Grid.Tile fromTile, Grid.Tile toTile) : Move(grid, fromTile, toTile)
{
    public override bool TryDoMove(Grid grid, bool isWhiteMoving)
    {
        grid[ToTile] = new Queen(isWhiteMoving);
        grid[FromTile] = new Empty();
        
        if (IsKingAttackedAfterMove(grid, isWhiteMoving))
        {
            UndoMove(grid);
            return false;
        }

        return true;
    }
    public override void UndoMove(Grid grid)
    {
        UndoStandardMove(grid);
    }
}

public class InvalidMove(Grid grid, Grid.Tile fromTile, Grid.Tile toTile) : Move(grid, fromTile, toTile)
{
    public override bool TryDoMove(Grid grid, bool isWhiteMoving)
    {
        return false;
    }

    public override void UndoMove(Grid grid)
    { }
}