using System;
using System.Collections.Generic;
using System.Linq;
using Api.Dtos;
using Logic.Students;
using Logic.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/students")]
    public sealed class StudentController : BaseController
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly StudentRepository _studentRepository;
        private readonly CourseRepository _courseRepository;

        public StudentController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _studentRepository = new StudentRepository(unitOfWork);
            _courseRepository = new CourseRepository(unitOfWork);
        }

        [HttpGet]
        public IActionResult GetList(string enrolled, int? number)
        {
            IReadOnlyList<Student> students = _studentRepository.GetList(enrolled, number);
            List<StudentDto> dtos = students.Select(x => ConvertToDto(x)).ToList();
            return Ok(dtos);
        }

        private StudentDto ConvertToDto(Student student)
        {
            return new StudentDto
            {
                Id = student.Id,
                Name = student.Name,
                Email = student.Email,
                Course1 = student.FirstEnrollment?.Course?.Name,
                Course1Grade = student.FirstEnrollment?.Grade.ToString(),
                Course1Credits = student.FirstEnrollment?.Course?.Credits,
                Course2 = student.SecondEnrollment?.Course?.Name,
                Course2Grade = student.SecondEnrollment?.Grade.ToString(),
                Course2Credits = student.SecondEnrollment?.Course?.Credits,
            };
        }

        [HttpPost]
        public IActionResult Register([FromBody] NewStudentDto dto)
        {
            var student = new Student(dto.Name, dto.Email);

            if (dto.Course1 != null && dto.Course1Grade != null)
            {
                Course course = _courseRepository.GetByName(dto.Course1);
                student.Enroll(course, Enum.Parse<Grade>(dto.Course1Grade));
            }

            if (dto.Course2 != null && dto.Course2Grade != null)
            {
                Course course = _courseRepository.GetByName(dto.Course2);
                student.Enroll(course, Enum.Parse<Grade>(dto.Course2Grade));
            }

            _studentRepository.Save(student);
            _unitOfWork.Commit();

            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult Unregister(long id)
        {
            Student student = _studentRepository.GetById(id);
            if (student == null)
                return Error($"No student found for Id {id}");

            _studentRepository.Delete(student);
            _unitOfWork.Commit();

            return Ok();
        }

        [HttpPost("{id}/enrollments")]
        public IActionResult Enroll(long id, [FromBody] StudentEnrollmentDto dto)
        {
            // check student exist
            Student student = _studentRepository.GetById(id);
            if (student == null)
                return Error($"No students found for id: '{id}'");
            // Check course exists
            Course cource = _courseRepository.GetByName(dto.Course);
            if (cource == null)
                return Error($"cource is incorrct: '{dto.Course}'");
            // check if Grade is Correct Enum
            bool success = Enum.TryParse(dto.Grade, out Grade grade);

            if (!success)
                return Error($"Grade is incorrect: '{dto.Grade}'");

            // enroll // registrera course on student.
            student.Enroll(cource, grade);
            _unitOfWork.Commit();

            return Ok();
        }

        [HttpPut("{id}/enrollments/{enrollmentNumber}")]
        public IActionResult Transfer(long id, int enrollmentNumber, [FromBody] StudentTransferDto dto)
        {
            // check student exist
            Student student = _studentRepository.GetById(id);
            if (student == null)
                return Error($"No students found for id: '{id}'");
            // Check course exists

            Course course = _courseRepository.GetByName(dto.Course);
            if (course == null)
                return Error($"cource is incorrct: '{dto.Course}'");
            // check if Grade is Correct Enum
            bool success = Enum.TryParse(dto.Grade, out Grade grade);

            if (!success)
                return Error($"Grade is incorrect: '{dto.Grade}'");

            var enrollment = student.GetEnrollment(enrollmentNumber);
            if (enrollment == null)
                return Error($"No Enrollment found with number: '{enrollment}'");
            Enrollment firstEnrollment = student.FirstEnrollment;

            firstEnrollment.Update(course, grade);
            _unitOfWork.Commit();

            return Ok();
        }

        [HttpPut("{id}/enrollments/{enrollmentNumber}/deletion")]
        public IActionResult Disenroll(long id, int enrollmentNumber, [FromBody] StudentDisenrollmentDto dto)
        {
            // check student exist
            Student student = _studentRepository.GetById(id);
            if (student == null)
                return Error($"No students found for id: '{id}'");
            // Check course exists

            if (string.IsNullOrEmpty(dto.comment))
                return Error($"Disenrollemnt comment is required!");

            var enrollment = student.GetEnrollment(enrollmentNumber);
            if (enrollment == null)
                return Error($"No Enrollment found with number: '{enrollment}'");
           

            student.RemoveEnrollment(enrollment,dto.comment);
           
            _unitOfWork.Commit();

            return Ok();
        }

        [HttpPut("{id}/")]
        public IActionResult EditPersonalInfo(long id, [FromBody] StudentPersonalInfoDto dto)
        {
            // check student exist
            Student student = _studentRepository.GetById(id);
            if (student == null)
                return Error($"No students found for id: '{id}'");


            student.Name = dto.Name;
            student.Email = dto.Email;


            _unitOfWork.Commit();

            return Ok();

        }

    }
}
