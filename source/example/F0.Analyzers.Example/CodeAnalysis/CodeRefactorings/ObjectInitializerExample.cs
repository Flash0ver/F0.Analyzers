namespace F0.Analyzers.Example.CodeAnalysis.CodeRefactorings
{
	internal sealed class Model
	{
		public int Number { get; set; }

		public string Text { get; set; }
	}

	internal sealed class ObjectInitializerExample
	{
		public Model GetModel()
		{
			var model = new Model();


			return model;
		}
	}
}
