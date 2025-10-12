using GameEngine.Utility.ExtensionMethods.ClassExtensions;
using GameEnginePipeline.Assets.Sprites;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using TRead = GameEngine.Assets.Sprites.SpriteRenderer;

namespace GameEnginePipeline.Readers.Sprites
{
	/// <summary>
	/// Transforms content into its game component form.
	/// </summary>
	public sealed class SpriteRendererReader : ContentTypeReader<TRead>
	{
		/// <summary>
		/// Reads a sprite renderer into the game.
		/// </summary>
		/// <param name="cin">The source of content data.</param>
		/// <param name="existingInstance">The existence instance of this content (if any).</param>
		/// <returns>Returns the sprite renderer specified by <paramref name="cin"/> or <paramref name="existingInstance"/> if we've already created an instance of this source asset.</returns>
		protected override TRead Read(ContentReader cin, TRead? existingInstance)
		{
			// We insist that sprite sheets are unique for each source
			if(existingInstance is not null)
				return existingInstance;
			
			// First read the sprite renderer's name
			string name = cin.ReadString();

			// Now create the thing to return
			TRead ret = new TRead(name,cin.GetGraphicsDevice());

			// Next check if we have a shader
			bool has_shade = cin.ReadBoolean();

			// First get the source texture
			if(has_shade)
			{
				ret.Shader = cin.ReadExternalReference<Effect>();

				// We append the shader's extension if it doesn't already have it from a Load elsewhere
				string extension = cin.ReadString();
				
				if(!Path.HasExtension(ret.Shader.Name))
					ret.Shader.Name += extension;
			}

			// Now read in all of the enums and assign their values
			ret.Order = cin.ReadEnum<SpriteSortMode>();

			switch(cin.ReadEnum<BasicBlendState>())
			{
			case BasicBlendState.Additive:
				ret.Blend = BlendState.Additive;
				break;
			case BasicBlendState.AlphaBlend:
				ret.Blend = BlendState.AlphaBlend;
				break;
			case BasicBlendState.NonPremultiplied:
				ret.Blend = BlendState.NonPremultiplied;
				break;
			case BasicBlendState.Opaque:
				ret.Blend = BlendState.Opaque;
				break;
			}

			switch(cin.ReadEnum<BasicSamplerState>())
			{
			case BasicSamplerState.PointClamp:
				ret.Wrap = SamplerState.PointClamp;
				break;
			case BasicSamplerState.PointWrap:
				ret.Wrap = SamplerState.PointWrap;
				break;
			case BasicSamplerState.LinearClamp:
				ret.Wrap = SamplerState.LinearClamp;
				break;
			case BasicSamplerState.LinearWrap:
				ret.Wrap = SamplerState.LinearWrap;
				break;
			case BasicSamplerState.AnisotropicClamp:
				ret.Wrap = SamplerState.AnisotropicClamp;
				break;
			case BasicSamplerState.AnisotropicWrap:
				ret.Wrap = SamplerState.AnisotropicWrap;
				break;
			}

			switch(cin.ReadEnum<BasicDepthStencilState>())
			{
			case BasicDepthStencilState.Default:
				ret.DepthStencil = DepthStencilState.Default;
				break;
			case BasicDepthStencilState.DepthRead:
				ret.DepthStencil = DepthStencilState.DepthRead;
				break;
			case BasicDepthStencilState.None:
				ret.DepthStencil = DepthStencilState.None;
				break;
			}

			switch(cin.ReadEnum<BasicRasterizerState>())
			{
			case BasicRasterizerState.CullNone:
				ret.Cull = RasterizerState.CullNone;
				break;
			case BasicRasterizerState.CullClockwise:
				ret.Cull = RasterizerState.CullClockwise;
				break;
			case BasicRasterizerState.CullCounterClockwise:
				ret.Cull = RasterizerState.CullCounterClockwise;
				break;
			}

			return ret;
		}
	}

	
}
