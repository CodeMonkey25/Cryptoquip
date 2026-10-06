using Cryptoquip.Models;

namespace Cryptoquip.Services;

public class Solver
{
    public void Run(Action<string> logMessage, Puzzle puzzle, DecoderRing ring, WordList? wordList = null, bool enableExclusionAnalysis = false)
    {
        logMessage($"Received puzzle: {puzzle.Text}");
        logMessage(string.Empty);
        
        Word[] words = puzzle
            .GetFilteredAndDistinctWords()
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

        DecoderRing partialSolution = ring.Clone();
        if (!SolveRecursively(words.AsSpan(startIndex), ring, partialSolution))
        {
            logMessage("Could not find a solution. Printing the best attempt.");
            ring.Overwrite(partialSolution);
        }

        logMessage(string.Empty);
        logMessage(ring.Decode(puzzle.Text));
    }
    
    private bool SolveRecursively(Span<Word> words, DecoderRing ring, DecoderRing partialSolution)
    {
        // if words is empty, we must have solved it...
        if (words.IsEmpty) return true;

        if (ring.SolveCount > partialSolution.SolveCount)
        {
            partialSolution.Overwrite(ring);
        }
		
        // pick the most-constrained word; bail out early if any word is dead
        int bestIndex = -1;
        int bestCount = int.MaxValue;
        for (int i = 0; i < words.Length; i++)
        {
            int count = 0;
            foreach (string m in words[i].Matches)
            {
                if (ring.Matches(words[i].Text, m))
                {
                    if (++count >= bestCount) break;
                }
            }

            if (count == 0) return false; // early pruning
            if (count < bestCount)
            {
                bestCount = count;
                bestIndex = i;
            }
        }
        if (bestIndex > 0) (words[0], words[bestIndex]) = (words[bestIndex], words[0]);
        
        Word word = words[0];
        Span<char> candidates = stackalloc char[word.Text.Length];
        foreach(string possibleMatch in word.Matches)
        {
            if (!ring.Matches(word.Text, possibleMatch)) continue;
            
            // add candidate letter matches
            int candidateCount = ring.Put(word.Text, possibleMatch, candidates);

            // recurse, returning if the puzzle is solved...
            if (SolveRecursively(words.Slice(1), ring, partialSolution)) return true;

            // remove candidate letter matches
            ring.Remove(candidates.Slice(0, candidateCount));
        }
		
        return false;
    }

    private bool SolveIteratively(Span<Word> words, DecoderRing ring, DecoderRing partialSolution)
    {
        int depth = 0;
        
        Span<int> matchesIndex = words.Length <= 100 ? stackalloc int[words.Length] : new int[words.Length];
        Span<int> candidatesCount = words.Length <= 100 ? stackalloc int[words.Length] : new int[words.Length];
        Span<char> candidatesBuffer = words.Length * 26 <= 4096
            ? stackalloc char[words.Length * 26]
            : new char[words.Length * 26];
        
        while (depth >= 0 && depth < words.Length)
        {
            Span<char> candidates = candidatesBuffer.Slice(depth * 26, 26);
            Word word = words[depth];
            List<string> matches = word.Matches;

            if (candidatesCount[depth] > 0)
            {
                ring.Remove(candidates.Slice(0, candidatesCount[depth]));
                candidatesCount[depth] = 0;
            }

            while (matchesIndex[depth] < matches.Count && !ring.Matches(word.Text, matches[matchesIndex[depth]]))
            {
                matchesIndex[depth]++;
            }

            if (matchesIndex[depth] >= matches.Count)
            {
                matchesIndex[depth] = 0;
                candidatesCount[depth] = 0;
                depth--;
                continue;
            }

            candidatesCount[depth] = ring.Put(word.Text, matches[matchesIndex[depth]], candidates);
            matchesIndex[depth]++;
            depth++;
            
            if (ring.SolveCount > partialSolution.SolveCount)
            {
                partialSolution.Overwrite(ring);
            }
        }

        return depth >= 0;
    }
}
