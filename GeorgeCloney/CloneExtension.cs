using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GeorgeCloney
{

    /// <summary>
    /// With thanks Stack Overflow: http://stackoverflow.com/questions/8025890/is-there-a-much-better-way-to-create-deep-and-shallow-clones-in-c/8026574#8026574
    /// </summary>
    public static class CloneExtension
    {
        private class FieldInfoComparer : IEqualityComparer<FieldInfo>
        {
            public bool Equals(FieldInfo x, FieldInfo y)
            {
                return x.DeclaringType == y.DeclaringType && x.Name == y.Name;
            }

            public int GetHashCode(FieldInfo obj)
            {
                return obj.Name.GetHashCode() ^ obj.DeclaringType.GetHashCode();
            }
        }

        public static T DeepClone<T>(this T original)
        {
            return original.GetType().IsSerializable
                ? original.DeepCloneWithSerialization()
                : original.DeepCloneWithoutSerialization();
        }

        public static T DeepCloneWithSerialization<T>(this T original)
        {
            if (!original.GetType().IsSerializable)
                throw new NotSupportedException(String.Format("Type '{0}' is not Serializable and cannot be cloned with this method.", original.GetType().Name));
            return original.Serialize().Deserialize<T>();
        }

        /// <summary>
        /// A DeepClone method for types that are not serializable.
        /// </summary>
        public static T DeepCloneWithoutSerialization<T>(this T original)
        {
            return original.deepClone(new Dictionary<Object, Object>());
        }

        static T deepClone<T>(this T original, Dictionary<Object, Object> copies)
        {
            return (T)original.deepClone(original.GetType(), copies);
        }

        public static FieldInfo[] GetFieldInfosIncludingBaseClasses(Type type, BindingFlags bindingFlags)
        {
            FieldInfo[] fieldInfos = type.GetFields(bindingFlags);

            // If this class doesn't have a base, don't waste any time
            if (type.BaseType == typeof(object))
            {
                return fieldInfos;
            }
            else
            {   // Otherwise, collect all types up to the furthest base class
                var currentType = type;
                var fieldComparer = new FieldInfoComparer();
                var fieldInfoList = new HashSet<FieldInfo>(fieldInfos, fieldComparer);
                while (currentType != typeof(object))
                {
                    fieldInfos = currentType.GetFields(bindingFlags);
                    fieldInfoList.UnionWith(fieldInfos);
                    currentType = currentType.BaseType;
                }
                return fieldInfoList.ToArray();
            }
        }

        /// <summary>
        /// Deep clone an object without using serialisation.
        /// Creates a copy of each field of the object (and recurses) so that we end up with
        /// a copy that doesn't include any reference to the original object.
        /// </summary>
        static object deepClone(this object original, Type t, Dictionary<object, object> copies)
        {
            // Check if object is immutable or copy on update
            if (t.IsValueType || original == null || t == typeof(string) || t == typeof(Guid)) return original;
            // Interfaces aren't much use to us
            if (t.IsInterface) t = original.GetType();

            Object tmpResult;
            // Check if the object already has been copied
            if (copies.TryGetValue(original, out tmpResult))
                return tmpResult;

            object result;
            if (!t.IsArray)
            {
                result = Activator.CreateInstance(t);
                copies.Add(original, result);


                var fields =
                    GetFieldInfosIncludingBaseClasses(t, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy |
                                BindingFlags.Instance);
                // Maybe you need here some more BindingFlags
                foreach (var field in fields)
                {
                    var fieldValue = field.GetValue(original);
                    var valueType = fieldValue == null ? field.FieldType : fieldValue.GetType();
                    field.SetValue(result, fieldValue.deepClone(valueType, copies));
                }
            }
            else
            {
                // Handle arrays here
                var originalArray = (Array)original;
                var resultArray = (Array)originalArray.Clone();
                copies.Add(original, resultArray);

                var elementType = t.GetElementType();
                // If the type is not a value type we need to copy each of the elements
                if (!elementType.IsValueType)
                {
                    var lengths = new Int32[t.GetArrayRank()];
                    var indicies = new Int32[lengths.Length];
                    // Get lengths from original array
                    for (var i = 0; i < lengths.Length; i++)
                        lengths[i] = resultArray.GetLength(i);

                    var p = lengths.Length - 1;

                    /* Now we need to iterate though each of the ranks
                            * we need to keep it generic to support all array ranks */
                    while (increment(indicies, lengths, p))
                    {
                        var value = resultArray.GetValue(indicies);
                        if (value != null)
                            resultArray.SetValue(value.deepClone(elementType, copies), indicies);

                    }
                }
                result = resultArray;
            }
            return result;
        }

        static Boolean increment(Int32[] indicies, Int32[] lengths, Int32 p)
        {
            if (p > -1)
            {
                indicies[p]++;
                if (indicies[p] < lengths[p])
                    return true;

                if (increment(indicies, lengths, p - 1))
                {
                    indicies[p] = 0;
                    return true;
                }
            }
            return false;
        }
    }
}
