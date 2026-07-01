namespace Schach;

public static class Test
{
    public static int PossiblePositions(Board board, int depth, bool isWhiteMoving, bool divide)
    {
        if (depth == 0) return 1;
        
        int positions = 0;

        Grid.Tile kingTile = Rules.FindKing(board.Grid, isWhiteMoving);
            
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Grid.Tile fromTile = new Grid.Tile(row, col);
                foreach (Grid.Tile legalTile in Rules.AllLegalMoves(board, fromTile, kingTile, isWhiteMoving))
                {
                    if (board.MakeMove(fromTile, legalTile))
                    {
                        // mit der divide flag werden im ersten Rekursionsdurchlauf für die einzelnen Züge die Positions angezeigt
                        if (divide)
                        {
                            Console.WriteLine($"{ConvertPositionToString(fromTile, legalTile)}: " +
                                              $"{PossiblePositions(board, depth - 1, !isWhiteMoving, false)}");
                        }
                        
                        positions += PossiblePositions(board, depth - 1, !isWhiteMoving, false);
                        board.UnmakeMove();
                    }
                }
            }
        }
        
        return positions;
    }

    private static string ConvertPositionToString(Grid.Tile fromTile, Grid.Tile toTile)
    {
        return $"{(char)(fromTile.Col + 'a')}{8 - fromTile.Row}{(char)(toTile.Col + 'a')}{8 - toTile.Row}";
    }
}