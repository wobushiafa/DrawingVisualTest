using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfApp4;

public class VisualHost : Canvas
{
    private readonly List<Visual> _visuals = new();
    protected override Visual GetVisualChild(int index) => _visuals.ElementAt(index);
    protected override int VisualChildrenCount => _visuals.Count;

    public IEnumerable<Visual> ItemsSource
    {
        get => (IEnumerable<Visual>)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable<Visual>), typeof(VisualHost), new PropertyMetadata(OnItemsSourceChangedCallback));

    private static void OnItemsSourceChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not VisualHost host) return;
        host.Clear();
        host.AddVisualRange((IEnumerable<Visual>)e.NewValue);
        host.CreateItemsSourceSubscription();
    }
    public VisualHost()
    {
        this.Loaded += VisualHost_Loaded;
        this.Unloaded += VisualHost_Unloaded;
    }

    private IDisposable? _itemsSubscription;
    private void VisualHost_Loaded(object sender, RoutedEventArgs e)
    {
        CreateItemsSourceSubscription();
    }

    private void VisualHost_Unloaded(object sender, RoutedEventArgs e)
    {
        _itemsSubscription?.Dispose();
    }

    private void CreateItemsSourceSubscription()
    {
        _itemsSubscription?.Dispose();
        if (ItemsSource is not INotifyCollectionChanged notifyCollection) return;
        _itemsSubscription = Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(handler => handler.Invoke,
        handler => notifyCollection.CollectionChanged += handler,
        handler => notifyCollection.CollectionChanged -= handler)
        .Subscribe(e =>
        {
            switch (e.EventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.EventArgs.NewItems?.OfType<Visual>() is { } addRange)
                    {
                        foreach (var item in addRange)
                        {
                            AddVisual(item);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.EventArgs.OldItems?.OfType<Visual>() is not { } oldRange) return;
                {
                    foreach (var item in oldRange)
                    {
                        DeleteVisual(item);
                    }
                }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    this.Clear();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        });
    }


    /// <summary>
    /// 添加可视化对象的方法
    /// </summary>
    /// <param name="visual">需要添加的可视化对象</param>
    public void AddVisual(Visual visual)
    {
        //注意 这里除了添加到集合中 还需要添加到容器的可视化树和逻辑树中
        this._visuals.Add(visual);
        base.AddLogicalChild(visual);
        base.AddVisualChild(visual);
    }

    /// <summary>
    /// 剔除可视化对象
    /// </summary>
    /// <param name="visual">想要剔除的可视化对象</param>
    public void DeleteVisual(Visual visual)
    {
        this._visuals.Remove(visual);
        base.RemoveLogicalChild(visual);
        base.RemoveVisualChild(visual);
    }


    public void Clear()
    {
        // 倒着删除或者这样删除!!!
        var v = _visuals.ToArray();
        foreach (var t in v)
        {
            DeleteVisual(t);
        }
        // var count = _visuals.Count;
        // for (int i = 0; i < count; i++)
        // {
        //     var lst = _visuals[count - i - 1];
        //     DeleteVisual(lst);
        //     Console.WriteLine(count - i - 1);
        // }
    }

    public void AddVisualRange(IEnumerable<Visual> visuals)
    {
        foreach (var visual in visuals)
        {
            AddVisual(visual);
        }
    }
}
