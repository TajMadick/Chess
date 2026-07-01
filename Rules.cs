namespace Schach;

public static class Rules
{ 
    public static IEnumerable<Grid.Tile> AllLegalMoves(Board board, Grid.Tile fromTile, Grid.Tile kingTile, bool isWhiteMoving)
    {
        // durch alle legalen Züge des Königs durchgehen und schauen obs eh nicht eigenes Feld oder Schach
        foreach (Grid.Tile possibleTile in board.Grid[fromTile].AllMoves(board.Grid, fromTile))
        {
            // Validation check
            if (Validation.IsTakingOwnPiece(ref board.Grid.GetRef(possibleTile), isWhiteMoving) ||
                !Validation.IsMovingOwnPiece(ref board.Grid.GetRef(fromTile), isWhiteMoving))
            {
                continue;
            }
            
            // Figur bewegen
            if (board.MakeMove(fromTile, possibleTile))
            {
                board.UnmakeMove();
                yield return possibleTile;
            }
        }
    }

    public static void EnPassantExpires(Grid grid)
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                if (grid[row, col] is Pawn { IsEnPassantable: true } enPassantPawn)
                {
                    enPassantPawn.EnPassantExpiresIn--;
                    if (enPassantPawn.EnPassantExpiresIn <= 0) enPassantPawn.IsEnPassantable = false;
                }
            }
        }
    }
    public static bool IsStalemate(Board board, bool isWhiteChecking)
    {
        Grid.Tile kingTile = FindKing(board.Grid, isWhiteChecking);
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Grid.Tile tile = new Grid.Tile(row, col);
                if (AllLegalMoves(board, tile, kingTile, isWhiteChecking).Any())
                {
                    return false;
                }
            }
        }

        return true;
    }
    public static bool IsCheckmate(Board board, bool isWhiteChecking, bool isWhiteMoving)
    {
        // Prüfen ob König überhaupt angegriffen wird -> wenn nein kein Schachmatt
        Grid.Tile kingTile = FindKing(board.Grid, isWhiteChecking);
        if (!IsAttackingField(board.Grid, kingTile, isWhiteChecking))
        {
            return false;
        }
        // wenn sein König momentan Check ist aber er nicht dran ist ist Checkmate
        // (passiert nur bei custom fen Notation)
        else if (IsAttackingField(board.Grid, kingTile, isWhiteChecking) && isWhiteMoving != isWhiteChecking)
        {
            return true;
        }

        // als Erstes prüfen, ob König abhauen kann -> kein Schachmatt 
        if (AllLegalMoves(board, kingTile, kingTile, isWhiteChecking).Any())
        {
            return false;
        }

        // Wenn der König nicht abhauen kann und mehr wie ein Angreifer ist -> Schachmatt
        if (CountAttacking(board.Grid, kingTile, isWhiteChecking) > 1)
        {
            return true;
        }
        
        Grid.Tile attackerTile = GetAttackerTile(board.Grid, kingTile, isWhiteChecking);
        ref Pieces attacker = ref board.Grid.GetRef(attackerTile);

        // kann der Angreifer geschlagen werden
        if (IsAttackingField(board.Grid, attackerTile, isWhiteAttacked:!isWhiteChecking)) 
            // isWhiteMoving = true er schaut, ob weiß geschlagen werden kann bei diesem Feld
            // wir wollen aber genau umgekehrt schauen kann der Angreifer geschlagen werden
        {
            foreach (Grid.Tile defenderTile in GetAllAttackerTiles(board.Grid, attackerTile, !isWhiteChecking))
            {
                // Schauen, ob der Move legal wäre, wenn ja kein Schachmatt
                if (board.MakeMove(defenderTile, attackerTile))
                {
                    board.UnmakeMove();
                    return false;
                }
            }
        }

        // letzte Prüfung kann Spieler eine Figur dazwischen bewegen 
        // und angreifende Figur muss ein Springer, Turm oder Dame sein
        if (attacker is SlidingPieces slidingPieceAttacker and not King)
        {
            // alle Tiles zwischen König und Angreifer durchgehen
            foreach (Grid.Tile tile in slidingPieceAttacker.AllFieldsOnPath(board.Grid, attackerTile, kingTile))
            {
                // durch alle Verteidiger durchgehen
                // wenn es keine Verteidiger gibt dann geht er kein einziges Mal durch
                // es wird für alle Verteidiger geprüft, ob sie auf das Feld dazwischen gehen dürfen
                foreach (Grid.Tile defenderTile in GetAllAttackerTiles(board.Grid, tile, !isWhiteChecking))
                {
                    // Schauen, ob der Move legal wäre, wenn ja kein Schachmatt
                    if (board.MakeMove(defenderTile, attackerTile))
                    {
                        board.UnmakeMove();
                        return false;
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
    public static bool IsAttackingField(Grid grid, Grid.Tile toTile, bool isWhiteAttacked)
    {
        return SearchAttackingPreset(grid, toTile, isWhiteAttacked).Any();
    }
    
    public static IEnumerable<Grid.Tile> GetAllAttackerTiles(Grid grid, Grid.Tile toTile, bool isWhiteAttacked)
    {
        return SearchAttackingPreset(grid, toTile, isWhiteAttacked).Select(((tuple) => tuple.attacker));
    }

    // kann man nur benutzen, wenn man geprüft hat IsAttackingField !!!
    public static int CountAttacking(Grid grid, Grid.Tile toTile, bool isWhiteAttacked)
    {
        return SearchAttackingPreset(grid, toTile, isWhiteAttacked).Last().count;
    }
    
    // kann man nur benutzen, wenn Count Attacking == 1!!!
    public static Grid.Tile GetAttackerTile(Grid grid, Grid.Tile toTile, bool isWhiteAttacked)
    {
        return SearchAttackingPreset(grid, toTile, isWhiteAttacked).First().attacker;
    }
    
    private static IEnumerable<(int count, Grid.Tile attacker)> SearchAttackingPreset(Grid grid, Grid.Tile toTile, bool isWhiteAttacked)
    {
        int count = 0;
        
        // Bei jeder Figuren schauen, ob sie den König schlagen kann
        for (int attackerRow = 0; attackerRow < 8; attackerRow++)
        {
            for (int attackerCol = 0; attackerCol < 8; attackerCol++)
            {
                Grid.Tile attackerTile = new Grid.Tile(attackerRow, attackerCol);
                // sucht nur die gegensätzliche Farbe -> Aufruf von weiß -> sucht nur schwarze Figuren ab
                if (grid[attackerTile].IsWhite != isWhiteAttacked && 
                    grid[attackerTile].DetermineMoveType(grid, attackerTile, toTile) != Types.MoveType.Invalid)
                {
                    count++;
                     yield return (count, attackerTile);
                }
            }
        }
    }
}