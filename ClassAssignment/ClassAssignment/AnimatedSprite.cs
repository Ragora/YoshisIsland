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
    /// A class representing a basic algorithmic frame-advance sprite.
    /// </summary>
    public class AnimatedSprite : IDrawable, ITickable
    {
        public SpriteEffects Effects { get; set; }

        /// <summary>
        /// Whether or not the animation should repeat once there is no frames
        /// left to play.
        /// </summary>
        public Boolean Repeat;

        /// <summary>
        /// The size of each frame in the sheet.
        /// </summary>
        public Point FrameSize;

        /// <summary>
        /// The position to be drawn at.
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Whether or not this animated sprite should be drawn to the screen.
        /// </summary>
        public bool Drawn { get; set; }

        /// <summary>
        /// The scale factor.
        /// </summary>
        public float Scale { get; set; }

        /// <summary>
        /// The color to be drawn with.
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// The origin of the sprite. This is basically the draw offset relative to the top left corner of the image.
        /// </summary>
        public Vector2 Origin { get; set; }

        /// <summary>
        /// The rotation of the sprite.
        /// </summary>
        public float Theta { get; set; }

        /// <summary>
        /// Whether or not this animated sprite should be updated.
        /// </summary>
        public bool Updated { get; set; }

        /// <summary>
        /// The current X,Y coordinates in sprite frames to draw.
        /// </summary>
        public Point CurrentFrame;

        /// <summary>
        /// The game instance this animated sprite is associated with.
        /// </summary>
        protected Game InternalGame;

        /// <summary>
        /// The path to the texture to load and use as the sprite sheet.
        /// </summary>
        private String InternalTexturePath;

        /// <summary>
        /// The internally used texture for the sprite sheet.
        /// </summary>
        protected Texture2D InternalSheet;

        /// <summary>
        /// How much time has passed since the last frame advance.
        /// </summary>
        protected int TimeSinceLastFrame;

        /// <summary>
        /// The internally used sheet size that is represented in sprite frames.
        /// </summary>
        protected Point? SheetSize;

        /// <summary>
        /// The internally used rectangle for drawing individual frames out of the sprite sheet. This is calculated for each
        /// update that triggers a frame advance.
        /// </summary>
        protected Rectangle CurrentFrameRectangle;

        /// <summary>
        /// A read-only property that returns a rectangle representing the animated sprite's current collision bounds.
        /// </summary>
        public Rectangle Rectangle
        {
            get
            {
                return new Rectangle((int)Math.Floor(Position.X), (int)Math.Floor(Position.Y), FrameSize.X, FrameSize.Y);
            }
        }

        /// <summary>
        /// A read-only property that enforces that the sprite sheet should not be changed
        /// once the animated sprite is created.
        /// </summary>
        public Texture2D SpriteSheet
        {
            get
            {
                return this.InternalSheet;
            }
        }

        public int MillisecondsPerFrame { set; get; }

        /// <summary>
        /// A prot
        /// </summary>
        /// <param name="game"></param>
        protected AnimatedSprite(Game game)
        {
            this.Scale = 1.0f;
            this.Color = Color.White;
            this.Position = new Vector2(0, 0);
            CurrentFrame = new Point(0, 0);
            Repeat = true;

            InternalGame = game;

            Drawn = true;
            Updated = true;
        }

        /// <summary>
        /// A constructor accepting a game instance, a path to the sprite sheet, the size of the frames and
        /// the sprite arrangement on the sheet.
        /// </summary>
        /// <param name="game">
        /// The game to associate this animated sprite with.
        /// </param>
        /// <param name="texturePath">
        /// The path to the image to be used as the sprite sheet.
        /// </param>
        /// <param name="sizeOfFrames">
        /// The size of each frame to use.
        /// </param>
        /// <param name="sizeOfSheet">
        /// The size of the sheet represented in number of sprites.
        /// </param>
        public AnimatedSprite(Game game, String texturePath, Point sizeOfFrames, Point? sizeOfSheet)
        {
            this.Scale = 1.0f;
            this.Color = Color.White;
            this.Origin = new Vector2(0, 0);
            this.Position = new Vector2(0, 0);
            FrameSize = sizeOfFrames;
            CurrentFrame = new Point(0, 0);
            SheetSize = sizeOfSheet;
            Repeat = true;

            CurrentFrameRectangle = new Rectangle(CurrentFrame.X * FrameSize.X,
                                                    CurrentFrame.Y * FrameSize.Y,
                                                    FrameSize.X,
                                                    FrameSize.Y);

            this.MillisecondsPerFrame = 50;
            InternalGame = game;
            InternalTexturePath = texturePath;
        }

        /// <summary>
        /// Initializes the animated sprite by loading the sprite sheet.
        /// </summary>
        public virtual void Initialize()
        {
            this.InternalSheet = InternalGame.Content.Load<Texture2D>(InternalTexturePath);

            if (this.SheetSize == null)
                this.SheetSize = new Point(this.InternalSheet.Width / this.FrameSize.X, this.InternalSheet.Height / this.FrameSize.Y);
        }

        /// <summary>
        /// Updates the animated sprite by advancing the frames where necessary.
        /// </summary>
        /// <param name="time">
        /// The game time passed in by the game's main Update method.
        /// </param>
        public virtual void Update(GameTime time)
        {
            if (!Updated)
                return;

            TimeSinceLastFrame += time.ElapsedGameTime.Milliseconds;

            if (TimeSinceLastFrame >= MillisecondsPerFrame)
            {
                TimeSinceLastFrame -= MillisecondsPerFrame;

                ++CurrentFrame.X;

                Point sheetSize = (Point)SheetSize;
                if (CurrentFrame.X >= sheetSize.X)
                {
                    CurrentFrame.X = 0;
                    ++CurrentFrame.Y;

                    if (CurrentFrame.Y >= sheetSize.Y)
                    {
                        CurrentFrame.Y = 0;

                        if (!Repeat)
                            Updated = false;
                    }
                }

                CurrentFrameRectangle.X = CurrentFrame.X * FrameSize.X;
                CurrentFrameRectangle.Y = CurrentFrame.Y * FrameSize.Y;
            }
        }

        /// <summary>
        /// Draws the animated sprite to the screen buffer.
        /// </summary>
        /// <param name="batch">
        /// The sprite batch to draw to.
        /// </param>
        public virtual void Draw(SpriteBatch batch, Vector2? position = null)
        {
            if (!Drawn)
                return;

            if (position == null)
                position = Position + InternalGame.DrawOffset;

            batch.Draw(SpriteSheet,
                (Vector2)position,
                CurrentFrameRectangle,
                Color,
                Theta,
                Origin,
                Scale,
                Effects,
                0);
        }
    }
}
