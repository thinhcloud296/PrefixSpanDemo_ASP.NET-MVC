using PrefixSpanDemo.PrefixSpanAglorithm;
using PrefixSpanDemo.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace PrefixSpanDemo.Controllers
{
    public class HomeController : Controller
    {
        private const double DefaultMinSupportFromDb = 0.1;
        private const string SessionKey = "UserClickSessionId";
        // Điều chỉnh CsvBatchSize tùy theo lượng RAM và kích thước CSV.
        // Đây là số lượng CHUỖI (dòng) từ CSV sẽ được xử lý trước khi lưu vào DB theo lô.
        private const int CsvProcessBatchSize = 500;

        public static readonly Dictionary<int, string> productMap = new Dictionary<int, string>
        {
            { 1, "Atiso" }, { 2, "Bắp cải tím" }, { 3, "Cà chua" }, { 4, "Cà rốt" },
            { 5, "Chanh vàng" }, { 6, "Dưa leo" }, { 7, "Khoai mỡ" }, { 8, "Khoai tây" },
            { 9, "Khổ qua" }, { 10, "Ớt chuông đỏ" }, { 11, "Ớt chuông vàng" }, { 12, "Tỏi" }
            // Đảm bảo danh sách này khớp với ProductId trong DB của bạn
        };

        public ActionResult Index()
        {
            Trace.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] HomeController.Index (GET) - START");
            var model = new DemoViewModel();
            using (AppDbContext db = new AppDbContext())
            {
                List<Product> products = db.Products.ToList();
                ViewBag.Products = products;

                string sequencePath = Server.MapPath("~/App_Data/sequences.txt");
                GenerateSequencesFileFromDatabase(db, sequencePath);

                List<int> recommendations = new List<int>();
                if (System.IO.File.Exists(sequencePath) && new FileInfo(sequencePath).Length > 0)
                {
                    var algo = new AlgoPrefixSpan();
                    List<SequentialPattern> patternsList = algo.RunAlgorithm(sequencePath, DefaultMinSupportFromDb);
                    Trace.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] HomeController.Index (GET) - PrefixSpan found {patternsList.Count} patterns.");

                    string sessionId = Session[SessionKey] as string;
                    int? lastClickedProductId = null;
                    if (!string.IsNullOrEmpty(sessionId))
                    {
                        lastClickedProductId = db.Sequences
                            .Where(s => s.SessionId == sessionId)
                            .OrderByDescending(s => s.ClickTime)
                            .Select(s => (int?)s.ProductId)
                            .FirstOrDefault();
                    }

                    if (lastClickedProductId.HasValue)
                    {
                        recommendations = GetRecommendationsBasedOnLastClick(patternsList, lastClickedProductId.Value);
                    }
                    if (!recommendations.Any() && patternsList.Any())
                    {
                        recommendations = GetGeneralRecommendations(patternsList);
                    }
                    Trace.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] HomeController.Index (GET) - Recommendations: {recommendations.Count} items. IDs: {string.Join(",", recommendations)}");
                }
                else
                {
                    Trace.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] HomeController.Index (GET) - sequences.txt not found or empty.");
                }
                model.Recommendations = recommendations;
            }
            Trace.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] HomeController.Index (GET) - END");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(DemoViewModel model) // Model chỉ dùng để nhận CsvFile
        {
            Trace.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] HomeController.Index (POST) - START");
            if (model.CsvFile != null && model.CsvFile.ContentLength > 0)
            {
                Trace.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] HomeController.Index (POST) - CSV file: {model.CsvFile.FileName}");
                try
                {
                    int totalSequencesProcessedFromCsv = 0; // Đếm số dòng CSV được xử lý (có thể tạo sequence hoặc không)
                    int sequencesSuccessfullyAddedToDb = 0; // Đếm số sequence thực sự được thêm vào DB
                    List<string> processingMessages = new List<string>(); // Để thu thập thông báo chi tiết
                    List<Sequence> currentBatchOfDbSequences = new List<Sequence>();

                    using (StreamReader reader = new StreamReader(model.CsvFile.InputStream))
                    {
                        string csvLine;
                        while ((csvLine = reader.ReadLine()) != null)
                        {
                            totalSequencesProcessedFromCsv++;
                            if (string.IsNullOrWhiteSpace(csvLine))
                            {
                                processingMessages.Add($"Dòng {totalSequencesProcessedFromCsv}: Bỏ qua (trống).");
                                continue;
                            }

                            string[] itemsStr = csvLine.Split(',');
                            List<int> productIdsInCurrentCsvSequence = new List<int>();
                            bool currentCsvLineIsValid = false;

                            using (var dbCheck = new AppDbContext()) // Context riêng để kiểm tra ProductID, tránh ảnh hưởng context chính
                            {
                                foreach (var itemStr in itemsStr)
                                {
                                    if (int.TryParse(itemStr.Trim(), out int productId))
                                    {
                                        if (dbCheck.Products.Any(p => p.ProductId == productId)) // Kiểm tra ProductID tồn tại
                                        {
                                            productIdsInCurrentCsvSequence.Add(productId);
                                            currentCsvLineIsValid = true; // Đánh dấu dòng này có ít nhất 1 ProductID hợp lệ
                                        }
                                        else
                                        {
                                            processingMessages.Add($"Dòng {totalSequencesProcessedFromCsv}: ProductID '{productId}' (từ item '{itemStr}') không tồn tại trong DB.");
                                        }
                                    }
                                    else
                                    {
                                        processingMessages.Add($"Dòng {totalSequencesProcessedFromCsv}: Item '{itemStr}' không phải là số nguyên hợp lệ.");
                                    }
                                }
                            }


                            if (productIdsInCurrentCsvSequence.Any() && currentCsvLineIsValid)
                            {
                                string csvSessionId = $"CSV_MERGE_{Guid.NewGuid().ToString("N").Substring(0, 16)}";
                                int sequenceOrder = 0;
                                foreach (int productIdInSeq in productIdsInCurrentCsvSequence)
                                {
                                    currentBatchOfDbSequences.Add(new Sequence // Thêm vào danh sách batch hiện tại
                                    {
                                        UserId = 0, // Đánh dấu dữ liệu từ CSV
                                        SessionId = csvSessionId,
                                        ProductId = productIdInSeq,
                                        ClickTime = DateTime.UtcNow,
                                        SequenceOrder = ++sequenceOrder
                                    });
                                }
                                sequencesSuccessfullyAddedToDb++;
                            }

                            // Khi số lượng chuỗi (dòng CSV) đã xử lý đạt đến kích thước lô, hoặc số lượng bản ghi Sequence trong lô lớn
                            if ((sequencesSuccessfullyAddedToDb > 0 && sequencesSuccessfullyAddedToDb % CsvProcessBatchSize == 0) ||
                                 (currentBatchOfDbSequences.Count > CsvProcessBatchSize * 5)) // Ước lượng (ví dụ mỗi chuỗi CSV có trung bình 5 items)
                            {
                                SaveBatchToDatabase(currentBatchOfDbSequences, processingMessages);
                                Trace.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] HomeController.Index (POST) - Saved batch of {currentBatchOfDbSequences.Count} Sequence records (from {sequencesSuccessfullyAddedToDb} CSV sequences processed so far).");
                                currentBatchOfDbSequences.Clear();
                            }
                        } // end while readline
                    } // StreamReader disposed

                    if (currentBatchOfDbSequences.Any()) // Lưu phần còn lại của lô cuối cùng
                    {
                        SaveBatchToDatabase(currentBatchOfDbSequences, processingMessages);
                        Trace.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] HomeController.Index (POST) - Saved final batch of {currentBatchOfDbSequences.Count} Sequence records.");
                        currentBatchOfDbSequences.Clear();
                    }

                    if (sequencesSuccessfullyAddedToDb > 0)
                    {
                        TempData["SuccessMessage"] = $"Đã gộp thành công {sequencesSuccessfullyAddedToDb} chuỗi hợp lệ từ file CSV. Đang cập nhật gợi ý...";
                        // Tạo lại sequences.txt sau khi tất cả các lô đã được lưu
                        using (AppDbContext db = new AppDbContext()) // Context mới để tạo file
                        {
                            string sequencePath = Server.MapPath("~/App_Data/sequences.txt");
                            GenerateSequencesFileFromDatabase(db, sequencePath);
                        }
                    }
                    else
                    {
                        TempData["InfoMessage"] = "Không có chuỗi sản phẩm hợp lệ nào được gộp từ file CSV.";
                    }

                    if (processingMessages.Any())
                    {
                        string summaryMessage = $"Đã xử lý {totalSequencesProcessedFromCsv} dòng CSV. ";
                        if (sequencesSuccessfullyAddedToDb > 0) summaryMessage += $"Gộp thành công {sequencesSuccessfullyAddedToDb} chuỗi. ";

                        TempData["WarningMessage"] = summaryMessage +
                                                    "Một số dòng/sản phẩm có thể đã bị bỏ qua (xem chi tiết bên dưới nếu có hoặc trong log server):<br/>- " +
                                                    string.Join("<br/>- ", processingMessages.Take(10)) + // Hiển thị tối đa 10 thông báo đầu tiên
                                                    (processingMessages.Count > 10 ? "<br/>... và thêm nữa." : "");
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessageFromPost"] = "Lỗi nghiêm trọng khi xử lý và gộp file CSV: " + ex.Message;
                    Trace.TraceError($"[{DateTime.Now:HH:mm:ss.fff}] HomeController.Index (POST) - CSV Processing/Merging Exception: {ex.ToString()}");
                }
            }
            else if (Request.HttpMethod == "POST")
            {
                TempData["ErrorMessageFromPost"] = "Vui lòng chọn một file CSV để gộp dữ liệu.";
            }
            Trace.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] HomeController.Index (POST) - END, Redirecting to Index GET.");
            return RedirectToAction("Index");
        }

        // Hàm phụ để lưu một lô sequence vào DB với context mới
        private void SaveBatchToDatabase(List<Sequence> batchToSave, List<string> processingMessages)
        {
            if (!batchToSave.Any()) return;

            using (var db = new AppDbContext())
            {
                db.Configuration.AutoDetectChangesEnabled = false; // Tắt để tăng tốc độ AddRange
                try
                {
                    db.Sequences.AddRange(batchToSave); // Dùng AddRange hiệu quả hơn Add từng cái
                    db.ChangeTracker.DetectChanges(); // Cần gọi nếu AutoDetectChangesEnabled = false
                    db.SaveChanges();
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    Trace.TraceError($"[{DateTime.Now:HH:mm:ss.fff}] SaveBatchToDatabase - DbUpdateException: {ex.ToString()}");
                    var detailedError = ex.ToString();
                    if (ex.InnerException != null) detailedError += "\nInner: " + ex.InnerException.ToString();
                    if (ex.InnerException?.InnerException != null) detailedError += "\nInnerInner: " + ex.InnerException.InnerException.ToString();

                    processingMessages.Add($"Lỗi DB khi lưu lô CSV: Một số ProductID có thể không hợp lệ hoặc có lỗi ràng buộc. Chi tiết lỗi đã được ghi ở server. ({ex.Message.Substring(0, Math.Min(ex.Message.Length, 100))}...)");
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"[{DateTime.Now:HH:mm:ss.fff}] SaveBatchToDatabase - General Exception: {ex.ToString()}");
                    processingMessages.Add($"Lỗi không xác định khi lưu lô CSV: {ex.Message.Substring(0, Math.Min(ex.Message.Length, 100))}...");
                }
                // Không cần bật lại AutoDetectChangesEnabled vì context này sẽ được dispose
            }
        }

        [HttpPost]
        public JsonResult LogClick(int productId)
        {
            Trace.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] HomeController.LogClick - START for productId: {productId}");
            try
            {
                using (AppDbContext db = new AppDbContext())
                {
                    string sessionId = Session[SessionKey] as string;
                    if (string.IsNullOrEmpty(sessionId))
                    {
                        sessionId = Guid.NewGuid().ToString();
                        Session[SessionKey] = sessionId;
                    }
                    int currentMaxOrder = db.Sequences
                                         .Where(s => s.SessionId == sessionId)
                                         .Select(s => (int?)s.SequenceOrder)
                                         .Max() ?? 0;
                    var newSequence = new Sequence
                    {
                        UserId = 1,
                        SessionId = sessionId,
                        ProductId = productId,
                        ClickTime = DateTime.UtcNow,
                        SequenceOrder = currentMaxOrder + 1
                    };
                    db.Sequences.Add(newSequence);
                    db.SaveChanges();
                    Trace.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] HomeController.LogClick - Click saved. ProductId: {productId}, New SequenceId: {newSequence.SequenceOrder}.");
                    string sequencePath = Server.MapPath("~/App_Data/sequences.txt");
                    GenerateSequencesFileFromDatabase(db, sequencePath);
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Trace.TraceError($"[{DateTime.Now:HH:mm:ss.fff}] HomeController.LogClick - ERROR: {ex.ToString()}");
                return Json(new { success = false, message = "Lỗi server khi ghi nhận lượt click." });
            }
        }

        private void GenerateSequencesFileFromDatabase(AppDbContext db, string outputPath)
        {
            Trace.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] GenerateSequencesFileFromDatabase - START generating {outputPath}");
            var sequencesData = db.Sequences
                                .AsNoTracking()
                                .OrderBy(s => s.SessionId)
                                .ThenBy(s => s.SequenceOrder)
                                .Select(s => new { s.SessionId, s.ProductId })
                                .ToList()
                                .GroupBy(s => s.SessionId)
                                .Select(g => g.Select(s => s.ProductId).ToList())
                                .ToList();

            List<string> lines = new List<string>();
            foreach (var seq in sequencesData)
            {
                if (seq.Any()) lines.Add(string.Join(" -1 ", seq) + " -2");
            }
            try
            {
                System.IO.File.WriteAllLines(outputPath, lines);
                Trace.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] GenerateSequencesFileFromDatabase - Generated {lines.Count} lines into {outputPath}");
            }
            catch (IOException ioEx)
            {
                Trace.TraceError($"[{DateTime.Now:HH:mm:ss.fff}] GenerateSequencesFileFromDatabase - IOException: {ioEx.ToString()}");
            }
        }

        private List<int> GetRecommendationsBasedOnLastClick(List<SequentialPattern> currentPatterns, int lastClickedProductId)
        {
            // Giữ nguyên logic
            if (currentPatterns == null || !currentPatterns.Any()) return new List<int>();
            return currentPatterns.Where(p => p.FlattenedSequence.Count > 1)
                .SelectMany(p => { var list = p.FlattenedSequence; var results = new List<int>(); for (int i = 0; i < list.Count - 1; i++) { if (list[i] == lastClickedProductId && list[i + 1] != lastClickedProductId) results.Add(list[i + 1]); } return results; })
                .GroupBy(x => x).OrderByDescending(g => g.Count()).Select(g => g.Key).Distinct().Take(5).ToList();
        }

        private List<int> GetGeneralRecommendations(List<SequentialPattern> currentPatterns)
        {
            // Giữ nguyên logic
            if (currentPatterns == null || !currentPatterns.Any()) return new List<int>();
            return currentPatterns.SelectMany(p => p.FlattenedSequence).Where(x => x > 0)
                .GroupBy(x => x).OrderByDescending(g => g.Count()).Select(g => g.Key).Distinct().Take(5).ToList();
        }
    }
}