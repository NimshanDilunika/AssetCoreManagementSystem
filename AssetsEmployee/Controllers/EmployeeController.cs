using AssetsEmployee.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.ContentModel;

namespace EmployeeControlle.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {

            var employees = _context.Employee.Include(e => e.Department).ToList();
            return View(employees);
        }

        public IActionResult AddEmployee()
        {
            ViewBag.Departments = _context.Department.ToList();
            return View();
        }




        [HttpPost]
        public IActionResult AddEmployeeItem(string employeename, string email, string phoneno, int departmentid)
        {

            var nameRegex = @"^[a-zA-Z\s]+$";
            var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            if (_context.Employee.Any(e => e.EmployeeName.ToLower() == employeename.ToLower()))
            {
                ModelState.AddModelError("EmployeeName", "This Name is already used another employee");
            }

            if (string.IsNullOrEmpty(employeename) || !System.Text.RegularExpressions.Regex.IsMatch(employeename, nameRegex))
            {
                ModelState.AddModelError("EmployeeName", "Employee name must contain only letters and spaces.");
            }




            if (string.IsNullOrEmpty(email) || !System.Text.RegularExpressions.Regex.IsMatch(email, emailRegex))
            {
                ModelState.AddModelError("Email", "Please enter a valid email containing '@' and a domain (e.g., example@domain.com).");
            }


            if (_context.Employee.Any(e => e.Email.ToLower() == email.ToLower()))
            {
                ModelState.AddModelError("Email", "This Email address is already registered.");
            }


            if (string.IsNullOrEmpty(phoneno) || !System.Text.RegularExpressions.Regex.IsMatch(phoneno, @"^\d{10}$"))
            {
                ModelState.AddModelError("PhoneNo", "Phone number must be exactly 10 digits.");
            }



            var newEmployee = new Employee
            {
                EmployeeName = employeename,
                Email = email,
                PhoneNo = phoneno,
                DepartmentId = departmentid
            };
            if (!ModelState.IsValid)
            {
                ViewBag.Departments = _context.Department.ToList();
                return View("AddEmployee");
            }
            _context.Employee.Add(newEmployee);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }




        public IActionResult UpdateEmployee(int employeeid)
        {
            var employee = _context.Employee.FirstOrDefault(e => e.EmployeeId == employeeid);

            ViewBag.Departments = _context.Department.ToList();
            return View(employee);
        }





        [HttpPost]


        public IActionResult UpdateEmployeeItem(int employeeid, string employeename, string email, string phoneno, int departmentid)
        {
            var nameRegex = @"^[a-zA-Z\s]+$";

            var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            if (_context.Employee.Any(e => e.EmployeeName.ToLower() == employeename.ToLower() && e.EmployeeId != employeeid))
            {
                ModelState.AddModelError("EmployeeName", "This Name is already used another employee.");
            }



            if (string.IsNullOrEmpty(employeename) || !System.Text.RegularExpressions.Regex.IsMatch(employeename, nameRegex))
            {
                ModelState.AddModelError("EmployeeName", "Employee name must contain only letters and spaces.");
            }



            if (string.IsNullOrEmpty(email) || !System.Text.RegularExpressions.Regex.IsMatch(email, emailRegex))
            {
                ModelState.AddModelError("Email", "Please enter a valid email containing '@' and a domain (e.g., example@domain.com).");
            }
            if (_context.Employee.Any(e => e.Email.ToLower() == email.ToLower() && e.EmployeeId != employeeid))
            {
                ModelState.AddModelError("Email", "This Email address is already registered to another employee.");
            }


            if (string.IsNullOrEmpty(phoneno) || !System.Text.RegularExpressions.Regex.IsMatch(phoneno, @"^\d{10}$"))
            {
                ModelState.AddModelError("PhoneNo", "Phone number must be exactly 10 digits.");
            }
            var employee = _context.Employee.FirstOrDefault(e => e.EmployeeId == employeeid);



            employee.EmployeeName = employeename;
            employee.Email = email;
            employee.PhoneNo = phoneno;
            employee.DepartmentId = departmentid;

            if (!ModelState.IsValid)
            {
                ViewBag.Departments = _context.Department.ToList();
                return View("UpdateEmployee", employee);
            }

            _context.Employee.Update(employee);
            _context.SaveChanges();


            return RedirectToAction("Index");
        }

        // GET: Confirmation View for Delete
        public IActionResult ConfirmDeleteEmployee(int employeeid)
        {
            var employee = _context.Employee
                .Include(e => e.Department)
                .FirstOrDefault(e => e.EmployeeId == employeeid);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Delete Action
        [HttpPost]
        public IActionResult DeleteEmployee(int employeeid)
        {
            var employee = _context.Employee.Find(employeeid);
            if (employee != null)
            {
                _context.Employee.Remove(employee);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        public IActionResult DetailEmployee(int employeeid)
        {
            var assignment = _context.EmployeeAsset
                .Include(ea => ea.Asset)
                .Include(ea => ea.Employee)
                .Where(ea => ea.EmployeeId == employeeid)
                .ToList();

            return View(assignment);
        }
    }
}