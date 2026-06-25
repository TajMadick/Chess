namespace Schach;

public abstract class SlidingPieces(bool isWhite) : Pieces(isWhite)
{
    // für alle Sliding Pieces gilt:
    // es wird nicht extra gecheckt darf diese Figur so fahren so oder so
    // es wird von dieser Funktion die Direction ausgespuckt und wenn die Figur so nicht fahren darf is die direction 0,0
    // Dann im IsFieldOnPath bzw in der Runner werden die Runner Coordianten += die Richtung gemacht 
    // Dadurch das sich da nichts tut ist es knapp außerhalb der Bounds also gleich abbruch -> false
    // Edit: Ist nicht mehr aktuell, da ich in den Child Klassen gleich von Beginn an prüfe jetzt
    // ob Figur in diese Richtung fahren darf finde die alte Lösung trotzdem spannend und würde auch ohne diese Checks
    // funktionieren ist aber bissl gemein da man sich nicht auskennt
    protected Grid.Tile DetermineDirection(Grid.Tile fromTile, Grid.Tile toTile)
    {
        Grid.Tile direction = new Grid.Tile();

        if (this is not Rook)   // Rook darf nicht diagonal laufen
        {
            if (IsDiagonal(fromTile, toTile))
            {
                direction.Col = (toTile.Col - fromTile.Col > 0) ? 1 : -1;
                direction.Row = (toTile.Row - fromTile.Row > 0) ? 1 : -1;
            }   
        }

        if (this is not Bishop) // Bishop darf nicht gerade laufen
        {
            if (IsVertical(fromTile, toTile))
            {
                direction.Row = (toTile.Row - fromTile.Row > 0) ? 1 : -1;
            }
            
            if (IsHorizontal(fromTile, toTile))
            {
                direction.Col = (toTile.Col - fromTile.Col > 0) ? 1 : -1;
            }
        }

        return direction;
    }
    protected bool IsDiagonal(Grid.Tile fromTile, Grid.Tile toTile)
    {
        int diffCol = Math.Abs(toTile.Col - fromTile.Col);
        int diffRow = Math.Abs(toTile.Row - fromTile.Row);

        return diffRow == diffCol;
    }

    // läuft auf Cols -> Horizontal
    protected bool IsHorizontal(Grid.Tile fromTile, Grid.Tile toTile) =>
        fromTile.Row == toTile.Row;
    
    // läuft auf Rows -> Vertikal
    protected bool IsVertical(Grid.Tile fromTile, Grid.Tile toTile) =>
        fromTile.Col == toTile.Col;
    
