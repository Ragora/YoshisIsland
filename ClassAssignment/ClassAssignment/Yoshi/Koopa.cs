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
using System.Timers;

namespace ClassAssignment.Yoshi
{
    /// <summary>
    /// A class representing the koopa enemy type in the game.
    /// </summary>
    public class Koopa : ControlledEntity
    {
        /// <summary>
        /// A constructor accepting a game instance.
        /// </summary>
        /// <param name="game">
        /// The game instance to associate this Koopa with.
        /// </param>
        public Koopa(Game game) : base(game)
        {
            MoveDirection = HorizontalDirection.Left;

            MaxWalkingSpeed = 80.0f;
        }

        /// <summary>
        /// A boolean representing whether or not the Koopa is currently shelled.
        /// </summary>
        public bool Shelled;

        /// <summary>
        /// An overwritten walk boolean property that returns whether or not the Koopa is currently walking
        /// by also factoring in whether or not the Koopa is currently shelled.
        /// </summary>
        public new bool Walking
        {
            get
            {
                return base.Walking && !this.Shelled;
            }
            set
            {
                base.Walking = value;
            }
        }

        /// <summary>
        /// Initializes the Koopa by creating its animation states as well as loading the necessary textures.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Texture2D walkRightSheet = InternalGame.Content.Load<Texture2D>("Images/koopa_right");
            AnimationState walkLeft = this.AddAnimationState(walkRightSheet, "walkleft", new Point(0, 0), new Point(1, 0), new Point(50, 50), 8);
            walkLeft.Effects = SpriteEffects.FlipHorizontally;
            AnimationState idleLeft = this.AddAnimationState(walkRightSheet, "idleleft", new Point(7, 0), new Point(0, 0), new Point(50, 50), 1);
            idleLeft.Effects = SpriteEffects.FlipHorizontally;
            this.SetAnimationState("idleLeft");

            AnimationState idleRight = this.AddAnimationState(walkRightSheet, "idleright", new Point(7, 0), new Point(0, 0), new Point(50, 50), 1);
            AnimationState walkRight = this.AddAnimationState(walkRightSheet, "walkright", new Point(0, 0), new Point(1, 0), new Point(50, 50), 8);

            Texture2D wingedRightSheet = InternalGame.Content.Load<Texture2D>("Images/koopa_winged_right");
            AnimationState wingedLeft = this.AddAnimationState(wingedRightSheet, "wingedleft", new Point(0, 0), new Point(1, 0), new Point(50, 50), 5);
            wingedLeft.Effects = SpriteEffects.FlipHorizontally;

            this.AddAnimationState(wingedRightSheet, "wingedright", new Point(0, 0), new Point(1, 0), new Point(50, 50), 5);

            this.AddAnimationState(InternalGame.Content.Load<Texture2D>("Images/koopa_shell"), "shell", new Point(0, 0), new Point(1, 0), new Point(50, 50), 4);


            // Add a simple whole-body collision for Koopas
            CollisionInformation body = new CollisionInformation(this, new Point(0, 0), new Point(50, 50));
            body.CollisionResponder = this.KoopaCollision;
            this.CollisionBoxes.Add(body);

            this.MillisecondsPerFrame = 80;
        }

        #region Collision Responders
        /// <summary>
        /// A callback method that is called when the Koopa has made a collision with any other entity.
        /// </summary>
        /// <param name="info">
        /// The information of the entity collided with.
        /// </param>
        private void KoopaCollision(CollisionInformation info)
        {
            if (this.Shelled && info.Entity is Koopa && !info.Entity.Dead)
            {
                Koopa koopa = (Koopa)info.Entity;
                koopa.Dead = true;
                SoundManager.Play("enemydie");

                InternalGame.Score += 100;
            }
        }
        #endregion

        /// <summary>
        /// The method that is called to process the animation state of the Koopa.
        /// </summary>
        /// <param name="successfulHorizontalMove">
        /// A boolean representing whether or not the Koopa has made a successful horizontal move in this
        /// current frame.
        /// </param>
        /// <param name="successfulVerticalMove">
        /// A boolean representing whether or not the Koopa has made a successful vertical move in this 
        /// current frame.
        /// </param>
        public override void ProcessAnimation(bool successfulHorizontalMove, bool successfulVerticalMove)
        {
            if (!Shelled && !Dead)
                base.ProcessAnimation(successfulHorizontalMove, successfulVerticalMove);
            else if (!Dead)
                this.SetAnimationState("shell");
            else
                this.SetAnimationState("wingedright");
        }

        /// <summary>
        /// Updates the Koopa.
        /// </summary>
        /// <param name="time">
        /// The GameTime passed in by the game's main Update method.
        /// </param>
        public override void Update(GameTime time)
        {
            if (!Updated)
                return;

            base.Update(time);

            if (Dead)
            {
                this.CollisionBoxes.Clear();


                float deltaSeconds = (float)time.ElapsedGameTime.Milliseconds / 1000;
                this.Position += new Vector2(0, -130 * deltaSeconds);

                return;
            }


            Point currentTile = this.TileCoordinates;
            switch (MoveDirection)
            {
                case HorizontalDirection.Left:
                {
                    Point nextGround = new Point(currentTile.X - 1, currentTile.Y + 1);
                    Point nextTile = new Point(currentTile.X - 1, currentTile.Y);

                        if (MapManager.GetTile(nextTile).Solid)
                        {
                            MoveLeft(false);
                            MoveRight(true);
                        }

                        if (!Shelled && !MapManager.GetTile(nextGround).Solid)
                        {
                            MoveLeft(false);
                            MoveRight(true);
                        }
                    break;
                }

                case HorizontalDirection.Right:
                {
                    Point nextGround = new Point(currentTile.X + 1, currentTile.Y + 1);
                    Point nextTile = new Point(currentTile.X + 1, currentTile.Y);

                    if (MapManager.GetTile(nextTile).Solid)
                        {
                        MoveRight(false);
                        MoveLeft(true);
                    }

                    if (!Shelled && !MapManager.GetTile(nextGround).Solid)
                        {
                            MoveRight(false);
                            MoveLeft(true);
                        }
                    break;
                }

                case HorizontalDirection.None:
                {
                    MoveLeft(false);
                    MoveRight(true);
                    break;
                }
            }

            if (Shelled)
            {
                this.Traction = 0;
                this.Walking = false;
                this.TermimalVelocity = new Vector2(1000, this.TermimalVelocity.Y);

                switch (MoveDirection)
                {
                    case HorizontalDirection.Right:
                        {
                            this.Velocity = new Vector2(400, this.Velocity.Y);
                            break;
                        }
                    case HorizontalDirection.Left:
                        {
                            this.Velocity = new Vector2(-400, this.Velocity.Y);
                            break;
                        }
                    case HorizontalDirection.None:
                        {
                            this.Velocity = new Vector2(0, this.Velocity.Y);
                            break;
                        }
                }
            }
        }
    }
}
