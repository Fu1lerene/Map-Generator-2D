using System.Windows.Media;

namespace MapGenerator2;

public static class Settings
{
    public static readonly int NumberSites = 1000;
    public static readonly int Width = 1800;
    public static readonly int Height = 1000;
    public static readonly int SideSize = 900;
    public static readonly float Scale = 4f;
    public static readonly int CountCentroids = 1;
    public static readonly bool IsBiome = true;
    public static readonly float WaterAndLandLevel = -0.2f; // opt: -0.075f // (-1, 1)

    public static readonly int MountainScale = 4; // (1, 5)
    public static readonly int GrassScale = 1; // чем больше, тем больше травы
    public static readonly int SandScale = 1; // (1, 5)
    
    public static readonly int Step = 50;
    
    public static readonly float SiteSize = 3;
    public static readonly float NodeSize = 3;
    public static readonly Brush SiteColor = Brushes.Red;
    public static readonly Brush NodeColor = Brushes.Blue;

    public static readonly Brush TriangleLineColor = Brushes.Red;
    public static readonly double TriangleLineThickness = 0.4;
    
    public static readonly Brush VoronoiLineColor = Brushes.Green;
    public static readonly double VoronoiLineThickness = 1;
    
    public static readonly double BorderLineThickness = 0.5;
    public static readonly Brush BorderLineColor = Brushes.Black;

    public static readonly Brush WaterColor = Brushes.Blue;
    public static readonly Brush LandColor = Brushes.ForestGreen;
    public static readonly Brush SandColor = Brushes.Goldenrod;
    public static readonly Brush MountainColor = Brushes.DimGray;

    public static readonly float WaterLevel = -0.3f;
    public static readonly float SandLevel = 0f;
    public static readonly float LandLevel = 0.7f;
    public static readonly float MountainLevel = 1f;

}