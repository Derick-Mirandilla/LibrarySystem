using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibrarySystem.Models;
using LibrarySystem.Helpers;
using System.Drawing;
using ClosedXML.Excel;
using System.IO;

namespace LibrarySystem.Controllers
{
    public class TransactionController : Controller
    {
        

        private readonly TransactionDbContext _context;

        public TransactionController(TransactionDbContext context)
        {
            _context = context;
        }

        // GET: Transaction
        public async Task<IActionResult> Index(string searchBy, string searchString, int pageNumber = 1, int pageSize = 10)
        {
            // Log the incoming search parameters for debugging
            Console.WriteLine($"Search By: {searchBy}, Search String: {searchString}");

            var query = _context.Transactions.AsQueryable();

            if (!string.IsNullOrEmpty(searchBy) && !string.IsNullOrEmpty(searchString))
            {
                // Check if we're searching by ID
                if (searchBy == "ID")
                {
                    // Try to parse the searchString into an integer (BookID)
                    int TransactionId;
                    bool isValidId = int.TryParse(searchString, out TransactionId);
                    Console.WriteLine($"Parsed BookID: {TransactionId}, Is Valid: {isValidId}");

                    // If valid ID, apply filter
                    if (isValidId)
                    {
                        query = query.Where(b => b.TransactionID == TransactionId);
                    }
                    else
                    {
                        // If not a valid ID, return empty or show a validation message
                        query = query.Where(b => false); // This ensures no results are returned for invalid input
                    }
                }
                else
                {
                    // Filter by Title, Author, or Category
                    if (searchBy == "BookTitle")
                    {
                        query = query.Where(b => b.BookTitle.Contains(searchString));
                    }
                    else if (searchBy == "StudentName")
                    {
                        query = query.Where(b => b.StudentName.Contains(searchString));
                    }
                    else if (searchBy == "Status")
                    {
                        query = query.Where(b => b.TransactionStatus.Contains(searchString));
                    }

                }
            }

            // Pagination logic
            var count = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            var model = new PaginatedList<Transaction>(items, count, pageNumber, pageSize);

            // Set ViewData to retain the search parameters
            ViewData["SearchString"] = searchString;
            ViewData["SearchBy"] = searchBy;
            ViewData["PageSize"] = pageSize;

            return View(model);
        }



        // GET: Transactions/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LRN,BookID")] Transaction transaction, BookInformation bookStatus)
        {
            try
            {
                // First check if student exists
                var student = await _context.StudentInformation
                    .FirstOrDefaultAsync(s => s.StudentLRN == transaction.LRN);
                if (student == null)
                {
                    ModelState.AddModelError("LRN", "Student not found");
                    return View(transaction);
                }

                // Then check if book exists
                var book = await _context.BookInformation
                    .FirstOrDefaultAsync(b => b.BookID == transaction.BookID);
                if (book == null)
                {
                    ModelState.AddModelError("BookID", "Book not found");
                    return View(transaction);
                }

                // Check if the book is already borrowed (Unavailable)
                if (book.BookStatus == "Unavailable")
                {
                    ModelState.AddModelError("BookID", "This book is currently unavailable.");
                    return View(transaction);
                }

                // Check if the book is already borrowed with an active transaction
                var activeTransaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.BookID == transaction.BookID && t.TransactionStatus == "Active");
                if (activeTransaction != null)
                {
                    ModelState.AddModelError("BookID", "This book is currently borrowed.");
                    return View(transaction);
                }

                // Check if student has any overdue books
                var overdueTransactions = await _context.Transactions
                    .AnyAsync(t => t.LRN == transaction.LRN &&
                                  t.TransactionStatus == "Active" &&
                                  t.IssueDate.AddDays(30) < DateTime.Now);
                if (overdueTransactions)
                {
                    ModelState.AddModelError("LRN", "Student has overdue books.");
                    return View(transaction);
                }

                // If all validations pass, create the transaction
                transaction.IssueDate = DateTime.Now;
                transaction.TransactionStatus = "Active";
                transaction.StudentName = student.StudentName;
                transaction.GradeAndSection = student.StudentGradeSection;
                transaction.ContactNumber = student.StudentContact;
                transaction.Email = student.StudentEmail;
                transaction.BookTitle = book.BookTitle;

                // Update book status to Unavailable
                book.BookStatus = "Unavailable";
                _context.BookInformation.Update(book); // Mark the book as updated

                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Transaction created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while processing your request.");
                return View(transaction);
            }
        }



