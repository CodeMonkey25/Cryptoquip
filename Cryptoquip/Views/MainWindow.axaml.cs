using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Cryptoquip.Utility;
using Cryptoquip.ViewModels;
using ReactiveUI;

namespace Cryptoquip.Views;

public partial class MainWindow : Window
{
    public static readonly DirectProperty<MainWindow, ICommand> LoadCommandProperty = AvaloniaProperty.RegisterDirect<MainWindow, ICommand>(nameof(LoadPuzzleCommand), o => o.LoadPuzzleCommand, (o, v) => o.LoadPuzzleCommand = v);
    private ICommand _loadPuzzleCommand = NullCommand.Instance;
    public ICommand LoadPuzzleCommand
    {
        get => _loadPuzzleCommand;
        set => SetAndRaise(LoadCommandProperty, ref _loadPuzzleCommand, value);
    }
    
    public static readonly DirectProperty<MainWindow, ICommand> ExitCommandProperty = AvaloniaProperty.RegisterDirect<MainWindow, ICommand>(nameof(ExitCommand), o => o.ExitCommand, (o, v) => o.ExitCommand = v);
    private ICommand _exitCommand = NullCommand.Instance;
    public ICommand ExitCommand
    {
        get => _exitCommand;
        set => SetAndRaise(ExitCommandProperty, ref _exitCommand, value);
    }
    
    public MainWindow()
    {
        InitializeComponent();
        LoadPuzzleCommand = ReactiveCommand.Create(LoadPuzzle);
        ExitCommand = ReactiveCommand.Create(Exit);
    }

    public async Task LoadPuzzle()
    {
        if (!(DataContext is MainWindowViewModel vm)) return;

        LoadPuzzleWindow dialog = new LoadPuzzleWindow();
        string? result = await dialog.ShowDialog<string?>(this);
        if (string.IsNullOrWhiteSpace(result)) return;
        
        vm.LoadPuzzle(result);
    }

    private void Exit()
    {
        Close();
    }
}