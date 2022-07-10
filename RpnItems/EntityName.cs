namespace Lang.RpnItems
{
    /// <summary>
    /// Name of the program entity (e.g., variable or built-in).
    /// </summary>
    public struct EntityName
    {
        public EntityName(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Value of the name.
        /// </summary>
        public string Value { get; }

        public static bool operator ==(EntityName n1, EntityName n2)
            => n1.Value == n2.Value;

        public static bool operator !=(EntityName n1, EntityName n2)
            => n1.Value != n2.Value;

        public override bool Equals(object obj)
            => obj is EntityName en && en.Value == this.Value;

        public override int GetHashCode()
            => Value.GetHashCode();

        public override string ToString()
            => Value;
    }
}