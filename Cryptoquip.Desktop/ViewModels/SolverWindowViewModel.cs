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

    private readonly List<string> _skipWords = new();
    private DecoderRingAbstract _partialSolution = new DecoderRingNull();
    
    public SolverWindowViewModel() {}

    public SolverWindowViewModel(string puzzle, bool enableExclusionAnalysis) : this()
    {
        DecoderRingAbstract ring = Locator.Current.GetRequiredService<DecoderRingAbstract>().Clone();
        WordList wordList = Locator.Current.GetRequiredService<WordList>();
        Task.Run(() => RunSolver(ring, wordList, puzzle, enableExclusionAnalysis));
    }

    private void LogMessage(string message = "")
    {
        Dispatcher.UIThread.Post(() => LogText += message + Environment.NewLine);
    }

    private void RunSolver(DecoderRingAbstract ring, WordList wordList, string puzzle, bool enableExclusionAnalysis)
    {
        Stopwatch watch = Stopwatch.StartNew();

        LogMessage($"Received puzzle: {puzzle}");
        LogMessage();
        
        Word[] words = Regex.Replace(puzzle, "[^A-Z0-9']", " ")
            .Split(" ", StringSplitOptions.RemoveEmptyEntries)
            .Where(static w => w.Any(char.IsLetter))
            .Where(static w => !w.Any(char.IsNumber))
            .Where(w => !_skipWords.Contains(w))
            .Distinct()
            .Select(static w => new Word(w))
            .ToArray();
        LogMessage($"Found {words.Length} unique words to solve.");
        
        LogMessage("Loading matches...");
        foreach (Word word in words)
        {
            word.Matches = wordList.GetMatches(word, ring);
        }
        words = words.OrderBy(static w => w.Matches.Length).ThenByDescending(static w => w.Text.Length).ToArray();
        
        foreach (Word word in words)
        {
            LogMessage("\t" + word.Text + " (" + word.Matches.Length + ")");
        }
        LogMessage($"Word matches are ready.");
        
        if (enableExclusionAnalysis)
        {
            LogMessage();
            LogMessage("Performing exclusion analysis...");

            int deleted = -1;
            while (deleted != 0)
            {
                deleted = 0;
                foreach (Word word in words)
                {
                    Dictionary<char, HashSet<char>> required = word.GetMatchRequirements();
                    if(required.Count == 0) continue;
                    foreach (Word otherWord in words)
                    {
                        if(word == otherWord) continue;
                        deleted += otherWord.Matches.Length;
                        otherWord.EnsureMatchRequirements(required);
                        deleted -= otherWord.Matches.Length;
                    }
                }
                LogMessage("\tDeleted " + deleted + " words...");
            }
            LogMessage();
			
            words = words.OrderBy(static w => w.Matches.Length).ThenByDescending(static w => w.Text.Length).ToArray();
            foreach (Word word in words)
            {
                LogMessage("\t" + word.Text + " (" + word.Matches.Length + ")");
            }
        }
        
        int startIndex = 0;
        while (startIndex < words.Length && words[startIndex].Matches.Length == 0)
        {
            LogMessage($"The word '{words[startIndex].Text}' is unsolvable - skipping this word");
            startIndex++;
        }
		
        if (!_solveLoop(ring, words, startIndex ))
        {
            LogMessage("Could not find a solution. Printing the best attempt.");
            ring = _partialSolution;
        }
        
        LogMessage();
        LogMessage(ring.Decode(puzzle));
        watch.Stop();
        
        LogMessage();
        LogMessage($"Total run time: {watch.Elapsed.ReadableTime()}");
    }
    
    private bool _solveLoop(DecoderRingAbstract ring, Word[] words, int depth)
    {
        // depth exceeds the length of the array, we must have solved it...
        if (depth >= words.Length) return true;

        if (ring.SolveCount > _partialSolution.SolveCount)
        {
            _partialSolution = ring.Clone();
        }
		
        Word word = words[depth];
        string[] possibleMatches = word.Matches.Where(w => ring.Matches(word.Text, w)).ToArray();
        foreach(string possibleMatch in possibleMatches)
        {
            // add candidate letter matches
            HashSet<char> candidates = [];
            for (int i = 0; i < word.Text.Length; i++)
            {
                char l = word.Text[i];
                if (!char.IsLetter(l)) continue;
                if (ring.Contains(l)) continue;

                char m = possibleMatch[i];
                ring.Put(l, m);
                candidates.Add(l);
            }

            // recurse, returning if the puzzle is solved...
            if (_solveLoop(ring, words, depth + 1))
                return true;

            // remove candidate letter matches
            foreach(char candidate in candidates)
            {
                ring.Remove(candidate);
            }
        }
		
        return false;
    }
}