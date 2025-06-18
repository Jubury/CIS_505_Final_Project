using Godot;
using System;

[Tool]
public partial class PaletteSwap : Sprite2D
{
    [Export] public Texture2D paletteTexture;
    [Export] public Texture2D baseTexture;
    [Export] public int paletteIndex = 0; // Row in the palette image (0 = top), (1 = second row, etc.)

    private Color[] baseColors = new Color[5]; // Base Sprite Colors, will we replaced 1-by-1 with the palette colors

    public override void _Ready()
    {
        SetBaseColors();
        SwapPalette();
    }
    
    public void SetBaseColors()
    {
        for (int i = 0; i < baseColors.Length; i++)
        {
            baseColors[i] = paletteTexture.GetImage().GetPixel(i, 0);
        }
    }

    private void SwapPalette()
    {
        if (baseTexture == null)
        {
            GD.PrintErr("Base texture not assigned.");
            return;
        }

        if (paletteTexture == null)
        {
            GD.PrintErr("Palette texture not assigned.");
            return;
        }

        Image baseImage = baseTexture.GetImage();
        Image swappedImage = (Image)baseImage.Duplicate(); // keep original intact
        Image paletteImage = paletteTexture.GetImage();

        int paletteWidth = paletteImage.GetWidth();
        int paletteHeight = paletteImage.GetHeight();

        if (paletteIndex < 0 || paletteIndex >= paletteHeight)
        {
            GD.PrintErr($"Palette index {paletteIndex} is out of bounds (height = {paletteHeight})");
            return;
        }

        // Get replacement colors from the selected row
        Color[] targetColors = new Color[baseColors.Length];
        for (int i = 0; i < baseColors.Length; i++)
        {
            if (i < paletteWidth)
                targetColors[i] = paletteImage.GetPixel(i, paletteIndex);
            else
                targetColors[i] = baseColors[i]; // fallback to base if palette row too short
        }

        // Apply swap
        for (int y = 0; y < swappedImage.GetHeight(); y++)
        {
            for (int x = 0; x < swappedImage.GetWidth(); x++)
            {
                Color pixel = swappedImage.GetPixel(x, y);

                for (int i = 0; i < baseColors.Length; i++)
                {
                    if (ColorsMatch(pixel, baseColors[i]))
                    {
                        Color newColor = targetColors[i];
                        swappedImage.SetPixel(x, y, new Color(newColor.R, newColor.G, newColor.B, pixel.A));
                        break;
                    }
                }
            }
        }

        ImageTexture newTex = ImageTexture.CreateFromImage(swappedImage);
        Texture = newTex; // Set the result to Sprite2D
    }


    private bool ColorsMatch(Color a, Color b, float tolerance = 0.01f)
    {
        return Mathf.Abs(a.R - b.R) < tolerance &&
            Mathf.Abs(a.G - b.G) < tolerance &&
            Mathf.Abs(a.B - b.B) < tolerance;
    }

}
