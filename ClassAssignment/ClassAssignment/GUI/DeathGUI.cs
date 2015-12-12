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
    /// The GUI used for when the player has died.
    /// </summary>
    class DeathGUI : GUI
    {
        /// <summary>
        /// The text UI element that says "You Lose".
        /// </summary>
        private Elements.Text DeathText;

        /// <summary>
        /// The black picture that is drawn to isolate the player from the
        /// rest of the universe.
        /// </summary>
        private Elements.Picture BlackPicture;

        /// <summary>
        /// The button that exits the game.
        /// </summary>
        private Elements.Button ExitButton;

        /// <summary>
        /// The button that allows the player to replay.
        /// </summary>
        private Elements.Button ReplayButton;

        /// <summary>
        /// A constructor accepting a game instance.
        /// </summary>
        /// <param name="game">
        /// The game instance to associate this GUI with.
        /// </param>
        public DeathGUI(Game game) : base(game)
        {
            #region GUI Initialization
            BlackPicture = new Elements.Picture(game, "Images/black")
            {
                Color = new Color(0, 0, 0, 0),
            };
            this.AddElement(BlackPicture);

            DeathText = new Elements.Text(game, "fonts/Arial")
            {
                DisplayText = "You Lose",
                Color = new Color(0, 0, 0, 0),
                Position = new Vector2(350, 200),
                BackgroundColor = Color.White,
            };

            this.AddElement(DeathText);

            ExitButton = new Elements.Button(game, "fonts/Arial", "Exit", "images/button_up", "images/button_down")
            {
                Color = new Color(0, 0, 0, 0),
                Position = new Vector2(100, 300),
                Responder = this.OnExit,
            };

            this.AddElement(ExitButton);

            ReplayButton = new Elements.Button(game, "fonts/Arial", "Replay", "images/button_up", "images/button_down")
            {
                Color = new Color(0, 0, 0, 0),
                Position = new Vector2(550, 300),
                Responder = OnReplay,
            };

            this.AddElement(ReplayButton);
            #endregion
        }

        /// <summary>
        /// Called when the Exit button is pressed by the user.
        /// </summary>
        private void OnExit()
        {
            InternalGame.Exit();
        }

        /// <summary>
        /// Called when the Replay button is pressed by the user.
        /// </summary>
        private void OnReplay()
        {
            InternalGame.CurrentState = Game.State.Play;

            InternalGame.Player.Velocity = new Vector2(0, 0);
            InternalGame.ReloadLevel();
        }

        /// <summary>
        /// Called when this GUI is first set as the active GUI.
        /// </summary>
        public override void OnWake()
        {
            base.OnWake();

            BlackPicture.Color = new Color(0, 0, 0, 0);
            DeathText.Color = new Color(0, 0, 0, 0);
            ExitButton.Color = new Color(0, 0, 0, 0);
            ReplayButton.Color = new Color(0, 0, 0, 0);

            InternalGame.IsMouseVisible = true;
            GUIManager.BindControllerListeners();

            SoundManager.PlayMusic("gameover");
        }

        /// <summary>
        /// Called when this GUI is no longer the active GUI.
        /// </summary>
        public override void OnSleep()
        {
            base.OnSleep();

            InternalGame.IsMouseVisible = false;
        }

        /// <summary>
        /// Updates the DeathGUI.
        /// </summary>
        /// <param name="time">
        /// The GameTime object passed in by the game's main Update method.
        /// </param>
        public override void Update(GameTime time)
        {
            base.Update(time);

            #region Blackout Implementation
            if (BlackPicture.Color.A < 255)
            {
                BlackPicture.Color.R += 3;
                BlackPicture.Color.G += 3;
                BlackPicture.Color.B += 3;
                BlackPicture.Color.A += 3;

                DeathText.Color.A += 3;

                ExitButton.Color.R += 3;
                ExitButton.Color.G += 3;
                ExitButton.Color.B += 3;
                ExitButton.Color.A += 3;

                ReplayButton.Color.R += 3;
                ReplayButton.Color.G += 3;
                ReplayButton.Color.B += 3;
                ReplayButton.Color.A += 3;
            }
            #endregion
        }
    }
}
