using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Cần cho [Display]
using System.Web; // Cần cho HttpPostedFileBase

namespace PrefixSpanDemo.Models
{
    public class DemoViewModel
    {
        /// <summary>
        /// Danh sách các ProductID được gợi ý cho người dùng.
        /// Dữ liệu này được tính toán trong HomeController dựa trên
        /// file sequences.txt (đã bao gồm cả dữ liệu click và dữ liệu CSV gộp).
        /// </summary>
        public List<int> Recommendations { get; set; }

        /// <summary>
        /// Thuộc tính để nhận file CSV được người dùng tải lên từ form.
        /// File này sẽ được xử lý để gộp dữ liệu vào cơ sở dữ liệu Sequences.
        /// </summary>
        [Display(Name = "Tải lên File CSV để Gộp")] // Nhãn sẽ hiển thị trên View nếu dùng Html.LabelFor
        public HttpPostedFileBase CsvFile { get; set; }

        // Các thuộc tính không còn cần thiết trong kịch bản chỉ gộp CSV:
        // public List<int> ProductIds { get; set; } // View sẽ lấy danh sách Product đầy đủ từ ViewBag.Products
        // public double MinSup { get; set; } // MinSup cho việc gộp sẽ dùng giá trị DefaultMinSupportFromDb trong Controller
        // public string CsvResults { get; set; } // Không hiển thị kết quả phân tích CSV riêng nữa
        // public string ErrorMessage { get; set; } // Sẽ dùng TempData cho thông báo

        /// <summary>
        /// Constructor khởi tạo danh sách Recommendations.
        /// </summary>
        public DemoViewModel()
        {
            Recommendations = new List<int>();
        }
    }
}