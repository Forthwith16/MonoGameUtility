using GameEngine.Framework;
using GameEngine.GameComponents;
using GameEngine.Input;
using GameEngine.Utility.ExtensionMethods.ClassExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ExampleAnimation
{
	/// <summary>
	/// This game demonstrates how to use animation files.
	/// </summary>
	public class ExampleAnimationGame : RenderTargetFriendlyGame
	{
		public ExampleAnimationGame()
		{
			// For this project, we will need to use a custom pipeline to process our animation file (at least if we want to make this as painless as possible for us long-term)
			// The details of how we actually make this custom pipeline are somewhat complicated and we put them off for later
			// For now, build the GameEnginePipeline project
			// In the bin folder, you should be able to find a GameEnginePipeline.dll file somewhere within (or whatever library file your computer compiles to)
			// Open up your Content.mgcb file in the editor
			// In the root Cotent, you should see a References property, which should contain your dll file
			// Then if you add the test.animation file (you can add its dependencies, but so long as they're in the right relative locations, you only need the animation file added), you can load it with the custom pipeline
			// The required importer and processor should be selected automatically for you
			// If not, select the Animation2D version of each
			Content.RootDirectory = "Content";
			
			return;
		}

		protected override void PreInitialize()
		{
			Components.Add(Input = new InputManager());
			
			Input.AddKeyInput("_E",Keys.Escape);
			Input.AddEdgeTriggeredInput("Back","_E",true);

			Input.AddKeyInput("U",Keys.Up);
			Input.AddKeyInput("D",Keys.Down);
			Input.AddKeyInput("L",Keys.Left);
			Input.AddKeyInput("R",Keys.Right);

			return;
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			// AnimatedComponents are also ImageComponents and we can use them as such
			// All we need to do is provide it with the resource file for our animation, and the game will take care of the rest
			// The format for the animation file is a simple XMLSerialization of an Animation2DAsset (look in Assets/AnimationAssets/Animation2DAsset.cs of the GameEnginePipleine for more information)
			// You can make more animations manually by editing these files or by serializing such an asset with its Serialize method
			Components.Add(_image = new AnimatedComponent(this,_spriteBatch,"Animations/test"));
			
			// When you want to perform reflections to a drawn sprite, use a SpriteEffect rather than scaling by a negative factor
			// MonoGame's SpriteBatch doesn't like negative scaling factors in the 2D world, unfortunately
			_image.Effect = SpriteEffects.FlipHorizontally;

			// You may notice some stray pixels with this scaling due to the sampler's LinearWrap setting below
			// This is how we tell the texture sampler on the GPU how to deal with 'going beyond a texture'
			// In this case, we wrap around to the other side and start over again (i.e. we take the mod of the texture coordiante)
			// This happens due to floating point errors and is a good example of one reason (amongst many) why you generally (but not always) don't want to scale textures up (just make them at the resolution you want to begin with)
			_image.Transform = _image.Transform.Scale(1.5f);

			return;
		}

		protected override void Update(GameTime delta)
		{
			if(Input!["Back"].CurrentDigitalValue)
				Exit();

			float speed = 200.0f * (float)delta.ElapsedGameTime.TotalSeconds;

			if(Input["L"].CurrentDigitalValue)
				_image!.Transform = _image.Transform.Translate(-speed,0.0f);

			if(Input["R"].CurrentDigitalValue)
				_image!.Transform = _image.Transform.Translate(speed,0.0f);

			if(Input["U"].CurrentDigitalValue)
				_image!.Transform = _image.Transform.Translate(0.0f,-speed);

			if(Input["D"].CurrentDigitalValue)
				_image!.Transform = _image.Transform.Translate(0.0f,speed);

			base.Update(delta);
			return;
		}

		protected override void PreDraw(GameTime delta)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);
			_spriteBatch!.Begin(SpriteSortMode.BackToFront,BlendState.NonPremultiplied,SamplerState.LinearWrap,null,null,null,null);
			
			return;
		}

		protected override void PostDraw(GameTime delta)
		{
			_spriteBatch!.End();
			return;
		}
		
		private SpriteBatch? _spriteBatch;
		private ImageComponent? _image;
		protected InputManager? Input;
	}
}
