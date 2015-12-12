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

namespace ClassAssignment
{
    /// <summary>
    /// The ControlledEntity class is a base class in which all interactive objects that exhibit player-like
    /// behavior derive from. 
    /// </summary>
    public class ControlledEntity : StateSprite
    {
        /// <summary>
        /// A static boolean representing whether or not the state debugger is enabled.
        /// </summary>
        public static bool StateDebugEnabled = false;

        /// <summary>
        /// A static boolean representing whether or not the physics debugger is enabled.
        /// </summary>
        public static bool PhysicsDebugEnabled = false;

        /// <summary>
        /// An enumeration that is used to control the horizontal movement of this given controlled entity, it's
        /// basically just a move state value.
        /// </summary>
        public enum HorizontalDirection
        {
            None = 0,
            Left = 1,
            Right = 2,
        };

        /// <summary>
        /// A class representing a discrete section of collision for a given entity. These discrete units are
        /// then assembled into a list to form a composite collision mesh of sorts with different actions taken
        /// based on which ones were collided with and with what entities.
        /// </summary>
        public class CollisionInformation
        {
            /// <summary>
            /// A delegate representing the method to be called when a collision has occurred.
            /// </summary>
            /// <param name="other">
            /// The collision information to be passed in.
            /// </param>
            public delegate void CollisionCallback(CollisionInformation other);

            /// <summary>
            /// The entity that this collision information block is associated with.
            /// </summary>
            public ControlledEntity Entity;

            /// <summary>
            /// The offset of this collision box from the entity's position.
            /// </summary>
            public Point Offset;

            /// <summary>
            /// The size of this discrete collision box.
            /// </summary>
            public Point Size;

            /// <summary>
            /// A reference to the collision responder delegate to be called when a collision has
            /// occurred.
            /// </summary>
            public CollisionCallback CollisionResponder;

            /// <summary>
            /// A read-only property that returns a rectangle that represents this collision box in the current
            /// frame.
            /// </summary>
            public Rectangle Box
            {
                get
                {
                    return new Rectangle((int)Entity.Position.X + Offset.X, (int)Entity.Position.Y + Offset.Y,
                        Size.X, Size.Y);
                }
            }

            /// <summary>
            /// A constructor accepting an entity, a offset and a size to produce a new collision
            /// box.
            /// </summary>
            /// <param name="entity">
            /// The entity to associate with.
            /// </param>
            /// <param name="offset">
            /// The offset this collision box will be from the given entity.
            /// </param>
            /// <param name="size">
            /// The size of the collision box.
            /// </param>
            public CollisionInformation(ControlledEntity entity, Point offset, Point size)
            {
                this.Offset = offset;
                this.Size = size;
                this.Entity = entity;
            }
        };

        public float MoveSpeed;

        /// <summary>
        /// A list of collision information used to make up the composite collision box of this controlled
        /// entity.
        /// </summary>
        public List<CollisionInformation> CollisionBoxes;

        /// <summary>
        /// The current moving velocity of this ControlledEntity.
        /// </summary>
        public Vector2 Velocity;

        /// <summary>
        /// A boolean representing whether or not the controlled entity is currently jumping.
        /// </summary>
        protected bool IsJumping;

        /// <summary>
        /// A boolean representing whether or not the controlled entity can make any horizontal moves.
        /// </summary>
        public bool CanMove;

        /// <summary>
        /// A boolean representing whether or not the controlled entity is dead.
        /// </summary>
        public bool Dead;

        /// <summary>
        /// The current direction that this controlled entity is currently moving in.
        /// </summary>
        public HorizontalDirection MoveDirection;

        /// <summary>
        /// The last move direction that this controlled entity was moving in.
        /// </summary>
        public HorizontalDirection LastMoveDirection;

        /// <summary>
        /// The direction the controlled entity was facing during its last jump.
        /// </summary>
        public HorizontalDirection JumpDirection;
        
        /// <summary>
        /// How long the controlled entity can jump for before starting to fall.
        /// </summary>
        public static float JumpSeconds = 1.4f;

