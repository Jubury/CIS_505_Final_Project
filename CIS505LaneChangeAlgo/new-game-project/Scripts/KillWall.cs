using Godot;
using System;

public partial class KillWall : Area2D
{
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node body)
    {
        if (body is NPCCar car)
        {
            car.DeleteCar();
        }
    }
}
