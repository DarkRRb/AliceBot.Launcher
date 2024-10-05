using System.Collections;
using System.Collections.Generic;
using AliceBot.Core.Messages.Segments;

namespace AliceBot.Core.Messages;

public class MessageContent : IReadOnlyList<ISegment> {
    private readonly IReadOnlyList<ISegment> _segments;

    public int Count => _segments.Count;

    public ISegment this[int index] => _segments[index];

    private MessageContent(IReadOnlyList<ISegment> segments) {
        _segments = segments;
    }

    public IEnumerator<ISegment> GetEnumerator() {
        return _segments.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return ((IEnumerable)_segments).GetEnumerator();
    }

    public class Builder() {
        private readonly List<ISegment> _segments = [];

        private Type _type = Type.RichText;

        public Builder At(string userId) {
            RichTextPreprocess();

            _segments.Add(new AtSegment(userId));
            return this;
        }

        public Builder Emoji(string emojiId) {
            RichTextPreprocess();

            _segments.Add(new EmojiSegment(emojiId));
            return this;
        }

        public Builder Forward(IReadOnlyList<ForwardSegment.ForwardMessage> messages) {
            SignlePreprocess();

            _segments.Add(new ForwardSegment(messages));
            return this;
        }

        public Builder Image(string url) {
            RichTextPreprocess();

            _segments.Add(new ImageSegment(url));
            return this;
        }

        public Builder Reply(string messageId) {
            RichTextPreprocess();

            ReplySegment segment = new(messageId);

            if (_segments.Count > 0 && _segments[0] is ReplySegment) _segments[0] = segment;
            else _segments.Insert(0, segment);

            return this;
        }

        public Builder Text(string text) {
            RichTextPreprocess();

            _segments.Add(new TextSegment(text));
            return this;
        }

        public MessageContent Build() {
            return new(_segments);
        }

        private void RichTextPreprocess() {
            if (_type != Type.RichText) {
                _segments.Clear();
                _type = Type.RichText;
            }
        }

        private void SignlePreprocess() {
            _segments.Clear();
            _type = Type.Single;
        }

        private enum Type {
            RichText,
            Single,
        }
    }
}