        /// <summary>
        /// The last simulation time that the controlled entity jumped at.
        /// </summary>
        protected float LastJumpTime;

        /// <summary>
        /// The current simulation time for the controlled entity.
        /// </summary>
        protected float SimTime;
        
        /// <summary>
        /// How much horizontal velocity is lost to the ground per second when not moving?
        /// </summary>
        public float Traction;

        /// <summary>
        /// The maximum possible velocity that can be ever achieved by this controlled entity. The velocity is clamped
        /// between TerminalVelocity and -TerminalVelocity. Generally, this value shouldn't be toyed with.
        /// </summary>
        public Vector2 TermimalVelocity;

        /// <summary>
        /// A boolean representing whether or not this controlled entity is jumping.
        /// </summary>
        public bool Walking;

        /// <summary>
        /// The maximum possible velocity that this controlled entity can achieve when walking on solid ground.
        /// The X component of the velocity is clamped between MaxWalkingSpeed and -MaxWalkingSpeed.
        /// </summary>
        public float MaxWalkingSpeed;

        /// <summary>
        /// The strength of the jump that this controlled entity will use.
        /// </summary>
        public float JumpStrength;

        /// <summary>
        /// The texture drawn for collision debugging.
        /// </summary>
        private Texture2D DebugTexture;

        /// <summary>
        /// A read-only property that represents whether or not this controlled entity is currently falling.
        /// </summary>
        public bool Falling
        {
            get
            {
                return this.Velocity.Y > 0 && !this.Grounded;
            }
        }

        /// <summary>
        /// A read-only property that represents whether or not this controlled entity is currently on solid ground.
        /// </summary>
        public bool Grounded
        {
            get
            {
                Point tileDown = MapManager.PositionToTile(this.BottomEdge + new Vector2(0, 1));
                return MapManager.GetTile(tileDown).Solid;
            }
        }

        /// <summary>
        /// A constructor accepting a Game instance.
        /// </summary>
        /// 
        /// <param name="game">
        /// The Game instance that this controlled entity belongs to.
        /// </param>
        public ControlledEntity(Game game) : base(game)
        {
            Velocity = new Vector2(0, 100);
            Position = new Vector2(0, 0);

            IsJumping = false;

            Traction = 500.0f;
            TermimalVelocity = new Vector2(300, 600);

            MaxWalkingSpeed = 800.0f;
            JumpStrength = 80.0f;
            CanMove = true;
            MoveSpeed = 10;
            Walking = true;

            CollisionBoxes = new List<CollisionInformation>();
        }

        #region Action Functionality
        /// <summary>
        /// Causes the controlled entity to attempt to move leftwards.
        /// </summary>
        /// <param name="pressed">
        /// A boolean representing whether or not the controlled entity should move leftwards.
        /// </param>
        public virtual void MoveLeft(bool pressed)
        {
            MoveDirection = pressed && MoveDirection == HorizontalDirection.None ? HorizontalDirection.Left : HorizontalDirection.None;

            if (MoveDirection != HorizontalDirection.None)
                LastMoveDirection = MoveDirection;
        }

        /// <summary>
        /// Causes the controlled entity to attempt to move rightwards.
        /// </summary>
        /// <param name="pressed">
        /// A boolean representing whether or not the controlled entity should move rightwards.
        /// </param>
        public virtual void MoveRight(bool pressed)
        {
            MoveDirection = pressed && MoveDirection == HorizontalDirection.None ? HorizontalDirection.Right : HorizontalDirection.None;

            if (MoveDirection != HorizontalDirection.None)
                LastMoveDirection = MoveDirection;
        }

        /// <summary>
        /// Causes the controlled entity to jump if they are currently on the ground.
        /// </summary>
        /// <param name="pressed">
        /// A boolean representing whether or not to attempt a jump.
        /// </param>
        public virtual void Jump(bool pressed)
        {
            if (pressed && this.Grounded && !IsJumping)
            {
                Velocity -= new Vector2(0, this.JumpStrength);
                IsJumping = true;

                LastJumpTime = SimTime;
                JumpDirection = LastMoveDirection;
            }
            else if (IsJumping)
                IsJumping = false;
        }
        #endregion

