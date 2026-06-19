namespace Schach;

public class Board
{
    private Pieces[,] boardPieces = new Pieces[8,8];
    public Board()
    {
        FillBoard();
    }

    public struct Tile(int row, int col)
    {
        public int Row = row;
        public int Col = col;
    }
    
    public void DrawBoard()
    {
        Console.Clear();
        Console.WriteLine("    a   b   c   d   e   f   g   h");
        for (int i = 0; i < 8; i++)
        {
            Console.Write($" {8-i} ");
            for (int j = 0; j < 8; j++)
            {
                Console.Write($"[{boardPieces[i, j].GetIcon()} ]");
            }
            Console.Write($" {8-i} ");
            Console.WriteLine();
        }
        Console.WriteLine("    a   b   c   d   e   f   g   h");
    }

    public bool InputMove(string move, bool isWhiteMoving)
    {
        if (!Rules.PassesSanityChecks(move, boardPieces, isWhiteMoving))
        {
            return false;
        }
        
        Rules.CalculateCoordinates(move, out int fromRow, out int fromCol, out int toRow, out int toCol);

        ref Pieces fromTile = ref boardPieces[fromRow, fromCol];
        ref Pieces toTile = ref boardPieces[toRow, toCol];

        Pieces.MoveType determinedMoveType = fromTile.DetermineMoveType(fromRow, fromCol,
            toRow, toCol, boardPieces);

        if (determinedMoveType == Pieces.MoveType.Promotion)
        {
            if (MovePiece(ref fromTile, ref toTile, isWhiteMoving))
            { 
                toTile = new Queen(isWhiteMoving);
                return true;
            }
            else
            {
                return false;
            }
        }
        
        if (determinedMoveType == Pieces.MoveType.EnPassant)
        {
            if (MovePiece(ref fromTile, ref toTile, isWhiteMoving))
            { 
                int diffCol = toCol - fromCol;
                
                // Bauern daneben schlagen
                boardPieces[fromRow, fromCol + diffCol] = new Empty();
                return true;
            }
            else
            {
                return false;
            }
        }
        
        if (determinedMoveType == Pieces.MoveType.Castling)
        {
            int diffCol = toCol - fromCol;
            
            // note bei castling ist fromRow immer toRow
            int dir = (diffCol > 0) ? 1 : -1;
            int rookCol = (diffCol > 0) ? 7 : 0;
            
            // Tiles dazwischen checken, ob in check und mit toCol+dir auch das Tile wohin King will
            for (int col = fromCol; col != (toCol+dir); col += dir)
            {
                if (Rules.IsCheck(boardPieces, new Tile(toRow, col), isWhiteMoving))
                {
                    Console.Write("Can't castle! Tiles are in check");
                    Console.ReadKey();
                    return false;
                }
            }

            ref Pieces fromRookTile = ref boardPieces[fromRow, rookCol];
            ref Pieces toRookTile = ref boardPieces[fromRow, fromCol + dir];

            // moving rook
            toRookTile = fromRookTile;
            fromRookTile = new Empty();

            // moving king
            toTile = fromTile;
            fromTile = new Empty();

            return true;
        }
        
        if (determinedMoveType == Pieces.MoveType.Normal)
        {
            if (MovePiece(ref fromTile, ref toTile, isWhiteMoving))
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

    public Tile FindKing(bool isWhite)
    {
        // König finden
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (boardPieces[i, j] is King && boardPieces[i, j].IsWhite == isWhite)
                {
                    return new Tile(i, j);

                }
            }
        }

        return new Tile(0, 0);
    }

    private bool MovePiece(ref Pieces fromTile, ref Pieces toTile, bool isWhiteMoving)
    {
        if (fromTile is King king) king.HasMoved = true;
        if (fromTile is Rook rook) rook.HasMoved = true;

        Pieces oldToTile = toTile;
        toTile = fromTile;
        fromTile = new Empty();

        // Check if King will be in Check after moving
        if (Rules.IsCheck(boardPieces,FindKing(isWhiteMoving), isWhiteMoving))
        {
            fromTile = toTile;
            toTile = oldToTile;

            Console.Write($"Your King will be in Check");
            Console.ReadKey();
            return false;
        }

        return true;
    }
    private void FillBoard()
    {
        // Empty
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                boardPieces[i, j] = new Empty();
            }
        }
        
        // Pawn
        for (int i = 0; i < 8; i++)
        {
            boardPieces[1, i] = new Pawn(false);
            boardPieces[6, i] = new Pawn(true);
        }
        
        // Rook
        boardPieces[0, 0] = new Rook(false);
        boardPieces[0, 7] = new Rook(false);
        boardPieces[7, 0] = new Rook(true);
        boardPieces[7, 7] = new Rook(true);
        
        // Knight
        boardPieces[0, 1] = new Knight(false);
        boardPieces[0, 6] = new Knight(false);
        boardPieces[7, 1] = new Knight(true);
        boardPieces[7, 6] = new Knight(true);
        
        // Bishop
        boardPieces[0, 2] = new Bishop(false);
        boardPieces[0, 5] = new Bishop(false);
        boardPieces[7, 2] = new Bishop(true);
        boardPieces[7, 5] = new Bishop(true);
        
        // Queen
        boardPieces[0, 3] = new Queen(false);
        boardPieces[7, 3] = new Queen(true);
        
        // King
        boardPieces[0, 4] = new King(false);
        boardPieces[7, 4] = new King(true);
    }
}