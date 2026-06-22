using System;

namespace Schach
{
    static class Programm
    {
        static void Main()
        {
            Grid grid = new Grid();
            Game game = new Game();
            Board.FenParser("r1bqkb1r/pppp1Qpp/2n2n2/4p1N1/4P3/8/PPPP1PPP/RNB1K2R b KQkq -", grid, game);
            game.GameLoop(grid);
        }
    }
}

