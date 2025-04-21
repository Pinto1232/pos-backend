using System;

namespace PosBackend.Models
{
    public enum ScopeType
    {
        Global = 0,
        Store = 1,
        Terminal = 2
    }

    public class Scope
    {
        public Scope()
        {
        }
        public Scope(ScopeType type)
        { Type = type; }
        public int Id { get; set; }
        public ScopeType Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}