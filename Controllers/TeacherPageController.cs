using Microsoft.AspNetCore.Mvc;
using Assignment_part_1.Controllers; // Namespace for TeacherAPIController2
using Assignment_part_1.Models;
using System.Collections.Generic;

namespace Assignment_part_1.Controllers
{
    public class TeacherPageController : Controller
    {
        private readonly TeacherAPIController2 _teacherApiController;

        public TeacherPageController()
        {
            _teacherApiController = new TeacherAPIController2(); // Instantiate the API Controller
        }

        // Route: /Teacher/List
        public IActionResult List()
        {
            // Use the TeacherAPIController2 to fetch data
            IEnumerable<Teacher> teachers = _teacherApiController.GetAllTeachers();

            return View(teachers);
        }

        // Route: /Teacher/Show/{id}
        public IActionResult Show(int id)
        {
            // Use the TeacherAPIController2 to fetch a single teacher by ID
            Teacher teacher = _teacherApiController.GetTeacherById(id);

            if (teacher == null || teacher.TeacherId == 0)
            {
                return NotFound("Teacher not found.");
            }

            return View(teacher);
        }


        // Route: /Teacher/New
        public IActionResult New()
        {
            return View();
        }

        // Route: /Teacher/Add (POST)
        [HttpPost]
        public IActionResult Add(Teacher teacher)
        {
            if (ModelState.IsValid)
            {
                var result = _teacherApiController.AddTeacher(teacher);
                if (result is OkObjectResult)
                {
                    return RedirectToAction("List");
                }
                else
                {
                    ModelState.AddModelError("", "Error adding teacher. Please try again.");
                }
            }
            return View("New", teacher);
        }

        // Route: /Teacher/DeleteConfirm/{id}
        public IActionResult DeleteConfirm(int id)
        {
            var teacher = _teacherApiController.GetTeacherById(id);
            if (teacher == null || teacher.TeacherId == 0)
            {
                return NotFound("Teacher not found.");
            }
            return View(teacher);
        }

        // Route: /Teacher/Delete (POST)
        [HttpPost]
        public IActionResult Delete(int teacherId)
        {
            var result = _teacherApiController.DeleteTeacher(teacherId);
            if (result is OkObjectResult)
            {
                return RedirectToAction("List");
            }
            else
            {
                return BadRequest("Error deleting teacher.");
            }
        }

        // Route: /Teacher/Edit/{id}
        public IActionResult Edit(int id)
        {
            var teacher = _teacherApiController.GetTeacherById(id);
            if (teacher == null || teacher.TeacherId == 0)
            {
                return NotFound("Teacher not found.");
            }
            return View(teacher);
        }

        // Route: /Teacher/Update (POST)
        [HttpPost]
        public IActionResult Update(Teacher teacher)
        {
            if (ModelState.IsValid)
            {
                var result = _teacherApiController.UpdateTeacher(teacher.TeacherId, teacher);
                if (result is OkObjectResult)
                {
                    return View("Success");
                }
                else
                {
                    ModelState.AddModelError("", "Error updating teacher. Please try again.");
                }
            }
            return View("Edit", teacher);
        }
    }
}
