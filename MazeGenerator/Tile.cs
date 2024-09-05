using SFML.System;

namespace MazeGenerator;

public class Tile
{
    public Vector2i Position { get; }

    public bool Visited { get; private set; }

    public WallSides OpenedWalls { get; private set; }

    public Tile(Vector2i position)
    {
        Position = position;
    }

    public bool HasWallOpen(WallSides side) => (OpenedWalls & side) == side;

    public void OpenSide(WallSides side)
    {
        OpenedWalls |= side;
        Visited = true;
    }
}

[Flags]
public enum WallSides { None = 0, Left = 1, Right = 1 << 1, Up = 1 << 2, Down = 1 << 3 }