using System;
using CommandSystem;

namespace VotingPlugin.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class AnswerCommand : ICommand
    {
        public string Command => "answer";
        public string[] Aliases => Array.Empty<string>();
        public string Description => "Проголосовать в активном голосовании. Использование: .answer <номер_варианта>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 1)
            {
                response = "Использование: .answer <номер_варианта>";
                return false;
            }

            if (!int.TryParse(arguments.At(0), out int optionNumber))
            {
                response = $"Некорректный номер варианта: \"{arguments.At(0)}\". Укажите число.";
                return false;
            }

            var pollManager = VotingPlugin.Instance?.PollManager;
            if (pollManager == null)
            {
                response = "Ошибка: плагин не инициализирован.";
                return false;
            }

            string oderId = null;
            if (sender is CommandSender cmdSender)
            {
                oderId = cmdSender.SenderId;
            }

            if (string.IsNullOrEmpty(oderId))
            {
                response = "Эта команда доступна только игрокам.";
                return false;
            }

            bool success = pollManager.Vote(oderId, optionNumber, out string message);
            response = message;
            return success;
        }
    }
}
