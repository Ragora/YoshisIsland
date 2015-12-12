using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ClassAssignment
{
    /// <summary>
    /// The StateSprite class is an animated sprite whose actual animation
    /// contents are controlled via named states.Each state can contain
    /// its own sprite sheet, frame size, frame count, etc.Each state may
    /// also have listeners attached to its start and end points.
    /// </summary>
    public class StateSprite : AnimatedSprite
    {
        /// <summary>
        /// The current frame in our current animation state to use.
        /// </summary>
        public int CurrentFrameIdentifier;

        /// <summary>
        /// The currently state animation state.
        /// </summary>
        protected AnimationState CurrentAnimationState;

        /// <summary>
        /// A dictionary mapping the shorthand state names to their respective animation state information.
        /// </summary>
        private SortedDictionary<String, AnimationState> AnimationStates;

        /// <summary>
        /// The AnimationState class contains animation state information for a particular state that
        /// our sprite may be in.
        /// </summary>
        public class AnimationState
        {
            /// <summary>
            /// A delegate declaring a listener for the animation state starts and ends.
            /// </summary>
            public delegate void AnimationStateListener();

            /// <summary>
            /// What frame in the sprite sheet do we start at?
            /// </summary>
            public Point StartFrame;

            /// <summary>
            /// By what vector are we supposed to translate by when changing sprites?
            /// </summary>
            public Point Modifier;

            /// <summary>
            /// What is the size in pixels of each animation frame?
            /// </summary>
            public Point FrameSize;

            /// <summary>
            /// How many frames are there total?
            /// </summary>
            public int FrameCount;

            /// <summary>
            /// The sprite sheet to use.
            /// </summary>
            public Texture2D Sheet;

            /// <summary>
            /// Should we loop? If this is true, the state start and state end listeners are
            /// called in a looping fashion as well.
            /// </summary>
            public bool Looping;

            /// <summary>
            /// How many milliseconds to wait for each frame?
            /// </summary>
            public int MillisecondsPerFrame;

            /// <summary>
            /// The name of this animation state.
            /// </summary>
            public String Name;

            /// <summary>
            /// A delegate function that is called when the animation state starts.
            /// </summary>
            public AnimationStateListener StateStartListener;

            /// <summary>
            /// A delegate function that is called when the animation state ends.
            /// </summary>
            public AnimationStateListener StateEndListener;

            /// <summary>
            /// A list of transitions that this animation state cannot transition to.
            /// </summary>
            public List<String> IncompatibleTransitions;

            public SpriteEffects Effects;

            /// <summary>
            /// A boolean representing whether or not the state sprite should lock state transitions for the
            /// duration of this state.
            /// </summary>
            public bool StateLock;

            /// <summary>
            /// A constructor accepting a sprite sheet, the start frame, the frame modifier, the frame size
            /// and the frame count.
            /// </summary>
            /// <param name="name">
            /// The name of the new animation state.
            /// </param>
            /// <param name="sheet">
            /// The sprite sheet associated with this animation state.
            /// </param>
            /// <param name="startFrame">
            /// The starting X,Y position animation frame in sprite frames.
            /// </param>
            /// <param name="modifier">
            /// The frame advance modifier in sprite frames.
            /// </param>
            /// <param name="frameSize">
            /// The size of a single frame in the animation.
            /// </param>
            /// <param name="frameCount">
            /// The number of frames in the animation.
            /// </param>
            public AnimationState(String name, Texture2D sheet, Point startFrame, Point modifier, Point frameSize, int frameCount)
            {
                FrameCount = frameCount;
                Modifier = modifier;
                StartFrame = startFrame;
                FrameSize = frameSize;
                Sheet = sheet;
                Name = name;
                Effects = SpriteEffects.None;
                MillisecondsPerFrame = 80;
                IncompatibleTransitions = new List<String>();

                Looping = true;
            }
        };

        /// <summary>
        /// A constructor accepting a game object.
        /// </summary>
        /// <param name="game">
        /// The game instance to associate this state sprite with.
        /// </param>
        public StateSprite(Game game) : base(game)
        {

        }

        /// <summary>
        /// Initializes the state sprite by creating its animation state dictionary.
        /// </summary>
        public override void Initialize()
        {
            AnimationStates = new SortedDictionary<String, AnimationState>();
        }

        /// <summary>
        /// Resets the currently active animation to its beginning.
        /// </summary>
        public void ResetAnimation()
        {
            CurrentFrameIdentifier = 0;
        }

        /// <summary>
        /// Registers a new animation state with the given information. If a state already exists by the given name, the existing state
        /// is simply returned.If not, a new one is created under that name.
        /// </summary>
        /// <param name="sheet">
        /// The sprite sheet to use for this state.
        /// </param>
        /// <param name="name">
        /// The name of the animation state.
        /// </param>
        /// <param name="startFrame">
        /// The X,Y of the start frame in sprite frames.
        /// </param>
        /// <param name="modifier">
        /// The frame advance delta in sprite frames.
        /// </param>
        /// <param name="frameSize">
        /// The size of an individual frame in the animation.
        /// </param>
        /// <param name="frameCount">
        /// How many frames there are in the animation.
        /// </param>
        /// <returns>
        /// The newly created animation state. If there is already an animation state with the given name, that 
        /// one is simply returned.
        /// </returns>
        public AnimationState AddAnimationState(Texture2D sheet, String name, Point startFrame, Point modifier, Point frameSize, int frameCount)
        {
            name = name.ToLower();

            AnimationState result = GetAnimationState(name);
            if (result == null)
            {
                result = new AnimationState(name, sheet, startFrame, modifier, frameSize, frameCount);
                AnimationStates[name] = result;
            }

            return result;
        }

        /// <summary>
        /// Gets an existing animation state. If there is not an animation state under the given name, null is returned.
        /// </summary>
        /// <param name="name">
        /// The name of the animation state to get.
        /// </param>
        /// <returns>
        /// The animation state with the given name. This is null if there is no state by that name.
        /// </returns>
        public AnimationState GetAnimationState(String name)
        {
            name = name.ToLower();

            if (!AnimationStates.ContainsKey(name))
                return null;

            return AnimationStates[name];
        }

        /// <summary>
        /// A read-only property representing whether or not the state sprite's current animation state is complete.
        /// </summary>
        public bool StateComplete
        {
            get
            {
                if (CurrentAnimationState == null)
                    return true;

                return CurrentFrameIdentifier >= CurrentAnimationState.FrameCount && TimeSinceLastFrame >= MillisecondsPerFrame;
            }
        }

        /// <summary>
        /// Sets the current animation state. If the name is null or "none", the state is set to nothing. If
        /// The given animation state otherwise doesn't exist, a no-op occurs.
        /// </summary>
        /// <param name="name">
        /// The name of the animation state to use.
        /// </param>
        public void SetAnimationState(String name)
        {
            if (name == null || name == "none")
            {
                CurrentAnimationState = null;
                return;
            }

            name = name.ToLower();
            if (!AnimationStates.ContainsKey(name) || CurrentAnimationState == AnimationStates[name])
                return;

            if ((!StateComplete && CurrentAnimationState.StateLock) || (CurrentAnimationState != null && CurrentAnimationState.IncompatibleTransitions.Contains(name)))
                return;

            CurrentAnimationState = AnimationStates[name];

            CurrentFrame = CurrentAnimationState.StartFrame;
            InternalSheet = CurrentAnimationState.Sheet;
            FrameSize = CurrentAnimationState.FrameSize;
            MillisecondsPerFrame = CurrentAnimationState.MillisecondsPerFrame;

            CurrentFrameRectangle.X = CurrentFrame.X * FrameSize.X;
            CurrentFrameRectangle.Y = CurrentFrame.Y * FrameSize.Y;
            CurrentFrameRectangle.Width = FrameSize.X;
            CurrentFrameRectangle.Height = FrameSize.Y;
            Effects = CurrentAnimationState.Effects;

            if (CurrentAnimationState.StateStartListener != null)
                CurrentAnimationState.StateStartListener();
        }

        /// <summary>
        /// Updates the state sprite.
        /// </summary>
        /// <param name="time">
        /// The GameTime passed into the game's main Update method.
        /// </param>
        public override void Update(GameTime time)
        {
            if (!Updated || CurrentAnimationState == null)
                return;

            TimeSinceLastFrame += time.ElapsedGameTime.Milliseconds;

            if (TimeSinceLastFrame >= MillisecondsPerFrame)
            {
                if (!CurrentAnimationState.StateLock)
                    TimeSinceLastFrame -= MillisecondsPerFrame;

                if (CurrentFrameIdentifier >= CurrentAnimationState.FrameCount)
                {
                    if (CurrentAnimationState.StateEndListener != null)
                        CurrentAnimationState.StateEndListener();

                    if (CurrentAnimationState.Looping)
                    {
                        CurrentFrameIdentifier = 0;
                        CurrentFrame = CurrentAnimationState.StartFrame;

                        if (CurrentAnimationState.StateStartListener != null)
                            CurrentAnimationState.StateStartListener();
                    }
                    else
                        return;
                }
                else
                    CurrentFrame = new Point(CurrentFrame.X + CurrentAnimationState.Modifier.X, CurrentFrame.Y + CurrentAnimationState.Modifier.Y);

                ++CurrentFrameIdentifier;

                if (!Repeat)
                    Updated = false;

                CurrentFrameRectangle.X = CurrentFrame.X * CurrentAnimationState.FrameSize.X;
                CurrentFrameRectangle.Y = CurrentFrame.Y * CurrentAnimationState.FrameSize.Y;
                CurrentFrameRectangle.Width = CurrentAnimationState.FrameSize.X;
                CurrentFrameRectangle.Height = CurrentAnimationState.FrameSize.Y;
            }
        }
    }
}
