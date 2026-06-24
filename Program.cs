using System;

namespace Schach
{
    static class Programm
    {
        static void Main()
        {
            Grid grid = new Grid();
            Game game = new Game();
            Board.FenParser("K2k4/2q5/8/8/8/8/8/8 b - - 0 1", grid, game);
            game.GameLoop(grid);
        }
    }
}

