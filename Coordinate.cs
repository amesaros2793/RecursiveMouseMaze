/********************************************************
* Coordinate.cs
* Austen Mesaros
*
* This class defines a coordinate
*********************************************************/
public class Coordinate
{
    private int row;
    private int col;

    // no argument constructor
    public Coordinate()
    {
        this.row = 0;
        this.col = 0;
    }

    // two argument constructor
    public Coordinate(int row, int col)
    {
        this.row = row;
        this.col = col;
    }

    // copy constructor
    public Coordinate(Coordinate coordinate)
    {
        this.row = coordinate.row;
        this.col = coordinate.col;
    }

    //*******************************************************

    // accessors

    public int getRow()
    {
        return row;
    }

    public int getCol()
    {
        return col;
    }
}