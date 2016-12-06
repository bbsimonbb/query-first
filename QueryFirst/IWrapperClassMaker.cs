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
        /// <param name="ctx">The CodeGenerationContext</param>
        /// <returns>The brace that closes the class</returns>
        string CloseClass(CodeGenerationContext ctx);
        /// <summary>
        /// The brace that closes the namespace
        /// </summary>
        /// <param name="ctx">The CodeGenerationContext</param>
        /// <returns>The brace that closes the namespace, if present.</returns>
        string CloseNamespace(CodeGenerationContext ctx);
        /// <summary>
        /// Makes a method that takes an IDataRecord and fills a Results class.
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        string MakeCreateMethod(CodeGenerationContext ctx);
        string MakeExecuteScalarWithConn(CodeGenerationContext ctx);
        string MakeExecuteScalarWithoutConn(CodeGenerationContext ctx);
        string MakeExecuteNonQueryWithConn(CodeGenerationContext ctx);
        string MakeExecuteNonQueryWithoutConn(CodeGenerationContext ctx);
        string MakeExecuteWithConn(CodeGenerationContext ctx);
        string MakeExecuteWithoutConn(CodeGenerationContext ctn);
        string MakeGetOneWithConn(CodeGenerationContext ctx);
        string MakeGetOneWithoutConn(CodeGenerationContext ctx);
        string MakeGetCommandTextMethod(CodeGenerationContext ctx);
        string MakeOtherMethods(CodeGenerationContext ctx);
        string StartClass(CodeGenerationContext ctx);
        string StartNamespace(CodeGenerationContext ctx);
        string Usings(CodeGenerationContext ctx);
        string MakeInterface(CodeGenerationContext ctx);
        string SelfTestUsings(CodeGenerationContext ctx);
        string MakeSelfTestMethod(CodeGenerationContext ctx);
    }
}