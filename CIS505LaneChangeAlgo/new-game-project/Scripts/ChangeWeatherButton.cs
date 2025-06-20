using Godot;
using System;

public partial class ChangeWeatherButton : Button
{
    public TextureRect weather;
    public override void _Ready()
    {
        // Connect the button's pressed signal to the on_pressed method
        weather = GetTree().Root.GetNode<TextureRect>("Level/RainTexture");
        // this.Pressed += on_pressed;
    }
    public void on_pressed()
    {
        GD.Print("Change Weather Button Pressed");
        weather.Visible = !weather.Visible; // Toggle the visibility of the weather texture
    }
}
