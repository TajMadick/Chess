using System.Text.RegularExpressions;

namespace Schach;

public static class Validation
{
    private const string Pattern = "^[a-h][0-8][a-h][0-8]$";
    
    public static bool PassesSanityChecks(Grid grid, string move, bool isWhiteMoving)
    {
        Regex rg = new Regex(Pattern);
        
        if (!rg.IsMatch(move))
        {
            Console.Write($"Format only: {Pattern}");
            Console.ReadKey();
            return false;
        }

        Utils.CalculateCoordinates(move, out Grid.Tile fromTile, out Grid.Tile toTile);

        ref Pieces fromPiece = ref grid.GetRef(fromTile);
        ref Pieces toPiece = ref grid.GetRef(toTile);
        
        // Falls versucht leeres Feld zu bewegen
        if (fromPiece is Empty)
        {
            Console.Write("From Location is Empty");
            Console.ReadKey();
            return false;
        }

        // Falls versucht auf selbes Feld zu gehen (selbe Koordinaten)
        if (fromTile == toTile)
        {
            Console.Write("From Location same as To Location");
            Console.ReadKey();
            return false;
        }
        
        // Falls versucht falsche Farbe zu bewegen
        if (!IsMovingOwnPiece(ref fromPiece, isWhiteMoving))
        {
            string color = (isWhiteMoving) ? "white" : "black";
            Console.Write($"You can only move {color} pieces");
            Console.ReadKey();
            return false;
        }
        

        if (IsTakingOwnPiece(ref toPiece, isWhiteMoving))
        {
            Console.Write("Can't take pieces from your own color");
            Console.ReadKey();
            return false;
        }

        return true;
    }
        
    
    // das Zielfeld darf nicht von der gleichen Farbe sein, 
    // wenn das Zielfeld von der gleichen Farbe ist aber leer dann passts
    public static bool IsTakingOwnPiece(ref Pieces toPiece, bool isWhiteMoving) =>
        toPiece.IsWhite == isWhiteMoving && toPiece is not Empty;

    public static bool IsMovingOwnPiece(ref Pieces fromPiece, bool isWhiteMoving) =>
        fromPiece.IsWhite == isWhiteMoving;
}