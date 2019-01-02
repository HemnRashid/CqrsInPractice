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
            Type handlerType = type.MakeGenericType(typeArgs);

            dynamic handler = _provider.GetService(handlerType);
            Result result = handler.Handle((dynamic)command);

            return result;
        }
    }
}
