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
        string CloseClass(ICodeGenerationContext ctx);
        /// <summary>
        /// The brace that closes the namespace
        /// </summary>
        /// <param name="ctx">The ICodeGenerationContext</param>
        /// <returns>The brace that closes the namespace, if present.</returns>
        string CloseNamespace(ICodeGenerationContext ctx);
        /// <summary>
        /// Makes a method that takes an IDataRecord and fills a Results class.
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        string MakeCreateMethod(ICodeGenerationContext ctx);
        string MakeExecuteScalarWithConn(ICodeGenerationContext ctx);
        string MakeExecuteScalarWithoutConn(ICodeGenerationContext ctx);
        string MakeExecuteNonQueryWithConn(ICodeGenerationContext ctx);
        string MakeExecuteNonQueryWithoutConn(ICodeGenerationContext ctx);
        string MakeExecuteWithConn(ICodeGenerationContext ctx);
        string MakeExecuteWithoutConn(ICodeGenerationContext ctn);
        string MakeGetOneWithConn(ICodeGenerationContext ctx);
        string MakeGetOneWithoutConn(ICodeGenerationContext ctx);
        string MakeGetCommandTextMethod(ICodeGenerationContext ctx);
        string MakeOtherMethods(ICodeGenerationContext ctx);
        string StartClass(ICodeGenerationContext ctx);
        string MakeProperties(ICodeGenerationContext ctx);
        string StartNamespace(ICodeGenerationContext ctx);
        string Usings(ICodeGenerationContext ctx);
        string MakeInterface(ICodeGenerationContext ctx);
        string SelfTestUsings(ICodeGenerationContext ctx);
        string MakeSelfTestMethod(ICodeGenerationContext ctx);
    }
}