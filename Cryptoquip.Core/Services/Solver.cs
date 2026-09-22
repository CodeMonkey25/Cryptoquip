using Cryptoquip.Models;

namespace Cryptoquip.Services;

public class Solver
{
    // private readonly List<string> _skipWords = new();
    private DecoderRing _partialSolution = new DecoderRingNull();
    
    public void Run(Action<string> logMessage, DecoderRing ring, WordList? wordList, Puzzle puzzle,
        bool enableExclusionAnalysis)
    {
        _partialSolution = ring.Clone();
        
        logMessage($"Received puzzle: {puzzle}");
        logMessage(string.Empty);
        
        Word[] words = puzzle
            .GetFilteredAndDistinctWords()
            // .Where(w => !_skipWords.Contains(w))
            .Select(static w => new Word(w))
            .ToArray();
        logMessage($"Found {words.Length} unique words to solve.");

        if (wordList == null)
        {
            logMessage("No word list provided. Loading word list from disk.");
            wordList = new WordList(words.Select(static w => w.Pattern).ToHashSet());
        }
        
        logMessage("Loading matches...");
        foreach (Word word in words)
        {
            word.Matches = wordList.GetMatches(word, ring);
        }
        
        if (!enableExclusionAnalysis)
        {
            words.Sort();
        }
        
        foreach (Word word in words)
        {
            logMessage("\t" + word.Text + " (" + word.Matches.Count + ")");
        }
        logMessage("Word matches are ready.");
        
        if (enableExclusionAnalysis)
        {
            logMessage(string.Empty);
            logMessage("Performing exclusion analysis...");

            ExclusionAnalysis exclusionAnalysis = new();
            int deleted = exclusionAnalysis.Run(words);
            logMessage("Deleted " + deleted + " words...");

            words.Sort();
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
        
        while (startIndex < words.Length && words[startIndex].Matches.Count == 1)
        {
            ring.Put(words[startIndex].Text, words[startIndex].Matches[0]);
            startIndex++;
        }

        if (!SolveRecursively(ring, words.AsSpan(startIndex)))
        {
            logMessage("Could not find a solution. Printing the best attempt.");
            ring.Overwrite(_partialSolution);
        }

        logMessage(string.Empty);
        logMessage(ring.Decode(puzzle.Text));
    }
    
    private bool SolveRecursively(DecoderRing ring, Span<Word> words)
    {
        // if words is empty, we must have solved it...
        if (words.IsEmpty) return true;

        if (ring.SolveCount > _partialSolution.SolveCount)
        {
            _partialSolution.Overwrite(ring);
        }
		
        Word word = words[0];
        Span<char> candidates = stackalloc char[word.Text.Length];
        int candidateCount = 0;
        foreach(string possibleMatch in word.Matches)
        {
            if (!ring.Matches(word.Text, possibleMatch)) continue;
            
            // add candidate letter matches
            for (int i = 0; i < word.Text.Length; i++)
            {
                char l = word.Text[i];
                if (!char.IsAsciiLetterUpper(l)) continue;
                if (ring.Contains(l)) continue;

                char m = possibleMatch[i];
                ring.Put(l, m);
                candidates[candidateCount] = l;
                candidateCount++;
            }

            // recurse, returning if the puzzle is solved...
            if (SolveRecursively(ring, words.Slice(1)))
                return true;

            // remove candidate letter matches
            for (int i = 0; i < candidateCount; i++)
            {
                ring.Remove(candidates[i]);
            }
            candidateCount = 0;
        }
		
        return false;
    }
}