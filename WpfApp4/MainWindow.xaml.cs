using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;

namespace WpfApp4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private MainViewModel _mainViewModel;
        public MainWindow()
        {
            InitializeComponent();

            _mainViewModel = new();
            this.DataContext = _mainViewModel;
            this.Loaded += (_, _) =>
            {
                //Random random = new Random();
                //for (int i = 0; i < 1000; i++)
                //{
                //    var stroke = new StrokeElement();
                //    var points = new List<Point>();
                //    for (int j = 0; j < 100; j++)
                //    {
                //        points.Add(new Point(random.Next(0, 1920), random.Next(0, 1080)));
                //    }
                //    stroke.Points = points;
                //    _mainViewModel.Strokes.Add(stroke);
                //}
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var collection = _mainViewModel.Strokes.Skip(50);
            for (int i = 0;i < collection.Count(); i++)
            {
                _mainViewModel.Strokes.Remove(collection.ElementAt(i));
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var collection = _mainViewModel.Strokes.Skip(50);
            for (int i = 0; i < collection.Count(); i++)
            {
                collection.ElementAt(i).Stroke = Brushes.White;
            }
        }

        private StrokeElement stroke;
        private void VisualHost_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            stroke = new StrokeElement();
            stroke.StrokeThickness = 2;
            stroke.Points = new ObservableCollection<Point>();
            var p = e.GetPosition(this);
            stroke.AddPoint(p);
            _mainViewModel.Strokes.Add(stroke);
        }

        private void VisualHost_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton != System.Windows.Input.MouseButtonState.Pressed) return;
            if (stroke is null) return;
            stroke.AddPoint(e.GetPosition(this));
        }
    }
}
