using CSharpFunctionalExtensions;
using Logic.Students;
using Logic.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Logic.Students
{

    public interface ICommand
    {

    }


    public sealed class EditPersonalInfoCommand:ICommand
    {

        public long Id { get; set; }
        public string Name { get; set; }
        public String Email { get; set; } 
    }


    public interface ICommandHandler<TCommand> where TCommand:ICommand
    {
        Result Handle(TCommand command);
    }

    public sealed class EditPersonalInfoCommandHandler:ICommandHandler<EditPersonalInfoCommand>
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

            if (student    == null)
                return Result.Fail($"No students found for id: '{command.Id}'");


            student.Name = command.Name;
            student.Email = command.Email;


            _unitOfWork.Commit();
            return Result.Ok();

        }
    }
}
