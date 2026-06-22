namespace Schach;

public class Grid
{
    private Pieces[,] grid = new Pieces[8, 8];

    public Pieces this[Tile tile]
    {
        get => grid[tile.Row, tile.Col];
        set => grid[tile.Row, tile.Col] = value;
    }
    
    public Pieces this[int row, int col]
    {
        get => grid[row, col];
        set => grid[row, col] = value;
    }

    public ref Pieces GetRef(int row, int col)
    {
        return ref grid[row, col];
    }
    
    public ref Pieces GetRef(Tile tile)
    {
        return ref grid[tile.Row, tile.Col];
    }

    public Grid()
    {
    }
    
    public Grid(Grid source)
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                grid[row, col] = source.grid[row, col].Copy();
            }
        }
    }

    public struct Tile(int row, int col)
    {
        public int Row = row;
        public int Col = col;

        public bool IsValid()
        {
            return Row is >= 0 and < 8 && Col is >= 0 and < 8;
        }
        
        public static bool operator ==(Tile t1, Tile t2)
        {
            return t1.Row == t2.Row && t1.Col == t2.Col;
        }
        
        public static bool operator !=(Tile t1, Tile t2)
        {
            return t1.Row != t2.Row || t1.Col != t2.Col;
        }
    }
}