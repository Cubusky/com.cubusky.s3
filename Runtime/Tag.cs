#nullable enable
using System;

namespace Cubusky.S3
{
    [Serializable]
    public record Tag
    {
        public string? Key;
        public string? Value;

        public static implicit operator global::Amazon.S3.Model.Tag(Tag tag) => new()
        {
            Key = tag.Key ?? string.Empty,
            Value = tag.Value ?? string.Empty
        };

        public static implicit operator Tag(global::Amazon.S3.Model.Tag tag) => new()
        {
            Key = tag.Key,
            Value = tag.Value
        };
    }
}
