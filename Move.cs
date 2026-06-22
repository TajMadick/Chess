using System.Data;

namespace Schach;

public static class Move
{
    public static bool IsCheckmatePostMove(Board board, bool isWhiteMoving)
    {
        Board.Tile kingPos = FindKing(board, isWhiteMoving);
        if (Rules.IsCheck(board, kingPos, isWhiteMoving))
        {
            if (Rules.IsCheckmate(board, kingPos, isWhiteMoving))
            {
                return true;
            }
        }

        return false;
    }
    
    public static bool InputMove(Board board, string move, bool isWhiteMoving)
    {
        if (!Rules.PassesSanityChecks(board, move, isWhiteMoving))
        {
            return false;
        }
        
        Rules.CalculateCoordinates(move, out Board.Tile fromTile, out Board.Tile toTile);

        ref Pieces fromPiece = ref board.GetRef(fromTile);
        ref Pieces toPiece = ref board.GetRef(toTile);

        Pieces.MoveType determinedMoveType = fromPiece.DetermineMoveType(board, fromTile, toTile);

        if (determinedMoveType == Pieces.MoveType.Promotion)
        {
            if (MovePiece(board, fromTile, toTile, isWhiteMoving))
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
            if (MovePiece(board, fromTile, toTile, isWhiteMoving))
            { 
                int diffCol = toTile.Col - fromTile.Col;
                
                // Bauern daneben schlagen
                board[fromTile.Row, fromTile.Col + diffCol] = new Empty();
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
                if (Rules.IsCheck(board, new Board.Tile(toTile.Row, col), isWhiteMoving))
                {
                    Console.Write("Can't castle! Tiles are in check");
                    Console.ReadKey();
                    return false;
                }
            }

            ref Pieces fromRookTile = ref board.GetRef(fromTile.Row, rookCol);
            ref Pieces toRookTile = ref board.GetRef(fromTile.Row, fromTile.Col + dir);

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
            if (MovePiece(board, fromTile, toTile, isWhiteMoving))
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

    private static Board.Tile FindKing(Board board, bool isWhite)
    {
        // König finden
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Board.Tile kingTile = new Board.Tile(row, col);
                if (board[kingTile] is King && board[kingTile].IsWhite == isWhite)
                {
                    return kingTile;
                }
            }
        }

        return new Board.Tile(0, 0);
    }

    private static bool MovePiece(Board board, Board.Tile fromTile, Board.Tile  toTile, bool isWhiteMoving)
    {
        if (board[fromTile] is King king) king.HasMoved = true;
        if (board[fromTile] is Rook rook) rook.HasMoved = true;

        Pieces oldToTile = board[toTile];
        board[toTile] = board[fromTile];
        board[fromTile] = new Empty();

        // Check if King will be in Check after moving
        Board temporaryBoard = Rules.TemporaryMove(board, fromTile, toTile);
        if (Rules.IsCheck(temporaryBoard,FindKing(temporaryBoard, isWhiteMoving), isWhiteMoving))
        {
            board[fromTile] = board[toTile];
            board[toTile] = oldToTile;

            Console.Write($"Your King will be in Check");
            Console.ReadKey();
            return false;
        }

        return true;
    }
}