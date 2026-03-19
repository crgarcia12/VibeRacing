namespace VibeRacing.Game.Models;

public class TrackData
{
    public string Name { get; set; } = string.Empty;
    public int Cols { get; set; }
    public int Rows { get; set; }
    public int TileSize { get; set; }
    public List<TileData> Tiles { get; set; } = new();
    public List<CheckpointData> Checkpoints { get; set; } = new();
    public List<StartPosition> StartPositions { get; set; } = new();

    public double Width => Cols * TileSize;
    public double Height => Rows * TileSize;

    public bool IsOnRoad(double x, double y)
    {
        if (x < 0 || y < 0 || x >= Width || y >= Height || TileSize <= 0)
            return false;

        int col = (int)(x / TileSize);
        int row = (int)(y / TileSize);
        return Tiles.Any(tile => tile.Col == col && tile.Row == row);
    }

    public bool IsInsidePlayableArea(double x, double y, double margin)
    {
        foreach (var tile in Tiles)
        {
            GetExpandedTileBounds(tile, margin, out var left, out var top, out var right, out var bottom);
            if (x >= left && x <= right && y >= top && y <= bottom)
                return true;
        }

        return false;
    }

    public bool TryClampToPlayableArea(double x, double y, double margin, out double clampedX, out double clampedY, out double normalX, out double normalY)
    {
        clampedX = x;
        clampedY = y;
        normalX = 0;
        normalY = 0;

        if (Tiles.Count == 0 || IsInsidePlayableArea(x, y, margin))
            return false;

        double bestDistanceSq = double.MaxValue;

        foreach (var tile in Tiles)
        {
            GetExpandedTileBounds(tile, margin, out var left, out var top, out var right, out var bottom);

            double candidateX = Math.Clamp(x, left, right);
            double candidateY = Math.Clamp(y, top, bottom);
            double dx = x - candidateX;
            double dy = y - candidateY;
            double distanceSq = dx * dx + dy * dy;

            if (distanceSq >= bestDistanceSq)
                continue;

            bestDistanceSq = distanceSq;
            clampedX = candidateX;
            clampedY = candidateY;
            normalX = dx;
            normalY = dy;
        }

        if (bestDistanceSq <= 1e-12)
            return false;

        double length = Math.Sqrt(normalX * normalX + normalY * normalY);
        if (length > 1e-6)
        {
            normalX /= length;
            normalY /= length;
        }
        else
        {
            normalX = 0;
            normalY = 0;
        }

        return true;
    }

    private void GetExpandedTileBounds(TileData tile, double margin, out double left, out double top, out double right, out double bottom)
    {
        left = (tile.Col * TileSize) - margin;
        top = (tile.Row * TileSize) - margin;
        right = left + TileSize + (margin * 2.0);
        bottom = top + TileSize + (margin * 2.0);
    }
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
