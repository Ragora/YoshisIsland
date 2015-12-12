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
    /// A class representing the primary player controlled object in the game.
    /// </summary>
    public class Player : ControlledEntity
    {
        /// <summary>
        /// A boolean representing whether or not the player is currently fluttering
        /// or has previously fluttered in their current airborne state.
        /// </summary>
        private bool IsFluttering;

        /// <summary>
        /// A boolean representing whether or not the player is currently crouching.
        /// </summary>
        public bool IsCrouching;

        /// <summary>
        /// The strength of the flutter affects how much height the player gains from
        /// performing a successful flutter jump.
        /// </summary>
        public float FlutterStrength;

        /// <summary>
        /// A boolean representing whether or not Yoshi is attacking.
        /// </summary>
        public bool IsAttacking;

        /// <summary>
        /// The enemy yoshi has captured with his tongue.
        /// </summary>
        private ControlledEntity CapturedEnemy;

        /// <summary>
        /// The texture used to render tongue segments.
        /// </summary>
        Texture2D TongueSegmentTexture;

        /// <summary>
        /// The sprite used for the tongue tip.
        /// </summary>
        private Sprite TongueTip;

        /// <summary>
        /// The list of sprites for each tongue segment.
        /// </summary>
        private List<Sprite> TongueSegments;

        private ControlledEntity.CollisionInformation TongueCollision;

        /// <summary>
        /// Constructor accepting a game instance.
        /// </summary>
        /// <param name="game">
        /// The game instance to associate with this new player object.
        /// </param>
        public Player(Game game) : base(game)
        {
            this.JumpStrength = 400;
            this.FlutterStrength = 500;
            this.MaxWalkingSpeed = 3000;
            this.Traction = 350;
            this.MoveSpeed = 8;
        }

        /// <summary>
        /// Binds the player controls to the input manager. This is mostly used
        /// for when you're transitioning control focus to the player object.
        /// </summary>
        public void BindControls()
        {
            InputManager.SetKeyResponder(Keys.D, this.MoveRight);
            InputManager.SetKeyResponder(Keys.A, this.MoveLeft);
            InputManager.SetKeyResponder(Keys.W, this.Jump);
            InputManager.SetKeyResponder(Keys.S, this.Crouch);
            InputManager.SetKeyResponder(Keys.Escape, this.Pause);
            InputManager.SetKeyResponder(Keys.Space, this.Attack);

            InputManager.AButtonListener = this.Jump;
            InputManager.LeftStickListener = this.ControlStickListener;
            InputManager.StartButtonListener = this.Pause;
            InputManager.RightTriggerListener = this.RightTriggerListener;
        }

        /// <summary>
        /// Unbinds the player controls from the input manager. This is mostly used
        /// for when you're transitioning control focus from the player object to something else.
        /// </summary>
        public void UnbindControls()
        {
            InputManager.SetKeyResponder(Keys.D, null);
            InputManager.SetKeyResponder(Keys.A, null);
            InputManager.SetKeyResponder(Keys.W, null);
            InputManager.SetKeyResponder(Keys.S, null);
            InputManager.SetKeyResponder(Keys.Escape, null);
            InputManager.SetKeyResponder(Keys.Space, null);

            InputManager.AButtonListener = null;
            InputManager.LeftStickListener = null;
            InputManager.StartButtonListener = null;
            InputManager.RightTriggerListener = null;
        }

        #region Input Responders
        public void RightTriggerListener(float pressure)
        {
            if (pressure >= 0.5f && !this.IsAttacking)
                this.Attack(true);
            else if (this.IsAttacking && pressure < 0.5f)
                this.Attack(false);
        }

        /// <summary>
        /// Sets the IsAttacking boolean to true if the controlled entity is currently on the ground
        /// and the pressed boolean is true.
        /// </summary>
        /// <param name="pressed">
        /// A boolean representing whether or not the controlled entity should attempt an attack.
        /// </param>
        public void Attack(bool pressed)
        {
            if (pressed && this.Grounded && this.Velocity.X <= 2.0f)
            {
                IsAttacking = true;
                this.CollisionBoxes.Remove(this.TongueCollision);
                this.CollisionBoxes.Add(this.TongueCollision);

                this.TongueSegments.Clear();
                SoundManager.Play("yoshitongue");

                switch(this.LastMoveDirection)
                {
                    case HorizontalDirection.None:
                    case HorizontalDirection.Left:
                        {
                            SetAnimationState("attackleft");
                            break;
                        }

                    case HorizontalDirection.Right:
                        {
                            SetAnimationState("attackright");
                            break;
                        }

                }
            }
            else if (!pressed && IsAttacking)
            {
                switch (this.LastMoveDirection)
                {
                    case HorizontalDirection.None:
                    case HorizontalDirection.Left:
                        {
                            SetAnimationState("endattackleft");
                            break;
                        }

                    case HorizontalDirection.Right:
                        {
                            SetAnimationState("endattackright");
                            break;
                        }

                }
            }
        }

        /// <summary>
        /// Called when the player attempts to pause the game.
        /// </summary>
        /// <param name="pressed">
        /// A boolean representing the current button state.
        /// </param>
        private void Pause(bool pressed)
        {
            if (pressed && !Dead)
                GUI.GUIManager.SetGUI("pause");
        }

        /// <summary>
        /// Called each frame to pump the current status of the left
        /// control stick to the player code.
        /// </summary>
        /// <param name="x">
        /// How far to the left or to the right the control stick is sitting (idle = 0).
        /// </param>
        /// <param name="y">
        /// How far up or down the control stick is sitting (idle = 0).
        /// </param>
        private void ControlStickListener(float x, float y)
        {
            if (x > 0.5f)
            {
                this.MoveLeft(false);
                this.MoveRight(true);   
            }
            else if (x < -0.5f)
            {
                this.MoveRight(false);
                this.MoveLeft(true);
            }
            else
            {
               this.MoveLeft(false);
               this.MoveRight(false);
            }

            if (y < -0.5f)
                this.Crouch(true);
            else if (IsCrouching)
                this.Crouch(false);
        }

        /// <summary>
        /// Called when the player attempts to move left on the screen.
        /// </summary>
        /// <param name="pressed">
        /// A boolean representing whether or not the player is currently trying to move left.
        /// </param>
        public override void MoveLeft(bool pressed)
        {
            if (Dead)
                return;

            if (CurrentAnimationState.Name == "slideleft" || CurrentAnimationState.Name == "slideright")
                return;

            base.MoveLeft(pressed);
        }

        /// <summary>
        /// Called when the player attempts to move right on the screen.
        /// </summary>
        /// <param name="pressed">
        /// A boolean representing whether or not the player is currently trying to move right.
        /// </param>
        public override void MoveRight(bool pressed)
        {
            if (Dead)
                return;

            if (CurrentAnimationState.Name == "slideleft" || CurrentAnimationState.Name == "slideright")
                return;

            base.MoveRight(pressed);
        }

        /// <summary>
        /// Called when the player is attempting to crouch.
        /// </summary>
        /// <param name="pressed">
        /// A boolean representing whether or not the player is still trying to crouch.
        /// </param>
        public void Crouch(bool pressed)
        {
            if (Dead)
                return;

            IsCrouching = this.Grounded && pressed;
            CanMove = !IsCrouching;

            if (IsCrouching && UnderneathTile.SpecialFunction == MapManager.SPECIAL_FUNCTION.ADVANCE_LEVEL)
                InternalGame.AdvanceLevel();

            MoveDirection = HorizontalDirection.None;
        }

        /// <summary>
        /// Called when the player attempts to jump.
        /// </summary>
        /// <param name="pressed">
        /// A boolean representing whether or not the player is currently trying to jump.
        /// </param>
        public override void Jump(bool pressed)
        {
            if (Dead)
                return;

            if (pressed && this.Grounded)
            {
                Velocity -= new Vector2(0, this.JumpStrength);
                IsJumping = true;
                IsFluttering = false;

                LastJumpTime = SimTime;
                JumpDirection = LastMoveDirection;

                SoundManager.Play("yoshijump");
            }
            else if (pressed && Falling && IsJumping && !IsFluttering)
            {
                Velocity -= new Vector2(0, this.FlutterStrength);
                IsJumping = true;
                IsFluttering = true;

                LastJumpTime = SimTime;
                JumpDirection = LastMoveDirection;

                SoundManager.Play("yoshiflutter");
            }
        }
        #endregion

        /// <summary>
        /// Updates the player object by updating its base and running some player specific
        /// game logic.
        /// </summary>
        /// <param name="time">
        /// The GameTime passed in by the game's main Update function.
        /// </param>
        public override void Update(GameTime time)
        {
            if (!Updated)
                return;

            base.Update(time);

            if (this.IsAttacking)
            {
                Vector2 tongueStart = this.RightEdge;
                float tongueDirection = 1.0f;

                switch (this.LastMoveDirection)
                {
                    case HorizontalDirection.None:
                    case HorizontalDirection.Left:
                        {
                            this.TongueTip.Effects = SpriteEffects.FlipHorizontally;
                            tongueStart = this.LeftEdge;

                            tongueDirection = -tongueDirection;


                            Vector2 tongueTipOffset = -(this.Position - this.TongueTip.Position);
                            this.TongueCollision.Offset = new Point((int)tongueTipOffset.X, (int)tongueTipOffset.Y);
                            this.TongueCollision.Size = new Point(this.TongueSegmentTexture.Width * (this.TongueSegments.Count + 1), this.TongueSegmentTexture.Height);

                            break;
                        }

                    case HorizontalDirection.Right:
                        {
                            this.TongueTip.Effects = SpriteEffects.None;
                            this.TongueTip.Position = this.RightEdge;

                            this.TongueCollision.Offset = new Point(50, 25);
                            this.TongueCollision.Size = new Point(this.TongueSegmentTexture.Width * (this.TongueSegments.Count + 1), this.TongueSegmentTexture.Height);
                            break;
                        }
                }

                this.TongueTip.Position = tongueStart;
                this.TongueTip.Position = new Vector2(this.TongueTip.Position.X + (this.TongueSegments.Count * this.TongueSegmentTexture.Width * tongueDirection), this.TongueTip.Position.Y);

                for (int iteration = 0; iteration < this.TongueSegments.Count; iteration++)
                {
                    Sprite segment = this.TongueSegments[iteration];
                    segment.Position = tongueStart;
                    segment.Position = new Vector2(segment.Position.X + (this.TongueSegmentTexture.Width * iteration * tongueDirection), segment.Position.Y);
                }
            }

            if (this.Grounded)
                IsFluttering = false;

            if (this.Position.Y >= MapManager.TileDimensions.Y * (MapManager.ForegroundTiles.GetUpperBound(1) + 6))
                InternalGame.CurrentState = Game.State.Death;
        }

        /// <summary>
        /// Called when the player should process their animation.
        /// </summary>
        /// <param name="successfulHorizontalMove">
        /// A boolean representing whether or not the player had a successful horizontal move in this
        /// frame.
        /// </param>
        public override void ProcessAnimation(bool successfulHorizontalMove, bool successfulVerticalMove)
        {
            if (Dead)
            {
                SetAnimationState("death");
                return;
            }

            if (IsAttacking)
                return;
            else if (!this.Grounded)
            {
                if (Velocity.Y < 0 && successfulVerticalMove)
                    switch (LastMoveDirection)
                    {
                        case HorizontalDirection.None:
                        case HorizontalDirection.Left:
                            {
                                if (IsFluttering)
                                    SetAnimationState("flutterleft");
                                else
                                    SetAnimationState("jumpleft");
                                break;
                            }

                        case HorizontalDirection.Right:
                            {
                                if (IsFluttering)
                                    SetAnimationState("flutterright");
                                else
                                    SetAnimationState("jumpright");
                                break;
                            }
                    }
                else if (!successfulVerticalMove)
                    switch (LastMoveDirection)
                    {
                        case HorizontalDirection.None:
                        case HorizontalDirection.Left:
                            {
                                SetAnimationState("smashleft");
                                break;
                            }

                        case HorizontalDirection.Right:
                            {
                                SetAnimationState("smashright");
                                break;
                            }
                    }
            }
            else if (this.IsCrouching)
            {
                switch (LastMoveDirection)
                {
                    case HorizontalDirection.None:
                    case HorizontalDirection.Left:
                        {
                            SetAnimationState("crouchleft");
                            break;
                        }

                    case HorizontalDirection.Right:
                        {
                            SetAnimationState("crouchright");
                            break;
                        }
                }
            }

            base.ProcessAnimation(successfulHorizontalMove, successfulVerticalMove);
        }

        /// <summary>
        /// Called when the player has impacted a map tile. This is used for processing events based on
        /// tile collision.
        /// </summary>
        /// <param name="tile">
        /// The tile information of the tile we have impacted.
        /// </param>
        public override void ImpactedTile(MapManager.TileInformation tile)
        {
            base.ImpactedTile(tile);

            if (tile.Lethal)
            {
                this.Dead = true;
                InternalGame.CurrentState = Game.State.Death;
            }
        }

        #region Collision Responders
        /// <summary>
        /// Collision responder for Yoshi's body, used for determining deaths against
        /// hostile entities.
        /// </summary>
        /// <param name="other">
        /// The information of the other entity possibly killing Yoshi.
        /// </param>
        private void BodyCollisionResponder(CollisionInformation other)
        {
            this.Dead = true;
            InternalGame.CurrentState = Game.State.Death;
        }

        /// <summary>
        /// Collision responder for Yoshi's feet used in stomping enemies.
        /// </summary>
        /// <param name="other">
        /// The information of the other entity possibly getting stomped on.
        /// </param>
        private void FeetCollisionResponder(CollisionInformation other)
        {
            if (this.Dead)
                return;

            this.Velocity = new Vector2(this.Velocity.X, -200);

            Yoshi.Koopa stomped = (Yoshi.Koopa)other.Entity;
            SoundManager.Play("yoshistomp");

            this.IsJumping = false;
            this.IsFluttering = false;

            if (stomped.Shelled && stomped.MoveDirection != HorizontalDirection.None)
            {
                stomped.MoveLeft(false);
                stomped.MoveRight(false);
            }
            else if (stomped.Shelled)
            {
                stomped.MoveLeft(false);
                stomped.MoveRight(true);
            }
            else
                stomped.Shelled = true;
        }

        /// <summary>
        /// Collision responder for Yoshi's tongue used in eating enemies.
        /// </summary>
        /// <param name="other">
        /// The information of the other entity possibly getting eaten
        /// </param>
        private void TongueCollisionResponder(CollisionInformation other)
        {
            if (other.Entity is Yoshi.Koopa)
            {
                other.Entity.Drawn = true;
                other.Entity.Updated = false;
                CapturedEnemy = other.Entity;

                switch (this.LastMoveDirection)
                {
                    case HorizontalDirection.None:
                    case HorizontalDirection.Left:
                        {
                            this.SetAnimationState("endattackleft");
                            break;
                        }
                    case HorizontalDirection.Right:
                        {
                            this.SetAnimationState("endattackright");
                            break;
                        }
                }
            }
        }
        #endregion

        #region State Responders
        /// <summary>
        /// Method called for each animation tick that Yoshi's tongue should be extending.
        /// </summary>
        private void TongueLengtheningTick()
        {
            if (this.TongueSegments.Count >= 5 || MapManager.GetTile(MapManager.PositionToTile(this.TongueTip.Position)).Solid)
            {
                switch (this.LastMoveDirection)
                {
                    case HorizontalDirection.None:
                    case HorizontalDirection.Left:
                        {
                            this.SetAnimationState("endattackleft");
                            break;
                        }
                    case HorizontalDirection.Right:
                        {
                            this.SetAnimationState("endattackright");
                            break;
                        }
                }

                return;
            }

            CanMove = false;
            Sprite segment = new Sprite(this.InternalGame, "Images/yoshi_tongue_segment");
            segment.Initialize();

            this.TongueSegments.Add(segment);
        }

        /// <summary>
        /// Method called for each animation tick that Yoshi's tongue should be shortened.
        /// </summary>
        private void TongueShorteningTick()
        {
            if (this.TongueSegments.Count != 0)
            {
                if (CapturedEnemy != null)
                    CapturedEnemy.Center = this.TongueTip.Position;

                this.TongueSegments.RemoveAt(0);
            }
            else
            {
                this.CollisionBoxes.Remove(this.TongueCollision);

                this.CanMove = true;
                this.IsAttacking = false;

                if (CapturedEnemy != null)
                {
                    CapturedEnemy.Drawn = false;
                    CapturedEnemy.CollisionBoxes.Clear();

                    InternalGame.Score += 200;
                    SoundManager.Play("enemydie");
                }
                CapturedEnemy = null;
            }
        }
  
        /// <summary>
        /// Called at the start of the state in which Yoshi cracks his skull off the
        /// ceiling.
        /// </summary>
        public void OnSmashStateStart()
        {
            SoundManager.Play("yoshiceiling");
        }

        /// <summary>
        /// Called at the start of the state in which Yoshi pushes up against a wall.
        /// </summary>
        public void OnPushStateStart()
        {
            SoundManager.Play("yoshipush");
        }

        /// <summary>
        /// Called by the fall transition animation state to advance Yoshi into the
        /// falling animation.
        /// </summary>
        private void FallLeftTransition()
        {
            SetAnimationState("fallleft");
        }

        /// <summary>
        /// Called by the fall transition animation state to advance Yoshi into the
        /// falling animation.
        /// </summary>
        private void FallRightTransition()
        {
            SetAnimationState("fallright");
        }
        #endregion

        /// <summary>
        /// An overwritten draw method used for drawing Yoshi's tongue as a separate entity.
        /// </summary>
        /// <param name="batch">
        /// The sprite batch to draw to.
        /// </param>
        /// <param name="position">
        /// The position to draw to.
        /// </param>
        public override void Draw(SpriteBatch batch, Vector2? position = default(Vector2?))
        {
            if (this.IsAttacking)
            {
                this.TongueTip.Draw(batch, position);
                foreach (Sprite sprite in this.TongueSegments)
                    sprite.Draw(batch, position);
            }

            base.Draw(batch, position);
        }

        /// <summary>
        /// Initializes the player by calling the base initialize and by initializing the complex animation state
        /// system used by the player.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            #region Initialize Animation States

            this.TongueSegmentTexture = InternalGame.Content.Load<Texture2D>("Images/yoshi_tongue_segment");

            this.TongueTip = new Sprite(this.InternalGame, "Images/yoshi_tongue_tip");
            this.TongueTip.Initialize();
            this.TongueCollision = new ControlledEntity.CollisionInformation(this, new Point(0, 0), new Point(0, 0));
            this.TongueCollision.CollisionResponder = this.TongueCollisionResponder;

            this.TongueSegments = new List<Sprite>();

            Texture2D yoshiSheet = InternalGame.Content.Load<Texture2D>("Images/yoshi");

            // Idling
            AnimationState idleLeft = this.AddAnimationState(yoshiSheet, "idleleft", new Point(3, 0), new Point(0, 0), new Point(50, 50), 1);
            idleLeft.Effects = SpriteEffects.FlipHorizontally;
            this.AddAnimationState(yoshiSheet, "idleright", new Point(3, 0), new Point(0, 0), new Point(50, 50), 1);

            // Walking
            AnimationState walkRight = this.AddAnimationState(yoshiSheet, "walkright", new Point(0, 3), new Point(1, 0), new Point(50, 50), 2);
            walkRight.MillisecondsPerFrame = 70;

            AnimationState walkLeft = this.AddAnimationState(yoshiSheet, "walkleft", new Point(0, 3), new Point(1, 0), new Point(50, 50), 2);
            walkLeft.MillisecondsPerFrame = 70;
            walkLeft.Effects = SpriteEffects.FlipHorizontally;

            // Sliding
            AnimationState slideLeft = this.AddAnimationState(yoshiSheet, "slideleft", new Point(1, 0), new Point(0, 0), new Point(50, 50), 1);
            slideLeft.Effects = SpriteEffects.FlipHorizontally;

            this.AddAnimationState(yoshiSheet, "slideright", new Point(1, 0), new Point(0, 0), new Point(50, 50), 1);

            // Pushing
            AnimationState pushRight = this.AddAnimationState(yoshiSheet, "pushright", new Point(0, 1), new Point(1, 0), new Point(50, 50), 3);
            pushRight.StateStartListener = this.OnPushStateStart;

            AnimationState pushLeft = this.AddAnimationState(yoshiSheet, "pushleft", new Point(0, 1), new Point(1, 0), new Point(50, 50), 3);
            pushLeft.StateStartListener = this.OnPushStateStart;
            pushLeft.Effects = SpriteEffects.FlipHorizontally;

            // Jumping
            AnimationState jumpLeft = this.AddAnimationState(yoshiSheet, "jumpLeft", new Point(2, 0), new Point(0, 0), new Point(50, 50), 1);
            jumpLeft.Effects = SpriteEffects.FlipHorizontally;

            this.AddAnimationState(yoshiSheet, "jumpright", new Point(2, 0), new Point(0, 0), new Point(50, 50), 1);

            // Crouching
            this.AddAnimationState(yoshiSheet, "crouchright", new Point(6, 0), new Point(1, 0), new Point(50, 50), 1);

            AnimationState crouchLeft = this.AddAnimationState(yoshiSheet, "crouchleft", new Point(6, 0), new Point(1, 0), new Point(50, 50), 1);
            crouchLeft.Effects = SpriteEffects.FlipHorizontally;

            // Attacking
            AnimationState attackLeft = this.AddAnimationState(yoshiSheet, "attackleft", new Point(4, 1), new Point(0, 0), new Point(50, 50), 1);
            attackLeft.Looping = true;
            attackLeft.Effects = SpriteEffects.FlipHorizontally;
            attackLeft.StateEndListener = this.TongueLengtheningTick;
            attackLeft.MillisecondsPerFrame = 25;

            AnimationState attackRight = this.AddAnimationState(yoshiSheet, "attackright", new Point(4, 1), new Point(0, 0), new Point(50, 50), 1);
            attackRight.Looping = true;
            attackRight.StateEndListener = this.TongueLengtheningTick;
            attackRight.MillisecondsPerFrame = 25;

            AnimationState endedAttackLeft = this.AddAnimationState(yoshiSheet, "endattackleft", new Point(4, 1), new Point(0, 0), new Point(50, 50), 1);
            endedAttackLeft.Looping = true;
            endedAttackLeft.Effects = SpriteEffects.FlipHorizontally;
            endedAttackLeft.StateEndListener = this.TongueShorteningTick;
            endedAttackLeft.StateLock = true;
            endedAttackLeft.MillisecondsPerFrame = 25;
            endedAttackLeft.IncompatibleTransitions.Add("attackleft");
            endedAttackLeft.IncompatibleTransitions.Add("attackright");

            AnimationState endAttackRight = this.AddAnimationState(yoshiSheet, "endattackright", new Point(4, 1), new Point(0, 0), new Point(50, 50), 1);
            endAttackRight.Looping = true;
            endAttackRight.StateLock = true;
            endAttackRight.StateEndListener = this.TongueShorteningTick;
            endAttackRight.MillisecondsPerFrame = 25;
            endAttackRight.IncompatibleTransitions.Add("attackleft");
            endAttackRight.IncompatibleTransitions.Add("attackright");

            // Falling
            AnimationState fallLeft = this.AddAnimationState(yoshiSheet, "fallleft", new Point(4, 0), new Point(1, 0), new Point(50, 50), 1);
            fallLeft.Effects = SpriteEffects.FlipHorizontally;

            this.AddAnimationState(yoshiSheet, "fallright", new Point(4, 0), new Point(1, 0), new Point(50, 50), 1);

            // Falling transitions
            AnimationState fallLeftTransition = this.AddAnimationState(yoshiSheet, "falllefttransition", new Point(5, 0), new Point(0, 0), new Point(50, 50), 1);
            fallLeftTransition.MillisecondsPerFrame = 1200;
            fallLeftTransition.StateEndListener = FallLeftTransition;
            fallLeftTransition.Effects = SpriteEffects.FlipHorizontally;

            AnimationState fallRightTransition = this.AddAnimationState(yoshiSheet, "fallrighttransition", new Point(5, 0), new Point(0, 0), new Point(50, 50), 1);
            fallRightTransition.MillisecondsPerFrame = 1200;
            fallRightTransition.StateEndListener = FallRightTransition;

            // Flutter
            AnimationState flutterLeft = this.AddAnimationState(yoshiSheet, "flutterleft", new Point(0, 2), new Point(1, 0), new Point(50, 50), 2);
            flutterLeft.MillisecondsPerFrame = 100;
            flutterLeft.IncompatibleTransitions.Add("jumpleft");
            flutterLeft.IncompatibleTransitions.Add("jumpright");
            flutterLeft.Effects = SpriteEffects.FlipHorizontally;

            AnimationState flutterRight = this.AddAnimationState(yoshiSheet, "flutterright", new Point(0, 2), new Point(1, 0), new Point(50, 50), 2);
            flutterRight.MillisecondsPerFrame = 100;
            flutterRight.IncompatibleTransitions.Add("jumpleft");
            flutterRight.IncompatibleTransitions.Add("jumpright");

            // Death
            AnimationState death = this.AddAnimationState(yoshiSheet, "death", new Point(0, 4), new Point(1, 0), new Point(50, 50), 5);
            death.Looping = false;
            death.MillisecondsPerFrame = 200;

            // Head Smash
            AnimationState smashLeft = this.AddAnimationState(yoshiSheet, "smashleft", new Point(0, 0), new Point(1, 0), new Point(50, 50), 1);
            smashLeft.MillisecondsPerFrame = 350;
            smashLeft.Looping = false;
            smashLeft.StateLock = true;
            smashLeft.StateStartListener = OnSmashStateStart;
            smashLeft.Effects = SpriteEffects.FlipHorizontally;

            AnimationState smashRight = this.AddAnimationState(yoshiSheet, "smashright", new Point(0, 0), new Point(1, 0), new Point(50, 50), 1);
            smashRight.MillisecondsPerFrame = 350;
            smashRight.Looping = false;
            smashRight.StateLock = true;
            smashRight.StateStartListener = OnSmashStateStart;
            #endregion

            #region Initialize Collision Regions
            CollisionInformation body = new CollisionInformation(this, new Point(0, 0), new Point(50, 40));
            body.CollisionResponder = this.BodyCollisionResponder;
            this.CollisionBoxes.Add(body);

            CollisionInformation feet = new CollisionInformation(this, new Point(3, 40), new Point(45, 10));
            feet.CollisionResponder = this.FeetCollisionResponder;
            this.CollisionBoxes.Add(feet);
            #endregion

            this.SetAnimationState("idleleft");
        }
    }
}
