using System.Collections.Specialized;
using AipmRegister.Gui.ViewModels;
using Avalonia.Controls;
using Avalonia.Threading;

namespace AipmRegister.Gui.Views;

public partial class RegisteringView : UserControl
{
    private INotifyCollectionChanged? _subscribed;

    public RegisteringView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_subscribed is not null)
        {
            _subscribed.CollectionChanged -= OnLogEntriesChanged;
            _subscribed = null;
        }

        if (DataContext is RegisteringViewModel vm)
        {
            _subscribed = vm.LogEntries;
            _subscribed.CollectionChanged += OnLogEntriesChanged;
        }
    }

    private void OnLogEntriesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action != NotifyCollectionChangedAction.Add) return;
        Dispatcher.UIThread.Post(() => LogScroller.ScrollToEnd(), DispatcherPriority.Background);
    }
}
