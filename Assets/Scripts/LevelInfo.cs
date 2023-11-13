public struct LevelInfo
{
    public int level_number;
    public int grid_width;
    public int grid_height;
    public int move_count;
    public string[] grid;
    
    public LevelInfo(int level_number, int grid_width, int grid_height, int move_count, string[] grid)
    {
        this.level_number = level_number;
        this.grid_width = grid_width;
        this.grid_height= grid_height;
        this.move_count = move_count;
        this.grid = grid;
    }
}