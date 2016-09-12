namespace QueryFirst
{
    public interface IQueryParam
    {
        string CSType { get; }
        bool ExplicitlyDeclared { get; set; }
        int Length { get; set; }
        string Name { get; }
        string SqlDbType { get; set; }
        string SqlTypeAndLength { get; }

        void Populate(string name, string sqlTypeAndLength, bool explicitlyDeclared);
    }
}