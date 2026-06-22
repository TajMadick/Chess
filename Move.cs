
namespace Schach;

public static class Move
{
    public static bool InputMove(Grid grid, string move, bool isWhiteMoving)
    {
        if (!Rules.PassesSanityChecks(grid, move, isWhiteMoving))
        {
            return false;
        }
        
        Rules.CalculateCoordinates(move, out Grid.Tile fromTile, out Grid.Tile toTile);

        ref Pieces fromPiece = ref grid.GetRef(fromTile);
        ref Pieces toPiece = ref grid.GetRef(toTile);

        Pieces.MoveType determinedMoveType = fromPiece.DetermineMoveType(grid, fromTile, toTile);

        if (determinedMoveType == Pieces.MoveType.Promotion)
        {
            if (MovePiece(grid, fromTile, toTile, isWhiteMoving))
            { 
                toPiece = new Queen(isWhiteMoving);
                return true;
            }
            else
            {
                return false;
            }
        }
        
        if (determinedMoveType == Pieces.MoveType.EnPassant)
        {
            if (MovePiece(grid, fromTile, toTile, isWhiteMoving))
            { 
                int diffCol = toTile.Col - fromTile.Col;
                
                // Bauern daneben schlagen
                grid[fromTile.Row, fromTile.Col + diffCol] = new Empty();
                return true;
            }
            else
            {
                return false;
            }
        }
        
        if (determinedMoveType == Pieces.MoveType.Castling)
        {
            int diffCol = toTile.Col - fromTile.Col;
            
            // note bei castling ist fromRow immer toRow
            int dir = (diffCol > 0) ? 1 : -1;
            int rookCol = (diffCol > 0) ? 7 : 0;
            
            // Tiles dazwischen checken, ob in check und mit toCol+dir auch das Tile wohin King will
            // TODO Schon wieder bissl her schauen warum überhaupt +dir
            for (int col = fromTile.Col; col != (toTile.Col+dir); col += dir)
            {
                if (Rules.IsCheck(grid, new Grid.Tile(toTile.Row, col), isWhiteMoving))
                {
                    Console.Write("Can't castle! Tiles are in check");
                    Console.ReadKey();
                    return false;
                }
            }

            ref Pieces fromRookTile = ref grid.GetRef(fromTile.Row, rookCol);
            ref Pieces toRookTile = ref grid.GetRef(fromTile.Row, fromTile.Col + dir);

            // moving rook
            toRookTile = fromRookTile;
            fromRookTile = new Empty();

            // moving king
            toPiece = fromPiece;
            fromPiece = new Empty();

            return true;
        }
        
        if (determinedMoveType == Pieces.MoveType.Normal)
        {
            if (MovePiece(grid, fromTile, toTile, isWhiteMoving))
            { 
                return true;
            }
            else
            {
                return false;
            }
        }
        
        if (determinedMoveType == Pieces.MoveType.Invalid)
        {
            Console.WriteLine("Incorrect movement");
            Console.ReadKey();
            return false;
        }

        return false;
    }

    private static bool MovePiece(Grid grid, Grid.Tile fromTile, Grid.Tile  toTile, bool isWhiteMoving)
    {
        if (grid[fromTile] is King king) king.HasMoved = true;
        if (grid[fromTile] is Rook rook) rook.HasMoved = true;
        
        // Check if King will be in Check after moving
        Grid temporaryGrid = Rules.TemporaryMove(grid, fromTile, toTile);
        if (Rules.IsCheck(temporaryGrid,Rules.FindKing(temporaryGrid, isWhiteMoving), isWhiteMoving))
        {
            Console.Write($"Your King will be in Check");
            Console.ReadKey();
            return false;
        }
        else
        {
            // Kein Check man kann bewegen
            grid[toTile] = grid[fromTile];
            grid[fromTile] = new Empty();
            return true;
        }
    }
}