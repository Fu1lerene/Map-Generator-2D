using System.Windows.Media;
using DelaunatorSharp;

namespace MapGenerator2;

public class Map
{
    public IEnumerable<IPoint> Points { get; set; }
    public int NumRegions { get; set; }
    public int NumTriangles { get; set; }
    public int NumEdges { get; set; }
    public int[] Halfedges { get; set; }
    public int[] Triangles { get; set; }
    public IPoint[] Centers { get; set; }
    public List<IVoronoiCell> Cells { get; set; }
    public float[] Noise { get; set; }
    public float[] BiomesNoise { get; set; }
    public Color[] Color { get; set; }
}