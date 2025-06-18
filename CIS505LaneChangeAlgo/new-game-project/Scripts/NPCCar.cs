using Godot;
using System;

public partial class NPCCar : Car
{
    [Export] public float desiredSpeed = 150f;    // Ideal cruising speed
    [Export] public float actualSpeed = 150f;        // Current speed
    [Export] public float acceleration = 100f;     // Pixels/sec^2
    [Export] public float deceleration = 300f;     // Pixels/sec^2
    [Export] public float followDistance = 40f;    // Ideal spacing in pixels
    [Export] public RayCast2D raycastAhead;        // Forward raycast

    [Export] public PointLight2D backLeftLight;
    [Export] public PointLight2D backRightLight;
    [Export] public PointLight2D frontLeftLight;
    [Export] public PointLight2D frontRightLight;

    public CarSpawn carSpawn; // Reference to the CarSpawn node

    public override void _Ready()
    {
        //Randomize desired speed within a range 3 to 50
        RandomNumberGenerator rng = new RandomNumberGenerator();
        rng.Randomize();
        desiredSpeed = rng.RandfRange(3f, 50f);
        actualSpeed = desiredSpeed;
        raycastAhead = GetNode<RayCast2D>("RayCast2D");
        followDistance = rng.RandfRange(20f, 75f); // Random follow distance between 20 and 75 pixels
        raycastAhead.TargetPosition = new Vector2(0, -followDistance); // Set raycast length based on follow distance
        base._Ready();
    }

    public override void _PhysicsProcess(double delta)
    {
        float targetSpeed = desiredSpeed;

        if (raycastAhead.IsColliding())
        {
            Node2D collider = raycastAhead.GetCollider() as Node2D;

            if (collider is NPCCar carAhead)
            {
                float distance = GlobalPosition.DistanceTo(carAhead.GlobalPosition);

                // Always slow down if raycast is colliding, regardless of distance
                if (distance < followDistance - 5)  //-5 to allow for some buffer
                {
                    targetSpeed = Mathf.Max(-10f, actualSpeed - deceleration * (float)delta);
                    SetBrakeLights(true);
                }
                else
                {
                    // Maintain carAhead’s speed or  own if it's slower
                    targetSpeed = Mathf.Min(carAhead.actualSpeed, desiredSpeed);
                    SetBrakeLights(false);
                }
            }
            else if (collider is InputController playerCar) //Player car
            {
                float distance = GlobalPosition.DistanceTo(playerCar.GlobalPosition);

                // Always slow down if raycast is colliding, regardless of distance
                if (distance < followDistance - 5) //-5 to allow for some buffer
                {
                    targetSpeed = Mathf.Max(-10f, actualSpeed - deceleration * (float)delta);
                    SetBrakeLights(true);
                }
                else
                {
                    // Maintain player's speed or own if it's slower
                    targetSpeed = Mathf.Min(0f, desiredSpeed); // Since speed is relative to the player car, 0 is player's speed
                    SetBrakeLights(false);
                }
            }
            else
            {
                // Unknown object ahead, come to stop
                targetSpeed = -25f;
                SetBrakeLights(true);
            }
        }
        else
        {
            // No collision, maintain desired speed
            targetSpeed = desiredSpeed;
            SetBrakeLights(false);
        }

        // Apply speed change
        if (actualSpeed < targetSpeed)
        {
            actualSpeed = Mathf.Min(actualSpeed + acceleration * (float)delta, targetSpeed);
        }
        else if (actualSpeed > targetSpeed)
        {
            actualSpeed = Mathf.Max(actualSpeed - deceleration * (float)delta, targetSpeed);
        }


        // Move forward
        Position += -Transform.Y * actualSpeed * (float)delta;
    }


    // Sets Brake lights
    private void SetBrakeLights(bool state)
    {
        backLeftLight.Visible = state;
        backRightLight.Visible = state;
    }

    public void DeleteCar()
    {
        if (carSpawn != null)
        {
            carSpawn.RemoveCar(this);
            QueueFree(); // Remove from scene
        }
        else
        {
            GD.PrintErr("Parent CarSpawn not found for NPCCar deletion.");
        }
    }
}
