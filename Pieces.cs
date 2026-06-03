namespace Schach;

public abstract class Pieces
{
    protected Pieces(bool isWhite)
    {
        IsWhite = isWhite;
    }
    public abstract char GetIcon();
    public abstract bool MoveAllowed(int fromRow, int fromCol, int toRow, int toCol);
    public bool IsWhite { get; }
}

public class Pawn : Pieces
{
    public Pawn(bool isWhite) : base(isWhite) {}

    public override bool MoveAllowed(int fromRow, int fromCol, int toRow, int toCol)
    {
        int start = (IsWhite) ? 6 : 1;
        int dir = (IsWhite) ? 1 : -1;
        
        // Pawn first move 2 fields
        if (fromRow == start && fromRow - 2 * dir == toRow && fromCol == toCol)
        {
            return true;
        }
        
        // Pawn normal move
        if (fromRow - 1 * dir == toRow)
        {
            return true;
        }

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

    public override bool MoveAllowed(int fromRow, int fromCol, int toRow, int toCol)
    {
        int diffCol = Math.Abs(toCol - fromCol);
        int diffRow = Math.Abs(toRow - fromRow);

        if (diffCol == 1 && diffRow == 2 ||
            diffCol == 2 && diffRow == 1)
        {
            return true;
        }

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

    public override bool MoveAllowed(int fromRow, int fromCol, int toRow, int toCol)
    {
        int diffCol = Math.Abs(toCol - fromCol);
        int diffRow = Math.Abs(toRow - fromRow);
        
        if (diffRow == diffCol)
        {
            return true;
        }

        return false;
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

    public override bool MoveAllowed(int fromRow, int fromCol, int toRow, int toCol)
    {
        if (fromRow == toRow || fromCol == toCol)
        {
            return true;
        }

        return false;
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

    public override bool MoveAllowed(int fromRow, int fromCol, int toRow, int toCol)
    {
        int diffCol = Math.Abs(toCol - fromCol);
        int diffRow = Math.Abs(toRow - fromRow);

        if (diffCol == diffRow || fromRow == toRow || fromCol == toCol)
        {
            return true;
        }

        return false;
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

    public override bool MoveAllowed(int fromRow, int fromCol, int toRow, int toCol)
    {
        int diffCol = Math.Abs(toCol - fromCol);
        int diffRow = Math.Abs(toRow - fromRow);

        if (diffCol <= 1 && diffRow <= 1)
        {
            return true;
        }

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
    public override bool MoveAllowed(int fromRow, int fromCol, int toRow, int toCol) => false;
    public override char GetIcon() => ' ';
}