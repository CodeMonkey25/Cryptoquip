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
    
    public static readonly DirectProperty<MainWindow, ICommand> RunSolverProperty = AvaloniaProperty.RegisterDirect<MainWindow, ICommand>(nameof(RunSolverCommand), o => o.RunSolverCommand, (o, v) => o.RunSolverCommand = v);
    private ICommand _runSolverCommand = NullCommand.Instance;
    public ICommand RunSolverCommand
    {
        get => _runSolverCommand;
        set => SetAndRaise(RunSolverProperty, ref _runSolverCommand, value);
    }
    
    public static readonly DirectProperty<MainWindow, bool> EnableExclusionAnalysisProperty = AvaloniaProperty.RegisterDirect<MainWindow, bool>(nameof(EnableExclusionAnalysis), o => o.EnableExclusionAnalysis, (o, v) => o.EnableExclusionAnalysis = v);
    private bool _enableExclusionAnalysis = false;
    public bool EnableExclusionAnalysis
    {
        get => _enableExclusionAnalysis;
        set => SetAndRaise(EnableExclusionAnalysisProperty, ref _enableExclusionAnalysis, value);
    }
    
    public MainWindow()
    {
        InitializeComponent();
        LoadPuzzleCommand = ReactiveCommand.Create(LoadPuzzle);
        ExitCommand = ReactiveCommand.Create(Exit);
        RunSolverCommand = ReactiveCommand.Create(RunSolver);
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

    private async Task RunSolver()
    {
        if (!(DataContext is MainWindowViewModel mainWindowViewModel)) return;

        SolverWindowViewModel solverWindowViewModel = new(mainWindowViewModel.OriginalMessage, EnableExclusionAnalysis);
        SolverWindow dialog = new SolverWindow() { DataContext = solverWindowViewModel};
        string? result = await dialog.ShowDialog<string?>(this);
        if (string.IsNullOrWhiteSpace(result)) return;
        
        // vm.LoadPuzzle(result);
    }
}