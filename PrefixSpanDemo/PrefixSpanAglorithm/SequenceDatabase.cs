using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
// using System.Web; // Không cần thiết trong class này

namespace PrefixSpanDemo.PrefixSpanAglorithm
{
    public class SequenceDatabase
    {
        private List<int[]> sequences = new List<int[]>();
        // private long itemOccurrenceCount = 0; // Đã xóa vì không dùng

        public void LoadFile(string path)
        {
            // itemOccurrenceCount = 0; // Đã xóa
            sequences.Clear();

            try
            {
                using (var reader = new StreamReader(path))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("#") && !line.StartsWith("%") && !line.StartsWith("@"))
                        {
                            var tokens = line.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries); // Trim và xử lý tab
                            if (tokens.Length > 0) // Đảm bảo có token sau khi split
                            {
                                int[] sequence = new int[tokens.Length];
                                bool validLine = true;
                                for (int i = 0; i < tokens.Length; i++)
                                {
                                    if (!int.TryParse(tokens[i], out sequence[i]))
                                    {
                                        // Xử lý lỗi nếu một token không phải là số, ví dụ ghi log và bỏ qua dòng này
                                        System.Diagnostics.Debug.WriteLine($"Lỗi parse token '{tokens[i]}' trong file sequence, dòng: {line}");
                                        validLine = false;
                                        break;
                                    }
                                }
                                if (validLine)
                                {
                                    sequences.Add(sequence);
                                }
                            }
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi: File sequence '{path}' không tìm thấy.");
                // Có thể throw lại lỗi hoặc xử lý theo cách khác nếu cần
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi đọc file sequence '{path}': {ex.Message}");
                // Có thể throw lại lỗi hoặc xử lý theo cách khác
            }
        }

        public int Size() => sequences.Count;
        public List<int[]> GetSequences() => sequences;
    }
}