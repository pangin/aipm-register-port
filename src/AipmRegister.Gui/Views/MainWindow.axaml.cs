using Avalonia.Controls;
using AipmRegister.Gui.ViewModels;

namespace AipmRegister.Gui.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(MainViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