        public override void Initialize()
        {
            base.Initialize();

            this.DebugTexture = InternalGame.Content.Load<Texture2D>("Images/white");
        }

        private static Color[] DebugColors =
        {
            new Color(100, 0, 0, 100),
            new Color(0, 100, 0, 100),
            new Color(0, 0, 100, 100),
        };

        public override void Draw(SpriteBatch batch, Vector2? position = default(Vector2?))
        {
            base.Draw(batch, position);

            if (Game.CollisionDebugEnabled)
            {
                int drawColor = -1;

                foreach (CollisionInformation collision in this.CollisionBoxes)
                {
                    Rectangle destination = new Rectangle((int)(this.Position.X + InternalGame.DrawOffset.X) + collision.Offset.X,
                        (int)(this.Position.Y + InternalGame.DrawOffset.Y) + collision.Offset.Y,
                        collision.Size.X, collision.Size.Y);

                    batch.Draw(this.DebugTexture, destination, null, DebugColors[++drawColor % DebugColors.Length]);
                }
            }

            if (ControlledEntity.StateDebugEnabled)
                batch.DrawString(InternalGame.Arial, String.Format("{0}:{1}", this.CurrentAnimationState.Name, this.CurrentFrameIdentifier), 
                    this.BottomEdge + new Vector2(-20, 0) + InternalGame.DrawOffset, Color.Red);

            if (ControlledEntity.PhysicsDebugEnabled)
            {
                batch.DrawString(InternalGame.Arial, String.Format("P {0:f2}, {1:f2}", this.Position.X, this.Position.Y),
                     this.TopEdge + new Vector2(-20, -60) + InternalGame.DrawOffset, Color.Red);

                batch.DrawString(InternalGame.Arial, String.Format("T {0}, {1}", this.TileCoordinates.X, this.TileCoordinates.Y),
                    this.TopEdge + new Vector2(-20, -40) + InternalGame.DrawOffset, Color.Red);

                batch.DrawString(InternalGame.Arial, String.Format("V {0:f2}, {1:f2}", this.Velocity.X, this.Velocity.Y),
                    this.TopEdge + new Vector2(-20, -20) + InternalGame.DrawOffset, Color.Red);
            }
        }

        /// <summary>
        /// A method called for when the controlled entity impacts a tile. This method should be
        /// overwritten by child classes to implement special functionality.
        /// </summary>
        /// <param name="tile">
        /// Information about the tile that was impacted.
        /// </param>
        public virtual void ImpactedTile(MapManager.TileInformation tile)
        {

        }

        /// <summary>
        /// Determines and sets which animation state to use for the controlled entity.
        /// </summary>
        /// <param name="successfulHorizontalMove">
        /// A boolean representing whether or not the controlled entity last made a successful horizontal
        /// move.
        /// </param>
        /// <param name="successfulVerticalMove">
        /// A boolean representing whether or not the controlled entity last made a successful vertical
        /// move.
        /// </param>
        public virtual void ProcessAnimation(bool successfulHorizontalMove, bool successfulVerticalMove)
        {
            if (!this.Grounded)
            {
                if (Velocity.Y < 0)
                    switch (LastMoveDirection)
                    {
                        case HorizontalDirection.None:
                        case HorizontalDirection.Left:
                            {
                                SetAnimationState("jumpleft");
                                break;
                            }

                        case HorizontalDirection.Right:
                            {
                                SetAnimationState("jumpright");
                                break;
                            }
                    }
                else if (CurrentAnimationState.Name != "fallleft" && CurrentAnimationState.Name != "fallright")
                {
                    switch (LastMoveDirection)
                    {
                        case HorizontalDirection.None:
                        case HorizontalDirection.Left:
                            {
                                SetAnimationState("falllefttransition");
                                break;
                            }

                        case HorizontalDirection.Right:
                            {
                                SetAnimationState("fallrighttransition");
                                break;
                            }
                    }
                }
            }
            else
                switch (MoveDirection)
                {
                    case HorizontalDirection.Left:
                        {
                            if (successfulHorizontalMove)
                                SetAnimationState("walkleft");
                            else
                                SetAnimationState("pushleft");
                            break;
                        }
                    case HorizontalDirection.Right:
                        {
                            if (successfulHorizontalMove)
                                SetAnimationState("walkright");
                            else
                                SetAnimationState("pushright");
                            break;
                        }
                    case HorizontalDirection.None:
                        {
                            if (Velocity.X == 0)
                            {
                                switch (LastMoveDirection)
                                {
                                    case HorizontalDirection.Right:
                                        {
                                            SetAnimationState("idleright");
                                            break;
                                        }
                                    case HorizontalDirection.None:
                                    case HorizontalDirection.Left:
                                        {
                                            SetAnimationState("idleleft");
                                            break;
                                        }
                                }
                            }
                            else
                            {
                                if (Velocity.X > 0)
                                    SetAnimationState("slideright");
                                else
                                    SetAnimationState("slideleft");
                            }

                            break;
                        }
                }
        }

