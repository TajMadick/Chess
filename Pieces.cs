namespace Schach;

public abstract class Pieces
{
    protected Pieces(bool isWhite)
    {
        IsWhite = isWhite;
    }
    public abstract char GetIcon();
    public abstract bool MoveAllowed(int fromRow, int fromCol, int toRow, int toCol, Pieces[,] boardPieces);

    public bool LoopThrough(int fromRow, int fromCol, int toRow, int toCol, int dirRow, int dirCol, Pieces[,] boardPieces)
    {
        fromRow += dirRow;
        fromCol += dirCol;
        for (; fromRow is >= 0 and < 8 && fromCol is >= 0 and < 8; fromRow += dirRow, fromCol += dirCol)
        {
            // wenn die Runner es bis zum Ziel geschafft haben -> true
            if (fromCol == toCol && fromRow == toRow)
            {
                return true;
            }

            // wenn ein Feld auf dem Weg nicht leer ist -> false
            if (boardPieces[fromRow, fromCol] is not Empty)
            {
                Console.Write("Piece is in the way");
                Console.ReadKey();
                return false;
            }
        }

        Console.Write("Incorrect movement");
        Console.ReadKey();
        return false;
    }
    public bool IsWhite { get; }
}

public class Pawn : Pieces
{
    public Pawn(bool isWhite) : base(isWhite) {}

    public override bool MoveAllowed(int fromRow, int fromCol, int toRow, int toCol, Pieces[,] boardPieces)
    {
        int start = (IsWhite) ? 6 : 1;
        int dir = (IsWhite) ? 1 : -1;

        int moveTwoField = fromRow - 2 * dir;
        int moveOneField = fromRow - 1 * dir;

        int diffCol = Math.Abs(toCol - fromCol);

        // Diagonal nehmen -> nur 1 nach links oder rechts, es muss eine Figur dort stehen, es muss das nächste Feld sein
        if (diffCol == 1 && boardPieces[toRow, toCol] is not Empty && moveOneField == toRow)
        {
            return true;
        }
        
        if (fromCol != toCol)
        {
            Console.Write("Pawn can only move vertically");
            Console.ReadKey();
            return false;
        }
        
        // Pawn first move 2 fields
        if (fromRow == start && moveTwoField == toRow)
        {
            // check if nextField and next-nextField is free
            if (boardPieces[moveOneField, fromCol] is Empty && boardPieces[moveTwoField, fromCol] is Empty)
            {
                return true;
            }
        }
        
        // Pawn normal move
        if (moveOneField == toRow)
        {
            // check if nextField is free
            if (boardPieces[moveOneField, fromCol] is Empty)
            {
                return true;
            }
        }

        Console.Write("Incorrect movement");
        Console.ReadKey();
        return false;
    }
    public override char GetIcon()
    {
        if (IsWhite) return '♟';
        else return '♙';
    }
}

public class Knight : Pieces
{
    public Knight(bool isWhite) : base(isWhite) { }

    public override bool MoveAllowed(int fromRow, int fromCol, int toRow, int toCol, Pieces[,] boardPieces)
    {
        int diffCol = Math.Abs(toCol - fromCol);
        int diffRow = Math.Abs(toRow - fromRow);

        if (diffCol == 1 && diffRow == 2 ||
            diffCol == 2 && diffRow == 1)
        {
            return true;
        }

        Console.Write("Incorrect movement");
        Console.ReadKey();
        return false;
    }

    public override char GetIcon()
    {
        if (IsWhite) return '♞';
        else return '♘';
    }
}

public class Bishop : Pieces
{
    public Bishop(bool isWhite) : base(isWhite) { }

    public override bool MoveAllowed(int fromRow, int fromCol, int toRow, int toCol, Pieces[,] boardPieces)
    {
        int diffCol = Math.Abs(toCol - fromCol);
        int diffRow = Math.Abs(toRow - fromRow);
        
        int dirCol = (toCol - fromCol > 0) ? 1 : -1;
        int dirRow = (toRow - fromRow > 0) ? 1 : -1;
            
        return LoopThrough(fromRow, fromCol, toRow, toCol, dirRow, dirCol, boardPieces);
    }

    public override char GetIcon()
    {
        if (IsWhite) return '♝';
        else return '♗';
    }
}

public class Rook : Pieces
{
    public Rook(bool isWhite) : base(isWhite) { }

    public override bool MoveAllowed(int fromRow, int fromCol, int toRow, int toCol, Pieces[,] boardPieces)
    {
        int dirCol = 0, dirRow = 0;
        
        // Rook läuft auf Cols -> Horizontal
        if (fromRow == toRow)
        {
            dirCol = (toCol - fromCol > 0) ? 1 : -1;

        }
        
        // Rook läuft auf Rows -> Vertikal
        if (fromCol == toCol)
        {
            dirRow = (toRow - fromRow > 0) ? 1 : -1;
        }
            
        return LoopThrough(fromRow, fromCol, toRow, toCol, dirRow, dirCol, boardPieces);
    }

    public override char GetIcon()
    {
        if (IsWhite) return '♜';
        else return '♖';
    }
}

public class Queen : Pieces
{
    public Queen(bool isWhite) : base(isWhite) { }

    public override bool MoveAllowed(int fromRow, int fromCol, int toRow, int toCol, Pieces[,] boardPieces)
    {
        int diffCol = Math.Abs(toCol - fromCol);
        int diffRow = Math.Abs(toRow - fromRow);
        
        int dirCol = 0, dirRow = 0;
        
        // Queen läuft auf Cols -> Horizontal
        if (fromRow == toRow)
        {
            dirCol = (toCol - fromCol > 0) ? 1 : -1;

        }
        
        // Queen läuft auf Rows -> Vertikal
        if (fromCol == toCol)
        {
            dirRow = (toRow - fromRow > 0) ? 1 : -1;
        }

        // Queen läuft diagonal
        if (diffCol == diffRow)
        {
            dirCol = (toCol - fromCol > 0) ? 1 : -1;
            dirRow = (toRow - fromRow > 0) ? 1 : -1;
        }
        
        return LoopThrough(fromRow, fromCol, toRow, toCol, dirRow, dirCol, boardPieces);
    }

    public override char GetIcon()
    {
        if (IsWhite) return '♛';
        else return '♕';
    }
}

public class King : Pieces
{
    public King(bool isWhite) : base(isWhite) { }

    public override bool MoveAllowed(int fromRow, int fromCol, int toRow, int toCol, Pieces[,] boardPieces)
    {
        int diffCol = Math.Abs(toCol - fromCol);
        int diffRow = Math.Abs(toRow - fromRow);

        if (diffCol <= 1 && diffRow <= 1)
        {
            return true;
        }

        Console.Write("Incorrect movement");
        Console.ReadKey();
        return false;
    }

    public override char GetIcon()
    {
        if (IsWhite) return '♚';
        else return '♔';
    }
}

public class Empty : Pieces
{
    public Empty() : base(false) { }
    public override bool MoveAllowed(int fromRow, int fromCol, int toRow, int toCol, Pieces[,] boardPieces) => false;
    public override char GetIcon() => ' ';
}