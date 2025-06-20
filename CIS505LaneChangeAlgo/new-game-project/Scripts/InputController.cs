using Godot;
using System;

public partial class InputController : Car
{
    [Export] public PointLight2D[] leftBlinkers;
    [Export] public PointLight2D[] rightBlinkers;
    [Export] public float blinkInterval = 0.2f; // Time in seconds between blinks
    [Export] public RoadScroll roadScroll; // Reference to the RoadScroll node

    public bool isLeftSignalOn = false; // Flag to check if left signal is on
    public bool isRightSignalOn = false; // Flag to check if right signal is on
    public bool isChangingLanes = false; // Flag to check if the car is changing lanes
    public bool isSafeToChangeLane = true; // Flag to check if it's safe to change lanes

    private float laneChangeCooldown = 0f; // Cooldown after lane change
    private float mergeBufferTimer = 0f; // Tracks how long the lane has been safe
    private float mergeBufferThreshold = 1.5f; // Lane must be safe for this long

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _Input(InputEvent @event)
    {
        if (!isChangingLanes)
        {
            if (@event.IsActionPressed("SignalLeft"))
            {
                if (isRightSignalOn)
                {
                    // Turn off right signal if it's on
                    isRightSignalOn = false;
                    foreach (var blinker in rightBlinkers)
                    {
                        blinker.Visible = false; // Turn off right blinkers
                        blinkInterval = 0.2f;
                    }
                }
                else
                {
                    // Turn on left signal
                    isLeftSignalOn = true;
                    foreach (var blinker in leftBlinkers)
                    {
                        blinker.Visible = true; // Turn on left blinkers
                    }
                }
            }
            if (@event.IsActionPressed("SignalRight"))
            {
                if (isLeftSignalOn)
                {
                    // Turn off left signal if it's on
                    isLeftSignalOn = false;
                    foreach (var blinker in leftBlinkers)
                    {
                        blinker.Visible = false; // Turn off left blinkers
                        blinkInterval = 0.2f;
                    }
                }
                else
                {
                    // Turn on right signal
                    isRightSignalOn = true;
                    foreach (var blinker in rightBlinkers)
                    {
                        blinker.Visible = true; // Turn on right blinkers
                    }
                }
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        UpdateRaycastDebugLines();

        // Update cooldown timer
        if (laneChangeCooldown > 0f)
            laneChangeCooldown -= (float)delta;

        // Handle left turn signal
        if (isLeftSignalOn)
        {
            // Blinker logic for left side
            if (blinkInterval <= 0)
            {
                foreach (var blinker in leftBlinkers)
                {
                    if (blinker.Energy == 0f)
                        blinker.Energy = 1f;
                    else
                        blinker.Energy = 0f;
                }
                blinkInterval = 0.2f; // Reset the interval
            }
            blinkInterval -= (float)delta;

            if (!isChangingLanes)
            {
                if (IsSafeToChangeLane())
                    mergeBufferTimer += (float)delta;
                else
                    mergeBufferTimer = 0f;

                if (mergeBufferTimer >= mergeBufferThreshold && laneChangeCooldown <= 0f)
                {
                    Merging(new Vector2(-16, 0), true); // Start merging to the left
                    mergeBufferTimer = 0f;
                }
            }
        }

        // Handle right turn signal
        if (isRightSignalOn)
        {
            // Blinker logic for right side
            if (blinkInterval <= 0)
            {
                foreach (var blinker in rightBlinkers)
                {
                    if (blinker.Energy == 0f)
                        blinker.Energy = 1f;
                    else
                        blinker.Energy = 0f;
                }
                blinkInterval = 0.2f; // Reset the interval
            }
            blinkInterval -= (float)delta;

            if (!isChangingLanes)
            {
                if (IsSafeToChangeLane())
                    mergeBufferTimer += (float)delta;
                else
                    mergeBufferTimer = 0f;

                if (mergeBufferTimer >= mergeBufferThreshold && laneChangeCooldown <= 0f)
                {
                    Merging(new Vector2(16, 0), false); // Start merging to the right
                    mergeBufferTimer = 0f;
                }
            }
        }

        // This line came from version 1 and must be kept
        roadScroll.ChangeRoadSpeed();
    }

    // Update the visual debug lines for all rays
    private void UpdateRaycastDebugLines()
    {
        string[] rayNames = {
            "Bottom_Right_1", "Bottom_Right_2", "Bottom_Right_3",
            "Top_Right_1", "Top_Right_2", "Top_Right_3",
            "Bottom_Left_1", "Bottom_Left_2", "Bottom_Left_3",
            "Top_Left_1", "Top_Left_2", "Top_Left_3",
            "Left", "Right"
        };

        foreach (string name in rayNames)
        {
            var ray = GetNodeOrNull<RayCast2D>("Rays/" + name);
            var line = GetNodeOrNull<Line2D>("Rays/" + name + "_Line");

            if (ray == null || line == null)
            {
                GD.PrintErr("RayCast2D or Line2D node not found: " + name);
                continue;
            }
            if (ray.IsColliding())
            {
                Vector2 hitPos = ray.GetCollisionPoint();
                line.Points = new Vector2[] { ray.Position, ToLocal(hitPos) };
                line.DefaultColor = Colors.Red;
            }
            else
            {
                line.Points = new Vector2[] { ray.Position, ray.TargetPosition };
                line.DefaultColor = Colors.Green;
            }
        }
    }

    // Time to collision calculation
    public float TimeToCollision(Vector2 otherVehiclePostion, float otherVehicleSpeed)
    {
        float currentSpeed = GetCurrentSpeed();
        float relativeSpeed = Math.Abs(otherVehicleSpeed - currentSpeed);

        if (relativeSpeed < 0.1f)
            relativeSpeed = 0.1f;

        float distance = otherVehiclePostion.DistanceTo(GlobalPosition);
        return (distance / relativeSpeed) / 60;
    }

    // Determines if it is safe to change lanes
    public bool IsSafeToChangeLane()
    {
        bool isLeft = isLeftSignalOn;
        bool isRight = isRightSignalOn;

        if (!isLeft && !isRight)
            return false;

        string rayPrefix = isLeft ? "Left" : "Right";

        // Check for wall or lane boundary
        RayCast2D sideCheck = GetNode<RayCast2D>("Rays/" + rayPrefix);
        if (sideCheck.IsColliding() && !isChangingLanes)
        {
            GD.Print("Blocked: cannot change lane to the " + rayPrefix);
            return false;
        }

        // Check for immediate danger on the side
        RayCast2D dangerRay = GetNode<RayCast2D>("Rays/" + rayPrefix);
        if (dangerRay.IsColliding() && !isChangingLanes)
        {
            GD.Print("Immediate danger: " + rayPrefix + " detects a car");
            return false;
        }

        // Front vehicle TTC check
        string frontRayName = isLeft ? "Top_Left_2" : "Top_Right_2";
        RayCast2D frontRay = GetNode<RayCast2D>("Rays/" + frontRayName);
        if (frontRay.IsColliding() && !isChangingLanes)
        {
            Node2D frontCollider = frontRay.GetCollider() as Node2D;
            if (frontCollider != null && frontCollider.HasMethod("GetCurrentSpeed"))
            {
                float frontSpeed = (float)frontCollider.Call("GetCurrentSpeed");
                float frontTTC = TimeToCollision(frontCollider.GlobalPosition, frontSpeed);
                GD.Print("Front TTC: " + frontTTC);
                if (frontTTC < 3f)
                {
                    GD.Print("Unsafe FRONT TTC: Staying in lane.");
                    return false;
                }
            }
        }

        // Rear vehicle TTC check
        string rearRayName = isLeft ? "Bottom_Left_2" : "Bottom_Right_2";
        RayCast2D rearRay = GetNode<RayCast2D>("Rays/" + rearRayName);
        if (rearRay.IsColliding() && !isChangingLanes)
        {
            Node2D rearCollider = rearRay.GetCollider() as Node2D;
            if (rearCollider != null && rearCollider.HasMethod("GetCurrentSpeed"))
            {
                float rearSpeed = (float)rearCollider.Call("GetCurrentSpeed");
                float mySpeed = GetCurrentSpeed();

                if (rearSpeed > mySpeed)
                {
                    float rearTTC = TimeToCollision(rearCollider.GlobalPosition, rearSpeed);
                    GD.Print("Rear TTC: " + rearTTC);

                    if (rearTTC < 5f)
                    {
                        GD.Print("Unsafe REAR TTC: Staying in lane.");
                        return false;
                    }
                }
            }
        }

        // Check if target lane is clear using multiple rays
        string[] clearLaneRayNames = isLeft
            ? new[] { "Bottom_Left_1", "Bottom_Left_2", "Bottom_Left_3", "Top_Left_3" }
            : new[] { "Bottom_Right_1", "Bottom_Right_2", "Bottom_Right_3", "Top_Right_3" };

        foreach (var rayName in clearLaneRayNames)
        {
            var ray = GetNode<RayCast2D>("Rays/" + rayName);
            if (ray.IsColliding() && !isChangingLanes)
            {
                GD.Print("Lane not clear: " + rayName + " hit object");
                return false;
            }
        }

        // Lane is safe
        GD.Print(isLeft ? "SAFE TO CHANGE LANE LEFT" : "SAFE TO CHANGE LANE RIGHT");
        return true;
    }

    // Coroutine that handles mid-merge safety monitoring and potential abort
    private async void Merging(Vector2 targetOffset, bool isLeft)
    {
        isChangingLanes = true;
        Tween tween = GetTree().CreateTween();
        Vector2 startPosition = Position;
        Vector2 targetPosition = startPosition + targetOffset;

        // Begin lane change
        tween.TweenProperty(this, "position", targetPosition, 0.6f);
        float elapsed = 0f;
        float checkInterval = 0.1f; // Check every 100ms
        float mergeDuration = 0.6f;
        float mergeCommitPoint = 0.5f; // After this, too late to abort

        while (elapsed < mergeDuration)
        {
            await ToSignal(GetTree().CreateTimer(checkInterval), "timeout");
            elapsed += checkInterval;

            float progress = elapsed / mergeDuration;
            if (progress > mergeCommitPoint)
                continue; // Too late to abort

            // Real-time rear TTC check using rear rays
            string rearRayName = isLeft ? "Bottom_Left_2" : "Bottom_Right_2";
            RayCast2D rearRay = GetNode<RayCast2D>("Rays/" + rearRayName);

            if (rearRay.IsColliding())
            {
                Node2D rearCollider = rearRay.GetCollider() as Node2D;
                if (rearCollider != null && rearCollider.HasMethod("GetCurrentSpeed"))
                {
                    float rearSpeed = (float)rearCollider.Call("GetCurrentSpeed");
                    float rearTTC = TimeToCollision(rearCollider.GlobalPosition, rearSpeed);
                    GD.Print("Rear TTC (during merge): " + rearTTC);

                    if (rearTTC < 3f)
                    {
                        GD.Print("ABORTING MERGE: Vehicle speeding in!");
                        tween.Kill(); // Stop current tween
                        Tween cancelTween = GetTree().CreateTween();
                        cancelTween.TweenProperty(this, "position", startPosition, 0.3f);
                        await ToSignal(cancelTween, "finished");

                        // Reset lane change state
                        isChangingLanes = false;
                        isLeftSignalOn = false;
                        isRightSignalOn = false;
                        foreach (var blinker in leftBlinkers) blinker.Visible = false;
                        foreach (var blinker in rightBlinkers) blinker.Visible = false;
                        blinkInterval = 0.2f;
                        laneChangeCooldown = 1.5f;
                        return; // Exit the function early
                    }
                }
            }
        }

        // Merge completed successfully
        GD.Print("Merge completed.");
        isChangingLanes = false;
        isLeftSignalOn = false;
        isRightSignalOn = false;
        foreach (var blinker in leftBlinkers) blinker.Visible = false;
        foreach (var blinker in rightBlinkers) blinker.Visible = false;
        blinkInterval = 0.2f;
        laneChangeCooldown = 1.5f;
    }
}
