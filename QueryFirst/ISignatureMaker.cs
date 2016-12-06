namespace QueryFirst
{
    using System.Collections.Generic;
    /// <summary>
    /// Interface for generating  method signatures from the parameters found in a sql query.
    /// </summary>
    public interface ISignatureMaker
    {
        /// <summary>
        /// Generates comma separated signatures from an array of parameters
        /// </summary>
        /// <param name="ParamNamesAndTypes">A two dimensional string array of C# type names and param names from the query.</param>
        /// <param name="MethodSignature">Output a comma separated string of typename paramname.</param>
        /// <param name="CallingSignature">Output a comma separated string of just paramname.</param>
        void MakeMethodAndCallingSignatures(List<QueryParamInfo> ParamNamesAndTypes, out string MethodSignature, out string CallingSignature);
    }
}