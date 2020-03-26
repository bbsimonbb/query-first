namespace QueryFirst
{
    /// <summary>
    /// Generates the wrapper class for a sql query.
    /// </summary>
    public interface IWrapperClassMaker
    {
        /// <summary>
        /// The brace that closes the class
        /// </summary>
        /// <param name="ctx">The ICodeGenerationContext</param>
        /// <returns>The brace that closes the class</returns>
        string CloseClass(State sate);
        /// <summary>
        /// The brace that closes the namespace
        /// </summary>
        /// <param name="ctx">The ICodeGenerationContext</param>
        /// <returns>The brace that closes the namespace, if present.</returns>
        string CloseNamespace(State sate);
        /// <summary>
        /// Makes a method that takes an IDataRecord and fills a Results class.
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        string MakeCreateMethod(State sate);
        string MakeExecuteScalarWithConn(State sate);
        string MakeExecuteScalarWithoutConn(State sate);
        string MakeExecuteNonQueryWithConn(State sate);
        string MakeExecuteNonQueryWithoutConn(State sate);
        string MakeExecuteWithConn(State sate);
        string MakeExecuteWithoutConn(State state);
        string MakeGetOneWithConn(State sate);
        string MakeGetOneWithoutConn(State sate);
        string MakeGetCommandTextMethod(State sate);
        string MakeOtherMethods(State sate);
        string StartClass(State sate);
        string StartNamespace(State sate);
        string Usings(State sate);
        string MakeInterface(State sate);
        string SelfTestUsings(State sate);
        string MakeSelfTestMethod(State sate);
        string MakeTvpPocos(State state);
    }
}