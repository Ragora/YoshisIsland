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
    /// The MainGUI is the GUI that is rendered when the game is first started. It is pretty
    /// much the GUI that the game will default to when the player completes the game, etc.
    /// </summary>
    class MainGUI : GUI
    {
        /// <summary>
        /// The GUI to be drawn when the player is viewing the main menu.
        /// </summary>
        /// <param name="game">
        /// The game instance to associate this new MainGUI with.
        /// </param>
        public MainGUI(Game game) : base(game)
        {
            #region GUI Initialization
            Elements.Button ExitButton = new Elements.Button(game, "fonts/Arial", "Exit", "images/button_up", "images/button_down")
            {
                Position = new Vector2(100, 500),
                Responder = this.OnExit,
            };

            this.AddElement(ExitButton);

            Elements.Button CreditsButton = new Elements.Button(game, "fonts/Arial", "Credits", "images/button_up", "images/button_down")
            {
                Position = new Vector2(100, 300),
                Responder = this.OnCredits,
            };

            this.AddElement(CreditsButton);

            Elements.Button InstructionsButton = new Elements.Button(game, "fonts/Arial", "Instructions", "images/button_up", "images/button_down")
            {
                Position = new Vector2(100, 200),
                Responder = this.OnInstructions,
            };

            this.AddElement(InstructionsButton);

            Elements.Button PlayButton = new Elements.Button(game, "fonts/Arial", "Play", "images/button_up", "images/button_down")
            {
                Position = new Vector2(100, 100),
                Responder = this.OnPlay,
            };

            this.AddElement(PlayButton);

            // Place some hint text on the main screen for basic navigational tips
            Elements.Text hintText = new Elements.Text(game, "fonts/Arial")
            {
                DisplayText = "<Hint Unset>",
                Position = new Vector2(150, 50),
                BackgroundColor = Color.Black,
            };

            if (InputManager.UseController)
                hintText.DisplayText = "Hint: Use the directional pad to cycle buttons and press A to activate.";
            else
                hintText.DisplayText = "Hint: Mouse over the buttons and click to activate.";

            this.AddElement(hintText);
            #endregion
        }

        #region Button Responders
        /// <summary>
        /// Called when the player hits the "Play" button.
        /// </summary>
        private void OnPlay()
        {
            InternalGame.Restart();

            InternalGame.CurrentState = Game.State.Play;
            GUIManager.SetGUI("play");
        }

        /// <summary>
        /// Called when the player hits the "Exit" button.
        /// </summary>
        private void OnExit()
        {
            InternalGame.Exit();
        }

        /// <summary>
        /// Called when the player hits the "Instructions" button.
        /// </summary>
        private void OnInstructions()
        {
            GUIManager.SetGUI("instructions");
        }

        /// <summary>
        /// Called when the player hits the "Credits" button.
        /// </summary>
        private void OnCredits()
        {
            GUIManager.SetGUI("credits");
        }
        #endregion

        /// <summary>
        /// Called when the GUI is first set to be the active GUI.
        /// </summary>
        public override void OnWake()
        {
            base.OnWake();

            InternalGame.CurrentState = Game.State.GUI;

            InternalGame.IsMouseVisible = true;
            GUIManager.BindControllerListeners();

            if (SoundManager.MusicName != "demo")
                SoundManager.PlayMusic("demo");
            else
                SoundManager.PauseMusic(false);

            GUIManager.BindControllerListeners();
        }
    }
}
