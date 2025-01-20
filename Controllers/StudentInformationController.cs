using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibrarySystem.Models;
using LibrarySystem.Helpers;
using ClosedXML.Excel;
using System.IO;


namespace LibrarySystem.Controllers
{
    public class StudentInformationController : Controller
    {
        private readonly TransactionDbContext _context;

        public StudentInformationController(TransactionDbContext context)
        {
            _context = context;
        }

        // GET: StudentInformation/Index
        public async Task<IActionResult> Index(string searchString, string searchBy, int? pageNumber, int? pageSize)
        {
            var students = from s in _context.StudentInformation select s;

            // Filter by search criteria
            if (!string.IsNullOrEmpty(searchString))
            {
                switch (searchBy)
                {
                    case "Name":
                        students = students.Where(s => EF.Functions.Like(s.StudentName, $"%{searchString}%"));
                        break;
                    case "GradeSection":
                        students = students.Where(s => EF.Functions.Like(s.StudentGradeSection, $"%{searchString}%"));
                        break;
                    case "LRN":
                        students = students.Where(s => s.StudentLRN.Contains(searchString));
                        break;
                    default:
                        students = students.Where(s =>
                            EF.Functions.Like(s.StudentName, $"%{searchString}%") ||
                            EF.Functions.Like(s.StudentGradeSection, $"%{searchString}%") ||
                            s.StudentLRN.Contains(searchString));
                        break;
                }
            }

            // Set pagination defaults
            pageNumber ??= 1;
            pageSize ??= 10;

            ViewData["SearchString"] = searchString;
            ViewData["SearchBy"] = searchBy;
            ViewData["PageSize"] = pageSize;

            return View(await PaginatedList<StudentInformation>.CreateAsync(students.AsNoTracking(), pageNumber.Value, pageSize.Value));
        }

        // GET: StudentInformation/AddOrEdit
        public async Task<IActionResult> AddOrEdit(string? id = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                return View(new StudentInformation());
            }

            else
            {
                return View(_context.StudentInformation.Find(id));
            }

            
        }

        // POST: StudentInformation/AddOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(string? id, [Bind("StudentLRN,StudentName,StudentGradeSection,StudentContact,StudentEmail")] StudentInformation student)
        {
            if (!ModelState.IsValid)
            {

                if (string.IsNullOrEmpty(id)) // Add new
                {
                    _context.Add(student);
                }
                else // Edit existing
                {
                    try
                    {
                        _context.Update(student);
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!StudentExists(student.StudentLRN))
                        {
                            return NotFound();
                        }
                        throw;
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
        return View(student);

        }

        // POST: StudentInformation/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var student = await _context.StudentInformation.FindAsync(id);
            if (student != null)
            {
                _context.StudentInformation.Remove(student);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> DownloadStudentInformation(string searchBy, string searchString)
        {
            try
            {
                // Get the filtered query similar to Index action
                var query = _context.StudentInformation.AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(searchBy) && !string.IsNullOrEmpty(searchString))
                {
                    switch (searchBy)
                    {
                        case "LRN":
                            if (int.TryParse(searchString, out int studentLRN))
                            {
                                query = query.Where(b => b.StudentLRN.Equals(studentLRN));
                            }
                            break;
                        case "StudentName":
                            query = query.Where(b => b.StudentName.Contains(searchString));
                            break;
                        case "StudentGradeSection":
                            query = query.Where(b => b.StudentGradeSection.Contains(searchString));
                            break;
                        case "StudentContact":
                            query = query.Where(b => b.StudentContact.Contains(searchString));
                            break;
                        case "StudentEmail":
                            query = query.Where(b => b.StudentEmail.Contains(searchString));
                            break;
                    }
                }

                // Get all transactions based on the filter
                var studentinformation = await query.ToListAsync();

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("StudentInformation");

                    // Add headers
                    worksheet.Cell(1, 1).Value = "LRN";
                    worksheet.Cell(1, 2).Value = "Name";
                    worksheet.Cell(1, 3).Value = "Grade & Section";
                    worksheet.Cell(1, 4).Value = "Contact";
                    worksheet.Cell(1, 5).Value = "Email";

                    // Style the header row
                    var headerRow = worksheet.Row(1);
                    headerRow.Style.Font.Bold = true;
                    headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

                    // Add data
                    int row = 2;
                    foreach (var studentinfo in studentinformation)
                    {
                        worksheet.Cell(row, 1).Value = studentinfo.StudentLRN;
                        worksheet.Cell(row, 2).Value = studentinfo.StudentName;
                        worksheet.Cell(row, 3).Value = studentinfo.StudentGradeSection;
                        worksheet.Cell(row, 4).Value = studentinfo.StudentContact;
                        worksheet.Cell(row, 5).Value = studentinfo.StudentEmail;
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
                            $"StudentInformation_{DateTime.Now:yyyyMMdd}.xlsx"
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

        private bool StudentExists(string id)
        {
            return _context.StudentInformation.Any(e => e.StudentLRN == id);
        }
    }
}
