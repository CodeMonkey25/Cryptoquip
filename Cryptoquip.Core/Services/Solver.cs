using Cryptoquip.Models;
using Cryptoquip.Utility;

namespace Cryptoquip.Services;

public class Solver
{
    private readonly List<string> _skipWords = new();
    private DecoderRing _partialSolution = new DecoderRingNull();
    
    public void Run(Action<string> logMessage, DecoderRing ring, WordList? wordList, Puzzle puzzle,
        bool enableExclusionAnalysis)
    {
        _partialSolution = new DecoderRingNull();
        
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
            logMessage("No word list provided. Loading word list from disk.");
            wordList = new WordList(words.Select(static w => w.Pattern).ToHashSet());
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
            
            MatchRequirements?[] requirementsArray = new MatchRequirements[words.Length];
            int deleted = -1;
            while (deleted != 0)
            {
                deleted = 0;
                for (int i = 0; i < words.Length; i++)
                {
                    Word word = words[i];
                    if (requirementsArray[i] == null)
                    {
                        requirementsArray[i] = word.GetMatchRequirements();
                    }

                    MatchRequirements requirements = requirementsArray[i]!;
                    if (requirements.Count == 0) continue;
                    for (int j = 0; j < words.Length; j++)
                    {
                        Word otherWord = words[j];
                        if (i == j) continue;
                        if ((word.LetterMask & otherWord.LetterMask) == 0) continue;

                        int count = otherWord.EnsureMatchRequirements(requirements);

                        if (count > 0)
                        {
                            requirementsArray[j] = null;
                            deleted += count;
                        }
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

        if (!_solveLoop(ring, words, startIndex))
        {
            logMessage("Could not find a solution. Printing the best attempt.");
            ring.Overwrite(_partialSolution);
        }

        logMessage(string.Empty);
        logMessage(ring.Decode(puzzle.Text));
    }
    
    private bool _solveLoop(DecoderRing ring, Word[] words, int depth)
    {
        // depth exceeds the length of the array, we must have solved it...
        if (depth >= words.Length) return true;

        if (ring.SolveCount > _partialSolution.SolveCount)
        {
            _partialSolution = ring.Clone();
        }
		
        Word word = words[depth];
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
            if (_solveLoop(ring, words, depth + 1))
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