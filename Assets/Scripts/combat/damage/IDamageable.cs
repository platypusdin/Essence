public interface IDamageable
{
    bool IsDead { get; }

    int TakeDamage(int amount, bool processStatuses = true);
}