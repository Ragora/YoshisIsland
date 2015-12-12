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

namespace ClassAssignment.GUI
{
    /// <summary>
    /// The CreditGUI's purpose is to tell the player who has made what. Who did the 
    /// programming and that all of the artwork belongs to Nintendo.
    /// </summary>
    class CreditsGUI : GUI
    {
        /// <summary>
        /// The GUI to be drawn when the player is viewing the credits
        /// section of the game.
        /// </summary>
        /// <param name="game">
        /// The game instance to associate this new CreditsGUI with.
        /// </param>
        public CreditsGUI(Game game) : base(game)
        {
            #region GUI Initialization
            Elements.Button BackButton = new Elements.Button(game, "fonts/Arial", "Back", "images/button_up", "images/button_down")
            {
                Position = new Vector2(320, 500),
                Responder = this.OnBack,
            };

            this.AddElement(BackButton);

            Elements.Text programmerText = new Elements.Text(game, "fonts/Arial")
            {
                DisplayText = "Programming by Robert MacGregor",
                Position = new Vector2(150, 200),
                BackgroundColor = Color.Black,
            };

            this.AddElement(programmerText);


            Elements.Text nintendoText = new Elements.Text(game, "fonts/Arial")
            {
                DisplayText = "Yoshi and Yoshi's Island is a Licensed Trademark of Nintendo",
                Position = new Vector2(150, 250),
                BackgroundColor = Color.Black,
            };

            this.AddElement(nintendoText);
            #endregion
        }

        /// <summary>
        /// Called when the GUI is first set to the active GUI.
        /// </summary>
        public override void OnWake()
        {
            InternalGame.CurrentState = Game.State.GUI;
            InternalGame.IsMouseVisible = true;
        }

        #region Button Responders
        /// <summary>
        /// Called when the player presses the "Back" button.
        /// </summary>
        private void OnBack()
        {
            GUIManager.SetGUI("main");
        }
        #endregion
    }
}
