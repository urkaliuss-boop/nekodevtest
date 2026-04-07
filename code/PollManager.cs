using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Exiled.API.Features;

namespace VotingPlugin
{
    public class PollManager
    {
        public bool IsActive { get; private set; }
        public string Question { get; private set; }
        public List<string> Options { get; private set; }

        private Dictionary<string, int> _votes;
        private Timer _timer;

        public PollManager()
        {
            IsActive = false;
            _votes = new Dictionary<string, int>();
            Options = new List<string>();
        }

        public bool CreatePoll(string question, int durationSeconds, List<string> options, out string error)
        {
            if (IsActive)
            {
                error = "Голосование уже активно! Дождитесь завершения текущего голосования.";
                return false;
            }

            if (options == null || options.Count < 2)
            {
                error = "Необходимо указать минимум 2 варианта ответа.";
                return false;
            }

            if (durationSeconds <= 0)
            {
                error = "Время голосования должно быть больше 0 секунд.";
                return false;
            }

            Question = question;
            Options = new List<string>(options);
            _votes = new Dictionary<string, int>();
            IsActive = true;

            var sb = new StringBuilder();
            sb.AppendLine($"<b><size=28>📊 ГОЛОСОВАНИЕ</size></b>");
            sb.AppendLine($"<size=24>{Question}</size>");
            sb.AppendLine();
            sb.AppendLine($"<size=20>Для голосования введите в консоль .answer и вариант</size>");
            sb.AppendLine($"<size=18>Время на голосование: {durationSeconds} сек.</size>");

            Map.Broadcast((ushort)(durationSeconds > 15 ? 15 : durationSeconds), sb.ToString());

            _timer = new Timer(_ =>
            {
                EndPoll();
            }, null, durationSeconds * 1000, Timeout.Infinite);

            error = null;
            return true;
        }

        public bool Vote(string oderId, int optionNumber, out string message)
        {
            if (!IsActive)
            {
                message = "В данный момент нет активного голосования.";
                return false;
            }

            if (optionNumber < 1 || optionNumber > Options.Count)
            {
                message = $"Некорректный номер варианта. Допустимые значения: 1-{Options.Count}.";
                return false;
            }

            if (_votes.ContainsKey(oderId))
            {
                message = "Вы уже проголосовали! Повторное голосование невозможно.";
                return false;
            }

            _votes[oderId] = optionNumber - 1;
            message = $"Ваш голос за вариант №{optionNumber} (\"{Options[optionNumber - 1]}\") принят.";
            return true;
        }

        public void EndPoll()
        {
            if (!IsActive)
                return;

            IsActive = false;

            _timer?.Dispose();
            _timer = null;

            int totalVotes = _votes.Count;
            var results = new int[Options.Count];
            foreach (var vote in _votes.Values)
            {
                results[vote]++;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"=== Результаты голосования ===");
            sb.AppendLine($"Вопрос: {Question}");
            sb.AppendLine($"Всего голосов: {totalVotes}");
            sb.AppendLine("---");

            for (int i = 0; i < Options.Count; i++)
            {
                double percent = totalVotes > 0 ? (results[i] * 100.0 / totalVotes) : 0;
                sb.AppendLine($"[{i + 1}] {Options[i]}: {results[i]} голос(ов) ({percent:F1}%)");
            }

            int maxVotes = results.Max();
            if (totalVotes > 0)
            {
                var winners = new List<string>();
                for (int i = 0; i < Options.Count; i++)
                {
                    if (results[i] == maxVotes)
                        winners.Add($"[{i + 1}] {Options[i]}");
                }
                sb.AppendLine("---");
                if (winners.Count == 1)
                    sb.AppendLine($"Победитель: {winners[0]} ({maxVotes} голос(ов))");
                else
                    sb.AppendLine($"Ничья между: {string.Join(", ", winners)} ({maxVotes} голос(ов))");
            }

            string resultText = sb.ToString();

            Log.Info(resultText);

            Map.Broadcast(10, "<b><size=28>📊 Голосование завершено!</size></b>\n<size=22>Спасибо за участие в голосовании!</size>");

            foreach (var player in Player.List)
            {
                if (player.RemoteAdminAccess)
                {
                    player.RemoteAdminMessage(resultText);
                }
            }
        }

        public void Cleanup()
        {
            _timer?.Dispose();
            _timer = null;
            IsActive = false;
            _votes?.Clear();
        }
    }
}
