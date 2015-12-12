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
    class EndGUI : GUI
    {
        public Elements.Text ScoreText;

        /// <summary>
        /// The GUI to be drawn when the player is playing the game.
        /// </summary>
        /// <param name="game">
        /// The game instance to associate this new PlayGUI with.
        /// </param>
        public EndGUI(Game game) : base(game)
        {
            Elements.Button MenuButton = new Elements.Button(game, "fonts/Arial", "Main Menu", "images/button_up", "images/button_down")
            {
                Position = new Vector2(100, 500),
                Responder = this.OnMenu,
            };

            this.AddElement(MenuButton);

            Elements.Button CreditsButton = new Elements.Button(game, "fonts/Arial", "Credits", "images/button_up", "images/button_down")
            {
                Position = new Vector2(100, 300),
                Responder = this.OnCredits,
            };

            this.AddElement(CreditsButton);

            Elements.Button ReplayButton = new Elements.Button(game, "fonts/Arial", "Play", "images/button_up", "images/button_down")
            {
                Position = new Vector2(100, 100),
                Responder = this.OnReplay,
            };

            this.AddElement(ReplayButton);

            Elements.Text winText = new Elements.Text(game, "fonts/Arial")
            {
                DisplayText = "YOU WIN!",
                Position = new Vector2(150, 50),
                BackgroundColor = Color.Black,
            };

            this.AddElement(winText);

            // Place some hint text on the main screen for basic navigational tips
            ScoreText = new Elements.Text(game, "fonts/Arial")
            {
                DisplayText = "Final Score: <UNSET>",
                Position = new Vector2(150, 70),
                BackgroundColor = Color.Black,
            };

            this.AddElement(ScoreText);
        }

        private void OnReplay()
        {
            InternalGame.Restart();

            InternalGame.CurrentState = Game.State.Play;
            GUIManager.SetGUI("play");
        }

        private void OnMenu()
        {
            GUIManager.SetGUI("main");
        }

        private void OnCredits()
        {
            GUIManager.SetGUI("credits");
        }

        public override void OnWake()
        {
            base.OnWake();

            InternalGame.CurrentState = Game.State.GUI;

            InternalGame.IsMouseVisible = true;
            GUIManager.BindControllerListeners();

            ScoreText.DisplayText = "Final Score: " + InternalGame.Score;

            SoundManager.PlayMusic("goal");
        }
    }
}
