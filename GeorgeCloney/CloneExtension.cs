using System;
using System.Collections.Generic;
using System.Reflection;

namespace GeorgeCloney
{
    /// <summary>
    /// With thanks Stack Overflow: http://stackoverflow.com/questions/8025890/is-there-a-much-better-way-to-create-deep-and-shallow-clones-in-c/8026574#8026574
    /// </summary>
    public static class CloneExtension
    {
        public static T DeepClone<T>(this T original)
        {
            return typeof(T).IsSerializable 
                ? original.DeepCloneWithSerialization() 
                : original.DeepCloneWithoutSerialization();
        }

        public static T DeepCloneWithSerialization<T>(this T original)
        {
            if (!typeof(T).IsSerializable)
                throw new NotSupportedException(String.Format("Type '{0}' is not Serializable and cannot be cloned with this method.", typeof(T).Name));
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
            return (T)original.deepClone(typeof(T), copies);
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

                // Maybe you need here some more BindingFlags
                foreach (var field in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance))
                {
                    var fieldValue = field.GetValue(original);
                    field.SetValue(result, fieldValue.deepClone(field.FieldType, copies));
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
