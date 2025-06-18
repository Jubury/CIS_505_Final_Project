using Godot;
using System;
using System.Collections.Generic;

public partial class CarSpawn : Node2D
{
    [Export] PackedScene carScene = GD.Load<PackedScene>("res://Scenes/Car.tscn"); // The scene to instantiate for the car
    public List<NPCCar> cars = new List<NPCCar>(); // Array to hold the spawned cars
    public int maxCars = 5; // Maximum number of cars to spawn
    public float spawnTimer = 0f; // Timer to control spawn rate
    [Export] public float spawnInterval = 2f; // Time in seconds between spawns


    public override void _Ready()
    {
        RandomNumberGenerator rng = new RandomNumberGenerator();
        rng.Randomize();
        spawnInterval = rng.RandfRange(3f, 7f); // Random spawn interval between 3 and 7 seconds
    }
    public override void _PhysicsProcess(double delta)
    {
        if (cars.Count < maxCars)
        {
            bool canSpawn = cars.Count == 0 || IsCarVisibleFromCamera(cars[cars.Count - 1]);
            if (canSpawn)
            {
                spawnTimer += (float)delta;
                if (spawnTimer >= spawnInterval)
                {
                    SpawnCar();
                    spawnTimer = 0f;

                    // Randomize spawn interval for next car
                    RandomNumberGenerator rng = new RandomNumberGenerator();
                    rng.Randomize();
                    spawnInterval = rng.RandfRange(3f, 7f);
                }
            }
        }

        float viewportHeight = GetViewport().GetVisibleRect().Size.Y;
        float spawnerY = GlobalPosition.Y;
        float cameraCenterY = GetViewport().GetCamera2D()?.GlobalPosition.Y ?? viewportHeight / 2f;
        float despawnDistance = viewportHeight + 2 * Mathf.Abs(spawnerY - cameraCenterY);

        // Despawn cars if beyond threshold
        for (int i = cars.Count - 1; i >= 0; i--)
        {
            NPCCar car = cars[i];

            if (Mathf.Abs(car.GlobalPosition.Y - cameraCenterY) > despawnDistance)
            {
                RemoveCar(car);
            }
        }
    }

    private void SpawnCar()
    {
        NPCCar newCar = carScene.Instantiate<NPCCar>();
        newCar.GlobalPosition = GlobalPosition;

        // Setup color palette
        RandomNumberGenerator rng = new RandomNumberGenerator();
        rng.Randomize();
        newCar.colorIndex = rng.RandiRange(1, 6); // palette 0 reserved for player car, 7 palette colors available
        newCar.GetNode<PaletteSwap>("Sprite2D").paletteIndex = newCar.colorIndex;
        newCar.carSpawn = this;
        GetTree().CurrentScene.AddChild(newCar); // add to root scene
        cars.Add(newCar);
        newCar._Ready();
    }


    public void RemoveCar(NPCCar car)
    {
        if (cars.Contains(car))
        {
            cars.Remove(car);
        }
    }
    
    private bool IsCarVisibleFromCamera(NPCCar car)
    {
        Camera2D camera = GetViewport().GetCamera2D();
        if (camera == null)
            return true; // fallback: allow spawn if no camera

        Rect2 cameraView = new Rect2(
            camera.GlobalPosition - (GetViewport().GetVisibleRect().Size / 2),
            GetViewport().GetVisibleRect().Size
        );

        return cameraView.HasPoint(car.GlobalPosition);
    }

}
