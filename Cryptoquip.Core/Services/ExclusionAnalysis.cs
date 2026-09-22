using Cryptoquip.Models;

namespace Cryptoquip.Services;

public class ExclusionAnalysis
{
    public int Run(Word[] words)
    {
        PriorityQueue<int, int> worklist = new();
        for (int i = 0; i < words.Length; i++)
        {
            worklist.Enqueue(i, words[i].Matches.Count);
        }

        MatchRequirements requirements = MatchRequirements.Build();
        int deleted = 0;
        while (worklist.TryDequeue(out int i, out int priority))
        {
            Word word = words[i];
            if (priority != word.Matches.Count) continue; // stale entry, skip it

            requirements.Rebuild(word.Text, word.Matches);

            for (int j = 0; j < words.Length; j++)
            {
                if (j == i) continue;
                if ((word.LetterMask & words[j].LetterMask) == 0) continue; // no shared letters, skip it

                int removed = words[j].EnsureMatchRequirements(requirements);
                if (removed == 0) continue;

                deleted += removed;
                worklist.Enqueue(j, words[j].Matches.Count);
            }
        }

        return deleted;
    }
}
