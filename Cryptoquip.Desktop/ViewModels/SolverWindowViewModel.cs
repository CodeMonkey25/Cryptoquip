using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Threading;
using Cryptoquip.Extensions;
using Cryptoquip.Models;
using Cryptoquip.Services;
using ReactiveUI;
using Splat;

namespace Cryptoquip.ViewModels;

public class SolverWindowViewModel : ViewModelBase
{
    private Puzzle? Puzzle { get; }
    private bool EnableExclusionAnalysis { get; }
    private DecoderRingAbstract Ring { get; } = new DecoderRingNull();
    private WordList? WordList { get; }
    
    private string _logText = string.Empty;
    public string LogText
    {
        get => _logText;
        set => this.RaiseAndSetIfChanged(ref _logText, value);
    }

    public SolverWindowViewModel() {}

    public SolverWindowViewModel(Puzzle puzzle, bool enableExclusionAnalysis) : this()
    {
        Puzzle = puzzle;
        EnableExclusionAnalysis = enableExclusionAnalysis;
        Ring = Locator.Current.GetRequiredService<DecoderRingAbstract>().Clone();
        WordList = Locator.Current.GetRequiredService<WordList>();
        Task.Run(RunSolver);
    }

    private void RunSolver()
    {
        Stopwatch watch = Stopwatch.StartNew();
        Solver solver = new();
        solver.Run(LogMessage, Ring, WordList!, Puzzle!, EnableExclusionAnalysis);
        watch.Stop();
        
        LogMessage(string.Empty);
        LogMessage($"Total run time: {watch.Elapsed.ReadableTime()}");
    }
    
    private void LogMessage(string message)
    {
        Dispatcher.UIThread.Post(() => LogText += message + Environment.NewLine);
    }
}