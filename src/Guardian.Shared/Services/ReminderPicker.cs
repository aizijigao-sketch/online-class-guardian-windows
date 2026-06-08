using System.Security.Cryptography;
using Guardian.Shared.Models;

namespace Guardian.Shared.Services;

public sealed class ReminderPicker
{
    private const string Fallback = "现在是网课时间，先把注意力交给课堂。";

    public string Pick(GuardianConfig config)
    {
        var messages = config.Notification.Messages.Where(m => !string.IsNullOrWhiteSpace(m)).ToList();
        if (messages.Count == 0)
        {
            return Fallback;
        }

        return messages[RandomNumberGenerator.GetInt32(messages.Count)];
    }
}
