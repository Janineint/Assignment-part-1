namespace Assignment_part_1.Models
{
    public class Teacher
    {
        public int TeacherId { get; set; }
        public string? TeacherFName { get; set; }
        public string? TeacherLName { get; set; }
        public string? EmployeeNumber { get; set; }
        public DateTime HireDate { get; set; }
        public string? Salary { get; set; }
        public string? TeacherEmployeeNumber { get; set; }
        public List<string> CourseNames { get; set; } = new List<string>();


    }
}
