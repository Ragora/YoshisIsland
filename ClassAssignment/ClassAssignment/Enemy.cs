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
    public class Enemy : Player
    {
        private float LastJumpTime;

        public Enemy(Game game, String texturePath) : base(game, texturePath)
        {
            MoveDirection = HorizontalDirection.Left;
        }

        public override void Update(GameTime time)
        {
            base.Update(time);

            if (SimTime - LastJumpTime >= 5.0f && !IsJumping)
            {
                IsJumping = true;
                Velocity -= new Vector2(0, 150);
                LastJumpTime = SimTime;
            }

            Point currentTile = GetTile();
            switch (MoveDirection)
            {
                case HorizontalDirection.Left:
                {
                    Point nextGround = new Point(currentTile.X - 1, currentTile.Y + 1);

                    if (LeftTile() != 'g' || TileManager.Tiles[nextGround.X, nextGround.Y] == 'g')
                        MoveDirection = HorizontalDirection.Right;
                    break;
                }

                case HorizontalDirection.Right:
                {
                     Point nextGround = new Point(currentTile.X + 1, currentTile.Y + 1);

                     if (RightTile() != 'g' || TileManager.Tiles[nextGround.X, nextGround.Y] == 'g')
                        MoveDirection = HorizontalDirection.Left;
                     break;
                }
            }
        }
    }
}
