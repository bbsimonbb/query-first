namespace QueryFirst
{
    /// <summary>
    /// Interface that for mapping C# types onto SQL types.
    /// </summary>
    public interface IMap
    {
        /// <summary>
        /// Returns the C# type to which the reader result can be safely cast, and from which a sql parameter
        /// can be safely created.
        /// </summary>
        /// <param name="p">The Transact SQL type name.</param>
        /// <returns>The C# type name.</returns>
        string DBType2CSType(string p, bool nullable=true);
    }
}