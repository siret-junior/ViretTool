using System;
using System.Collections.Generic;
using System.Linq;

namespace ViretTool.Core
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property)
        {
            return items.GroupBy(property).Select(x => x.First());
        }

        public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
        }

        public static double AverageOrZero<TSource>(this IEnumerable<TSource> source, Func<TSource, double> valueSelector)
        {
            var sourceToList = source as IList<TSource> ?? source.ToList();
            if (!sourceToList.Any())
            {
                return 0;
            }

            return sourceToList.Average(valueSelector);
        }

        public static IEnumerable<TElement> Append<TElement>(this IEnumerable<TElement> collection, TElement item)
        {
            foreach (var element in collection)
            {
                yield return element;
            }
            yield return item;
        }

        public static IList<TElement> AsList<TElement>(this IEnumerable<TElement> source)
        {
            return source as IList<TElement> ?? source.ToList();
        }

        public static TElement[] AsArray<TElement>(this IEnumerable<TElement> source)
        {
            return source as TElement[] ?? source.ToArray();
        }

        public static IEnumerable<TSource> NotNull<TSource>(this IEnumerable<TSource> items) where TSource : class
        {
            return items.Where(x => x != null);
        }

        public static IEnumerable<TSource> NotNull<TSource>(this IEnumerable<TSource?> items) where TSource : struct
        {
            return items.Where(x => x != null).Select(x => x.Value);
        }

        public static Dictionary<TKey, TElement> ToDictionary<TKey, TElement, TSource>(
          this IEnumerable<TSource> source,
          Func<TSource, int, TKey> keySelector,
          Func<TSource, int, TElement> elementSelector)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }
            if (elementSelector == null)
            {
                throw new ArgumentNullException("elementSelector");
            }

            Dictionary<TKey, TElement> dictionary = new Dictionary<TKey, TElement>();
            int index = 0;
            foreach (var item in source)
            {
                dictionary.Add(keySelector(item, index), elementSelector(item, index));
                index++;
            }
            return dictionary;
        }

        public static Dictionary<TKey, TElement> ToDictionary<TKey, TElement>(this IEnumerable<KeyValuePair<TKey, TElement>> source)
        {
            return source.ToDictionary(s => s.Key, s => s.Value);
        }

        public static IEnumerable<T> ToEnumerable<T>(this T item)
        {
            yield return item;
        }

        /// <summary>
        /// Porovná dvě sekvence, jestli obsahují stejné prvky ve stejném pořadí za pomoci zadané funkce.
        /// </summary>
        public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TSource, bool> comparer)
        {
            if (comparer == null)
                throw new ArgumentException("comparer");
            if (first == null)
                throw new ArgumentException("first");
            if (second == null)
                throw new ArgumentException("second");

            using (IEnumerator<TSource> enumerator1 = first.GetEnumerator())
            {
                using (IEnumerator<TSource> enumerator2 = second.GetEnumerator())
                {
                    while (enumerator1.MoveNext())
                    {
                        if (!enumerator2.MoveNext() || !comparer(enumerator1.Current, enumerator2.Current))
                            return false;
                    }
                    if (enumerator2.MoveNext())
                        return false;
                }
            }
            return true;
        }

        public static bool? AllOrNone<TSource>(this IEnumerable<TSource> source, Predicate<TSource> predicate)
        {
            if (source == null)
                throw new ArgumentException("source");

            var t = source.Select(i => predicate(i)).Distinct().ToList();
            switch (t.Count)
            {
                case 1:
                    return t[0];
                case 2:
                    return null;
                default:
                    throw new InvalidOperationException();
            }
        }

        public static HashSet<TSource> ToHashSet<TSource>(this IEnumerable<TSource> source)
        {
            return new HashSet<TSource>(source);
        }

        public static bool ContainsAny<T>(this ICollection<T> hashSet, IList<T> list)
        {
            return list.Any(hashSet.Contains);
        }
    }
}
