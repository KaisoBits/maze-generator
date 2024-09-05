using SFML.Graphics;
using SFML.System;

namespace MazeGenerator;

public class Tilemap : Transformable, Drawable
{
    private readonly Texture _texture;
    private readonly Vector2i _atlasSize;
    private readonly Vector2i _gridSize;

    private readonly VertexArray _vertexArray;
    private readonly Vector2i _textureTileSize;

    private readonly Tile[,] _map;

    private readonly Stack<Tile> _currentPath = new();

    public Tilemap(Texture texture, Vector2i atlasSize, Vector2i gridSize)
    {
        _texture = texture;
        _atlasSize = atlasSize;
        _gridSize = gridSize;

        _vertexArray = new VertexArray(PrimitiveType.Quads, 4);

        _textureTileSize = new Vector2i((int)_texture.Size.X / atlasSize.X, (int)_texture.Size.Y / atlasSize.Y);

        _map = new Tile[gridSize.X, gridSize.Y];
        for (int y = 0; y < gridSize.Y; y++)
            for (int x = 0; x < gridSize.X; x++)
            {
                Tile t = new(new Vector2i(x, y));
                _map[x, y] = t;
            }

        _currentPath.Push(GetTileAt(new(0, 0)) ?? throw new Exception("The starting node was out of range"));
    }

    public void Tick()
    {
        if (_currentPath.Count == 0)
        {
            Console.WriteLine("No unvisited tiles left");
            return;
        }

        Tile currentPos = _currentPath.Peek();

        Tile? unvisitedNeighbor = GetRandomUnvisitedNeighbor(currentPos.Position);
        if (unvisitedNeighbor == null)
        {
            _currentPath.Pop();
            Tick();
            return;
        }

        Vector2i difference = unvisitedNeighbor.Position - currentPos.Position;
        if (difference == new Vector2i(0, -1))
        {
            currentPos.OpenSide(WallSides.Up);
            unvisitedNeighbor.OpenSide(WallSides.Down);
        }
        else if (difference == new Vector2i(0, 1))
        {
            currentPos.OpenSide(WallSides.Down);
            unvisitedNeighbor.OpenSide(WallSides.Up);
        }
        else if (difference == new Vector2i(1, 0))
        {
            currentPos.OpenSide(WallSides.Right);
            unvisitedNeighbor.OpenSide(WallSides.Left);
        }
        else if (difference == new Vector2i(-1, 0))
        {
            currentPos.OpenSide(WallSides.Left);
            unvisitedNeighbor.OpenSide(WallSides.Right);
        }
        else
        {
            throw new Exception("Invalid neighbor offset");
        }

        _currentPath.Push(unvisitedNeighbor);
    }

    public void Draw(RenderTarget target, RenderStates states)
    {
        Vector2f fullSize = new Vector2f(_textureTileSize.X * _gridSize.X, _textureTileSize.Y * _gridSize.Y);

        for (uint x = 0; x < _gridSize.X; x++)
        {
            for (uint y = 0; y < _gridSize.Y; y++)
            {
                Tile tile = _map[x, y];

                RenderStates rs = states;
                rs.Transform.Translate(new Vector2f(x * _textureTileSize.X, y * _textureTileSize.Y) - (fullSize / 2.0f));

                DrawTile(tile, target, rs);
            }
        }
    }

    public void DrawTile(Tile tile, RenderTarget target, RenderStates states)
    {
        Vector2f textureCoord =
            (tile.HasWallOpen(WallSides.Right), tile.HasWallOpen(WallSides.Up), tile.HasWallOpen(WallSides.Left), tile.HasWallOpen(WallSides.Down))
            switch
            {
                (false, false, false, false) => new Vector2f(0, 0),
                (true, false, false, false) => new Vector2f(5, 0),
                (true, false, true, false) => new Vector2f(6, 0),
                (false, false, true, false) => new Vector2f(7, 0),
                (false, false, false, true) => new Vector2f(4, 1),
                (false, true, false, true) => new Vector2f(4, 2),
                (false, true, false, false) => new Vector2f(4, 3),
                (true, false, false, true) => new Vector2f(5, 1),
                (true, false, true, true) => new Vector2f(6, 1),
                (false, false, true, true) => new Vector2f(7, 1),
                (true, true, false, true) => new Vector2f(5, 2),
                (true, true, true, true) => new Vector2f(6, 2),
                (false, true, true, true) => new Vector2f(7, 2),
                (true, true, false, false) => new Vector2f(5, 3),
                (true, true, true, false) => new Vector2f(6, 3),
                (false, true, true, false) => new Vector2f(7, 3),
            };

        textureCoord = new Vector2f(textureCoord.X * _textureTileSize.X, textureCoord.Y * _textureTileSize.Y);

        _vertexArray[0] = new Vertex(new Vector2f(_textureTileSize.X, 0), textureCoord + new Vector2f(_textureTileSize.X, 0));
        _vertexArray[1] = new Vertex(new Vector2f(0, 0), textureCoord + new Vector2f(0, 0));
        _vertexArray[2] = new Vertex(new Vector2f(0, _textureTileSize.Y), textureCoord + new Vector2f(0, _textureTileSize.Y));
        _vertexArray[3] = new Vertex(new Vector2f(_textureTileSize.X, _textureTileSize.Y), textureCoord + new Vector2f(_textureTileSize.X, _textureTileSize.Y));
        states.Texture = _texture;

        target.Draw(_vertexArray, states);
    }

    private Tile? GetRandomUnvisitedNeighbor(Vector2i position)
    {
        Tile? up = GetTileAt(position - new Vector2i(0, 1));
        Tile? left = GetTileAt(position - new Vector2i(1, 0));
        Tile? down = GetTileAt(position + new Vector2i(0, 1));
        Tile? right = GetTileAt(position + new Vector2i(1, 0));

        List<Tile> _tiles = new(4);
        if (up is { Visited: false }) _tiles.Add(up);
        if (left is { Visited: false }) _tiles.Add(left);
        if (down is { Visited: false }) _tiles.Add(down);
        if (right is { Visited: false }) _tiles.Add(right);

        if (_tiles.Count == 0)
            return null;

        return _tiles[Random.Shared.Next(_tiles.Count)];
    }

    private Tile? GetTileAt(Vector2i position)
    {
        return IsInBounds(position) ? _map[position.X, position.Y] : null;
    }

    private bool IsInBounds(Vector2i position)
    {
        return position.X >= 0 && position.X < _gridSize.X &&
            position.Y >= 0 && position.Y < _gridSize.Y;
    }
}
