using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExampleShaders
{
	/// <summary>
	/// A bare bones model game component.
	/// </summary>
	public class SimpleModel : DrawableGameComponent
	{
		/// <summary>
		/// Wraps this class around a model <paramref name="m"/> as a decorator to improve its function.
		/// </summary>
		/// <param name="game">The game this model will belong to.</param>
		/// <param name="m">The model to draw.</param>
		public SimpleModel(Game game, Model m) : base(game)
		{
			MyModel = m;
			UseBasicEffect = true;

			ShaderParameterization = null;
			return;
		}

		public override void Draw(GameTime delta)
		{
			if(UseBasicEffect || Shader is null)
				DrawWithBasicEffect(delta);
			else
				DrawWithEffect(delta);

			return;
		}

		/// <summary>
		/// Draws this model using its BasicEffect.
		/// </summary>
		/// <param name="delta">The elapsed game time since the last draw.</param>
		protected void DrawWithBasicEffect(GameTime delta)
		{
			foreach(ModelMesh mesh in MyModel.Meshes)
			{
				foreach(BasicEffect effect in mesh.Effects)
				{
					if(_be is null)
						_be = effect;

					effect.EnableDefaultLighting();
					effect.DiffuseColor = BasicEffectTint.ToVector3();

					effect.View = View;
					effect.Projection = Projection;
					effect.World = World;
				}

				mesh.Draw();
			}

			return;
		}

		/// <summary>
		/// Draws this model using its specialized shader.
		/// </summary>
		/// <param name="delta">The elapsed game time since the last draw.</param>
		protected void DrawWithEffect(GameTime delta)
		{
			// Like with BasicEffects, we need to draw each mesh
			foreach(ModelMesh mesh in MyModel.Meshes)
			{
				// Unlike with BasicEffects, we need to draw each part of a mesh (it is not impossible [or even unlikely] that a mesh only has one part; it depends on how the model was made and with what software)
				foreach(ModelMeshPart part in mesh.MeshParts)
				{
					// We need to set what shader we'll use to draw this part with
					part.Effect = Shader;

					// If we have any parameters to set, then do that now
					if(ShaderParameterization is not null)
						ShaderParameterization(this,mesh,part,Shader!);
				}

				mesh.Draw();
			}

			return;
		}

		/// <summary>
		/// The model this game component will draw.
		/// </summary>
		public Model MyModel
		{get; protected set;}

		/// <summary>
		/// If true, then this model will draw using its default basic effect.
		/// If false, it will use whatever custom shader it has attached to it.
		/// </summary>
		public bool UseBasicEffect
		{
			get => _ube;
			
			set
			{
				if(_ube == value)
					return;

				_ube = value;

				if(_ube)
					foreach(ModelMesh mesh in MyModel.Meshes)
						foreach(ModelMeshPart part in mesh.MeshParts)
							part.Effect = _be;

				return;
			}
		}
		protected bool _ube = true;

		/// <summary>
		/// The tint to use with lighting for a basic effect.
		/// </summary>
		public Color BasicEffectTint
		{get; set;}

		/// <summary>
		/// The shader to use when not drawing this model with a BasicEffect.
		/// </summary>
		public Effect? Shader
		{
			protected get => _s;
			
			set
			{
				_s = value;
				ShaderParameterization = null;

				return;
			}
		}

		protected Effect? _s;
		protected Effect? _be;

		/// <summary>
		/// A delegate that specifies how to set a shader's parameters.
		/// This is nulled every time the shader is changed.
		/// </summary>
		public SetShaderParameters? ShaderParameterization
		{protected get; set;}

		/// <summary>
		/// The world matrix.
		/// </summary>
		public Matrix World
		{get; set;}

		/// <summary>
		/// The view matrix.
		/// </summary>
		public Matrix View
		{get; set;}

		/// <summary>
		/// The projection matrix.
		/// </summary>
		public Matrix Projection
		{get; set;}
	}

	/// <summary>
	/// A delegate that describes how to set a shader's parameters.
	/// </summary>
	/// <param name="model">The model we are working with.</param>
	/// <param name="mesh">The mesh of the model we are drawing.</param>
	/// <param name="part">The part of the mesh we are currently drawing.</param>
	/// <param name="effect">The shader we are using (provided as a convenience).</param>
	public delegate void SetShaderParameters(SimpleModel model, ModelMesh mesh, ModelMeshPart part, Effect effect);
}
