using System.Collections.Generic;
using Flunt.Notifications;
using Newtonsoft.Json;

namespace SC.SDK.NetStandard.DomainCore.Commands
{
    public class CommandResult
    {
        public bool Success { get; private set; }
        public string Message { get; private set; }
        public CommandResultError Reason { get; private set; }
        public string ResourceId { get; private set; }
        public CommandResultResourceAction ResourceAction { get; private set; }

        private CommandResult(bool success)
        {
            Success = success;
        }

        private CommandResult(string resourceId, CommandResultResourceAction action)
            : this(true)
        {
            ResourceId = resourceId;
            ResourceAction = action;
        }

        private CommandResult(CommandResultError reason, string message = null)
            : this(false)
        {
            Message = message;
            Reason = reason;
        }

        public static CommandResult Ok() => new CommandResult(true);
        public static CommandResult Ok(string resourceId, CommandResultResourceAction action) => new CommandResult(resourceId, action);

        public static CommandResult Error(string message) =>
            new CommandResult(CommandResultError.BusinessException, message);

        public static CommandResult Error(IEnumerable<Notification> notifications)
        {
            var messages = JsonConvert.SerializeObject(notifications, Formatting.Indented);
            return new CommandResult(CommandResultError.BusinessException, messages);
        }

        public static CommandResult NotFound(string resourceName, string resourceId)
        {
            var message = $"Recurso '{resourceName}' ID '{resourceId}' não encontrado";
            return new CommandResult(CommandResultError.ResourceNotFound, message);
        }
    }

    public enum CommandResultError
    {
        BusinessException = 1,
        ResourceNotFound = 2
    }

    public enum CommandResultResourceAction
    {
        Created = 1,
        Updated = 2,
        Deleted = 3
    }
}