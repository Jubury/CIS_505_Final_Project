using Godot;
using System;

public partial class RoadScroll : TileMapLayer
{
    [Export] public float scrollSpeed = 60f;     // Pixels per second
    [Export] public float desiredSpeed = 60f;    // Desired speed of the road
    [Export] public int tileHeight = 16;         // Height of each tile in pixels
    [Export] public int mapWidth = 10;           // Number of tiles across
    [Export] public int mapHeight = 20;          // Number of tiles down (visible area)
    [Export] public InputController simulationCar; // Reference to the InputController
    public float difference = 0f; // Difference between desired speed and current road speed
    private float pixelOffset = 0f;

    public override void _PhysicsProcess(double delta)
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

    public void ChangeRoadSpeed()
    {
        NPCCar frontCar = null;
        if (simulationCar == null)
            return;
        // if car in front of simulationCar is too slow, decrease road speed
        // then increase speed of all other cars by the same amount
        if (simulationCar.rayForward.IsColliding())
        {
            var collider = simulationCar.rayForward.GetCollider();
            NPCCar firstCar = null;
            if (collider is NPCCar carInFront)
            {
                firstCar = carInFront;
                while (carInFront.rayForward.IsColliding())
                {
                    // Keep checking until we find a car that is not colliding
                    carInFront = carInFront.rayForward.GetCollider() as NPCCar;
                }
                frontCar = carInFront;
            }
            var targetSpeed = frontCar.actualSpeed;
            if (targetSpeed == 0f)
            {
                simulationCar.SetBrakeLights(false); // Turn on brake lights
                return;
            }
            else if (targetSpeed < 0f)
            {
                simulationCar.SetBrakeLights(true); // Turn on brake lights
                float newScrollSpeed = Mathf.Max(desiredSpeed - 30f, scrollSpeed + targetSpeed);
                if(scrollSpeed > newScrollSpeed)
                {
                    scrollSpeed -= newScrollSpeed / 5f; // Don't go below logical min
                }
            }
            //if the distance to the first car is less than 15, then we need to slow down the road speed and speed up the other cars until distance is greater than 15
            if (simulationCar.GlobalPosition.DistanceTo(frontCar.GlobalPosition) < 15f)
            {
                simulationCar.SetBrakeLights(true); // Turn on brake lights
                targetSpeed = frontCar.actualSpeed - 15f;
                float newScrollSpeed = Mathf.Max(desiredSpeed - 30f, scrollSpeed + targetSpeed);
                if(scrollSpeed > newScrollSpeed)
                {
                    scrollSpeed -= newScrollSpeed / 5f; // Don't go below logical min
                }
            }
            // Update all other cars' speeds
            foreach (var car in GetTree().GetNodesInGroup("Cars"))
            {
                if (car is NPCCar otherCar)
                {
                    otherCar.desiredSpeed += -targetSpeed; // normalize the speed of all other cars
                    otherCar.actualSpeed += -targetSpeed; // normalize the speed of all other cars
                }
            }

            GD.Print($"Car in front new speed: {frontCar.actualSpeed}, Current road speed: {scrollSpeed}");

        }
        else
        {
            difference = desiredSpeed - scrollSpeed;
            difference = Mathf.Clamp(difference, 0f, 20f); // Prevent it from exceeding logical bounds

            if (difference == 0f)
            {
                
                return; // No change needed if already at desired speed
            }
            if (scrollSpeed < desiredSpeed)
                scrollSpeed += difference/5f; // Increase road speed to desired speed

            foreach (var car in GetTree().GetNodesInGroup("Cars"))
            {
                if (car is NPCCar otherCar)
                {
                    otherCar.desiredSpeed -= difference/5f; // Reset all cars to desired speed
                    otherCar.actualSpeed -= difference/5f; // Reset all cars to desired speed
                }
            }
            if (frontCar != null)
                GD.Print($"Car in front new speed: {frontCar.actualSpeed}, Current road speed: {scrollSpeed}");
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
