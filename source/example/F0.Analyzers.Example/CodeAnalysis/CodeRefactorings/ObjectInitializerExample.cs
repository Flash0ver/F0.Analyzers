namespace F0.Analyzers.Example.CodeAnalysis.CodeRefactorings
{
	internal sealed class Model
	{
		public int Number { get; set; }

		public string Text { get; set; }
	}

	internal sealed class ObjectInitializerExample
	{
		Model GetModel()
		{
			var model = new Model()
			{
				Number = default,
				Text = default
			};

			var model2 = new Model() { Number = default, Text = default };

			return model;
		}
	}
}
