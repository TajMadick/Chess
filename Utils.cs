namespace Schach;

public static class Utils
{
    public static void CalculateCoordinates(string move, out Grid.Tile fromTile, out Grid.Tile toTile)
    {
        char fromLetter = move[0];
        char fromNumber = move[1];
        char toLetter = move[2];
        char toNumber = move[3];

        fromTile = new Grid.Tile(7 - (fromNumber - '1'), fromLetter - 'a');
        toTile = new Grid.Tile(7 - (toNumber - '1'), toLetter - 'a');
    }
}