using System;

namespace Schach
{
    static class Programm
    {
        static void Main()
        {
            Grid grid = new Grid();
            Game game = new Game();
            Board.FenParser("r2qk2r/8/8/8/8/8/8/RBBQKBBR w KQkq -", grid, game);
            game.GameLoop(grid);
        }
    }
}

