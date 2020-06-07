namespace QueryFirst.TypeMappings
{
    /// <summary>
    /// Interface that for mapping C# types onto SQL types.
    /// </summary>
    public interface IDB2CS
    {
        /// <summary>
        /// Returns the C# type to which the reader result can be safely cast, and from which a sql parameter
        /// can be safely created.
        /// </summary>
        /// <param name="DBType">The Transact SQL type name.</param>
        /// <param name="DBTypeNormalized">Outputs the supplied DBType with capitalization corrected.</param>
        /// <returns>The C# type name.</returns>
        string Map(string DBType, out string DBTypeNormalized, bool nullable=true);
    }
}