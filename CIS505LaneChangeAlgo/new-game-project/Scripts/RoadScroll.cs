using Godot;
using System;

public partial class RoadScroll : TileMapLayer
{
    [Export] public float scrollSpeed = 60f;     // Pixels per second
    [Export] public int tileHeight = 16;         // Height of each tile in pixels
    [Export] public int mapWidth = 10;           // Number of tiles across
    [Export] public int mapHeight = 20;          // Number of tiles down (visible area)

    private float pixelOffset = 0f;

    public override void _Process(double delta)
    {
        float moveAmount = scrollSpeed * (float)delta;
        pixelOffset += moveAmount;

        // Move the TileMapLayer downward
        Position += new Vector2(0, moveAmount);

        // Once moved down by 1 full tile, wrap and scroll
        if (pixelOffset >= tileHeight)
        {
            pixelOffset -= tileHeight;
            Position -= new Vector2(0, tileHeight);
            ScrollRoadDownByOneRow();
        }
    }

    private void ScrollRoadDownByOneRow()
    {
        for (int y = mapHeight - 1; y >= 0; y--)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                Vector2I from = new Vector2I(x, y - 1);
                Vector2I to = new Vector2I(x, y);

                if (y == 0)
                {
                    // Insert a new tile at the top row
                    SetCell(to, 0, GetLoopedTile(x));
                }
                else
                {
                    int sourceId = GetCellSourceId(from);
                    Vector2I atlasCoords = GetCellAtlasCoords(from);

                    if (sourceId != -1)
                        SetCell(to, sourceId, atlasCoords);
                    else
                        SetCell(to, -1);

                    SetCell(from, -1);
                }
            }
        }
    }

    private Vector2I GetLoopedTile(int x)
    {
        return new Vector2I(0, 0); // Tile at column 0, row 0 in your TileSet atlas
    }
}