        /// <summary>
        /// Updates the controlled entity, performing physics calculations and whatever input code there is
        /// to execute.
        /// </summary>
        /// <param name="time">
        /// The GameTime object passed into the program via its main Update method.
        /// </param>
        public override void Update(GameTime time)
        {
            if (!Updated)
                return;

            base.Update(time);

            if (Dead)
            {
                this.ProcessAnimation(false, false);
                return;
            }

            float deltaSeconds = (float)time.ElapsedGameTime.Milliseconds / 1000;
            SimTime += deltaSeconds;

            // Perform horizontal movement
            if (CanMove)
                switch (MoveDirection)
                {
                    case HorizontalDirection.Left:
                    {
                            if (this.Grounded)
                                Velocity -= new Vector2(this.MoveSpeed, 0);
                            else
                                Velocity -= new Vector2(1, 0);

                            break;
                    }
                    case HorizontalDirection.Right:
                    {
                            if (this.Grounded)
                                Velocity += new Vector2(this.MoveSpeed, 0);
                            else
                                Velocity += new Vector2(1, 0);

                            break;
                    }
                }

            // Modify our velocity if we're grounded
            if (this.Grounded && this.Walking)
            {
                float newX = Velocity.X;
                if (Velocity.X > 0)
                {
                    newX -= Traction * deltaSeconds;
                    newX = newX < 0 ? 0 : newX;
                }
                else if (Velocity.X < 0)
                {
                    newX += Traction * deltaSeconds;
                    newX = newX > 0 ? 0 : newX;
                }

                Velocity = new Vector2(newX, Velocity.Y > 0 ? 0 : Velocity.Y);

                float velocityX = Velocity.X;

                if (velocityX > MaxWalkingSpeed)
                    velocityX = MaxWalkingSpeed;
                else if (velocityX < -MaxWalkingSpeed)
                    velocityX = -MaxWalkingSpeed;

                Velocity = new Vector2(velocityX, Velocity.Y);
            }

            // Apply terminal velocity restrictions
            Velocity = new Vector2(Velocity.X < -TermimalVelocity.X ? -TermimalVelocity.X : Velocity.X, 
                                    Velocity.Y < -TermimalVelocity.Y ? -TermimalVelocity.Y : Velocity.Y);

            Velocity = new Vector2(Velocity.X > TermimalVelocity.X ? TermimalVelocity.X : Velocity.X,
                        Velocity.Y > TermimalVelocity.Y ? TermimalVelocity.Y : Velocity.Y);


            // Now a Y move
            Vector2 oldPosition = Position;

            bool successfulVerticalMove = true;
            if (Velocity.Y > 0 && !Grounded)
            {
                Vector2 moveDirection = new Vector2(0, Velocity.Y * deltaSeconds);
                Position += moveDirection;

                if (BottomTile.Solid)
                {
                    this.ImpactedTile(this.BottomTile);

                    Position = oldPosition;
                    Velocity = new Vector2(Velocity.X, 0);
                }
            }
            else if (Velocity.Y < 0)
            {
                Vector2 moveDirection = new Vector2(0, Velocity.Y * deltaSeconds);
                Position += moveDirection;

                if (TopTile.Solid)
                {
                    this.ImpactedTile(this.TopTile);

                    Position = oldPosition;

                    successfulVerticalMove = false;
                    Velocity = new Vector2(Velocity.X, 0);
                }
            }

            // Perform the X move
            oldPosition = Position;

            bool successfulHorizontalMove = true;
            if (Velocity.X < 0)
            {
                Vector2 moveDirection = new Vector2(Velocity.X * deltaSeconds, 0);
                Position += moveDirection;

                if (LeftTile.Solid)
                {
                    this.ImpactedTile(this.LeftTile);

                    Position = oldPosition;
                    Velocity = new Vector2(0, Velocity.Y);

                    successfulHorizontalMove = false;
                }
            }
            else if (Velocity.X > 0)
            {
                Vector2 moveDirection = new Vector2(Velocity.X * deltaSeconds, 0);
                Position += moveDirection;

                if (RightTile.Solid)
                {
                    this.ImpactedTile(this.RightTile);

                    Position = oldPosition;
                    Velocity = new Vector2(0, Velocity.Y);

                    successfulHorizontalMove = false;
                }
            }

            if (UnderneathTile.Solid)
            {
                this.ImpactedTile(this.UnderneathTile);
            }

            if (this.Grounded)
                this.IsJumping = false;

            this.ProcessAnimation(successfulHorizontalMove, successfulVerticalMove);
        }

