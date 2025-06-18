using Godot;
using System;

public partial class InputController : Car
{
    [Export] public PointLight2D[] leftBlinkers;
    [Export] public PointLight2D[] rightBlinkers;
    [Export] public float blinkInterval = 0.2f; // Time in seconds between blinks
    public bool isLeftSignalOn = false;
    public bool isRightSignalOn = false;
    public bool isChangingLanes = false; // Flag to check if the car is changing lanes
    public bool isSafeToChangeLane = true; // Flag to check if it's safe to change lanes



    public override void _Input(InputEvent @event)
    {
        if (!isChangingLanes)
        {
            if (@event.IsActionPressed("SignalLeft"))
            {
                if (isRightSignalOn)
                {
                    isRightSignalOn = false;
                    foreach (var blinker in rightBlinkers)
                    {
                        blinker.Visible = false; // Turn off left blinkers
                        blinkInterval = 0.2f;
                    }
                }
                else
                {
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
                    isLeftSignalOn = false;
                    foreach (var blinker in leftBlinkers)
                    {
                        blinker.Visible = false; // Turn off right blinkers
                        blinkInterval = 0.2f;
                    }
                }
                else
                {
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
        if (isLeftSignalOn)
        {
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

            if (IsSafeToChangeLane() && !isChangingLanes)
            {
                isChangingLanes = true; // Set the flag to true when changing lanes
                Tween tween = GetTree().CreateTween();
                Vector2 targetPosition = Position + new Vector2(-16, 0);
                tween.TweenProperty(this, "position", targetPosition, 0.6f);

                tween.Finished += () =>
                {
                    isChangingLanes = false; // Reset the flag after the tween is finished
                    isLeftSignalOn = false;
                    foreach (var blinker in leftBlinkers)
                    {
                        blinker.Visible = false; // Turn off left blinkers after changing lanes
                        blinkInterval = 0.2f;
                    }
                };
            }

        }
        if (isRightSignalOn)
        {
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

            if (IsSafeToChangeLane() && !isChangingLanes)
            {
                isChangingLanes = true; // Set the flag to true when changing lanes
                Tween tween = GetTree().CreateTween();
                Vector2 targetPosition = Position + new Vector2(16, 0);
                tween.TweenProperty(this, "position", targetPosition, 0.6f);

                tween.Finished += () =>
                {
                    isChangingLanes = false; // Reset the flag after the tween is finished
                    isRightSignalOn = false;
                    foreach (var blinker in rightBlinkers)
                    {
                        blinker.Visible = false; // Turn off right blinkers after changing lanes
                        blinkInterval = 0.2f;
                    }
                };
            }

        }
    }
    // private void UpdateRaycastDebugLines()
    // {
    //     string[] rayNames = {
    //         "Bottom_Right_1", "Bottom_Right_2", "Bottom_Right_3",
    //         "Top_Right_1", "Top_Right_2", "Top_Right_3",
    //         "Bottom_Left_1", "Bottom_Left_2", "Bottom_Left_3",
    //         "Top_Left_1", "Top_Left_2", "Top_Left_3",
    //         "Left", "Right"
    //     };

    //     foreach (string name in rayNames)
    //     {
    //         var ray = GetNodeOrNull<RayCast2D>("Rays/" + name);
    //         var line = GetNodeOrNull<Line2D>(name + "_Debug");

    //         if (ray == null || line == null)
    //             continue;

    //         if (ray.IsColliding())
    //         {
    //             Vector2 hitPos = ray.GetCollisionPoint();
    //             line.Points = new Vector2[] { ray.Position, ToLocal(hitPos) };
    //             line.DefaultColor = Colors.Red;
    //         }
    //         else
    //         {
    //             line.Points = new Vector2[] { ray.Position, ray.TargetPosition };
    //             line.DefaultColor = Colors.Green;
    //         }
    //     }
    // }

    public bool IsSafeToChangeLane()
    {
        bool isLeft = isLeftSignalOn;
        bool isRight = isRightSignalOn;

        if (!isLeft && !isRight)
            return false;

        string rayPrefix = "";
        if (isLeft)
        {
            rayPrefix = "Left";
        }
        else if(isRight)
        {
            rayPrefix = "Right";
        }

        // Check for wall or edge blocking the lane
        RayCast2D sideCheck = GetNode<RayCast2D>("Rays/" + rayPrefix);
        if (sideCheck.IsColliding() && !isChangingLanes)
        {
            GD.Print("Blocked: cannot change lane to the " + rayPrefix);
            return false;
        }

        // Hard safety block: avoid collision with car
        string dangerRayName = "";
        if (isLeft)
        {
            dangerRayName = "Left";
        }
        else if(isRight)
        {
            dangerRayName = "Right";
        }
        var dangerRay = GetNode<RayCast2D>("Rays/" + dangerRayName);
        if (dangerRay.IsColliding() && !isChangingLanes)
        {
            GD.Print("Immediate danger: " + dangerRayName + " detects a car");
            return false;
        }

        // Is target lane clear?
        string[] clearLaneRayNames = [];
        if (isLeft) {
            clearLaneRayNames = ["Bottom_Left_1", "Bottom_Left_2", "Bottom_Left_3", "Top_Left_3"];
        }
        else if(isRight) {
            clearLaneRayNames = ["Bottom_Right_1", "Bottom_Right_2", "Bottom_Right_3", "Top_Right_3"];
        }

        foreach (var rayName in clearLaneRayNames)
        {
            var ray = GetNode<RayCast2D>("Rays/" + rayName);
            if (ray.IsColliding() && !isChangingLanes)
            {
                GD.Print("Lane not clear: " + rayName + " hit object");
                return false;
            }
        }

        // Need to check if there is a speed advantage to changing lanes
        // ADD MORE HERE:

        if (isLeft)
        {
            GD.Print("SAFE TO CHANGE LANE LEFT");
        }
        else
        {
            GD.Print("SAFE TO CHANGE LANE RIGHT");
        }

        return true;
    }


}
