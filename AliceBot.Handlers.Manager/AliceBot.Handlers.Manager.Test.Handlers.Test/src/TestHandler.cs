using System.Threading;
using System.Threading.Tasks;
using AliceBot.Core;
using AliceBot.Core.Events.Data;
using AliceBot.Core.Handlers;
using AliceBot.Core.Messages;
using AliceBot.Core.Messages.Segments;
using AliceBot.Core.Protocols;

namespace AliceBot.Handlers.Manager.Test.Handlers.Test;

public class TestHandler(Alice alice) : IHandler {
    private readonly Alice _alice = alice;

    public static TestHandler Create(Alice alice) {
        return new(alice);
    }

    private static bool IsMatchTest(MessageContent content) {
        return content.Count == 1 && content[0] is TextSegment segment && segment.Text == "#test";
    }

    private void OnGroupMessageHandler(IProtocol protocol, GroupMessageData data) {
        GroupMessage message = data.Message;

        if (IsMatchTest(message.Content)) {
            protocol.Actions.SendGroupMessageAsync(
                message.GroupId,
                new MessageContent.Builder()
                    .Reply(message.MessageId)
                    .Text("Hi~ ")
                    .At(message.UserId)
                    .Text("I'm Alice")
                    .Build(),
                CancellationToken.None
            );
        }
    }

    public Task StartAsync(CancellationToken token) {
        _alice.Events.OnGroupMessage += OnGroupMessageHandler;

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken token) {
        _alice.Events.OnGroupMessage -= OnGroupMessageHandler;

        return Task.CompletedTask;
    }
}