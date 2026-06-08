using System.Text.RegularExpressions;

namespace Schach;

public class Board
{
    private Regex Rg;
    private const string Pattern = "^[a-h][0-8][a-h][0-8]$";
    private Pieces[,] boardPieces = new Pieces[8,8];
    public Board()
    {
        Rg = new Regex(Pattern);
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

    public bool MovePiece(string move, bool isWhiteMoving)
    {
        if (!Rg.IsMatch(move))
        {
            Console.Write($"Format only: {Pattern}");
            Console.ReadKey();
            return false;
        }
        
        char fromLetter = move[0];
        char fromNumber = move[1];
        char toLetter = move[2];
        char toNumber = move[3];

        Tile fromTileCoords = new Tile
        {
            Row = 7 - (fromNumber - '1'),
            Col = fromLetter - 'a'
        };

        Tile toTileCoords = new Tile
        {
            Row = 7 - (toNumber - '1'),
            Col = toLetter - 'a'
        };

        ref Pieces fromTile = ref boardPieces[fromTileCoords.Row, fromTileCoords.Col];
        ref Pieces toTile = ref boardPieces[toTileCoords.Row, toTileCoords.Col];

        // Falls versucht leeres Feld zu bewegen
        if (fromTile is Empty)
        {
            Console.Write("From Location is Empty");
            Console.ReadKey();
            return false;
        }

        // Falls versucht auf selbes Feld zu gehen (selbe Koordinaten)
        if (fromTileCoords.Row == toTileCoords.Row && fromTileCoords.Col == toTileCoords.Col)
        {
            Console.Write("From Location same as To Location");
            Console.ReadKey();
            return false;
        }
        
        // Falls versucht flasche Farbe zu bewegen
        if (fromTile.IsWhite != isWhiteMoving)
        {
            string color = (isWhiteMoving) ? "white" : "black";
            Console.Write($"You can only move {color} pieces");
            Console.ReadKey();
            return false;
        }
        
        // das Zielfeld darf nicht von der gleichen Farbe sein, 
        // wenn das Zielfeld von der gleichen Farbe ist aber leer dann passts
        if (toTile.IsWhite == isWhiteMoving && toTile is not Empty)
        {
            Console.Write("Can't take pieces from your own color");
            Console.ReadKey();
            return false;
        }
        
        if (fromTile.MoveAllowed(fromTileCoords.Row, fromTileCoords.Col, toTileCoords.Row, toTileCoords.Col, boardPieces))
        {
            // Castle Check
            int diffCol = toTileCoords.Col - fromTileCoords.Col;
            if (fromTile is King && Math.Abs(diffCol) == 2)
            {
                // note bei castling ist fromRow immer toRow
                int dir = (diffCol > 0) ? 1 : -1;
                int rookCol = (diffCol > 0) ? 7 : 0;

                // Tiles dazwischen (momentan auch king) checken ob in check und mit toTileCoords.Col+dir auch das Tile wohin King will
                // TODO für später am anfang col = ...+=dir weil ich will den king check noch machen ob der king anfang des moves schon in check ist
                for (int col = fromTileCoords.Col; col != (toTileCoords.Col+dir); col += dir)
                {
                    if (IsCheck(new Tile(toTileCoords.Row, col), isWhiteMoving))
                    {
                        Console.Write("Can't castle! Tiles are in check");
                        Console.ReadKey();
                        return false;
                    }
                }
                
                ref Pieces rookTile = ref boardPieces[fromTileCoords.Row, rookCol];

                // moving rook
                boardPieces[fromTileCoords.Row, fromTileCoords.Col + dir] = rookTile;
                rookTile = new Empty();
                
                // moving king
                toTile = fromTile;
                fromTile = new Empty();
                
                return true;
            }
            
            if (fromTile is King king) king.HasMoved = true;
            if (fromTile is Rook rook) rook.HasMoved = true; 
            
            Pieces oldToTile = toTile;
            toTile = fromTile;
            fromTile = new Empty();

            // Check if King will be in Check after moving
            if (IsCheck(FindQueen(isWhiteMoving), isWhiteMoving))
            {
                fromTile = toTile;
                toTile = oldToTile;

                Console.Write($"Your King will be in Check");
                Console.ReadKey();
                return false;
            }

            // Pawn Promotion
            if (toTile is Pawn checkPromotablePawn)
            {
                if (checkPromotablePawn.IsPromotable(toTileCoords.Row))
                {
                    toTile = new Queen(isWhiteMoving);
                }
            }
        }
        else
        {
            Console.WriteLine("Incorrect movement");
            Console.ReadKey();
            return false;
        }
        
        return true;
    }

    Tile FindQueen(bool isWhite)
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
    private bool IsCheck(Tile toTile, bool isWhite)
    {
        // Bei jeder Figuren schauen, ob sie den König schlagen kann
        for (int fromRow = 0; fromRow < 8; fromRow++)
        {
            for (int fromCol = 0; fromCol < 8; fromCol++)
            {
                if (boardPieces[fromRow,fromCol].IsWhite != isWhite && boardPieces[fromRow, fromCol].MoveAllowed(fromRow, fromCol, toTile.Row, toTile.Col, boardPieces))
                {
                    return true;
                }
            }
        }

        return false;
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
        
        /*
        // Pawn
        for (int i = 0; i < 8; i++)
        {
            boardPieces[1, i] = new Pawn(false);
            boardPieces[6, i] = new Pawn(true);
        }
        */
        
        // Rook
        boardPieces[0, 0] = new Rook(false);
        boardPieces[0, 7] = new Rook(false);
        boardPieces[7, 0] = new Rook(true);
        boardPieces[7, 7] = new Rook(true);
        
        /*
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
        */
        
        // Queen
        boardPieces[0, 3] = new Queen(false);
        boardPieces[7, 3] = new Queen(true);
        
        // King
        boardPieces[0, 4] = new King(false);
        boardPieces[7, 4] = new King(true);
    }
}