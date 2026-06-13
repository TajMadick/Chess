using System.Text.RegularExpressions;

namespace Schach;

public static class Rules
{
    private static Regex Rg;
    private const string Pattern = "^[a-h][0-8][a-h][0-8]$";
    
    public static bool PassesSanityChecks(string move, Pieces[,] boardPieces, bool isWhiteMoving)
    {
        Rg = new Regex(Pattern);
        
        if (!Rg.IsMatch(move))
        {
            Console.Write($"Format only: {Pattern}");
            Console.ReadKey();
            return false;
        }

        CalculateCoordinates(move, out int fromRow, out int fromCol, out int toRow, out int toCol);

        ref Pieces fromTile = ref boardPieces[fromRow, fromCol];
        ref Pieces toTile = ref boardPieces[toRow, toCol];
        
        // Falls versucht leeres Feld zu bewegen
        if (fromTile is Empty)
        {
            Console.Write("From Location is Empty");
            Console.ReadKey();
            return false;
        }

        // Falls versucht auf selbes Feld zu gehen (selbe Koordinaten)
        if (fromRow == toRow && fromCol == toCol)
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

        return true;
    }

    public static void CalculateCoordinates(string move, out int fromRow, out int fromCol, out int toRow, out int toCol)
    {
        char fromLetter = move[0];
        char fromNumber = move[1];
        char toLetter = move[2];
        char toNumber = move[3];

        fromRow = 7 - (fromNumber - '1');
        fromCol = fromLetter - 'a';

        toRow = 7 - (toNumber - '1');
        toCol = toLetter - 'a';
    }
    public static bool IsCheck(Pieces[,] boardPieces, Board.Tile toTile, bool isWhite)
    {
        // Bei jeder Figuren schauen, ob sie den König schlagen kann
        for (int fromRow = 0; fromRow < 8; fromRow++)
        {
            for (int fromCol = 0; fromCol < 8; fromCol++)
            {
                if (boardPieces[fromRow,fromCol].IsWhite != isWhite && boardPieces[fromRow, fromCol].DetermineMoveType(fromRow, 
                        fromCol, toTile.Row, toTile.Col, boardPieces) != Pieces.MoveType.Invalid)
                {
                    return true;
                }
            }
        }

        return false;
    }
}