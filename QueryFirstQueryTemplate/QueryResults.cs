﻿using System;

namespace $rootnamespace$
{
	[Serializable]
	public partial class $safeitemname$
	{
		// Partial class extends the generated results class
		// Serializable by default, but you can change this here		
		// Add your supplementary methods and properties here, or 
		// choose a class name more to your liking :-)
		internal void OnLoad()
		{
		}       
	}
	

	// POCO factory, called for each line of results. For polymorphic POCOs, put your instantiation logic here.
	// Tag the results class as abstract above, add some virtual methods, create some subclasses, then instantiate them here based on data in the row.
	// This name follows the root name of the query. You can change it by renaming the parent .sql file.
	public partial class $safeitemname$¤
	{
		$safeitemname$ CreatePoco(System.Data.IDataRecord record)
		{
			return new $safeitemname$();
		}
	}
}
