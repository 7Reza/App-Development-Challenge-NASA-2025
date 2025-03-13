using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using HelixToolkit.Wpf;
using System.Windows; // Required for WPF applications

public class PositionData
{
    public double Time { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
}

public class CsvReader
{
    public static List<PositionData> ReadCsv(string filePath)
    {
        var positions = new List<PositionData>();

        if (!File.Exists(filePath))
        {
            Console.WriteLine("Error: File not found!");
            return positions;
        }

        foreach (var line in File.ReadLines(filePath).Skip(1)) // Skip header
        {
            var parts = line.Split(',');

            if (parts.Length < 4) continue;

            if (double.TryParse(parts[0].Trim(), out double time) &&
                double.TryParse(parts[1].Trim(), out double x) &&
                double.TryParse(parts[2].Trim(), out double y) &&
                double.TryParse(parts[3].Trim(), out double z))
            {
                positions.Add(new PositionData
                {
                    Time = time,
                    X = x,
                    Y = y,
                    Z = z
                });
            }
            else
            {
                Console.WriteLine($"Warning: Skipping invalid row -> {line}");
            }
        }

        return positions;
    }
}

public class Simulation : Application
{
    private HelixViewport3D helixViewport3D;
    private SphereVisual3D earth;
    private SphereVisual3D moon;
    private DispatcherTimer timer;
    private double currentTime = 0;
    private List<PositionData> positionData;

    public Simulation()
    {
        InitializeScene();
        StartAnimation();
    }

    private void InitializeScene()
    {
        helixViewport3D = new HelixViewport3D();

        // Create Earth and Moon spheres
        earth = new SphereVisual3D
        {
            Radius = 6371, // Earth Radius in km
            Fill = System.Windows.Media.Brushes.Blue
        };

        moon = new SphereVisual3D
        {
            Radius = 1737, // Moon Radius in km
            Fill = System.Windows.Media.Brushes.Gray
        };

        // Add them to the viewport
        helixViewport3D.Children.Add(earth);
        helixViewport3D.Children.Add(moon);

        // Load CSV data
        string filePath = @"Enter the csv file path";
        positionData = CsvReader.ReadCsv(filePath);
    }

    private void StartAnimation()
    {
        timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromMilliseconds(100);
        timer.Tick += (s, e) =>
        {
            currentTime += 1;

            var currentPos = positionData.FirstOrDefault(p => p.Time == currentTime);
            if (currentPos != null)
            {
                // Move the rocket (assuming data represents a rocket's position)
                earth.Transform = new TranslateTransform3D(currentPos.X, currentPos.Y, currentPos.Z);
            }
        };
    }
}


