using System.Text.RegularExpressions;

namespace Schach;

public static class Rules
{
    private static Regex Rg;
    private const string Pattern = "^[a-h][0-8][a-h][0-8]$";
    
    public static bool PassesSanityChecks(Grid grid, string move, bool isWhiteMoving)
    {
        Rg = new Regex(Pattern);
        
        if (!Rg.IsMatch(move))
        {
            Console.Write($"Format only: {Pattern}");
            Console.ReadKey();
            return false;
        }

        CalculateCoordinates(move, out Grid.Tile fromTile, out Grid.Tile toTile);

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
        if (IsMovingOwnPiece(ref fromPiece, isWhiteMoving))
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
    public static void CalculateCoordinates(string move, out Grid.Tile fromTile, out Grid.Tile toTile)
    {
        char fromLetter = move[0];
        char fromNumber = move[1];
        char toLetter = move[2];
        char toNumber = move[3];

        fromTile = new Grid.Tile(7 - (fromNumber - '1'), fromLetter - 'a');
        toTile = new Grid.Tile(7 - (toNumber - '1'), toLetter - 'a');
    }

    public static Grid TemporaryMove(Grid grid, Grid.Tile fromTile, Grid.Tile toTile)
    {
        Grid temporaryGrid = new Grid(grid);
        temporaryGrid[toTile] = temporaryGrid[fromTile];
        temporaryGrid[fromTile] = new Empty();
        return temporaryGrid;
    }

    public static IEnumerable<Grid.Tile> AllLegalMoves(Grid grid, Grid.Tile fromTile, bool isWhiteMoving)
    {
        // durch alle legalen Züge des Königs durchgehen und schauen obs eh nicht eigenes Feld oder Schach
        foreach (Grid.Tile possibleTile in grid[fromTile].AllPossibleMoves(grid, fromTile))
        {
            Grid temporaryGrid = TemporaryMove(grid, fromTile, possibleTile);
            if (!IsTakingOwnPiece(ref grid.GetRef(possibleTile), isWhiteMoving) &&
                IsMovingOwnPiece(ref grid.GetRef(fromTile), isWhiteMoving) &&
                                     !Rules.IsCheck(temporaryGrid, possibleTile, isWhiteMoving))
            {
                yield return possibleTile;
            }
        }
    }

    public static bool IsStalemate(Grid grid, bool isWhiteMoving)
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                if (AllLegalMoves(grid, new Grid.Tile(row, col), isWhiteMoving).Any())
                {
                    return false;
                }
            }
        }

        return true;
    }
    public static bool IsCheckmate(Grid grid, bool isWhiteMoving)
    {
        Grid.Tile kingTile = FindKing(grid, isWhiteMoving);
        if (IsCheck(grid, kingTile, isWhiteMoving))
        {
            // als Erstes prüfen, ob König abhauen kann
            if (AllLegalMoves(grid, kingTile, isWhiteMoving).Any())
            {
                return false;
            }

            // wenn nur ein Angreifer ist und der König sich nicht bewegen kann ist es NOCH kein Schachmatt
            if (CountAttacking(grid, kingTile, isWhiteMoving) == 1)
            {
                Grid.Tile attackerTile = GetAttackerTile(grid, kingTile, isWhiteMoving);
                ref Pieces attacker = ref grid.GetRef(attackerTile);

                // kann der Angreifer geschlagen werden
                if (IsCheck(grid, attackerTile,
                        !isWhiteMoving)) // isWhiteMoving = true er schaut, ob weiß geschlagen werden kann bei diesem Feld
                    // wir wollen aber genau umgekehrt schauen kann der Angreifer geschlagen werden
                {
                    foreach (Grid.Tile defenderTile in GetAllAttackerTiles(grid, attackerTile, !isWhiteMoving))
                    {
                        // wenn Verteidiger König ist schauen, ob er beim Verteidigen nicht ins Schach kommt
                        if (grid[defenderTile] is King)
                        {
                            // temporäres Feld machen und mit König verteidigen
                            // König geht zur Angreifer Position und schauen ob er da in Check ist
                            Grid temporaryGrid = TemporaryMove(grid, kingTile, attackerTile);
                            if (!IsCheck(temporaryGrid, attackerTile, isWhiteMoving))
                            {
                                return false;
                            }
                        }
                        // Verteidiger ist nicht der König
                        else
                        {
                            // temporäres Feld machen und mit der Figur verteidigen
                            // wenn König danach nicht Schach ist kein Schachmatt
                            Grid temporaryGrid = TemporaryMove(grid, defenderTile, attackerTile);
                            if (!IsCheck(temporaryGrid, kingTile, isWhiteMoving))
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
                    foreach (Grid.Tile tile in attacker.FieldsOnPath(grid, attackerTile, kingTile))
                    {
                        // kann einer der verteidigenden Figuren auf dieses Tile gehen
                        if (IsCheck(grid, tile, !isWhiteMoving))
                        {
                            // durch alle Verteidiger durchgehen
                            foreach (Grid.Tile defenderTile in GetAllAttackerTiles(grid, attackerTile, !isWhiteMoving))
                            {
                                // wenn dieser Verteidiger nicht der König ist TODO: das muss ich noch durchüberlegen ob ich das brauche
                                if (grid[defenderTile] is not King)
                                {
                                    // wenn König danach nicht Schach ist kein Schachmatt
                                    Grid temporaryGrid = TemporaryMove(grid, defenderTile, attackerTile);
                                    if (!IsCheck(temporaryGrid, kingTile, isWhiteMoving))
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

        return false;
    }
    public static Grid.Tile FindKing(Grid grid, bool isWhite)
    {
        // König finden
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Grid.Tile kingTile = new Grid.Tile(row, col);
                if (grid[kingTile] is King && grid[kingTile].IsWhite == isWhite)
                {
                    return kingTile;
                }
            }
        }

        return new Grid.Tile(-1, -1);
    }
    public static bool IsCheck(Grid grid, Grid.Tile toTile, bool isWhite)
    {
        return SearchAttackingPreset(grid, toTile, isWhite).Any();
    }

    // kann man nur benutzen, wenn man geprüft hat IsCheck !!!
    public static int CountAttacking(Grid grid, Grid.Tile toTile, bool isWhite)
    {
        return SearchAttackingPreset(grid, toTile, isWhite).Last().count;
    }
    
    // kann man nur benutzen, wenn man geprüft hat IsCheck !!!
    public static IEnumerable<Grid.Tile> GetAllAttackerTiles(Grid grid, Grid.Tile toTile, bool isWhite)
    {
        return SearchAttackingPreset(grid, toTile, isWhite).Select(((tuple) => tuple.attacker));
    }
    
    // kann man nur benutzen, wenn Count Attacking == 2!!!
    public static Grid.Tile GetAttackerTile(Grid grid, Grid.Tile toTile, bool isWhite)
    {
        return SearchAttackingPreset(grid, toTile, isWhite).First().attacker;
    }
    
    private static IEnumerable<(int count, Grid.Tile attacker)> SearchAttackingPreset(Grid grid, Grid.Tile toTile, bool isWhite)
    {
        int count = 0;
        
        // Bei jeder Figuren schauen, ob sie den König schlagen kann
        for (int attackerRow = 0; attackerRow < 8; attackerRow++)
        {
            for (int attackerCol = 0; attackerCol < 8; attackerCol++)
            {
                Grid.Tile attackerTile = new Grid.Tile(attackerRow, attackerCol);
                // sucht nur die gegensätzliche Farbe -> Aufruf von weiß -> sucht nur schwarze Figuren ab
                if (grid[attackerTile].IsWhite != isWhite && 
                    grid[attackerTile].DetermineMoveType(grid, attackerTile, toTile) != Pieces.MoveType.Invalid)
                {
                    count++;
                    yield return (count, attackerTile);
                }
            }
        }
    }
}