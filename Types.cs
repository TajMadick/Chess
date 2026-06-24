namespace Schach;

public static class Types
{
    public enum Color
    {
        White,
        Black
    }
    
    public enum MoveType
    {
        Invalid,
        Promotion,
        Castling,
        Normal,
        EnPassant
    }

    public enum PieceType
    {
        Pawn,
        Knight,
        Bishop,
        Rook,
        Queen,
        King,
        Empty
    }
}