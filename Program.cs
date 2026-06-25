using System;

namespace Schach
{
    static class Programm
    {
        static void Main()
        {
            Grid grid = new Grid();
            Game game = new Game();
            //Board.FenParser("7k/8/4R3/8/4Q3/8/8/7K w KQkq - 0 1", grid, game);
            Board.FenParser("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", grid, game);
            game.GameLoop(grid);
        }
    }
}

