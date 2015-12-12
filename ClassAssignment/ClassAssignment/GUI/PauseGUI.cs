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
    /// The PauseGUI is the GUI that is drawn when the player pauses the game.
    /// </summary>
    class PauseGUI : GUI
    {
        /// <summary>
        /// The GUI to be drawn when the game is paused.
        /// </summary>
        /// <param name="game">
        /// The game instance to associate this new CreditsGUI with.
        /// </param>
        public PauseGUI(Game game) : base(game)
        {
            #region GUI Initialization
            Elements.Button ResumeButton = new Elements.Button(game, "fonts/Arial", "Resume", "images/button_up", "images/button_down")
            {
                Position = new Vector2(120, 300),
                Responder = this.OnResume,
            };

            this.AddElement(ResumeButton);

            Elements.Button MenuButton = new Elements.Button(game, "fonts/Arial", "Main Menu", "images/button_up", "images/button_down")
            {
                Position = new Vector2(520, 300),
                Responder = this.OnMenu,
            };

            this.AddElement(MenuButton);

            Elements.Text PauseText = new Elements.Text(game, "fonts/Arial")
            {
                Position = new Vector2(320, 200),
                DisplayText = "*** PAUSED ***",
            };

            this.AddElement(PauseText);
            #endregion
        }

        #region Input Responders
        /// <summary>
        /// Called when the player attempts to press the "Start" button when viewing the PauseGUI.
        /// </summary>
        /// <param name="pressed">
        /// A boolean representing whether or not the button is currently pressed.
        /// </param>
        public void Unpause(bool pressed)
        {
            if (pressed)
                GUIManager.SetGUI("play");
        }
        #endregion

        #region Button Responders
        /// <summary>
        /// Called when the player presses the "Main Menu" button.
        /// </summary>
        private void OnMenu()
        {
            GUIManager.SetGUI("main");
        }

        /// <summary>
        /// Called when the player presses the "Resume" button.
        /// </summary>
        private void OnResume()
        {
            GUIManager.SetGUI("play");
        }
        #endregion

        /// <summary>
        /// Called whenn the GUI is first set to the active GUI.
        /// </summary>
        public override void OnWake()
        {
            InternalGame.IsMouseVisible = true;
            InternalGame.CurrentState = Game.State.Pause;

            SoundManager.Play("pause");

            GUIManager.BindControllerListeners();
            InputManager.StartButtonListener = this.Unpause;
            InputManager.SetKeyResponder(Keys.Escape, this.Unpause);

            SoundManager.PauseMusic(true);
        }
    }
}
