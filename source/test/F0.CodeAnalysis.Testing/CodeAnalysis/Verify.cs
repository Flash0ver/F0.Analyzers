using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CodeRefactorings;

namespace F0.Testing.CodeAnalysis
{
	public static class Verify
	{
		public static CodeRefactoringVerifier<TRefactoringProvider> CodeRefactoring<TRefactoringProvider>()
			where TRefactoringProvider : CodeRefactoringProvider, new()
			=> new CodeRefactoringVerifier<TRefactoringProvider>();
	}
}
