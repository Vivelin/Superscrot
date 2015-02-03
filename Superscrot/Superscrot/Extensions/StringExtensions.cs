namespace Superscrot
{
    /// <summary>
    /// Provides additional functionality to strings.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Returns this string instance, or the first specified string that is
        /// not <c>null</c> or the empty string.
        /// </summary>
        /// <param name="value">
        /// The string value to return only if it is not <c>null</c> or empty.
        /// </param>
        /// <param name="values">
        /// A string array whose first non-null and non-empty value is returned.
        /// </param>
        /// <returns>
        /// <paramref name="value"/> if it is not <c>null</c> or empty, or a
        /// value from <paramref name="values"/> that is not <c>null</c> or 
        /// empty, or <c>null</c> if all parameters are <c>null</c> or empty.
        /// </returns>
        public static string Coalesce(this string value, params string[] values)
        {
            if (!string.IsNullOrEmpty(value))
                return value;

            foreach (var item in values)
            {
                if (!string.IsNullOrEmpty(item))
                    return item;
            }

            return null;
        }
    }
}
