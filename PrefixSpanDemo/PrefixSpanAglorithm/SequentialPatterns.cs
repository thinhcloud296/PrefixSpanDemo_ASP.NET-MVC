using System.Collections.Generic;

namespace PrefixSpanDemo.PrefixSpanAglorithm
{
    public class SequentialPatterns
    {
        private List<List<SequentialPattern>> levels = new List<List<SequentialPattern>>();
        private int sequenceCount = 0;
        private string name;

        public SequentialPatterns(string name)
        {
            this.name = name;
            levels.Add(new List<SequentialPattern>());
        }

        public void AddSequence(SequentialPattern sequence, int k)
        {
            while (levels.Count <= k)
                levels.Add(new List<SequentialPattern>());
            levels[k].Add(sequence);
            sequenceCount++;
        }

        public List<List<SequentialPattern>> GetLevels() => levels;
    }
}