using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace WpfApp4;

public class StrokeElement : DrawingVisual
{
	
	#region Properties
    public ICollection<Point> Points
    {
        get => (ICollection<Point>)GetValue(MyPropertyProperty);
        set => SetValue(MyPropertyProperty, value);
    }
    
    public Brush Stroke
    {
	    get => (Brush)GetValue(StrokeProperty);
	    set => SetValue(StrokeProperty, value);
    }
    
    public double StrokeThickness
    {
	    get => (double)GetValue(StrokeThicknessProperty);
	    set => SetValue(StrokeThicknessProperty, value);
    }
    #endregion
    
    #region DependencyProperties
    public static readonly DependencyProperty MyPropertyProperty =
        DependencyProperty.Register(nameof(Points), typeof(IEnumerable<Point>), typeof(StrokeElement), new PropertyMetadata(OnPointsChangedCallback));

    public static readonly DependencyProperty StrokeProperty =
        DependencyProperty.Register(nameof(Stroke), typeof(Brush), typeof(StrokeElement), new PropertyMetadata(Brushes.Black,OnPenChangedCallback));

    public static readonly DependencyProperty StrokeThicknessProperty =
        DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(StrokeElement), new PropertyMetadata(1d, OnPenChangedCallback));

    #endregion
    
    #region DependencyProperty Callbacks
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
    #endregion
    
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
    
    private void ReDraw()
    {
	    if (!(Points?.Count > 2)) return;
	    StreamGeometry? geometry = new();
	    using var streamctx = geometry.Open();
	    streamctx.BeginFigure(Points.First(), false, false);
	    for (var i = 0; i < Points.Count; i++)
		    if (i == 0)
			    streamctx.BeginFigure(new Point(Points.ElementAt(0).X, Points.ElementAt(0).Y), false, false);
		    else
		    {
			    var lstPoint = new Point(Points.ElementAt(i - 1).X, Points.ElementAt(i - 1).Y);
			    var point = new Point(Points.ElementAt(i).X, Points.ElementAt(i).Y);
			    DrawBezier(streamctx, lstPoint, point);
		    }
	    streamctx.Close();
	    
	    geometry.Freeze();
	    
	    using var ctx = RenderOpen();
	    ctx.DrawGeometry(null, _pen, geometry);
	    ctx.Close();
	    geometry = null;
    }

	private static void DrawBezier(StreamGeometryContext ctx, Point lastPoint, Point curPoint)
	{
		var offsetX = curPoint.X - lastPoint.X;
		var offsetY = curPoint.Y - lastPoint.Y;
		var fin = Math.Sqrt(Math.Pow(offsetX, 2.0) + Math.Pow(offsetY, 2.0));
		switch (fin)
		{
			case >= 2:
				{
					var ctrl = new Point(lastPoint.X + offsetX / 3.0, lastPoint.Y + offsetY / 3.0);
					var end = new Point(lastPoint.X + offsetX / 2.0, lastPoint.Y + offsetY / 2.0);
					ctx.BezierTo(lastPoint, ctrl, end, true, true);
					break;
				}
			case >= 1:
				{
					var ctrl = new Point(lastPoint.X + offsetX / 2.0, lastPoint.Y + offsetY / 2.0);
					ctx.QuadraticBezierTo(lastPoint, ctrl, true, true);
					break;
				}
		}
	}

	public void AddPoint(Point point)
    {
        Points ??= new List<Point>();
        Points.Add(point);
        ReDraw();
	}

}
