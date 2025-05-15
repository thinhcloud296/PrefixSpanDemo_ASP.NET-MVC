using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrefixSpanDemo.PrefixSpanAglorithm
{
    public class SequentialPattern : IComparable<SequentialPattern>
    {
        private List<List<int>> itemsets = new List<List<int>>();
        private List<int> sequenceIds = new List<int>();

        public SequentialPattern() { }

        // Thêm 1 itemset (mỗi itemset có thể chứa 1 hoặc nhiều item)
        public void AddItemset(List<int> itemset) => itemsets.Add(itemset);

        // Gán danh sách sequence ID (phiên xuất hiện)
        public void SetSequenceIDs(List<int> ids) => sequenceIds = ids;

        // Hỗ trợ tuyệt đối
        public int GetAbsoluteSupport() => sequenceIds.Count;

        // Trả danh sách itemsets thô
        public List<List<int>> Itemsets => itemsets;

        // Trả danh sách tuần tự các item (mỗi itemset chứa 1 item)
        public List<int> FlattenedSequence
        {
            get
            {
                var result = new List<int>();
                foreach (var itemset in itemsets)
                {
                    result.AddRange(itemset); // flatten all items
                }
                return result;
            }
        }

        // Hiển thị dạng (1)(2)(3)
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var itemset in itemsets)
            {
                sb.Append("(");
                sb.Append(string.Join(" ", itemset));
                sb.Append(")");
            }
            return sb.ToString();
        }

        // So sánh theo support
        public int CompareTo(SequentialPattern other)
        {
            int compare = this.GetAbsoluteSupport() - other.GetAbsoluteSupport();
            return compare != 0 ? compare : this.GetHashCode() - other.GetHashCode();
        }
    }
}
