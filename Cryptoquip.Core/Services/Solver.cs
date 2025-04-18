using System.Text.RegularExpressions;
using Cryptoquip.Models;

namespace Cryptoquip.Services;

public class Solver
{
    private readonly List<string> _skipWords = new();
    private DecoderRingAbstract _partialSolution = new DecoderRingNull();
    
    public void Run(Action<string> logMessage, DecoderRingAbstract ring, WordList wordList, string puzzle,
        bool enableExclusionAnalysis)
    {
        logMessage($"Received puzzle: {puzzle}");
        logMessage(string.Empty);
        
        Word[] words = Regex.Replace(puzzle, "[^A-Z0-9']", " ")
            .Split(" ", StringSplitOptions.RemoveEmptyEntries)
            .Where(static w => w.Any(char.IsLetter))
            .Where(static w => !w.Any(char.IsNumber))
            .Where(w => !_skipWords.Contains(w))
            .Distinct()
            .Select(static w => new Word(w))
            .ToArray();
        logMessage($"Found {words.Length} unique words to solve.");
        
        logMessage("Loading matches...");
        foreach (Word word in words)
        {
            word.Matches = wordList.GetMatches(word, ring);
        }
        words = words.OrderBy(static w => w.Matches.Length).ThenByDescending(static w => w.Text.Length).ToArray();
        
        foreach (Word word in words)
        {
            logMessage("\t" + word.Text + " (" + word.Matches.Length + ")");
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
                logMessage("\tDeleted " + deleted + " words...");
            }
            logMessage(string.Empty);
			
            words = words.OrderBy(static w => w.Matches.Length).ThenByDescending(static w => w.Text.Length).ToArray();
            foreach (Word word in words)
            {
                logMessage("\t" + word.Text + " (" + word.Matches.Length + ")");
            }
        }
        
        int startIndex = 0;
        while (startIndex < words.Length && words[startIndex].Matches.Length == 0)
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
        logMessage(ring.Decode(puzzle));
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