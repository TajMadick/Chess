namespace Schach;

public static class Rules
{ 
    // TODO: Umschreiben, dass er mit tempField macht für performance
    public static IEnumerable<Grid.Tile> AllLegalMoves(Grid grid, Grid.Tile fromTile, bool isWhiteMoving)
    {
        // durch alle legalen Züge des Königs durchgehen und schauen obs eh nicht eigenes Feld oder Schach
        foreach (Grid.Tile possibleTile in grid[fromTile].AllMoves(grid, fromTile))
        {
            Grid temporaryGrid = Move.TemporaryMove(grid, fromTile, possibleTile);
            if (!Validation.IsTakingOwnPiece(ref grid.GetRef(possibleTile), isWhiteMoving) &&
                Validation.IsMovingOwnPiece(ref grid.GetRef(fromTile), isWhiteMoving) &&
                !IsAttackingField(temporaryGrid, possibleTile, isWhiteMoving))
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
        // Prüfen ob König überhaupt angegriffen wird -> wenn nein kein Schachmatt
        Grid.Tile kingTile = FindKing(grid, isWhiteMoving);
        if (!IsAttackingField(grid, kingTile, isWhiteMoving))
        {
            return false;
        }

        // als Erstes prüfen, ob König abhauen kann -> kein Schachmatt 
        if (AllLegalMoves(grid, kingTile, isWhiteMoving).Any())
        {
            return false;
        }

        // Wenn der König nicht abhauen kann und mehr wie ein Angreifer ist -> Schachmatt
        if (CountAttacking(grid, kingTile, isWhiteMoving) > 1)
        {
            return true;
        }
        

        Grid.Tile attackerTile = GetAttackerTile(grid, kingTile, isWhiteMoving);
        ref Pieces attacker = ref grid.GetRef(attackerTile);

        // kann der Angreifer geschlagen werden
        if (IsAttackingField(grid, attackerTile, !isWhiteMoving)) 
            // isWhiteMoving = true er schaut, ob weiß geschlagen werden kann bei diesem Feld
            // wir wollen aber genau umgekehrt schauen kann der Angreifer geschlagen werden
        {
            foreach (Grid.Tile defenderTile in GetAllAttackerTiles(grid, attackerTile, !isWhiteMoving))
            {
                // wenn Verteidiger König ist schauen, ob er beim Verteidigen nicht ins Schach kommt
                if (grid[defenderTile] is King)
                {
                    // temporäres Feld machen und mit König verteidigen
                    // König geht zur Angreifer Position und schauen ob er da in Check ist
                    Grid temporaryGrid = Move.TemporaryMove(grid, kingTile, attackerTile);
                    if (!IsAttackingField(temporaryGrid, attackerTile, isWhiteMoving))
                    {
                        return false;
                    }
                }
                // Verteidiger ist nicht der König
                else
                {
                    // temporäres Feld machen und mit der Figur verteidigen
                    // wenn König danach nicht Schach ist kein Schachmatt
                    Grid temporaryGrid = Move.TemporaryMove(grid, defenderTile, attackerTile);
                    if (!IsAttackingField(temporaryGrid, kingTile, isWhiteMoving))
                    {
                        return false;
                    }
                }
            }
        }

        // letzte Prüfung kann Spieler eine Figur dazwischen bewegen 
        // und angreifende Figur muss ein Springer, Turm oder Dame sein
        if (attacker is ISlidingPieces slidingPieceAttacker)
        {
            // alle Tiles zwischen König und Angreifer durchgehen
            foreach (Grid.Tile tile in slidingPieceAttacker.FieldsOnPath(grid, attackerTile, kingTile))
            {
                // durch alle Verteidiger durchgehen
                // wenn es keine Verteidiger gibt dann geht er kein einziges Mal durch
                foreach (Grid.Tile defenderTile in GetAllAttackerTiles(grid, attackerTile, !isWhiteMoving))
                {
                    // wenn dieser Verteidiger nicht der König ist -> darf er nicht
                    if (grid[defenderTile] is not King)
                    {
                        // wenn König danach nicht Schach ist kein Schachmatt
                        Grid temporaryGrid = Move.TemporaryMove(grid, defenderTile, attackerTile);
                        if (!IsAttackingField(temporaryGrid, kingTile, isWhiteMoving))
                        {
                            return false;
                        }
                    }
                }
            }
        }

        // Schachmatt
        return true;
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
    public static bool IsAttackingField(Grid grid, Grid.Tile toTile, bool isWhite)
    {
        return SearchAttackingPreset(grid, toTile, isWhite).Any();
    }
    
    public static IEnumerable<Grid.Tile> GetAllAttackerTiles(Grid grid, Grid.Tile toTile, bool isWhite)
    {
        return SearchAttackingPreset(grid, toTile, isWhite).Select(((tuple) => tuple.attacker));
    }

    // kann man nur benutzen, wenn man geprüft hat IsAttackingField !!!
    public static int CountAttacking(Grid grid, Grid.Tile toTile, bool isWhite)
    {
        return SearchAttackingPreset(grid, toTile, isWhite).Last().count;
    }
    
    // kann man nur benutzen, wenn Count Attacking == 1!!!
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
                    grid[attackerTile].DetermineMoveType(grid, attackerTile, toTile) != Types.MoveType.Invalid)
                {
                    count++;
                    yield return (count, attackerTile);
                }
            }
        }
    }
}