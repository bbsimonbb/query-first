namespace QueryFirst
{
    public interface IConfigResolver
    {
        QFConfigModel GetConfig(string filePath, string queryText);
    }
}