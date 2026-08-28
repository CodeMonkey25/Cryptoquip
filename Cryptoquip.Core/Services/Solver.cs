using Cryptoquip.Models;
using Cryptoquip.Utility;

namespace Cryptoquip.Services;

public class Solver
{
    private readonly List<string> _skipWords = new();
    private DecoderRingAbstract _partialSolution = new DecoderRingNull();
    
    public void Run(Action<string> logMessage, DecoderRingAbstract ring, WordList? wordList, Puzzle puzzle,
        bool enableExclusionAnalysis)
    {
        logMessage($"Received puzzle: {puzzle}");
        logMessage(string.Empty);
        
        Word[] words = puzzle
            .GetFilteredAndDistinctWords()
            .Where(w => !_skipWords.Contains(w))
            .Select(static w => new Word(w))
            .ToArray();
        logMessage($"Found {words.Length} unique words to solve.");

        if (wordList == null)
        {
            IEqualityComparer<char[]> comparer = new ArrayEqualityComparer<char>();
            logMessage("No word list provided. Loading word list from disk.");
            wordList = new WordList(words.Select(static w => w.Pattern).ToHashSet(comparer));
        }
        
        logMessage("Loading matches...");
        foreach (Word word in words)
        {
            word.Matches = wordList.GetMatches(word, ring);
        }
        words = words.OrderBy(static w => w.Matches.Count).ThenByDescending(static w => w.Text.Length).ToArray();
        
        foreach (Word word in words)
        {
            logMessage("\t" + word.Text + " (" + word.Matches.Count + ")");
        }
        logMessage($"Word matches are ready.");
        
        if (enableExclusionAnalysis)
        {
            logMessage(string.Empty);
            logMessage("Performing exclusion analysis...");

            int deleted = -1;
            while (deleted != 0)
            {
                deleted = 0;
                foreach (Word word in words)
                {
                    MatchRequirements requirements = word.GetMatchRequirements();
                    if (requirements.Count == 0) continue;
                    foreach (Word otherWord in words)
                    {
                        if(word == otherWord) continue;
                        deleted += otherWord.Matches.Count;
                        otherWord.EnsureMatchRequirements(requirements);
                        deleted -= otherWord.Matches.Count;
                    }
                }
                logMessage("\tDeleted " + deleted + " words...");
            }
            logMessage(string.Empty);
			
            words = words.OrderBy(static w => w.Matches.Count).ThenByDescending(static w => w.Text.Length).ToArray();
            foreach (Word word in words)
            {
                logMessage("\t" + word.Text + " (" + word.Matches.Count + ")");
            }
        }
        
        int startIndex = 0;
        while (startIndex < words.Length && words[startIndex].Matches.Count == 0)
        {
            logMessage($"The word '{words[startIndex].Text}' is unsolvable - skipping this word");
            startIndex++;
        }
		
        if (!_solveLoop(ring, words, startIndex ))
        {
            logMessage("Could not find a solution. Printing the best attempt.");
            ring = _partialSolution;
        }
        
        logMessage(string.Empty);
        logMessage(ring.Decode(puzzle.Text));
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
        bool[] candidates = new bool[26];
        foreach(string possibleMatch in possibleMatches)
        {
            // add candidate letter matches
            for (int i = 0; i < word.Text.Length; i++)
            {
                char l = word.Text[i];
                if (!char.IsLetter(l)) continue;
                if (ring.Contains(l)) continue;

                char m = possibleMatch[i];
                ring.Put(l, m);
                candidates[l - 'A'] = true;
            }

            // recurse, returning if the puzzle is solved...
            if (_solveLoop(ring, words, depth + 1))
                return true;

            // remove candidate letter matches
            for (int i = 0; i < candidates.Length; i++)
            {
                if (candidates[i])
                {
                    ring.Remove((char)('A' + i));
                    candidates[i] = false;
                }
            }
        }
		
        return false;
    }
}