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
    /// A class representing the red coins that can be picked up throughout each level.
    /// </summary>
    public class RedCoin : AnimatedSprite
    {
        /// <summary>
        /// A constructor accepting a game instance and a texture path.
        /// </summary>
        /// <param name="game">
        /// The game instance to associate this red coin with.
        /// </param>
        /// <param name="texturePath">
        /// The path to the sprite sheet to be used for this red coin.
        /// </param>
        public RedCoin(Game game, String texturePath) : base(game, texturePath, new Point(50, 50), new Point(4, 1))
        {
            Updated = true;
            Drawn = true;

            MillisecondsPerFrame = 100;
        }
    }
}