        /// <summary>
        /// Gets and returns what tile is currently to the left of the controlled entity. This is determined
        /// using their left edge.
        /// </summary>
        /// <returns>
        /// The TileInformation structure of the tile to the left of the controlled entity.
        /// </returns>
        public MapManager.TileInformation LeftTile
        {
            get
            {
                MapManager.TileInformation tileEdge = MapManager.GetTile(MapManager.PositionToTile(this.LeftEdge));
                MapManager.TileInformation tileTop = MapManager.GetTile(MapManager.PositionToTile(this.TopLeftCorner));
                MapManager.TileInformation tileBottom = MapManager.GetTile(MapManager.PositionToTile(this.BottomLeftCorner));

                if (tileEdge.Solid)
                    return tileEdge;
                else if (tileTop.Solid)
                    return tileTop;
                else
                    return tileBottom;
            }
        }

        /// <summary>
        /// Gets and returns what tile is currently to the right of the controlled entity. This is determined
        /// using their right edge.
        /// </summary>
        /// <returns>
        /// The TileInformation structure of the tile to the right of the controlled entity.
        /// </returns>
        public MapManager.TileInformation RightTile
        {
            get
            {
                MapManager.TileInformation tileEdge = MapManager.GetTile(MapManager.PositionToTile(this.RightEdge));
                MapManager.TileInformation tileTop = MapManager.GetTile(MapManager.PositionToTile(this.TopRightCorner));
                MapManager.TileInformation tileBottom = MapManager.GetTile(MapManager.PositionToTile(this.BottomRightCorner));

                if (tileEdge.Solid)
                    return tileEdge;
                else if (tileTop.Solid)
                    return tileTop;
                else
                    return tileBottom;
            }
        }

        /// <summary>
        /// Gets and returns what tile is currently above the controlled entity. This is determined
        /// using their top edge.
        /// </summary>
        /// <returns>
        /// The TileInformation structure of the tile above the controlled entity.
        /// </returns>
        public MapManager.TileInformation TopTile
        {
            get
            {
                Point tileTop = MapManager.PositionToTile(this.TopEdge);
                return MapManager.GetTile(tileTop);
            }
        }

        /// <summary>
        /// Gets and returns what tile is currently below the controlled entity. This is determined
        /// using their bottom edge.
        /// </summary>
        /// <returns>
        /// The TileInformation structure of the tile below the controlled entity.
        /// </returns>
        public MapManager.TileInformation BottomTile
        {
            get
            {
                Point tileBottom = MapManager.PositionToTile(this.BottomEdge);
                return MapManager.GetTile(tileBottom);
            }
        }