        private bool Runner(Grid grid, Grid.Tile fromTile, Grid.Tile targetTile, Grid.Tile lowerBound, Grid.Tile upperBound, Grid.Tile direction)
    {
        // gleich mal eins in die Richtung gehen damit man nicht auf selben Feld startet
        int row = fromTile.Row += direction.Row;
        int col = fromTile.Col += direction.Col;
        
        for (; row > lowerBound.Row && row < upperBound.Row && col > lowerBound.Col && col < upperBound.Col; 
             row += direction.Row, col += direction.Col)
        {
            Grid.Tile runnerTile = new Grid.Tile(row, col);
            // wenn die Runner es bis zum Ziel geschafft haben -> true
            if (runnerTile == targetTile)
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
    protected bool IsFieldOnPath(Grid grid, Grid.Tile fromTile, Grid.Tile targetTile, Grid.Tile toTile, Grid.Tile direction)
    {
        Grid.Tile lowerBound = new Grid.Tile(-1, -1);
        Grid.Tile upperBound = new Grid.Tile(8, 8);
        
        // für die einzelnen Pieces da dort das target immer toTile ist
        // toTile muss eins weiter gemacht werden da sonst die Schleife dort abbrechen würde
        if (targetTile == toTile)
        {
            toTile.Row += direction.Row;
            toTile.Col += direction.Col;
        }
        
        // wenns gleich ist solls keine Bounds geben
        // damit wenn zb ein Rook fährt die Schleife nicht abbricht
        // weil unnötige Bounds gesetzt sind in eine Richtung in die er sich gar nicht bewegt
        if (fromTile.Row != toTile.Row)
        {
            lowerBound.Row = (fromTile.Row < toTile.Row) ? fromTile.Row : toTile.Row;
            upperBound.Row = (fromTile.Row < toTile.Row) ? toTile.Row : fromTile.Row;
        }

        if (fromTile.Col != toTile.Col)
        {
            lowerBound.Col = (fromTile.Col < toTile.Col) ? fromTile.Col : toTile.Col;
            upperBound.Col = (fromTile.Col < toTile.Col) ? toTile.Col : fromTile.Col;
        }

        return Runner(grid, fromTile, targetTile, lowerBound, upperBound, direction);
    }
    public IEnumerable<Grid.Tile> AllFieldsOnPath(Grid grid, Grid.Tile fromTile, Grid.Tile toTile)
    {
        Grid.Tile direction = DetermineDirection(fromTile, toTile);
        
        foreach (Grid.Tile tile in grid[fromTile].AllMoves(grid, fromTile))
        {
            if (IsFieldOnPath(grid, fromTile, toTile, direction, tile))
            {
                yield return tile;
            }
        }
    }
}

public class Bishop(bool isWhite) : SlidingPieces(isWhite)
{
    public override Pieces Copy()
    {
        return new Bishop(IsWhite);
    }
    
    public override Types.MoveType DetermineMoveType(Grid grid, Grid.Tile fromTile, Grid.Tile toTile)
    {
        // wenn Bishop NICHT Diagonal -> INVALID
        if (!IsDiagonal(fromTile, toTile)) return Types.MoveType.Invalid;
        
        Grid.Tile direction = DetermineDirection(fromTile, toTile);
        
        if (IsFieldOnPath(grid, fromTile, targetTile:toTile, toTile, direction))
        {
            return Types.MoveType.Normal;
        }
        else
        {
            return Types.MoveType.Invalid;
        }
    }

    public override char GetIcon() => (IsWhite) ? '♝' : '♗';
}

public class Rook(bool isWhite) : SlidingPieces(isWhite)
{
    public override Pieces Copy()
    {
        return new Rook(IsWhite) {HasMoved = HasMoved};
    }
    
    public bool HasMoved { get; set; } = true;

    public override Types.MoveType DetermineMoveType(Grid grid, Grid.Tile fromTile, Grid.Tile toTile)
    {
        // wenn Rook NICHT Vertical ODER Horizontal -> INVALID
        if (!(IsHorizontal(fromTile, toTile) || IsVertical(fromTile, toTile))) return Types.MoveType.Invalid;
        
        Grid.Tile direction = DetermineDirection(fromTile, toTile);

        if (IsFieldOnPath(grid, fromTile, targetTile:toTile, toTile, direction))
        {
            return Types.MoveType.Normal;
        }
        else
        {
            return Types.MoveType.Invalid;
        }
    }
    public override char GetIcon() => (IsWhite) ? '♜' : '♖';
}

public class Queen(bool isWhite) : SlidingPieces(isWhite)
{
    public override Pieces Copy()
    {
        return new Queen(IsWhite);
    }

    public override Types.MoveType DetermineMoveType(Grid grid, Grid.Tile fromTile, Grid.Tile toTile)
    {
        // wenn Queen NICHT Vertical ODER Horizontal ODER Diagonal -> INVALID
        if (!(IsHorizontal(fromTile, toTile) || IsVertical(fromTile, toTile) || IsDiagonal(fromTile, toTile)))
            return Types.MoveType.Invalid;
        
        Grid.Tile direction = DetermineDirection(fromTile, toTile);

        if (IsFieldOnPath(grid, fromTile, targetTile:toTile, toTile, direction))
        {
            return Types.MoveType.Normal;
        }
        else
        {
            return Types.MoveType.Invalid;
        }
    }

    public override char GetIcon() => (IsWhite) ? '♛' :'♕';
}

public class King(bool isWhite) : SlidingPieces(isWhite)
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
            if (IsFieldOnPath(grid, fromTile, targetTile:toTile, toTile, direction))
            {
                return Types.MoveType.Castling;
            }
        }
        
        return Types.MoveType.Invalid;
    }
    public override char GetIcon() => (IsWhite) ? '♚' : '♔';
}