        // GET: Fetch student data by LRN (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetStudentByLRN(string lrn)
        {
            try
            {
                var student = await _context.StudentInformation
                    .FirstOrDefaultAsync(s => s.StudentLRN == lrn);

                if (student == null)
                    return Json(new { success = false, message = "Student not found" });

                // Check if student has any overdue books
                var hasOverdueBooks = await _context.Transactions
                    .AnyAsync(t => t.LRN == lrn &&
                                  t.TransactionStatus == "Active" &&
                                  t.IssueDate.AddDays(7) < DateTime.Now);

                return Json(new
                {
                    success = true,
                    studentName = student.StudentName,
                    hasOverdueBooks = hasOverdueBooks
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error retrieving student data" });
            }
        }

        // GET: Fetch book data by BookID (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetBookByID(int bookID)
        {
            try
            {
                var book = await _context.BookInformation
                    .FirstOrDefaultAsync(b => b.BookID == bookID);

                if (book == null)
                    return Json(new { success = false, message = "Book not found" });

                // Check if book is already borrowed
                var isBookBorrowed = await _context.Transactions
                    .AnyAsync(t => t.BookID == bookID && t.TransactionStatus == "Active");

                return Json(new
                {
                    success = true,
                    bookTitle = book.BookTitle,
                    isBookBorrowed = isBookBorrowed
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error retrieving book data" });
            }
        }


        [HttpPost]
        public async Task<IActionResult> ReturnBook(int id)
        {
            try
            {
                var transaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.TransactionID == id);

                if (transaction == null)
                    return Json(new { success = false, message = "Transaction not found" });

                if (transaction.TransactionStatus != "Active")
                    return Json(new { success = false, message = "Book is already returned" });

                // Update the transaction
                transaction.ReturnDate = DateTime.Now;
                transaction.TransactionStatus = "Returned";

                // Find the book related to this transaction
                var book = await _context.BookInformation
                    .FirstOrDefaultAsync(b => b.BookID == transaction.BookID);

                if (book != null)
                {
                    // Revert the book status to "Available"
                    book.BookStatus = "Available";
                    _context.BookInformation.Update(book); // Mark the book as updated
                }

                // Save changes to both the transaction and the book status
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Book returned successfully",
                    returnDate = transaction.ReturnDate.Value.ToShortDateString()
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error processing return" });
            }
        }



        // POST: Transactions/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var transaction = await _context.Transactions.FindAsync(id);
                if (transaction == null)
                {
                    return Json(new { success = false, message = "Transaction not found" });
                }

                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error deleting transaction" });
            }
        }

        public async Task<IActionResult> DownloadTransactions(string searchBy, string searchString)
        {
            try
            {
                // Get the filtered query similar to Index action
                var query = _context.Transactions.AsQueryable();

                // Apply the same filters as in the Index action
                if (!string.IsNullOrEmpty(searchBy) && !string.IsNullOrEmpty(searchString))
                {
                    if (searchBy == "ID" && int.TryParse(searchString, out int transactionId))
                    {
                        query = query.Where(b => b.TransactionID == transactionId);
                    }
                    else if (searchBy == "BookTitle")
                    {
                        query = query.Where(b => b.BookTitle.Contains(searchString));
                    }
                    else if (searchBy == "StudentName")
                    {
                        query = query.Where(b => b.StudentName.Contains(searchString));
                    }
                    else if (searchBy == "Status")
                    {
                        query = query.Where(b => b.TransactionStatus.Contains(searchString));
                    }
                }

                // Get all transactions based on the filter
                var transactions = await query.ToListAsync();

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Transactions");

                    // Add headers
                    worksheet.Cell(1, 1).Value = "Transaction ID";
                    worksheet.Cell(1, 2).Value = "Student LRN";
                    worksheet.Cell(1, 3).Value = "Student Name";
                    worksheet.Cell(1, 4).Value = "Grade/Section";
                    worksheet.Cell(1, 5).Value = "Book ID";
                    worksheet.Cell(1, 6).Value = "Book Title";
                    worksheet.Cell(1, 7).Value = "Issue Date";
                    worksheet.Cell(1, 8).Value = "Return Date";
                    worksheet.Cell(1, 9).Value = "Status";

                    // Style the header row
                    var headerRow = worksheet.Row(1);
                    headerRow.Style.Font.Bold = true;
                    headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

                    // Add data
                    int row = 2;
                    foreach (var transaction in transactions)
                    {
                        worksheet.Cell(row, 1).Value = transaction.TransactionID;
                        worksheet.Cell(row, 2).Value = transaction.LRN;
                        worksheet.Cell(row, 3).Value = transaction.StudentName;
                        worksheet.Cell(row, 4).Value = transaction.GradeAndSection;
                        worksheet.Cell(row, 5).Value = transaction.BookID;
                        worksheet.Cell(row, 6).Value = transaction.BookTitle;
                        worksheet.Cell(row, 7).Value = transaction.IssueDate.ToString("MM/dd/yyyy");
                        worksheet.Cell(row, 8).Value = transaction.ReturnDate?.ToString("MM/dd/yyyy") ?? "";
                        worksheet.Cell(row, 9).Value = transaction.TransactionStatus;
                        row++;
                    }

                    // Auto-fit columns
                    worksheet.Columns().AdjustToContents();

                    // Prepare the memory stream
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Position = 0;

                        // Return the Excel file
                        return File(
                            stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"Transactions_{DateTime.Now:yyyyMMdd}.xlsx"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error downloading transactions: {ex.Message}");
                TempData["Error"] = "Error downloading transactions";
                return RedirectToAction(nameof(Index));
            }
        }


        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.TransactionID == id);
        }
    }
}
