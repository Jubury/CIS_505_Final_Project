using Godot;
using System;

public partial class NPCCar : Car
{
    [Export] public float desiredSpeed = 0;
    [Export] public float actualSpeed = 0;
    [Export] public float acceleration = 100f;
    [Export] public float deceleration = 300f;
    [Export] public float followDistance = 40f;
    [Export] public PointLight2D frontLeftLight;
    [Export] public PointLight2D frontRightLight;
    public RoadScroll roadScroll;
    public CarSpawn carSpawn;
    public bool IsPacingSimulationLane = false;

    public override void _Ready()
    {
        RandomNumberGenerator rng = new RandomNumberGenerator();
        rng.Randomize();
        followDistance = rng.RandfRange(30f, 55f);
        rayForward.TargetPosition = new Vector2(0, -followDistance);
        roadScroll = (RoadScroll) GetTree().GetNodesInGroup("RoadScroll")[0];
        base._Ready();
    }

    public override void _PhysicsProcess(double delta)
    {
        // Pacing behavior
        float targetSpeed = desiredSpeed;
        if (IsPacingSimulationLane)
        {
            targetSpeed = 0f;

            if (actualSpeed > targetSpeed)
                actualSpeed = Mathf.Max(actualSpeed - deceleration * (float)delta, targetSpeed);
            else if (actualSpeed < targetSpeed)
                actualSpeed = Mathf.Min(actualSpeed + acceleration * (float)delta, targetSpeed);

            SetBrakeLights(true);
            Position += -Transform.Y * actualSpeed * (float)delta;
            return;
        }

        targetSpeed = desiredSpeed;

        if (rayForward.IsColliding())
        {
            Node2D collider = rayForward.GetCollider() as Node2D;

            if (collider is NPCCar carAhead)
            {
                float distance = GlobalPosition.DistanceTo(carAhead.GlobalPosition);

                if (distance < followDistance - 5)
                {
                    targetSpeed = Mathf.Max(-25f, actualSpeed - deceleration * (float)delta);
                    SetBrakeLights(true);
                }
                else
                {
                    targetSpeed = Mathf.Min(carAhead.actualSpeed, desiredSpeed);
                    SetBrakeLights(false);
                }
            }
            else if (collider is InputController playerCar)
            {
                float distance = GlobalPosition.DistanceTo(playerCar.GlobalPosition);

                if (distance < followDistance - 5)
                {
                    targetSpeed = Mathf.Max(-25f, actualSpeed - deceleration * (float)delta);
                    SetBrakeLights(true);
                }
                else
                {
                    targetSpeed = Mathf.Min(0f, desiredSpeed);
                    SetBrakeLights(false);
                }
            }
            else
            {
                targetSpeed = -25f;
                SetBrakeLights(true);
                GD.Print("Unexpected collider");
            }
        }
        else
        {
            targetSpeed = desiredSpeed;
            SetBrakeLights(false);
        }

        if (actualSpeed < targetSpeed)
            actualSpeed = Mathf.Min(actualSpeed + acceleration * (float)delta, targetSpeed);
        else if (actualSpeed > targetSpeed)
        {
            SetBrakeLights(true);
            actualSpeed = Mathf.Max(actualSpeed - deceleration * (float)delta, targetSpeed);
        }

        Position += -Transform.Y * actualSpeed * (float)delta;
    }

    public void SetRandomSpeed(bool slowCar)
    {
        RandomNumberGenerator rng = new RandomNumberGenerator();
        rng.Randomize();

        if (slowCar) {
            desiredSpeed = rng.RandfRange(-20f, -3f) + roadScroll.difference;
        }
        else {
            desiredSpeed = rng.RandfRange(4f, 40f) + roadScroll.difference;
        } 
        actualSpeed = desiredSpeed;
    }

    public void DeleteCar()
    {
        carSpawn?.RemoveCar(this);
        QueueFree();
    }
}
