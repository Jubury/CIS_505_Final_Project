using Godot;
using System;

public partial class Car : CharacterBody2D
{
    [Export] public int colorIndex = 0; // Index of the color in the palette
    [Export] public RayCast2D rayForward;        // Forward raycast
    [Export] public RayCast2D rayBackward;        // Backward raycast
    [Export] public PointLight2D backLeftLight;
    [Export] public PointLight2D backRightLight;
    public override void _Ready()
    {
        backLeftLight = GetNode<PointLight2D>("LeftBlinker");
        backRightLight = GetNode<PointLight2D>("RightBlinker2");
        rayBackward = GetNode<RayCast2D>("RayBackward");
        rayForward = GetNode<RayCast2D>("RayForward");
        GetNode<PaletteSwap>("Sprite2D").paletteIndex = colorIndex;
        GetNode<PaletteSwap>("Sprite2D")._Ready();
    }

    protected void SetBrakeLights(bool state)
    {
        backLeftLight.Visible = state;
        backRightLight.Visible = state;
    }
}
