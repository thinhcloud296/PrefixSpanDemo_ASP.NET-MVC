using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PrefixSpanDemo.PrefixSpanAglorithm
{
    public class Pair
    {
        public int Item { get; }
        private List<PseudoSequence> pseudoSequences = new List<PseudoSequence>();

        public Pair(int item)
        {
            this.Item = item;
        }

        public int GetCount() => pseudoSequences.Count;
        public List<PseudoSequence> GetPseudoSequences() => pseudoSequences;

        public override bool Equals(object obj)
        {
            if (obj is Pair pair)
                return pair.Item == this.Item;
            return false;
        }

        public override int GetHashCode() => Item.ToString().GetHashCode();
    }
}