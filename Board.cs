using System.Text.RegularExpressions;

namespace Schach;

public class Board
{
    private Regex Rg;
    private const string Pattern = "^[a-h][0-8][a-h][0-8]$";
    public Board()
    {
        Rg = new Regex(Pattern);
        FillBoard();
    }
    
    private Pieces[,] boardPieces = new Pieces[8,8];
    
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
        
        int fromRow = 7 - ((int)fromNumber - '1'); 
        int toRow = 7 - ((int)toNumber - '1'); 
        
        int fromCol = (int)fromLetter - 'a';
        int toCol = (int)toLetter - 'a';

        // Falls versucht leeres Feld zu bewegen
        if (boardPieces[fromRow, fromCol] is Empty)
        {
            Console.Write("From Location is Empty");
            Console.ReadKey();
            return false;
        }

        // Falls versucht auf selbes Feld zu gehen
        if (fromRow == toRow && fromCol == toCol)
        {
            Console.Write("From Location ist die selbe als To Location");
            Console.ReadKey();
            return false;
        }
        
        // Falls versucht flasche Farbe zu bewegen
        if (boardPieces[fromRow, fromCol].IsWhite != isWhiteMoving)
        {
            string color = (isWhiteMoving) ? "white" : "black";
            Console.Write($"You can only move {color} pieces");
            Console.ReadKey();
            return false;
        }
        
        if (boardPieces[fromRow,fromCol].MoveAllowed(fromRow, fromCol, toRow, toCol, boardPieces))
        {
            // das Zielfeld darf nicht von der gleichen Farbe sein 
            // wenn das Zielfeld von der gleichen Farbe ist aber leer dann passts
            if (boardPieces[toRow, toCol].IsWhite == isWhiteMoving && boardPieces[toRow, toCol] is not Empty)
            {
                Console.Write("Can't take pieces from your own color");
                Console.ReadKey();
                return false;
            }
            
            boardPieces[toRow, toCol] = boardPieces[fromRow, fromCol];
            boardPieces[fromRow, fromCol] = new Empty();

            if (boardPieces[toRow, toCol] is Pawn)
            {
                Pawn checkPromotablePawn = (Pawn)boardPieces[toRow, toCol];
                if (checkPromotablePawn.IsPromotable(toRow))
                {
                    boardPieces[toRow, toCol] = new Queen(isWhiteMoving);
                }
            }
        }
        else
        {
            return false;
        }
        
        return true;
    }
    private void FillBoard()
    {
        // Empty
        for (int i = 2; i < 6; i++)
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