using LibrarySystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace LibrarySystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly TransactionDbContext _context;

        public HomeController(TransactionDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get total counts
            ViewBag.TotalBooks = await _context.BookInformation.CountAsync();
            ViewBag.TotalStudents = await _context.StudentInformation.CountAsync();
            ViewBag.ActiveBorrows = await _context.Transactions
                .Where(t => t.TransactionStatus == "Active")
                .CountAsync();
            ViewBag.OverdueBooks = await _context.Transactions
                .Where(t => t.TransactionStatus == "Active" &&
                           t.IssueDate.AddDays(30) < DateTime.Now)
                .CountAsync();

            // Get recent activities
            ViewBag.RecentActivities = await _context.Transactions
                .OrderByDescending(t => t.IssueDate)
                .Take(5)
                .Select(t => new {
                    Action = t.TransactionStatus == "Active" ? "Book Borrowed" : "Book Returned",
                    Date = t.TransactionStatus == "Active" ? t.IssueDate : t.ReturnDate.Value,
                    Description = $"{t.StudentName} - {t.BookTitle}"
                })
                .ToListAsync();

            return View();
        }
    }
}
