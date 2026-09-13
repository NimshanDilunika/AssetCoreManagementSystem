using AssetsEmployee.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.ContentModel;


[Authorize(Roles = "Admin")]
public class DepartmentController : Controller
{
    private readonly ApplicationDbContext _context;

    public DepartmentController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var data = _context.Department.ToList();
        return View(data);
    }

    public IActionResult AddDepartment()
    {
        return View();
    }
    public IActionResult AddDepartmentItem(string departmentName, string location, string description)
    {
        var newDepartment = new Department
        {
            DepartmentName = departmentName,
            Location = location,
            Description = description
        };

        _context.Department.Add(newDepartment);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }



    public IActionResult UpdateDepartment(int departmentid)
    {
        var data = _context.Department.FirstOrDefault(d => d.DepartmentId == departmentid);
        return View(data);
    }

    [HttpPost]
    public IActionResult UpdateDepartmentItem(int departmentid, string departmentName, string location, string description)
    {
        var data = _context.Department.FirstOrDefault(d => d.DepartmentId == departmentid);

        data.DepartmentName = departmentName;
        data.Location = location;
        data.Description = description;


        _context.Department.Update(data);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }













    // GET: Confirmation View for Delete
    public IActionResult ConfirmDeleteDepartment(int departmentid)
    {
        var department = _context.Department.FirstOrDefault(d => d.DepartmentId == departmentid);
        if (department == null)
        {
            return NotFound();
        }

        return View(department);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult DeleteDepartment(int departmentid)
    {
        var department = _context.Department.Find(departmentid);
        if (department != null)
        {
            _context.Remove(department);
            _context.SaveChanges();
        }
        return RedirectToAction("Index");
    }

    public IActionResult DetailDepartment(int departmentid)
    {
        var employees = _context.Employee.Include(e => e.Department)
            .Where(e => e.DepartmentId == departmentid)
            .ToList();
        return View(employees);


    }



}











