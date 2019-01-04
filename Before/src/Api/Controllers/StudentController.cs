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
                name: dto.Name,
                email: dto.Email,
                course1: dto.Course1,
                course1Grade: dto.Course1Grade,
                course2: dto.Course2,
                course2Grade: dto.Course2Grade
                );

            Result result = _messages.Dispatch(command);
            return result.IsSuccess ? Ok() : Error(result.Error);
        }

        // refactor done
        [HttpDelete("{id}")]// command,  it mutate the internal state, changing the state of the resource.
        public IActionResult Unregister(long id)
        {
            Result result = _messages.Dispatch(new UnRegisterCommand(id));
            return result.IsSuccess ? Ok() : Error(result.Error);
        }

        // refactor done 
        [HttpPost("{id}/enrollments")] // command, change state of the resource
        public IActionResult Enroll(long id, [FromBody] StudentEnrollmentDto dto)
        {
            var command = new EnrollCommand(id, dto.Course, dto.Grade);
            Result result = _messages.Dispatch(command);
            return result.IsSuccess ? Ok() : Error(result.Error);


        }

        // refactor done.
        [HttpPut("{id}/enrollments/{enrollmentNumber}")] // command, change state of the resource
        public IActionResult Transfer(long id, int enrollmentNumber, [FromBody] StudentTransferDto dto)
        {
            var command = new TransferCommand(id, enrollmentNumber, dto.Course, dto.Grade);
            Result result = _messages.Dispatch(command);
            return result.IsSuccess ? Ok() : Error(result.Error);
        }

        // Refactor done 
        [HttpPut("{id}/enrollments/{enrollmentNumber}/deletion")] // command, change state of the resource
        public IActionResult Disenroll(long id, int enrollmentNumber, [FromBody] StudentDisenrollmentDto dto)
        {

            var command = new DisenrollmentCommand(id, enrollmentNumber, dto.Comment);
            Result result = _messages.Dispatch(command);
            return result.IsSuccess ? Ok() : Error(result.Error);

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
