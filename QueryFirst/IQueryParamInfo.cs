namespace QueryFirst
{
    public interface IQueryParamInfo
    {
        string CSType { get; set; }
        //bool ExplicitlyDeclared { get; set; }
        int Length { get; set; }
        string CSName { get; set; }
        string DbName { get; set; }
        string DbType { get; set; }
        //string SqlTypeAndLength { get; set}
    }
}