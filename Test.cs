namespace Schach;

public static class Test
{
    public static int PossiblePositions(Grid grid, int depth, bool isWhiteMoving)
    {
        if (depth == 0) return 1;
        
        int positions = 0;

        Grid.Tile kingTile = Rules.FindKing(grid, isWhiteMoving);
            
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Grid.Tile fromTile = new Grid.Tile(row, col);
                foreach (Grid.Tile legalTile in Rules.AllLegalMoves(grid, fromTile, kingTile, isWhiteMoving))
                {
                    Pieces oldLegalTilePiece = grid[legalTile];
                    grid[legalTile] = grid[fromTile];
                    grid[fromTile] = new Empty();

                    positions += PossiblePositions(grid, depth - 1, !isWhiteMoving);
                    
                    grid[fromTile] = grid[legalTile];
                    grid[legalTile] = oldLegalTilePiece;
                }
            }
        }
        
        return positions;
    }

    public static void PossiblePositionsOfEachMove(Grid grid, int depth, bool isWhiteMoving)
    {
        if (depth == 0) return;

        Grid.Tile kingTile = Rules.FindKing(grid, isWhiteMoving);
            
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Grid.Tile fromTile = new Grid.Tile(row, col);
                foreach (Grid.Tile legalTile in Rules.AllLegalMoves(grid, fromTile, kingTile, isWhiteMoving))
                {
                    Pieces oldLegalTilePiece = grid[legalTile];
                    grid[legalTile] = grid[fromTile];
                    grid[fromTile] = new Empty();

                    Console.WriteLine($"{(char)(fromTile.Col + 'a')}{8-fromTile.Row}{(char)(legalTile.Col + 'a')}{8-legalTile.Row}: {PossiblePositions(grid, depth - 1, !isWhiteMoving)}");
                    
                    grid[fromTile] = grid[legalTile];
                    grid[legalTile] = oldLegalTilePiece;
                }
            }
        }
    }
}