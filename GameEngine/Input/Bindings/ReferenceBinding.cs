namespace GameEngine.Input.Bindings
{
	/// <summary>
	/// Encapsulates a binding that references another binding elsewhere.
	/// <para/>
	/// In C# 10, this must be accomplished via delegates to achieve safe code.
	/// In C# 11, this can be accomplished with reference fields.
	/// </summary>
	public class ReferenceBinding : InputBinding
	{
		/// <summary>
		/// Creates a new reference binding that allows a binding to be accessed 'by reference' in C# 10.
		/// </summary>
		/// <param name="ref_binding">A delegate providing an InputBinding 'by reference'.</param>
		public ReferenceBinding(RefInputBinding ref_binding) : base((state) => ref_binding().DigitalEvaluation(state),(state) => ref_binding().AnalogEvaluation(state))
		{return;}

		public override string ToString()
		{return "Reference Binding";}
	}

	/// <summary>
	/// Fetches an input binding 'by reference' in C# 10.
	/// In C# 11, a reference field would work better.
	/// </summary>
	/// <returns>Returns an InputBinding.</returns>
	public delegate InputBinding RefInputBinding();
}
