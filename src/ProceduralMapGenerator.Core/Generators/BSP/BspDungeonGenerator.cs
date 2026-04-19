using ProceduralMapGenerator.Core.Models;

namespace ProceduralMapGenerator.Core.Generators;

public class BspDungeonGenerator : IMapGenerator
{
    private readonly BspGeneratorOptions _options;
    private Random _rng = null!;
    private Map _map = null!;

    public BspDungeonGenerator(BspGeneratorOptions options)
    {
        _options = options;
    }

    public Map Generate(int width, int height, int seed)
    {
        _rng = new Random(seed);
        _map = new Map(width, height);

        var root = new BspNode(0, 0, width, height);
        Split(root);
        PlaceRooms(root);
        ConnectRooms(root);

        if (_options.PlaceDoors)
            PlaceDoors();

        return _map;
    }

    // ── Partitioning ────────────────────────────────────────────────────────

    private void Split(BspNode node)
    {
        bool canSplitH = node.Height >= _options.MinPartitionSize * 2;
        bool canSplitV = node.Width  >= _options.MinPartitionSize * 2;

        if (!canSplitH && !canSplitV) return;

        // Prefer splitting the longer axis; fall back to the only viable axis.
        bool splitH = canSplitH && (!canSplitV || node.Height >= node.Width);

        // Randomise the split position around the configured ratio.
        float ratio = Math.Clamp(
            _options.SplitRatio + (_rng.NextSingle() - 0.5f) * 0.4f,
            0.3f, 0.7f);

        if (splitH)
        {
            int splitAt = node.Y + (int)(node.Height * ratio);
            int topH    = splitAt - node.Y;
            int botH    = node.Height - topH;
            if (topH < _options.MinPartitionSize || botH < _options.MinPartitionSize) return;

            node.Left  = new BspNode(node.X, node.Y,   node.Width, topH);
            node.Right = new BspNode(node.X, splitAt,  node.Width, botH);
        }
        else
        {
            int splitAt = node.X + (int)(node.Width * ratio);
            int leftW   = splitAt - node.X;
            int rightW  = node.Width - leftW;
            if (leftW < _options.MinPartitionSize || rightW < _options.MinPartitionSize) return;

            node.Left  = new BspNode(node.X,   node.Y, leftW,  node.Height);
            node.Right = new BspNode(splitAt,  node.Y, rightW, node.Height);
        }

        Split(node.Left!);
        Split(node.Right!);
    }

    // ── Room placement ───────────────────────────────────────────────────────

    private void PlaceRooms(BspNode node)
    {
        if (!node.IsLeaf)
        {
            if (node.Left  != null) PlaceRooms(node.Left);
            if (node.Right != null) PlaceRooms(node.Right);
            return;
        }

        const int pad = 1;
        int maxW = Math.Min(_options.MaxRoomSize, node.Width  - pad * 2);
        int maxH = Math.Min(_options.MaxRoomSize, node.Height - pad * 2);

        if (maxW < _options.MinRoomSize || maxH < _options.MinRoomSize) return;

        int roomW = _rng.Next(_options.MinRoomSize, maxW + 1);
        int roomH = _rng.Next(_options.MinRoomSize, maxH + 1);

        int xSlack = node.Width  - roomW - pad * 2;
        int ySlack = node.Height - roomH - pad * 2;

        int roomX = node.X + pad + (xSlack > 0 ? _rng.Next(xSlack + 1) : 0);
        int roomY = node.Y + pad + (ySlack > 0 ? _rng.Next(ySlack + 1) : 0);

        var room = new Room(roomX, roomY, roomW, roomH);
        node.Room = room;
        _map.Rooms.Add(room);

        for (int x = roomX; x < roomX + roomW; x++)
            for (int y = roomY; y < roomY + roomH; y++)
                if (_map.InBounds(x, y))
                    _map.SetCell(x, y, TileType.Floor);
    }

    // ── Corridor carving ─────────────────────────────────────────────────────

    private void ConnectRooms(BspNode node)
    {
        if (node.IsLeaf) return;

        if (node.Left  != null) ConnectRooms(node.Left);
        if (node.Right != null) ConnectRooms(node.Right);

        var a = GetRoom(node.Left);
        var b = GetRoom(node.Right);

        if (a != null && b != null)
            CarveCorridor(a, b);
    }

    // Returns any reachable room from a subtree, chosen at random for variety.
    private Room? GetRoom(BspNode? node)
    {
        if (node == null) return null;
        if (node.Room != null) return node.Room;

        var left  = GetRoom(node.Left);
        var right = GetRoom(node.Right);

        return (left, right) switch
        {
            (null, _) => right,
            (_, null) => left,
            _         => _rng.Next(2) == 0 ? left : right
        };
    }

    private void CarveCorridor(Room a, Room b)
    {
        int x1 = a.CenterX, y1 = a.CenterY;
        int x2 = b.CenterX, y2 = b.CenterY;

        // L-shaped: randomly choose whether to go horizontal-first or vertical-first.
        if (_rng.Next(2) == 0)
        {
            CarveH(y1, x1, x2);
            CarveV(x2, y1, y2);
        }
        else
        {
            CarveV(x1, y1, y2);
            CarveH(y2, x1, x2);
        }
    }

    private void CarveH(int y, int x1, int x2)
    {
        for (int x = Math.Min(x1, x2); x <= Math.Max(x1, x2); x++)
            TryCarve(x, y);
    }

    private void CarveV(int x, int y1, int y2)
    {
        for (int y = Math.Min(y1, y2); y <= Math.Max(y1, y2); y++)
            TryCarve(x, y);
    }

    // Only overwrites Wall cells; leaves Floor and existing Corridor intact.
    private void TryCarve(int x, int y)
    {
        if (_map.InBounds(x, y) && _map.GetCell(x, y).Type == TileType.Wall)
            _map.SetCell(x, y, TileType.Corridor);
    }

    // ── Door placement ───────────────────────────────────────────────────────

    // Any Floor cell touching a Corridor is a room entrance → Door.
    private void PlaceDoors()
    {
        for (int x = 0; x < _map.Width; x++)
            for (int y = 0; y < _map.Height; y++)
                if (_map.GetCell(x, y).Type == TileType.Floor && HasAdjacentCorridor(x, y))
                    _map.SetCell(x, y, TileType.Door);
    }

    private bool HasAdjacentCorridor(int x, int y) =>
        IsCorridorAt(x - 1, y) || IsCorridorAt(x + 1, y) ||
        IsCorridorAt(x, y - 1) || IsCorridorAt(x, y + 1);

    private bool IsCorridorAt(int x, int y) =>
        _map.InBounds(x, y) && _map.GetCell(x, y).Type == TileType.Corridor;
}
