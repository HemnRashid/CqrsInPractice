using CSharpFunctionalExtensions;
using Logic.Dtos;
using Logic.Students;
using Logic.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Logic.Students
{


    public interface IQuery<TResult> { }
    public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
    {
        TResult Handle(TQuery query);
    }

    public sealed class GetListQuery : IQuery<List<StudentDto>>
    {
        public string EnrolledIn { get; }
        public int? NumberOfCourses { get; }

        public GetListQuery(string enrolledIn, int? numberOfNumbers)
        {
            EnrolledIn = enrolledIn;
            NumberOfCourses = numberOfNumbers;
        }
    }
    public sealed class GetListQueryHandler : IQueryHandler<GetListQuery, List<StudentDto>>
    {
        private readonly UnitOfWork _unitOfWork;

        public GetListQueryHandler(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<StudentDto> Handle(GetListQuery query)
        {

            // not inline.
            //var repository = new StudentRepository(_unitOfWork);
            //IReadOnlyList<Student> students = repository.GetList(query.EnrolledIn, query.NumberOfCourses);
            //List<StudentDto> dtos = students.Select(x => ConvertToDto(x)).ToList();
            //return dtos;

            // inline
            return new StudentRepository(_unitOfWork)
                .GetList(query.EnrolledIn, query.NumberOfCourses)
                .Select(x => ConvertToDto(x))
                .ToList();


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
    }




    public interface ICommand { }
    public interface ICommandHandler<TCommand> where TCommand : ICommand
    {
        Result Handle(TCommand command);
    }
    public sealed class EditPersonalInfoCommand : ICommand
    {

        public long Id { get; }
        public string Name { get; }
        public string Email { get; }


        public EditPersonalInfoCommand(long id, string name, string email)
        {
            Id = id;
            Name = name;
            Email = email;
        }



    }
    public sealed class EditPersonalInfoCommandHandler : ICommandHandler<EditPersonalInfoCommand>
    {
        private readonly UnitOfWork _unitOfWork;

        public EditPersonalInfoCommandHandler(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Result Handle(EditPersonalInfoCommand command)
        {

            // check student exist
            var repository = new StudentRepository(_unitOfWork);
            Student student = repository.GetById(command.Id);

            if (student == null)
                return Result.Fail($"No students found for id: '{command.Id}'");


            student.Name = command.Name;
            student.Email = command.Email;


            _unitOfWork.Commit();
            return Result.Ok();

        }
    }



    public sealed class RegisterCommand : ICommand
    {
        public string Name { get; }
        public string Email { get; }
        public string Course1 { get; }
        public string Course1Grade { get; }
        public string Course2 { get; }
        public string Course2Grade { get; }


        public RegisterCommand(string name, string email, string course1, string course1Grade, string course2, string course2Grade)
        {
            Name = name;
            Email = email;
            Course1 = course1;
            Course1Grade = Course1Grade;
            Course2 = course2;
            Course2Grade = course2Grade;


        }

    }

    public sealed class RegisterCommandHandler : ICommandHandler<RegisterCommand>
    {
        private readonly UnitOfWork _unitOfWork;

        public RegisterCommandHandler(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Result Handle(RegisterCommand command)
        {

            // check student exist

            var studentRepository = new StudentRepository(_unitOfWork);
            var courseRepository = new CourseRepository(_unitOfWork);

            var student = new Student(command.Name, command.Email);

            if (command.Course1 != null && command.Course1Grade != null)
            {
                Course course = courseRepository.GetByName(command.Course1);
                student.Enroll(course, Enum.Parse<Grade>(command.Course1Grade));
            }

            if (command.Course2 != null && command.Course2Grade != null)
            {
                Course course = courseRepository.GetByName(command.Course2);
                student.Enroll(course, Enum.Parse<Grade>(command.Course2Grade));
            }

            studentRepository.Save(student);
            _unitOfWork.Commit();
            return Result.Ok();

        }

    }


    public sealed class UnRegisterCommand : ICommand
    {

        public long Id { get; }

        public UnRegisterCommand(long id)
        {
            Id = id;
        }
    }

    public sealed class UnRegisterCommandHandler : ICommandHandler<UnRegisterCommand>
    {

        private readonly UnitOfWork _unitOfWork;
        public UnRegisterCommandHandler(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public Result Handle(UnRegisterCommand command)
        {

            var studentRepository = new StudentRepository(_unitOfWork);
            Student student = studentRepository.GetById(command.Id);
            if (student == null)
                return Result.Fail($"No student found for Id {command.Id}");

            studentRepository.Delete(student);
            _unitOfWork.Commit();

            return Result.Ok();

        }


    }


    public sealed class EnrollCommand : ICommand
    {
        public long Id { get; }
        public string Course { get; }

        public string Grade { get; }



        public EnrollCommand(long id, string course, string grade)
        {
            Id = id;
            Course = course;
            Grade = grade;
        }
    }


    public sealed class EnrolledCommandHandler : ICommandHandler<EnrollCommand>
    {
        private readonly UnitOfWork _unitOfWork;

        public EnrolledCommandHandler(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Result Handle(EnrollCommand command)
        {

            var studentRepository = new StudentRepository(_unitOfWork);
            var courseRepository = new CourseRepository(_unitOfWork);
            // check student exist
            Student student = studentRepository.GetById(command.Id);
            if (student == null)
                return Result.Fail($"No students found for id: '{command.Id}'");
            // Check course exists
            Course cource = courseRepository.GetByName(command.Course);
            if (cource == null)
                return Result.Fail($"cource is incorrct: '{command.Course}'");
            // check if Grade is Correct Enum
            bool success = Enum.TryParse(command.Grade, out Grade grade);

            if (!success)
                return Result.Fail($"Grade is incorrect: '{command.Grade}'");

            // enroll // registrera course on student.
            student.Enroll(cource, grade);
            _unitOfWork.Commit();

            return Result.Ok();

        }
    }


    public sealed class DisenrollmentCommand : ICommand
    {
        public long Id { get; }
        public int EnrollmentNumber { get; }
        public string Comment { get; }


        public DisenrollmentCommand(long id, int enrollmentNumber, string comment)
        {
            Id = id;
            EnrollmentNumber = enrollmentNumber;
            Comment = comment;
        }
    }
    public sealed class DisenrollmentCommandHandler : ICommandHandler<DisenrollmentCommand>
    {

        private readonly UnitOfWork _unitOfWork;

        public DisenrollmentCommandHandler(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Result Handle(DisenrollmentCommand command)
        {

            var studentRepository = new StudentRepository(_unitOfWork);

            // check student exist
            Student student = studentRepository.GetById(command.Id);
            if (student == null)
                return Result.Fail($"No students found for id: '{command.Id}'");
            // Check course exists

            if (string.IsNullOrEmpty(command.Comment))
                return Result.Fail($"Disenrollemnt comment is required!");

            var enrollment = student.GetEnrollment(command.EnrollmentNumber);
            if (enrollment == null)
                return Result.Fail($"No Enrollment found with number: '{enrollment}'");


            student.RemoveEnrollment(enrollment, command.Comment);
            _unitOfWork.Commit();

            return Result.Ok();

        }
    }


    public sealed class TransferCommand : ICommand
    {
        public long Id { get; }
        public int EnrollmentNumber { get; }

        public string Course { get; }

        public string Grade { get; }


        public TransferCommand(long id, int enrollmentNumber, string course, string grade)
        {
            Id = id;
            EnrollmentNumber = enrollmentNumber;
            Course = course;
            Grade = grade;
        }
    }

    public sealed class TransferCommandHandler : ICommandHandler<TransferCommand>
    {
        private readonly UnitOfWork _unitOfWork;

        public TransferCommandHandler(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Result Handle(TransferCommand command)
        {

            var studentRepository = new StudentRepository(_unitOfWork);
            var courseRepository = new CourseRepository(_unitOfWork);

            // check student exist
            Student student = studentRepository.GetById(command.Id);
            if (student == null)
                return Result.Fail($"No students found for id: '{command.Id}'");
            // Check course exists

            Course course = courseRepository.GetByName(command.Course);
            if (course == null)
                return Result.Fail($"cource is incorrct: '{command.Course}'");
            // check if Grade is Correct Enum
            bool success = Enum.TryParse(command.Grade, out Grade grade);

            if (!success)
                return Result.Fail($"Grade is incorrect: '{command.Grade}'");

            var enrollment = student.GetEnrollment(command.EnrollmentNumber);
            if (enrollment == null)
                return Result.Fail($"No Enrollment found with number: '{enrollment}'");

            Enrollment firstEnrollment = student.FirstEnrollment;

            firstEnrollment.Update(course, grade);
            _unitOfWork.Commit();

            return Result.Ok();
        }
    }
}
