using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibrarySystem.Models;
using LibrarySystem.Helpers;
using Microsoft.IdentityModel.Tokens;
using ClosedXML.Excel;
using System.IO;

namespace LibrarySystem.Controllers
{
    public class BookInformationController : Controller
    {
        private readonly TransactionDbContext _context;

        public BookInformationController(TransactionDbContext context)
        {
            _context = context;
        }

        // GET: BookInformation
        public async Task<IActionResult> Index(string searchBy, string searchString, int pageNumber = 1, int pageSize = 10)
        {
            // Log the incoming search parameters for debugging
            Console.WriteLine($"Search By: {searchBy}, Search String: {searchString}");

            var query = _context.BookInformation.AsQueryable();

            if (!string.IsNullOrEmpty(searchBy) && !string.IsNullOrEmpty(searchString))
            {
                // Check if we're searching by ID
                if (searchBy == "ID")
                {
                    // Try to parse the searchString into an integer (BookID)
                    int bookId;
                    bool isValidId = int.TryParse(searchString, out bookId);
                    Console.WriteLine($"Parsed BookID: {bookId}, Is Valid: {isValidId}");

                    // If valid ID, apply filter
                    if (isValidId)
                    {
                        query = query.Where(b => b.BookID == bookId);
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
                    if (searchBy == "Title")
                    {
                        query = query.Where(b => b.BookTitle.Contains(searchString));
                    }
                    else if (searchBy == "Author")
                    {
                        query = query.Where(b => b.BookAuthor.Contains(searchString));
                    }
                    else if (searchBy == "Category")
                    {
                        query = query.Where(b => b.BookCategory.Contains(searchString));
                    }
                }
            }

            // Pagination logic
            var count = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            var model = new PaginatedList<BookInformation>(items, count, pageNumber, pageSize);

            // Set ViewData to retain the search parameters
            ViewData["SearchString"] = searchString;
            ViewData["SearchBy"] = searchBy;
            ViewData["PageSize"] = pageSize;

            return View(model);
        }




        // GET: BookInformation/AddOrEdit
        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
            if (id==0)
            {
                return View(new BookInformation());
            }

            else
            {
                return View(_context.BookInformation.Find(id));
            }
        }

        // POST: BookInformation/AddOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(int id, [Bind("BookID,BookTitle,BookAuthor,BookClassification,BookDonor,BookCategory,BookStatus")] BookInformation book)
        {
            if (ModelState.IsValid)
            {
                // Check if the BookID already exists in the database (for adding a new book)
                if (id == 0) // Add new book
                {
                    var existingBook = await _context.BookInformation
                        .FirstOrDefaultAsync(b => b.BookID == book.BookID);

                    if (existingBook != null)
                    {
                        // If BookID already exists, add an error and return the view
                        ModelState.AddModelError("BookID", "The Book ID already exists.");
                        return View(book);
                    }

                    // Add new book
                    _context.Add(book);
                }
                else // Edit existing book
                {
                    try
                    {
                        _context.Update(book);
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!BookInformationExists(book.BookID))
                        {
                            return NotFound();
                        }
                        throw;
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(book);
        }




        // POST: BookInformation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bookInformation = await _context.BookInformation.FindAsync(id);
            if (bookInformation != null)
            {
                _context.BookInformation.Remove(bookInformation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> DownloadBookInformation(string searchBy, string searchString)
        {
            try
            {
                // Get the filtered query similar to Index action
                var query = _context.BookInformation.AsQueryable();

                // Apply the same filters as in the Index action
                if (!string.IsNullOrEmpty(searchBy) && !string.IsNullOrEmpty(searchString))
                {
                    if (searchBy == "ID" && int.TryParse(searchString, out int bookId))
                    {
                        query = query.Where(b => b.BookID == bookId);
                    }
                    else if (searchBy == "BookTitle")
                    {
                        query = query.Where(b => b.BookTitle.Contains(searchString));
                    }
                    else if (searchBy == "BookAuthor")
                    {
                        query = query.Where(b => b.BookAuthor.Contains(searchString));
                    }
                    else if (searchBy == "BookClassification")
                    {
                        query = query.Where(b => b.BookClassification.Contains(searchString));
                    }
                    else if (searchBy == "BookDonor")
                    {
                        query = query.Where(b => b.BookDonor.Contains(searchString));
                    }
                    else if (searchBy == "BookCategory")
                    {
                        query = query.Where(b => b.BookCategory.Contains(searchString));
                    }
                    else if (searchBy == "BookStatus")
                    {
                        query = query.Where(b => b.BookStatus.Contains(searchString));
                    }
                }

                // Get all transactions based on the filter
                var bookinformation = await query.ToListAsync();

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("BookInformation");

                    // Add headers
                    worksheet.Cell(1, 1).Value = "ID";
                    worksheet.Cell(1, 2).Value = "Title";
                    worksheet.Cell(1, 3).Value = "Author";
                    worksheet.Cell(1, 4).Value = "Classifaction";
                    worksheet.Cell(1, 5).Value = "Donor";
                    worksheet.Cell(1, 6).Value = "Category";
                    worksheet.Cell(1, 7).Value = "Status";

                    // Style the header row
                    var headerRow = worksheet.Row(1);
                    headerRow.Style.Font.Bold = true;
                    headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

                    // Add data
                    int row = 2;
                    foreach (var bookinfo in bookinformation)
                    {
                        worksheet.Cell(row, 1).Value = bookinfo.BookID;
                        worksheet.Cell(row, 2).Value = bookinfo.BookTitle;
                        worksheet.Cell(row, 3).Value = bookinfo.BookAuthor;
                        worksheet.Cell(row, 4).Value = bookinfo.BookClassification;
                        worksheet.Cell(row, 5).Value = bookinfo.BookDonor;
                        worksheet.Cell(row, 6).Value = bookinfo.BookCategory;
                        worksheet.Cell(row, 7).Value = bookinfo.BookStatus;
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
                            $"BookInformation_{DateTime.Now:yyyyMMdd}.xlsx"
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

        private bool BookInformationExists(int id)
        {
            return _context.BookInformation.Any(e => e.BookID == id);
        }
    }
}
