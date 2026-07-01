using System;

namespace Schach
{
    static class Programm
    {
        static void Main()
        {
            Board board = new Board("r3k3/p1ppqpb1/bn2pnpr/3PN3/1p2P3/5Q1p/PPPBBPPP/RN2K2R w KQq - 2 2");
            Game game = new Game();
            game.GameLoop(board);
        }
    }
}

