using System;

namespace QueryFirst
{
    [Serializable]
    public partial class ScaffoldInsertResults
    {
        // Partial class extends the generated results class
        // Serializable by default, but you can change this here		
        // Put your methods here :-)
        internal void OnLoad()
        {
        }
    }
    // POCO factory, called for each line of results. For polymorphic POCOs, put your instantiation logic here.
    // Tag the results class as abstract above, add some virtual methods, create some subclasses, then instantiate them here based on data in the row.
    public partial class ScaffoldInsert
    {
        ScaffoldInsertResults CreatePoco(System.Data.IDataRecord record)
        {
            return new ScaffoldInsertResults();
        }
    }
}
