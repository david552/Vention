namespace Vention.Domain.Common
{
    public abstract class Entity<TId> where TId : notnull
    {
        public TId Id { get; protected set; } = default!;

        protected Entity() { }

        protected Entity(TId id)
        {
            Id = id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Entity<TId> other) return false;
            if (ReferenceEquals(this, other)) return true;
            if (GetType() != other.GetType()) return false;
            return Id.Equals(other.Id);
        }

        public override int GetHashCode() => Id.GetHashCode();

        public static bool operator ==(Entity<TId>? a, Entity<TId>? b)
            => a is null && b is null || (a is not null && a.Equals(b));

        public static bool operator !=(Entity<TId>? a, Entity<TId>? b) => !(a == b);
    }
}
