using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace WpfApp4;

public class StrokeElement : DrawingVisual
{
    public ICollection<Point> Points
    {
        get { return (ICollection<Point>)GetValue(MyPropertyProperty); }
        set { SetValue(MyPropertyProperty, value); }
    }
    public static readonly DependencyProperty MyPropertyProperty =
        DependencyProperty.Register(nameof(Points), typeof(IEnumerable<Point>), typeof(StrokeElement), new PropertyMetadata(OnPointsChangedCallback));

    public Brush Stroke
    {
        get { return (Brush)GetValue(StrokeProperty); }
        set { SetValue(StrokeProperty, value); }
    }
    public static readonly DependencyProperty StrokeProperty =
        DependencyProperty.Register(nameof(Stroke), typeof(Brush), typeof(StrokeElement), new PropertyMetadata(Brushes.Black,OnPenChangedCallback));


    public double StrokeThickness
    {
        get { return (double)GetValue(StrokeThicknessProperty); }
        set { SetValue(StrokeThicknessProperty, value); }
    }
    public static readonly DependencyProperty StrokeThicknessProperty =
        DependencyProperty.Register("StrokeThickness", typeof(double), typeof(StrokeElement), new PropertyMetadata(1d, OnPenChangedCallback));

    private static void OnPointsChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StrokeElement strokeElement) return;
        strokeElement.ReDraw();
    }

    private static void OnPenChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StrokeElement strokeElement) return;
        strokeElement.CreatePen();
        strokeElement.ReDraw();
    }

    private Pen _pen;

    public StrokeElement()
    {
        _pen = new Pen(Brushes.Black, 1)
        {
            LineJoin = PenLineJoin.Round,
            EndLineCap = PenLineCap.Round,
            StartLineCap = PenLineCap.Round,
        };
        _pen.Freeze();
    }

    private void CreatePen()
    {
        _pen = new Pen(Stroke, StrokeThickness)
        {
            LineJoin = PenLineJoin.Round,
            EndLineCap = PenLineCap.Round,
            StartLineCap = PenLineCap.Round,
        };
        _pen.Freeze();
    }

    private readonly StreamGeometry _geometry = new();

    private void ReDraw()
    {
        if (Points?.Count > 2)
        {
            using var ctx = RenderOpen();
            _geometry.Clear();
            using var streamctx = _geometry.Open();
            streamctx.BeginFigure(Points.First(), false, false);
            foreach (var point in Points.Skip(1))
            {
                streamctx.LineTo(point, true, true);
            }
            ctx.DrawGeometry(null, _pen, _geometry);
        }
    } 


    public void AddPoint(Point point)
    {
        if (Points is null)
            Points = new List<Point>();
        Points.Add(point);
        ReDraw();
    }
}
