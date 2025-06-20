using Godot;
using System;

public partial class Car : CharacterBody2D
{
    [Export] public float speed = 200f; // Target speed 
    public float currentSpeed = 0f;     // Runtime update speed
    [Export] public int colorIndex = 0; // Index of the color in the palette

    [Export] public RayCast2D rayForward;    // Forward raycast
    [Export] public RayCast2D rayBackward;   // Backward raycast
    [Export] public PointLight2D backLeftLight;
    [Export] public PointLight2D backRightLight;

    public override void _Ready()
    {
        rayBackward = GetNode<RayCast2D>("RayBackward");
        rayForward = GetNode<RayCast2D>("RayForward");

        backLeftLight = GetNode<PointLight2D>("LeftBlinker");
        backRightLight = GetNode<PointLight2D>("RightBlinker2");

        var paletteSwap = GetNode<PaletteSwap>("Sprite2D");
        paletteSwap.paletteIndex = colorIndex;
        paletteSwap._Ready();
    }

    public override void _PhysicsProcess(double delta)
    {
        currentSpeed = -Transform.Y.Dot(Velocity);
    }

    public virtual float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    protected void SetBrakeLights(bool state)
    {
        backLeftLight.Visible = state;
        backRightLight.Visible = state;
    }
}
