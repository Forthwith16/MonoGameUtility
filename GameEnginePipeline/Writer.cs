using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace GameEnginePipeline
{
	/// <summary>
	/// Allows for writing assets to the pipeline.
	/// </summary>
	/// <typeparam name="TWrite">The type we will write to the pipeline.</typeparam>
	/// <typeparam name="TRead">The type we will later read from the pipeline.</typeparam>
	public abstract class Writer<TWrite,TReader,TRead> : ContentTypeWriter<TWrite> where TWrite : ContentItem where TReader : ContentTypeReader<TRead>
	{
		/// <summary>
		/// Specifies how to read content from the pipeline.
		/// </summary>
		/// <param name="targetPlatform">The target platform this is being compiled for.</param>
		/// <returns>Returns the ContentTypeReader for this asset type.</returns>
		public override string GetRuntimeReader(TargetPlatform targetPlatform)
		{return typeof(TReader).AssemblyQualifiedName ?? "";}
	}
}
