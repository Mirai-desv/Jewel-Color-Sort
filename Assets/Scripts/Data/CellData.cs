// Chứa dữ liệu của một ô
[System.Serializable]
public class CellData
{
    public Type type;
    public int x;
    public int y;

    public CellData(Type type, int x, int y)
    {
        this.type = type;
        this.x = x;
        this.y = y;
    }
}
