using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PrefixSpanDemo.PrefixSpanAglorithm
{
    public class AlgoPrefixSpan
    {
        private int minsuppAbsolute;
        private SequentialPatterns patterns;
        private SequenceDatabase sequenceDatabase;
        private const int BUFFERS_SIZE = 2000;
        private int[] patternBuffer = new int[BUFFERS_SIZE];

        public List<SequentialPattern> RunAlgorithm(string inputFile, double minsupRelative)
        {
            sequenceDatabase = new SequenceDatabase();
            sequenceDatabase.LoadFile(inputFile);
            int sequenceCount = sequenceDatabase.Size();
            minsuppAbsolute = (int)Math.Ceiling(minsupRelative * sequenceCount);
            if (minsuppAbsolute == 0) minsuppAbsolute = 1;

            patterns = new SequentialPatterns("FREQUENT SEQUENTIAL PATTERNS");
            PrefixSpan();

            // Ghép toàn bộ kết quả lại trả về
            var allPatterns = new List<SequentialPattern>();
            foreach (var level in patterns.GetLevels())
            {
                allPatterns.AddRange(level);
            }
            foreach (var p in allPatterns)
            {
                Console.WriteLine($"Pattern: {p} | Support: {p.GetAbsoluteSupport()}");
            }

            return allPatterns;
        }


        private void PrefixSpan()
        {
            var mapSequenceID = FindSequencesContainingItems();
            foreach (var entry in mapSequenceID.Where(e => e.Value.Count >= minsuppAbsolute))
            {
                int item = entry.Key;
                patternBuffer[0] = item;
                SavePattern(item, entry.Value);

                var projectedDatabase = BuildProjectedDatabase(item, entry.Value);
                Recursion(projectedDatabase, 2, 0);
            }
        }

        private Dictionary<int, List<int>> FindSequencesContainingItems()
        {
            var mapSequenceID = new Dictionary<int, List<int>>();
            for (int i = 0; i < sequenceDatabase.Size(); i++)
            {
                var sequence = sequenceDatabase.GetSequences()[i];
                foreach (var token in sequence)
                {
                    if (token > 0)
                    {
                        if (!mapSequenceID.ContainsKey(token))
                            mapSequenceID[token] = new List<int>();
                        if (mapSequenceID[token].LastOrDefault() != i)
                            mapSequenceID[token].Add(i);
                    }
                }
            }
            return mapSequenceID;
        }

        private List<PseudoSequence> BuildProjectedDatabase(int item, List<int> sequenceIDs)
        {
            var projectedDatabase = new List<PseudoSequence>();
            foreach (var sequenceID in sequenceIDs)
            {
                var sequence = sequenceDatabase.GetSequences()[sequenceID];
                for (int j = 0; sequence[j] != -2; j++)
                {
                    if (sequence[j] == item && sequence[j + 1] != -2)
                    {
                        projectedDatabase.Add(new PseudoSequence(sequenceID, j + 1));
                        break;
                    }
                }
            }
            return projectedDatabase;
        }

        private void SavePattern(int item, List<int> sequenceIDs)
        {
            var pattern = new SequentialPattern();
            pattern.AddItemset(new List<int> { item });
            pattern.SetSequenceIDs(sequenceIDs);
            patterns.AddSequence(pattern, 1);
        }

        private void Recursion(List<PseudoSequence> database, int k, int lastBufferPosition)
        {
            var itemsPseudoSequences = FindAllFrequentPairs(database);
            foreach (var entry in itemsPseudoSequences.Where(e => e.Value.Count >= minsuppAbsolute))
            {
                patternBuffer[lastBufferPosition + 1] = -1;
                patternBuffer[lastBufferPosition + 2] = entry.Key;
                var pattern = new SequentialPattern();
                for (int i = 0; i <= lastBufferPosition + 2; i++)
                {
                    if (patternBuffer[i] > 0)
                        pattern.AddItemset(new List<int> { patternBuffer[i] });
                }
                pattern.SetSequenceIDs(entry.Value.Select(ps => ps.SequenceID).ToList());
                patterns.AddSequence(pattern, k);

                if (k < 1000) // Maximum pattern length
                    Recursion(entry.Value, k + 1, lastBufferPosition + 2);
            }
        }

        private Dictionary<int, List<PseudoSequence>> FindAllFrequentPairs(List<PseudoSequence> sequences)
        {
            var mapItemsPseudoSequences = new Dictionary<int, List<PseudoSequence>>();
            foreach (var pseudoSequence in sequences)
            {
                var sequence = sequenceDatabase.GetSequences()[pseudoSequence.SequenceID];
                for (int i = pseudoSequence.IndexFirstItem; sequence[i] != -2; i++)
                {
                    if (sequence[i] > 0)
                    {
                        if (!mapItemsPseudoSequences.ContainsKey(sequence[i]))
                            mapItemsPseudoSequences[sequence[i]] = new List<PseudoSequence>();
                        if (!mapItemsPseudoSequences[sequence[i]].Any(ps => ps.SequenceID == pseudoSequence.SequenceID))
                            mapItemsPseudoSequences[sequence[i]].Add(new PseudoSequence(pseudoSequence.SequenceID, i + 1));
                    }
                }
            }
            return mapItemsPseudoSequences;
        }
    }
}