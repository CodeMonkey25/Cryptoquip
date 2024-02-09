using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Windows.Input;
using Cryptoquip.Extensions;
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

    public MainWindowViewModel() { }
    
    public void LoadPuzzle(string puzzle)
    {
        DecoderRing ring = Locator.Current.GetRequiredService<DecoderRing>();
        ring.Clear();
        
        string[] input = puzzle.ToUpper().Split("<HINT>:");
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