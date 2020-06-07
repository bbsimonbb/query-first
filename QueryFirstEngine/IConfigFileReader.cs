namespace QueryFirst
{
    public interface IConfigFileReader
    {
        string GetConfigFile(string filePath);
        QFConfigModel GetConfigObj(string filePath);
    }
}