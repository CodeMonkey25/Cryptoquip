using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
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
    private string _logText = string.Empty;
    public string LogText
    {
        get => _logText;
        set => this.RaiseAndSetIfChanged(ref _logText, value);
    }

    public SolverWindowViewModel() {}

    public SolverWindowViewModel(string puzzle, bool enableExclusionAnalysis) : this()
    {
        DecoderRingAbstract ring = Locator.Current.GetRequiredService<DecoderRingAbstract>().Clone();
        WordList wordList = Locator.Current.GetRequiredService<WordList>();
        Solver solver = new Solver();
        Task.Run(() => solver.RunSolver(LogMessage, ring, wordList, puzzle, enableExclusionAnalysis));
    }

    private void LogMessage(string message)
    {
        Dispatcher.UIThread.Post(() => LogText += message + Environment.NewLine);
    }
}