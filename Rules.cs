using System.Text.RegularExpressions;

namespace Schach;

public static class Rules
{
    private static Regex Rg;
    private const string Pattern = "^[a-h][0-8][a-h][0-8]$";
    
    public static bool PassesSanityChecks(Board board, string move, bool isWhiteMoving)
    {
        Rg = new Regex(Pattern);
        
        if (!Rg.IsMatch(move))
        {
            Console.Write($"Format only: {Pattern}");
            Console.ReadKey();
            return false;
        }

        CalculateCoordinates(move, out Board.Tile fromTile, out Board.Tile toTile);

        ref Pieces fromPiece = ref board.GetRef(fromTile);
        ref Pieces toPiece = ref board.GetRef(toTile);
        
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
        
        // Falls versucht flasche Farbe zu bewegen
        if (fromPiece.IsWhite != isWhiteMoving)
        {
            string color = (isWhiteMoving) ? "white" : "black";
            Console.Write($"You can only move {color} pieces");
            Console.ReadKey();
            return false;
        }
        

        if (IsOwnPiece(ref toPiece, isWhiteMoving))
        {
            Console.Write("Can't take pieces from your own color");
            Console.ReadKey();
            return false;
        }

        return true;
    }
        
    
    // das Zielfeld darf nicht von der gleichen Farbe sein, 
    // wenn das Zielfeld von der gleichen Farbe ist aber leer dann passts
    public static bool IsOwnPiece(ref Pieces tile, bool isWhiteMoving) =>
        tile.IsWhite == isWhiteMoving && tile is not Empty;
    public static void CalculateCoordinates(string move, out Board.Tile fromTile, out Board.Tile toTile)
    {
        char fromLetter = move[0];
        char fromNumber = move[1];
        char toLetter = move[2];
        char toNumber = move[3];

        fromTile = new Board.Tile(7 - (fromNumber - '1'), fromLetter - 'a');
        toTile = new Board.Tile(7 - (toNumber - '1'), toLetter - 'a');
    }

    public static Board TemporaryMove(Board board, Board.Tile fromTile, Board.Tile toTile)
    {
        Board temporaryBoard = new Board(board);
        temporaryBoard[toTile] = temporaryBoard[fromTile];
        temporaryBoard[fromTile] = new Empty();
        return temporaryBoard;
    }
    
