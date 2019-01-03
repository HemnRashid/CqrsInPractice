using CSharpFunctionalExtensions;
using System;


namespace Logic.Students
{
    public sealed class Messages
    {

        private readonly IServiceProvider _provider;

        public Messages(IServiceProvider provider)
        {
            _provider = provider;
        }

        // dispatch är avsändare, motsats är mottagare.
        public Result Dispatch(ICommand command) // avsändre
        {
            Type type = typeof(ICommandHandler<>);
            Type[] typeArgs = { command.GetType() };
            Type handlerType = type.MakeGenericType(typeArgs); // handlerType contains the" ICommandHandler<EditPersonalInfoCommand>

            dynamic handler = _provider.GetService(handlerType);
            Result result = handler.Handle((dynamic)command);

            return result;
        }

        public T Dispatch<T>(IQuery<T> query) // avsändre
        {
            Type type = typeof(IQueryHandler<,>);
            Type[] typeArgs = { query.GetType(),typeof(T)};
            Type handlerType = type.MakeGenericType(typeArgs); // handlerType contains the" ICommandHandler<EditPersonalInfoCommand>

            dynamic handler = _provider.GetService(handlerType);
            T result = handler.Handle((dynamic)query);

            return result;
        }
    }
}
