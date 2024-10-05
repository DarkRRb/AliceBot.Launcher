using System;
using System.Collections.Generic;

namespace AliceBot.Core.Messages.Segments;

public class ForwardSegment(IReadOnlyList<ForwardSegment.ForwardMessage> messages) : ISegment {
    public IReadOnlyList<ForwardMessage> Messages { get; } = messages;

    public class ForwardMessage {
        public DateTimeOffset Time { get; }

        public string UserId { get; }

        public string Avatar { get; }

        public string Name { get; }

        public MessageContent Content { get; }

        private ForwardMessage(DateTimeOffset time, string userId, string avatar, string name, MessageContent content) {
            Time = time;
            UserId = userId;
            Avatar = avatar;
            Name = name;
            Content = content;
        }

        public class Builder {
            private DateTimeOffset? _time;

            private string? _userId;

            private string? _avatar;

            private string? _name;

            public Builder SetTime(DateTimeOffset time) {
                _time = time;
                return this;
            }

            public Builder SetUserId(string userId) {
                _userId = userId;
                return this;
            }

            public Builder SetAvatar(string avatar) {
                _avatar = avatar;
                return this;
            }

            public Builder SetName(string name) {
                _name = name;
                return this;
            }

            public ForwardMessage Build(MessageContent content) {
                return new ForwardMessage(
                    _time ?? DateTimeOffset.Now,
                    _userId ?? "",
                    _avatar ?? "",
                    _name ?? "",
                    content
                );
            }
        }
    }
}