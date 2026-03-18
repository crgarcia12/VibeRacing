namespace DustRacing2D.Game.Models;

public class TrackData
{
    public string Name { get; set; } = string.Empty;
    public int Cols { get; set; }
    public int Rows { get; set; }
    public int TileSize { get; set; }
    public List<TileData> Tiles { get; set; } = new();
    public List<CheckpointData> Checkpoints { get; set; } = new();
    public List<StartPosition> StartPositions { get; set; } = new();
}

public class TileData
{
    public int Col { get; set; }
    public int Row { get; set; }
    public string Type { get; set; } = string.Empty;
    public int Rotation { get; set; }
}

public class CheckpointData
{
    public int Index { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public bool IsFinishLine { get; set; }
}

public class StartPosition
{
    public int Slot { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Angle { get; set; }
}
