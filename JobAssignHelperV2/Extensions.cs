using System;
using System.Collections.Generic;
using System.Linq;

namespace JobAssignHelperV2
{
    public static class Extensions
    {
        /// <summary>
        /// Returns default(T) if key not found.
        /// </summary>
        public static T TryGetValueOrDefault<T, U>(this IReadOnlyDictionary<U, T> dictionary, U key)
        {
            T result;
            return dictionary.TryGetValue(key, out result) ? result : default(T);
        }

        /// <summary>
        /// Throws exception if key not found, saying _which_ key was not found.
        /// </summary>
        public static T TryGetValueThrowException<T, U>(this IReadOnlyDictionary<U, T> dictionary, U key)
        {
            T result;
            if (dictionary.TryGetValue(key, out result))
                return result;

            throw new KeyNotFoundException($"Dictionary does not contain key: {key}. Possible keys: {string.Join(",", dictionary.Keys)}");
        }

        /// <summary>
        /// Safe version of TryGetValue (for objects) which may return null without throwing an exception. Returns new T() if key not found.
        /// </summary>
        public static T GetDictValue<T, U>(this IReadOnlyDictionary<U, T> dictionary, U key) where T : new()
        {
            T result;
            return dictionary.TryGetValue(key, out result) ? result : new T();
        }

        /// <summary>
        /// Safe version of TryGetValue (for objects) which may return null without throwing an exception. Returns specified <see cref="customDefaultValue"/> if key not found.
        /// </summary>
        public static T GetDictValue<T, U>(this IReadOnlyDictionary<U, T> dictionary, U key, T customDefaultValue) where T : new()
        {
            T result;
            return dictionary.TryGetValue(key, out result) ? result : customDefaultValue;
        }

        public static string ListThis<T>(this IEnumerable<T> list, string listName, bool addCount = false, string separator = ", ", string wrapChars = "<>")
            => $"<b>{listName}{(addCount ? $" ({list.Count()})" : "")}</b>: {wrapChars[0]}{string.Join(separator, list)}{wrapChars[1]}";


        public static Type GetEnumeratedType(this Type type)
        {
            // provided by Array
            var elType = type.GetElementType();
            if (null != elType) return elType;

            // otherwise provided by collection
            var elTypes = type.GetGenericArguments();
            if (elTypes.Length > 0) return elTypes[0];

            // otherwise is not an 'enumerated' type
            return null;
        }

        public static IEnumerable<Type> GetAllBaseTypesFromCollection<T>(this IEnumerable<T> array)
        {
            //if (!array.IsAssignableFrom(typeof(IEnumerable<Type>)))
            //    return new List<Type>();
            if (array == null || !array.Any())
                //yield break;
                return new List<Type>();

            List<Type> typeList = new List<Type>();
            foreach (var item in array)
            {
                typeList.AddRange(item.GetType().GetBaseTypes());
                //yield return item.GetType().GetBaseTypes();
            }

            return typeList;
        }

        public static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            if (type == null)
                yield break;

            var currentType = type;
            while (currentType != null)
            {
                yield return currentType;
                currentType = currentType.BaseType;
            }
        }

    }
}