namespace Microsoft.OpenPublishing.Build.Common
{
    using System.Linq;

    public static class ArrayExtensions
    {
        public static T[] UnionArray<T>(this T[] source, T[] value)
        {
            if (source == null)
            {
                return value;
            }
            if (value == null)
            {
                return source;
            }
            return source.Union(value).ToArray();
        }
    }
}