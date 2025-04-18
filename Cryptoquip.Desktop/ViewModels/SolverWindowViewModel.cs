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
    private string Puzzle { get; } = string.Empty;
    private bool EnableExclusionAnalysis { get; }
    private DecoderRingAbstract Ring { get; } = new DecoderRingNull();
    private WordList? Words { get; }
    
    private string _logText = string.Empty;
    public string LogText
    {
        get => _logText;
        set => this.RaiseAndSetIfChanged(ref _logText, value);
    }

    public SolverWindowViewModel() {}

    public SolverWindowViewModel(string puzzle, bool enableExclusionAnalysis) : this()
    {
        Puzzle = puzzle;
        EnableExclusionAnalysis = enableExclusionAnalysis;
        Ring = Locator.Current.GetRequiredService<DecoderRingAbstract>().Clone();
        Words = Locator.Current.GetRequiredService<WordList>();
        Task.Run(RunSolver);
    }

    private void RunSolver()
    {
        Stopwatch watch = Stopwatch.StartNew();
        Solver solver = new();
        solver.Run(LogMessage, Ring, Words!, Puzzle, EnableExclusionAnalysis);
        watch.Stop();
        
        LogMessage(string.Empty);
        LogMessage($"Total run time: {watch.Elapsed.ReadableTime()}");
    }
    
    private void LogMessage(string message)
    {
        Dispatcher.UIThread.Post(() => LogText += message + Environment.NewLine);
    }
}