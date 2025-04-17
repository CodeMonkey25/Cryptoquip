using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Cryptoquip.Extensions;
using Cryptoquip.Services;
using Cryptoquip.Utility;
using ReactiveUI;
using Splat;

namespace Cryptoquip.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ObservableCollection<WordViewModel> _words = new ObservableCollection<WordViewModel>();
    public ObservableCollection<WordViewModel> Words
    {
        get => _words;
        set => this.RaiseAndSetIfChanged(ref _words, value);
    }

    public string OriginalMessage { get; private set; } = string.Empty;
    
    public MainWindowViewModel() { }
    
    public void LoadPuzzle(string puzzle)
    {
        DecoderRingAbstract ring = Locator.Current.GetRequiredService<DecoderRingAbstract>();
        ring.Clear();
        
        string[] input = puzzle.ToUpper().Split("<HINT>:");
        OriginalMessage = input.First();
        if (input.Length >= 2)
        {
            ring.LoadHints(input[1]);
        }
        
        Words.Clear();
        Dictionary<char, LetterViewModel> letterMap = new();
        foreach (string word in input.First().Split(" ", StringSplitOptions.RemoveEmptyEntries))
        {
            Words.Add(new WordViewModel(word, letterMap));
        }
    }
}