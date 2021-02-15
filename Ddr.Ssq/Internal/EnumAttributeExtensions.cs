using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Ddr.Ssq.Internal
{
    internal static class EnumAttributeExtensions
    {
        public static bool HasAttribute<T>(this Enum Enum) where T : Attribute
            => Enum.GetAttribute<T>() is not null;
        public static T? GetAttribute<T>(this Enum Enum, bool ThrowNotFoundFiled = true) where T : Attribute
        {
            var field = Enum.GetType().GetField(Enum.ToString());
            if (field is null)
            {
                if (ThrowNotFoundFiled)
                    throw new InvalidOperationException($"not found filed: {Enum}");
                return null;
            }
            return field.GetCustomAttribute<T>();
        }

        public static bool TryGetAttribute<T>(this Enum Enum, [MaybeNullWhen(false)] out T Attribute) where T : Attribute
        {
            if (Enum.GetAttribute<T>() is T Value)
            {
                Attribute = Value;
                return true;
            }
            Attribute = null;
            return false;
        }
        public static IEnumerable<T> GetAttributes<T>(this Enum Enum, bool ThrowNotFoundFiled = true) where T : Attribute
        {
            var field = Enum.GetType().GetField(Enum.ToString());
            if (field is null)
            {
                if (ThrowNotFoundFiled)
                    throw new InvalidOperationException($"not found filed: {Enum}");
                return Enumerable.Empty<T>();
            }
            return field.GetCustomAttributes<T>();
        }
    }
}
