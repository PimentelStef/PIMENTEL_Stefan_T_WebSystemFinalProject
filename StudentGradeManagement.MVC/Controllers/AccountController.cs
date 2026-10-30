using Microsoft.AspNetCore.Mvc;

namespace StudentGradeManagement.MVC.Controllers;

public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }
}