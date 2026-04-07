using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CommandSystem;
using Exiled.Permissions.Extensions;
using RemoteAdmin;

namespace VotingPlugin.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class PollCreateCommand : ICommand
    {
        public string Command => "pollcreate";
        public string[] Aliases => new[] { "pcreate" };
        public string Description => "Создать голосование. Синтаксис: pollcreate \"<вопрос>\" <время_сек> \"<вариант1>\" \"<вариант2>\" [...]";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("poll.create"))
            {
                response = "У вас нет прав для создания голосований (poll.create).";
                return false;
            }

            string rawArgs = string.Join(" ", arguments);

            if (string.IsNullOrWhiteSpace(rawArgs))
            {
                response = "Использование: pollcreate \"<вопрос>\" <время_сек> \"<вариант1>\" \"<вариант2>\" [...]";
                return false;
            }

            var parsed = ParseQuotedArgs(rawArgs);

            if (parsed.Count < 4)
            {
                response = "Недостаточно аргументов. Нужно: \"вопрос\" время \"вариант1\" \"вариант2\" [...]";
                return false;
            }

            string question = parsed[0];

            if (!int.TryParse(parsed[1], out int duration) || duration <= 0)
            {
                response = $"Некорректное время: \"{parsed[1]}\". Укажите положительное число секунд.";
                return false;
            }

            var options = new List<string>();
            for (int i = 2; i < parsed.Count; i++)
            {
                options.Add(parsed[i]);
            }

            if (options.Count < 2)
            {
                response = "Необходимо указать минимум 2 варианта ответа.";
                return false;
            }

            var pollManager = VotingPlugin.Instance?.PollManager;
            if (pollManager == null)
            {
                response = "Ошибка: плагин не инициализирован.";
                return false;
            }

            if (!pollManager.CreatePoll(question, duration, options, out string error))
            {
                response = error;
                return false;
            }

            response = $"Голосование создано! Вопрос: \"{question}\", время: {duration} сек., вариантов: {options.Count}.";
            return true;
        }

        private List<string> ParseQuotedArgs(string input)
        {
            var result = new List<string>();
            var matches = Regex.Matches(input, @"""([^""]*)""|(\S+)");

            foreach (Match match in matches)
            {
                if (match.Groups[1].Success)
                    result.Add(match.Groups[1].Value);
                else if (match.Groups[2].Success)
                    result.Add(match.Groups[2].Value);
            }

            return result;
        }
    }
}
