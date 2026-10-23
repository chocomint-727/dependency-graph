using Godot;
using Godot.Collections;
using System;
using System.Linq;

public static class GodotArrayExtensions
{
    public static void RemoveWhere<[MustBeVariant] T>(this Array<T> array, Func<T, bool> predicate) where T : class
    {
        var toRemove = array.Where(predicate).ToList();
        foreach (var item in toRemove)
        {
            array.Remove(item);
        }
    }
}