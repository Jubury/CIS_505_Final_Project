using Godot;
using System;

public partial class Car : CharacterBody2D
{
    [Export] public float speed = 200f;
    [Export] public int colorIndex = 0; // Index of the color in the palette

    public override void _Ready()
    {
        GetNode<PaletteSwap>("Sprite2D").paletteIndex = colorIndex;
        GetNode<PaletteSwap>("Sprite2D")._Ready();
    }
}