        /// <summary>
        /// A read-only TileInformation representing the tile that is currently below the player.
        /// </summary>
        public MapManager.TileInformation UnderneathTile
        {
            get
            {
                Point tileBottom = MapManager.PositionToTile(this.BottomEdge + new Vector2(0, 10));

                return MapManager.GetTile(tileBottom);
            }
        }


        /// <summary>
        /// Gets and returns what tile the controlled entity is currently in. This is determined using their
        /// center.
        /// </summary>
        /// <returns>
        /// The TileInformation structure of the tile the controlled entity is currently in.
        /// </returns>
        public MapManager.TileInformation CurrentTile
        {
            get
            {
                Point tileCenter = MapManager.PositionToTile(this.Center);
                return MapManager.GetTile(tileCenter);
            }
        }

        /// <summary>
        /// Returns the coordinates of the tile that the controlled entity currently resides in. This is determined
        /// using their center.
        /// </summary>
        /// <returns>
        /// A Point representing the current tile coordinates of the controlled entity.
        /// </returns>
        public Point TileCoordinates
        {
            get
            {
                return MapManager.PositionToTile(this.Center);
            }
            set
            {
                this.Position = new Vector2(value.X * MapManager.TileDimensions.X, value.Y * MapManager.TileDimensions.Y);
            }
        }

        /// <summary>
        /// The current center of the controlled entity.
        /// </summary>
        public Vector2 Center
        {
            get
            {
                return new Vector2(Position.X + (Rectangle.Width / 2), Position.Y + (Rectangle.Height / 2));
            }
            set
            {
                this.Position = new Vector2(value.X - (Rectangle.Width / 2), value.Y - (Rectangle.Height / 2));
            }
        }

        /// <summary>
        /// A read-only vector2 representing the top left corner of the controlled entity.
        /// </summary>
        public Vector2 TopLeftCorner
        {
            get
            {
                return Position;
            }
        }

        /// <summary>
        /// A read-only vector2 representing the top bottom left corner of the controlled entity.
        /// </summary>
        public Vector2 BottomLeftCorner
        {
            get
            {
                return new Vector2(Position.X, Position.Y + Rectangle.Height);
            }
        }

        /// <summary>
        /// A read-only vector2 representing the top right corner of the controlled entity.
        /// </summary>
        public Vector2 TopRightCorner
        {
            get
            {
                return new Vector2(Position.X + Rectangle.Width, Position.Y);
            }
        }

        /// <summary>
        /// A read-only vector2 representing the top bottom right corner of the controlled entity.
        /// </summary>
        public Vector2 BottomRightCorner
        {
            get
            {
                return new Vector2(Position.X + Rectangle.Width, Position.Y + Rectangle.Height);
            }
        }

        /// <summary>
        /// A read-only vector2 representing the top edge of the controlled entity.
        /// </summary>
        public Vector2 TopEdge
        {
            get
            {
                return new Vector2(Position.X + (Rectangle.Width / 2), Position.Y);
            }
        }

        /// <summary>
        /// A read-only vector2 representing the bottom edge of the controlled entity.
        /// </summary>
        public Vector2 BottomEdge
        {
            get
            {
                return new Vector2(Position.X + (Rectangle.Width / 2), Position.Y + Rectangle.Height);
            }
        }

        /// <summary>
        /// A read-only vector2 representing the left edge of the controlled entity.
        /// </summary>
        public Vector2 LeftEdge
        {
            get
            {
                return new Vector2(Position.X, Position.Y + (Rectangle.Height / 2));
            }
        }

        /// <summary>
        /// A read-only vector2 representing the right edge of the controlled entity.
        /// </summary>
        public Vector2 RightEdge
        {
            get
            {
                return new Vector2(Position.X + Rectangle.Width, Position.Y + (Rectangle.Height / 2));
            }
        }
    }
}
