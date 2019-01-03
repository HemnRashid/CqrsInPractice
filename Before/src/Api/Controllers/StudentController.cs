using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Dtos;
using CSharpFunctionalExtensions;
using Logic.Students;
using Logic.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Logic.Controllers
{
    [Route("api/students")]
    public sealed class StudentController : BaseController
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly StudentRepository _studentRepository;
        private readonly CourseRepository _courseRepository;
        private readonly Messages _messages;

        public StudentController(UnitOfWork unitOfWork, Messages messages)
        {
            _unitOfWork = unitOfWork;
            _studentRepository = new StudentRepository(unitOfWork);
            _courseRepository = new CourseRepository(unitOfWork);
            _messages = messages;

        }

        // refactor done
        [HttpGet] // query, no change state of the resource.
        public IActionResult GetList(string enrolled, int? number)
        {
            //IReadOnlyList<Student> students = _studentRepository.GetList(enrolled, number);
            //List<StudentDto> dtos = students.Select(x => ConvertToDto(x)).ToList();

            List<StudentDto> list = _messages.Dispatch(new GetListQuery(enrolled, number));
            return Ok(list);


        }

        // refactor done.
        [HttpPost] // command, change/ mutate state of the resource
        public IActionResult Register([FromBody] NewStudentDto dto)
        {
            var command = new RegisterCommand
                (
                dto.Name, dto.Email, dto.Course1, dto.Course1Grade, dto.Course2, dto.Course2Grade
                );

            Result result = _messages.Dispatch(command);
            return result.IsSuccess ? Ok() : Error(result.Error);
        }

        // refactor done
        [HttpDelete("{id}")]// command,  it mutate the internal state, changing the state of the resource.
        public IActionResult Unregister(long id)
        {
            var command = new UnRegisterCommand(id);

            Result result = _messages.Dispatch(command);

            return result.IsSuccess ? Ok() : Error(result.Error);
        }

        [HttpPost("{id}/enrollments")] // command, change state of the resource
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

        [HttpPut("{id}/enrollments/{enrollmentNumber}")] // command, change state of the resource
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
        
        [HttpPut("{id}/enrollments/{enrollmentNumber}/deletion")] // command, change state of the resource
        public IActionResult Disenroll(long id, int enrollmentNumber, [FromBody] StudentDisenrollmentDto dto)
        {
            // check student exist
            Student student = _studentRepository.GetById(id);
            if (student == null)
                return Error($"No students found for id: '{id}'");
            // Check course exists

            if (string.IsNullOrEmpty(dto.Comment))
                return Error($"Disenrollemnt comment is required!");

            var enrollment = student.GetEnrollment(enrollmentNumber);
            if (enrollment == null)
                return Error($"No Enrollment found with number: '{enrollment}'");


            student.RemoveEnrollment(enrollment, dto.Comment);

            _unitOfWork.Commit();

            return Ok();
        }

        // refactor done 
        [HttpPut("{id}/")] // command, change state of the resource
        public IActionResult EditPersonalInfo(long id, [FromBody] StudentPersonalInfoDto dto)
        {

            //var handlder = new EditPersonalInfoCommandHandler(_unitOfWork);
            //var result = handlder.Handle(command);

            var command = new EditPersonalInfoCommand(id, dto.Name, dto.Email);
            Result result = _messages.Dispatch(command);
            return result.IsSuccess ? Ok() : Error(result.Error);

        }

    }
}
