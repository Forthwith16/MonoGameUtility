using GameEngine.Readers;
using System.Numerics;

namespace GameEnginePipeline.Assets.AnimationAssets
{
	/// <summary>
	/// Contains the base raw asset data of an Animation2D.
	/// </summary>
	public class Animation2DAsset : BaseAnimationAsset<Animation2DAsset>
	{
		/// <summary>
		/// Creates a Animation2D with garbage values.
		/// </summary>
		public Animation2DAsset() : base(AnimationType.Animation2D)
		{return;}

		/// <summary>
		/// The source sprite sheet for the animation.
		/// </summary>
		public string? Source
		{get; set;}

		public bool ShouldSerializeSource()
		{return true;}

		/// <summary>
		/// The keyframes of this animation.
		/// </summary>
		public Animation2DKeyFrame[]? Frames
		{get; set;}

		public bool ShouldSerializeFrames()
		{return Frames is not null;} // You can't have an animation without keyframes, but we really don't need a null value cluttering things

		/// <summary>
		/// If true, the animation loops.
		/// If false, the animation does not loop.
		/// </summary>
		/// <remarks>Since booleans default to false, we'll just use that default value as the default value for looping and not bother with letting this be nullable.</remarks>
		public bool Loops
		{get; set;}

		public bool ShouldSerializeLoops()
		{return true;}

		/// <summary>
		/// The time when the loop start should be set to regardless of whether or not the animation loops.
		/// <para/>
		/// If neither this nor LoopFrameStart are defined, the loop start will default to 0.
		/// </summary>
		public float? LoopStart
		{get; set;}

		public bool ShouldSerializeLoopStart()
		{return LoopStart is not null;}

		/// <summary>
		/// The frame when the loop start should be set to regardless of whether or not the animation loops.
		/// This will be converted into a time rather than remain as a frame.
		/// <para/>
		/// This value is ignored if LoopStart is defined.
		/// </summary>
		public int? LoopFrameStart
		{get; set;}

		public bool ShouldSerializeLoopFrameStart()
		{return LoopFrameStart is not null;}

		/// <summary>
		/// The time when the loop start should be set to regardless of whether or not the animation loops.
		/// <para/>
		/// If neither this nor LoopFrameEnd are defined, the loop end will default to the end of the animation.
		/// </summary>
		public float? LoopEnd
		{get; set;}

		public bool ShouldSerializeLoopEnd()
		{return LoopEnd is not null;}

		/// <summary>
		/// The frame when the loop end should be set to regardless of whether or not the animation loops.
		/// This will be converted into a time rather than remain as a frame.
		/// Note that this uses the frame's ending time.
		/// So for example, a loop from frame 0 to frame 0 loops across the entire 0th frame.
		/// <para/>
		/// This value is ignored if LoopStart is defined.
		/// </summary>
		public int? LoopFrameEnd
		{get; set;}

		public bool ShouldSerializeLoopFrameEnd()
		{return LoopFrameStart is not null;}
	}

	/// <summary>
	/// The information we need for an animation keyframe.
	/// </summary>
	public class Animation2DKeyFrame
	{
		/// <summary>
		/// The index of the sprite for this animation frame.
		/// </summary>
		public int Sprite
		{get; set;}

		/// <summary>
		/// The duration of this keyframe.
		/// </summary>
		public float Duration
		{get; set;}

		/// <summary>
		/// The translation component of this frame's transformation matrix.
		/// </summary>
		public Vector2 Translation
		{get; set;}

		public bool ShouldSerializeTranslation()
		{return Translation != Vector2.Zero;}

		/// <summary>
		/// The scale component of this frame's transformation matrix.
		/// </summary>
		public Vector2 Scale
		{get; set;} = Vector2.One;

		public bool ShouldSerializeScale()
		{return Scale != Vector2.One;}

		/// <summary>
		/// The rotation component of this frame's transformation matrix.
		/// </summary>
		public float Rotation
		{get; set;}

		public bool ShouldSerializeRotation()
		{return Rotation != 0.0f;}
	}
}
