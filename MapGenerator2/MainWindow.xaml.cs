using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DelaunatorSharp;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using Point = DelaunatorSharp.Point;


namespace MapGenerator2;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private int _numberSites = Settings.NumberSites;
    private float _scale = Settings.Scale;
    private float _waterAndLandLevel = Settings.WaterAndLandLevel;
    private long _seed = Random.Shared.NextInt64();
    private long _seedBiomes = Random.Shared.NextInt64();
    private Delaunator _delaunator;
    private List<IPoint> _sites;
    private Map _map;
    
    public MainWindow()
    {
        InitializeComponent();

        GenerateMap();
    }

    private void GenerateMap()
    {
        CreateSites();
        _delaunator = new Delaunator(_sites.ToArray());
        
        _map = new Map
        {
            Points = _sites,
            NumRegions = _sites.Count,
            NumTriangles = _delaunator.Halfedges.Length / 3,
            NumEdges = _delaunator.Halfedges.Length,
            Halfedges = _delaunator.Halfedges,
            Triangles = _delaunator.Triangles,
            Cells = _delaunator.GetVoronoiCellsBasedOnCentroids().ToList()
        };
        
        CalculateCentroids();
        AssignElevation();
        
        DrawVoronoiCells();
        Seed.Text = _seed.ToString();
    }

    private void SetCellType(IVoronoiCell cell, Polygon borderOfCell, bool isBiome = true)
    {
        isBiome = Settings.IsBiome;
        if (isBiome)
        {
            var color = new SolidColorBrush(_map.Color[cell.Index]);
            
            borderOfCell.Fill = color;
            borderOfCell.Stroke = color;
            return;
        }
        
        if (_map.Noise[cell.Index] < 0.15)
        {
            borderOfCell.Fill = new SolidColorBrush(Color.FromRgb(24, 32, 63));
            borderOfCell.Stroke = new SolidColorBrush(Color.FromRgb(24, 32, 63));
        }
        else if (_map.Noise[cell.Index] < 0.35)
        {
            borderOfCell.Fill = new SolidColorBrush(Color.FromRgb(48, 64, 127));
            borderOfCell.Stroke = new SolidColorBrush(Color.FromRgb(48, 64, 127));
        }
        else if (_map.Noise[cell.Index] < 0.5)
        {
            borderOfCell.Fill = new SolidColorBrush(Color.FromRgb(180, 210, 120));
            borderOfCell.Stroke = new SolidColorBrush(Color.FromRgb(180, 210, 120));
        }
        else if (_map.Noise[cell.Index] < 0.8)
        {
            borderOfCell.Fill = new SolidColorBrush(Color.FromRgb(200, 185, 130));
            borderOfCell.Stroke = new SolidColorBrush(Color.FromRgb(200, 185, 130));
        }
        else
        {
            borderOfCell.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            borderOfCell.Stroke = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        }
    }
    
    private void AssignElevation()
    {
        _map.Noise = new float[_map.Cells.Count];
        _map.BiomesNoise = new float[_map.Cells.Count];
        _map.Color = new Color[_map.Cells.Count];
        
        _delaunator.ForEachVoronoiCellBasedOnCentroids(cell =>
        {
            var nx = (float)(_map.Centers[cell.Index].X / Settings.SideSize * _scale);
            var ny = (float)(_map.Centers[cell.Index].Y / Settings.SideSize * _scale);
            _map.Noise[cell.Index] = (1 + OpenSimplex2S.Noise2_ImproveX(_seed, nx, ny)) / 2f;
            _map.BiomesNoise[cell.Index] = (1 + OpenSimplex2S.Noise2_ImproveX(_seedBiomes, nx, ny)) / 2f;
        });
        Biomes();
    }

    private void Biomes()
    {
        _delaunator.ForEachVoronoiCellBasedOnCentroids(cell =>
        {
            var e = (_map.Noise[cell.Index] - 0.5f) * 2;
            var m = _map.BiomesNoise[cell.Index];
            float r, g, b;
            var mountainScale = Settings.MountainScale;
            var grassScale = Settings.GrassScale;
            var sandScale = Settings.SandScale;
            
            if (e < _waterAndLandLevel)
            {
                r = 48 + 48 * e;
                g = 64 + 64 * e;
                b = 127 + 127 * e;
            }
            else 
            {
                m *= (float)Math.Pow(1-e/grassScale, sandScale); 
                e = (float)Math.Pow(e, mountainScale);
                r = 200 - 100 * m;
                g = 185 - 45 * m;
                b = 130 - 45 * m; 
                r = 255 * e + r * (1-e);
                g = 255 * e + g * (1-e);
                b = 255 * e + b * (1-e);
                
            }
            var rb = Convert.ToByte(r);
            var gb = Convert.ToByte(g);
            var bb = Convert.ToByte(b);
            _map.Color[cell.Index] = Color.FromRgb(rb, gb, bb);
        });
    }
    
    private void CalculateCentroids(int count = 1)
    {
        count = Settings.CountCentroids;
        for (int i = 0; i < count; i++)
        {
            CalculateCentroid();
            _delaunator = new Delaunator(_map.Centers.ToArray());
        }
    }

    private void CalculateCentroid()
    {
        _map.Centers = new IPoint[_sites.Count];
        
        _delaunator.ForEachVoronoiCellBasedOnCentroids(cell =>
        {
            (double x, double y) sum = (0, 0);
            foreach (var point in cell.Points)
            {
                sum.x += point.X;
                sum.y += point.Y;
            }
            _map.Centers[cell.Index] = new Point
            {
                X = sum.x / cell.Points.Length,
                Y = sum.y / cell.Points.Length
            };
        });
    }
    
    private void RefreshDelaunator()
    {
        if (_sites.Count == _delaunator.Points.Length)
            return;
        _delaunator = new Delaunator(_sites.ToArray());
    }
    
    private void CreateSites()
    {
        _sites = new();
        var step = Settings.SideSize / (int)Math.Sqrt(_numberSites);
        for (int x = -50; x <= step * Math.Sqrt(_numberSites); x += step)
        {
            for (int y = -50; y <= step * Math.Sqrt(_numberSites); y += step)
            {
                _sites.Add(new Point
                {
                    X = x + Random.Shared.Next(step / 2) - Random.Shared.Next(step / 2),
                    Y = y + Random.Shared.Next(step / 2) - Random.Shared.Next(step / 2)
                });
            }
        }
    }

    private void DrawDiagram(bool isColorful = true)
    {
        if (!isColorful)
        {
            Field.Children.Clear();
        }
        
        DrawDelaunay();
        DrawCentroids();
        DrawCentroidsForVoronoiCells();
        DrawVoronoiBorders();
        DrawBorder();
    }

    private void DrawCentroids()
    {
        _delaunator.ForEachVoronoiCellBasedOnCentroids(cell =>
        {
            foreach (var point in cell.Points)
            {
                var node = new Ellipse
                {
                    Width = Settings.NodeSize,
                    Height = Settings.NodeSize,
                    Fill = Settings.NodeColor,
                };
                
                Field.Children.Add(node);
                Canvas.SetLeft(node, point.X - node.Width / 2);
                Canvas.SetTop(node, point.Y - node.Height / 2);
            }
        });
    }
    private void DrawDelaunay()
    {
        RefreshDelaunator();
        _delaunator.ForEachTriangleEdge(edge =>
        {
            DrawLine(edge.P, edge.Q, Settings.TriangleLineColor, Settings.TriangleLineThickness);
        });
    }
    private void DrawVoronoiCells()
    {
        RefreshDelaunator();
        _delaunator.ForEachVoronoiCellBasedOnCentroids(cell =>
        {
            var borderOfCell = new Polygon
            {
                Stroke = Settings.VoronoiLineColor,
                StrokeThickness = Settings.VoronoiLineThickness,
                Points = new PointCollection(cell.Points.Select(point => new System.Windows.Point(point.X, point.Y)))
            };
            
            SetCellType(cell, borderOfCell);
            
            Field.Children.Add(borderOfCell);
        });
        
        var emptySquare = new Rectangle
        {
            Width = 600,
            Height = 900,
            HorizontalAlignment = HorizontalAlignment.Right,
            Stroke = Brushes.White,
            StrokeThickness = 30
            
        };
        SettingsField.Children.Add(emptySquare);
    }
    
    private void DrawCentroidsForVoronoiCells()
    {
        foreach (var centroid in _map.Centers)
        {
            var ellipse = new Ellipse
            {
                Width = Settings.SiteSize,
                Height = Settings.SiteSize,
                Fill = Settings.SiteColor
            };
            
            Field.Children.Add(ellipse);
            Canvas.SetLeft(ellipse, centroid.X - ellipse.Width / 2);
            Canvas.SetTop(ellipse, centroid.Y - ellipse.Height / 2);
        }
    }
    private void DrawVoronoiBorders()
    {
        RefreshDelaunator();
        _delaunator.ForEachVoronoiCellBasedOnCentroids(cell =>
        {
            var borderOfCell = new Polygon
            {
                Stroke = Settings.VoronoiLineColor,
                StrokeThickness = Settings.VoronoiLineThickness,
                Points = new PointCollection(cell.Points.Select(point => new System.Windows.Point(point.X, point.Y)))
            };
            
            Field.Children.Add(borderOfCell);
        });
    }
    private void DrawBorder()
    {
        RefreshDelaunator();
        foreach (var edge in _delaunator.GetHullEdges())
        {
            DrawLine(edge.P, edge.Q, Settings.BorderLineColor, Settings.BorderLineThickness);
        }
    }
    private void DrawSites()
    {
        foreach (var site in _sites)
        {
            var ellipse = new Ellipse
            {
                Width = Settings.SiteSize,
                Height = Settings.SiteSize,
                Fill = Settings.SiteColor
            };
            
            Field.Children.Add(ellipse);
            Canvas.SetLeft(ellipse, site.X - ellipse.Width / 2);
            Canvas.SetTop(ellipse, site.Y - ellipse.Height / 2);
        }
    }
    private void DrawLine(IPoint startPoint, IPoint endPoint, Brush stroke, double lineThickness)
    {
        var line = new Line
        {
            X1 = startPoint.X,
            Y1 = startPoint.Y,
            X2 = endPoint.X,
            Y2 = endPoint.Y,
            Stroke = stroke,
            StrokeThickness = lineThickness
        };
        
        Field.Children.Add(line);
    }

    private void GenerateButton_OnClick(object sender, RoutedEventArgs e)
    {
        var inputSeed = Convert.ToInt64(Seed.Text);
        _seed = inputSeed;
        
        Field.Children.Clear();
        GenerateMap();
        
        _seed = Random.Shared.NextInt64();
        _seedBiomes = Random.Shared.NextInt64();
        Seed.Text = _seed.ToString();
    }

    private void NumberSites_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        _numberSites = (int)NumberSites.Value;
        NumberSitesLabel.Content = "Количество ячеек: " + _numberSites;
    }

    private void Scale_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        _scale = (int)Scale.Value;
        ScaleLabel.Content = "Масштаб: " + _scale + "x";
    }

    private void WaterLevel_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        _waterAndLandLevel = (float)WaterLevel.Value;
        var percent = (_waterAndLandLevel + 1) / 2f * 100;
        WaterLevelLabel.Content = "Количество воды: " + Math.Round(percent) + "%";
    }
}