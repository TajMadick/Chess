using System;

namespace Schach
{
    static class Programm
    {
        static void Main()
        {
            Grid grid = new Grid();
            Game game = new Game();
            //Board.FenParser("r5k1/1R2R3/8/8/8/8/5PPP/6K1 b - - 0 1", grid, game);
            Board.FenParser("5pkp/6pp/8/8/8/2b5/6PP/5BKP b - - 0 1", grid, game);
            game.GameLoop(grid);
        }
    }
}

