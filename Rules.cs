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
    
    public static bool IsCheckmate(Pieces[,] boardPieces, Board.Tile kingPos, bool isWhiteMoving)
    {
        int kingRow = kingPos.Row;
        int kingCol = kingPos.Col;
        
        // als Erstes prüfen, ob König abhauen kann
        // durch alle legalen Züge des Königs durchgehen -> wenn er irgendwo nicht Check ist dann kein Schachmatt
        foreach (Board.Tile allowedMove in boardPieces[kingRow, kingCol].AllLegalMoves(kingRow, kingCol, boardPieces))
        {
            if (!Rules.IsCheck(boardPieces, allowedMove, isWhiteMoving))
            {
                return false;
            }
        }

        // wenn nur ein Angreifer ist und der König sich nicht bewegen kann ist es NOCH kein Schachmatt
        if (CountAttacking(boardPieces, kingPos, isWhiteMoving) == 1)
        {
            Board.Tile attackerTile = GetAttackerTile(boardPieces, kingPos, isWhiteMoving);
            ref Pieces attacker = ref boardPieces[attackerTile.Row, attackerTile.Col];
            
            // kann der Angreifer geschlagen werden
            // isWhiteMoving negieren da man schauen muss welche von den eigenen Figuren den Angreifer schlägt
            // IsCheck prüft nur die gegnerischen Figuren
            if (IsCheck(boardPieces, attackerTile, !isWhiteMoving))
            {
                return false;
            }
            
            // letzte Prüfung kann Spieler eine Figur dazwischen bewegen 
            // und angreifende Figur muss ein Springer, Turm oder Dame sein
            if (boardPieces[attackerTile.Row, attackerTile.Col] is Bishop or Rook or Queen)
            {
                foreach (Board.Tile tile in attacker.FieldsOnPath(attackerTile.Row, attackerTile.Col, kingRow, kingCol,
                             boardPieces))
                {
                    if (IsCheck(boardPieces, tile, !isWhiteMoving))
                    {
                        return false;
                    }
                }
            }
        }

        // Schachmatt
        return true;
    }
    public static bool IsCheck(Pieces[,] boardPieces, Board.Tile toTile, bool isWhite)
    {
        return SearchAttackingPreset(boardPieces, toTile, isWhite).Any();
    }

    // kann man nur benutzen, wenn man geprüft hat IsCheck !!!
    public static int CountAttacking(Pieces[,] boardPieces, Board.Tile toTile, bool isWhite)
    {
        return SearchAttackingPreset(boardPieces, toTile, isWhite).Last().count;
    }
    
    // kann man nur benutzen, wenn Count Attacking == 2!!!
    public static Board.Tile GetAttackerTile(Pieces[,] boardPieces, Board.Tile toTile, bool isWhite)
    {
        return SearchAttackingPreset(boardPieces, toTile, isWhite).First().attacker;
    }
    
    private static IEnumerable<(int count, Board.Tile attacker)> SearchAttackingPreset(Pieces[,] boardPieces, Board.Tile toTile, bool isWhite)
    {
        int count = 0;
        
        // Bei jeder Figuren schauen, ob sie den König schlagen kann
        for (int fromRow = 0; fromRow < 8; fromRow++)
        {
            for (int fromCol = 0; fromCol < 8; fromCol++)
            {
                // sucht nur die gegensätzliche Farbe -> Aufruf von weiß -> sucht nur schwarze Figuren ab
                if (boardPieces[fromRow,fromCol].IsWhite != isWhite && boardPieces[fromRow, fromCol].DetermineMoveType(fromRow, 
                        fromCol, toTile.Row, toTile.Col, boardPieces) != Pieces.MoveType.Invalid)
                {
                    count++;
                    yield return (count, new Board.Tile(fromRow, fromCol));
                }
            }
        }
    }
    
    
}