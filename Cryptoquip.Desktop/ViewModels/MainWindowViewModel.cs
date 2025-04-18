using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Cryptoquip.Extensions;
using Cryptoquip.Models;
using Cryptoquip.Services;
using ReactiveUI;
using Splat;

namespace Cryptoquip.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ObservableCollection<WordViewModel> _words = [];
    public ObservableCollection<WordViewModel> Words
    {
        get => _words;
        set => this.RaiseAndSetIfChanged(ref _words, value);
    }

    public Puzzle? Puzzle { get; private set; }

    public void LoadPuzzle(string text)
    {
        DecoderRingAbstract ring = Locator.Current.GetRequiredService<DecoderRingAbstract>();
        
        Puzzle = new(text, ring);
        
        Words.Clear();
        Dictionary<char, LetterViewModel> letterMap = new();
        foreach (string word in Puzzle.GetWords())
        {
            Words.Add(new WordViewModel(word, letterMap));
        }
    }
}