using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.Assets.Serialization
{
	/// <summary>
	/// A helpful class that allows for asset and resource linking.
	/// </summary>
	public class Linker
	{
		/// <summary>
		/// Creates an empty linker.
		/// </summary>
		/// <param name="g">The graphics device to use for all assets/resource requiring one.</param>
		/// <param name="content_root">
		/// If null, this will default <see cref="ContentRoot"/> to the working directory.
		/// Otherwise, this will directly assign the (simplified) full path to this location to <see cref="ContentRoot"/>.
		/// </param>
		public Linker(GraphicsDevice g, string? content_root = null)
		{
			Graphics = g;
			Content = null;

			if(content_root is null)
				ContentRoot = Path.GetFullPath(".");
			else
				ContentRoot = Path.GetFullPath(content_root);

			Initialized = false;
			return;
		}

		/// <summary>
		/// Creates an empty linker.
		/// </summary>
		/// <param name="g">The graphics device to use for all assets/resource requiring one.</param>
		/// <param name="content">If provided, this will be used to load assets/resources through the content pipeline when appropriate.</param>
		/// <param name="content_root">
		/// If null, this will default <see cref="ContentRoot"/> to the working directory.
		/// Otherwise, this will directly assign the (simplified) full path to this location to <see cref="ContentRoot"/>.
		/// </param>
		public Linker(GraphicsDevice g, ContentManager content, string? content_root = null)
		{
			Graphics = g;
			Content = content;

			if(content_root is null)
				ContentRoot = Path.GetFullPath(".");
			else
				ContentRoot = Path.GetFullPath(content_root);

			Initialized = false;
			return;
		}

		/// <summary>
		/// Creates an empty linker.
		/// </summary>
		/// <param name="g">A game that can provide a GraphicsDevice and ContentManager.</param>
		/// <param name="content_root">
		/// If null, this will default <see cref="ContentRoot"/> to the working directory.
		/// Otherwise, this will directly assign the (simplified) full path to this location to <see cref="ContentRoot"/>.
		/// </param>
		public Linker(Game g, string? content_root = null)
		{
			Graphics = g.GraphicsDevice;
			Content = g.Content;

			if(content_root is null)
				ContentRoot = Path.GetFullPath(".");
			else
				ContentRoot = Path.GetFullPath(content_root);

			Initialized = false;
			return;
		}

		/// <summary>
		/// Initializes this linker for asset building/writing/etc.
		/// </summary>
		/// <returns>Returns true if the initialization was successful and false otherwise (usually when it was already initialized).</returns>
		/// <remarks>
		/// The <see cref="Clear"/> method will unintialize this and prepare it for another linking task.
		/// <para/>
		/// This method should only be used by the <see cref="Asset"/> class.
		/// </remarks>
		protected internal bool Initialize()
		{
			if(Initialized)
				return false;

			Initialized = true;
			return true;
		}






		// Register asset/object function which assigns internal asset IDs should be here, and we throw absolutely everything at it that is a reference type







		/// <summary>
		/// Removes any instantiation/loading/writing information from this linker.
		/// </summary>
		public void Clear()
		{


			Initialized = false;
			return;
		}

		/// <summary>
		/// The graphics device used to instantiate, create, and write assets/resource, and otherwise link things into the system.
		/// </summary>
		public GraphicsDevice Graphics
		{get;}

		/// <summary>
		/// The content manager to use, if any, to load assets from disc through the content pipeline.
		/// </summary>
		public ContentManager? Content
		{get;}

		/// <summary>
		/// The root directory of assets stored on disc.
		/// <para/>
		/// By default, this is the (simplest) full path to the working directory.
		/// However, it is generally better to set this to the root directory containing uncompiled asset files (typically where your .mgcb file is located).
		/// </summary>
		public string ContentRoot
		{get; set;}

		/// <summary>
		/// If true, then this is in a ready state to link things.
		/// If false, then it must be initialized.
		/// </summary>
		protected bool Initialized
		{get; set;}
	}
}