    public static bool IsCheckmate(Board board, Board.Tile kingTile, bool isWhiteMoving)
    {
        // als Erstes prüfen, ob König abhauen kann
        // durch alle legalen Züge des Königs durchgehen -> wenn er irgendwo nicht Check ist dann kein Schachmatt
        foreach (Board.Tile allowedTile in board[kingTile].AllLegalMoves(board, kingTile))
        {
            Board temporaryBoard = TemporaryMove(board, kingTile, allowedTile);
            if (!IsOwnPiece(ref board.GetRef(allowedTile), isWhiteMoving) && 
                !Rules.IsCheck(temporaryBoard, allowedTile, isWhiteMoving))
            {
                return false;
            }
        }

        // wenn nur ein Angreifer ist und der König sich nicht bewegen kann ist es NOCH kein Schachmatt
        if (CountAttacking(board, kingTile, isWhiteMoving) == 1)
        {
            Board.Tile attackerTile = GetAttackerTile(board, kingTile, isWhiteMoving);
            ref Pieces attacker = ref board.GetRef(attackerTile);
            
            // kann der Angreifer geschlagen werden
            if (IsCheck(board, attackerTile, !isWhiteMoving)) // isWhiteMoving = true er schaut, ob weiß geschlagen werden kann bei diesem Feld
                                                              // wir wollen aber genau umgekehrt schauen kann der Angreifer geschlagen werden
            {
                foreach (Board.Tile defenderTile in GetAllAttackerTiles(board, attackerTile, !isWhiteMoving))
                {
                    // wenn Verteidiger König ist schauen, ob er beim Verteidigen nicht ins Schach kommt
                    if (board[defenderTile] is King)
                    {
                        // temporäres Feld machen und mit König verteidigen
                        // König geht zur Angreifer Position und schauen ob er da in Check ist
                        Board temporaryBoard = TemporaryMove(board, kingTile, attackerTile);
                        if (!IsCheck(temporaryBoard, attackerTile, isWhiteMoving))
                        {
                            return false;
                        }
                    }
                    // Verteidiger ist nicht der König
                    else
                    {
                        // temporäres Feld machen und mit der Figur verteidigen
                        // wenn König danach nicht Schach ist kein Schachmatt
                        Board temporaryBoard = TemporaryMove(board, defenderTile, attackerTile);
                        if (!IsCheck(temporaryBoard, kingTile, isWhiteMoving))
                        {
                            return false;
                        }
                    }
                }
            }
            
            // letzte Prüfung kann Spieler eine Figur dazwischen bewegen 
            // und angreifende Figur muss ein Springer, Turm oder Dame sein
            if (attacker is Bishop or Rook or Queen)
            {
                // alle Tiles zwischen König und Angreifer durchgehen
                foreach (Board.Tile tile in attacker.FieldsOnPath(board, attackerTile, kingTile))
                {
                    // kann einer der verteidigenden Figuren auf dieses Tile gehen
                    if (IsCheck(board, tile, !isWhiteMoving))
                    {
                        // durch alle Verteidiger durchgehen
                        foreach (Board.Tile defenderTile in GetAllAttackerTiles(board, attackerTile, !isWhiteMoving))
                        {
                            // wenn dieser Verteidiger nicht der König ist TODO: das muss ich noch durchüberlegen ob ich das brauche
                            if (board[defenderTile] is not King)
                            {
                                // wenn König danach nicht Schach ist kein Schachmatt
                                Board temporaryBoard = TemporaryMove(board, defenderTile, attackerTile);
                                if (!IsCheck(temporaryBoard, kingTile, isWhiteMoving))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
        }

        // Schachmatt
        return true;
    }
    public static bool IsCheck(Board board, Board.Tile toTile, bool isWhite)
    {
        return SearchAttackingPreset(board, toTile, isWhite).Any();
    }

    // kann man nur benutzen, wenn man geprüft hat IsCheck !!!
    public static int CountAttacking(Board board, Board.Tile toTile, bool isWhite)
    {
        return SearchAttackingPreset(board, toTile, isWhite).Last().count;
    }
    
    // kann man nur benutzen, wenn man geprüft hat IsCheck !!!
    public static IEnumerable<Board.Tile> GetAllAttackerTiles(Board board, Board.Tile toTile, bool isWhite)
    {
        return SearchAttackingPreset(board, toTile, isWhite).Select(((tuple) => tuple.attacker));
    }
    
    // kann man nur benutzen, wenn Count Attacking == 2!!!
    public static Board.Tile GetAttackerTile(Board board, Board.Tile toTile, bool isWhite)
    {
        return SearchAttackingPreset(board, toTile, isWhite).First().attacker;
    }
    
    private static IEnumerable<(int count, Board.Tile attacker)> SearchAttackingPreset(Board board, Board.Tile toTile, bool isWhite)
    {
        int count = 0;
        
        // Bei jeder Figuren schauen, ob sie den König schlagen kann
        for (int attackerRow = 0; attackerRow < 8; attackerRow++)
        {
            for (int attackerCol = 0; attackerCol < 8; attackerCol++)
            {
                Board.Tile attackerTile = new Board.Tile(attackerRow, attackerCol);
                // sucht nur die gegensätzliche Farbe -> Aufruf von weiß -> sucht nur schwarze Figuren ab
                if (board[attackerTile].IsWhite != isWhite && 
                    board[attackerTile].DetermineMoveType(board, attackerTile, toTile) != Pieces.MoveType.Invalid)
                {
                    count++;
                    yield return (count, attackerTile);
                }
            }
        }
    }
}