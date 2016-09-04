using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace GeorgeCloney
{
    public static class BinarySerializationExtension
    {
        public static Stream Serialize(this object source)
        {
            var formatter = new BinaryFormatter();
            var stream = new MemoryStream();
            formatter.Serialize(stream, source);
            return stream;
        }

        public static T Deserialize<T>(this Stream stream)
        {
            var formatter = new BinaryFormatter();
            stream.Position = 0;
            return (T) formatter.Deserialize(stream);
        }
    }
}
