using System;

public interface IGun
{
    void Shoot();
    void Reload();
    int CurrentAmmo { get; }
    int MaxAmmo { get; }
    event Action<int, int> OnAmmoChange;
}
