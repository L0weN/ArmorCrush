public class GridObject<T>
{
    GridSystem2D<GridObject<T>> grid;
    int x;
    int y;
    T crush;

    public GridObject(GridSystem2D<GridObject<T>> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
    }

    public void SetValue(T crush)
    {
        this.crush = crush;
    }

    public T GetValue()
    {
        return crush;
    }
}
