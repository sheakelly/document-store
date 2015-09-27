using System;

namespace DocumentStore.Tests
{
    public class Document
    {
        public string Id { get; set; }
        public string Data { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}