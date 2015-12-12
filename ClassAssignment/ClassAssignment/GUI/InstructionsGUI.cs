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
    /// The instructions GUI's purpose is to instruct the player how to play the game. It will
    /// change content based on whether or not the game is using the controller subsystem to
    /// reflect the currently used control scheme.
    /// </summary>
    class InstructionsGUI : GUI
    {
        /// <summary>
        /// The GUI to be drawn when the player is viewing the instructions
        /// section of the game.
        /// </summary>
        /// <param name="game">
        /// The game instance to associate this new InstructionsGUI with.
        /// </param>
        public InstructionsGUI(Game game) : base(game)
        {
            #region GUI Initialization
            Elements.Button BackButton = new Elements.Button(game, "fonts/Arial", "Back", "images/button_up", "images/button_down")
            {
                Position = new Vector2(320, 500),
                Responder = this.OnBack,
            };

            this.AddElement(BackButton);

            Elements.Text jumpText = new Elements.Text(game, "fonts/Arial")
            {
                DisplayText = "<Jump Unset>",
                Position = new Vector2(150, 200),
                BackgroundColor = Color.Black,
            };

            this.AddElement(jumpText);

            Elements.Text moveText = new Elements.Text(game, "fonts/Arial")
            {
                DisplayText = "<Movement Unset>",
                Position = new Vector2(150, 250),
                BackgroundColor = Color.Black,
            };

            this.AddElement(moveText);

            Elements.Text crouchText = new Elements.Text(game, "fonts/Arial")
            {
                DisplayText = "<Crouch Unset>",
                Position = new Vector2(150, 300),
                BackgroundColor = Color.Black,
            };

            this.AddElement(crouchText);

            Elements.Text goalText = new Elements.Text(game, "fonts/Arial")
            {
                DisplayText = "Collect the red coins for score and reach the warp pipe to complete levels!",
                Position = new Vector2(150, 350),
                BackgroundColor = Color.Black,
            };

            this.AddElement(goalText);

            if (InputManager.UseController)
            {
                jumpText.DisplayText = "Press A to jump, press A again at the apex to flutter!";
                crouchText.DisplayText = "Tilt the left control stick down to crouch and activate warp pipes!";
                moveText.DisplayText = "Tilt the left control stick left and right to move!";
            }
            else
            {
                jumpText.DisplayText = "Press W to jump, press W again at the apex to flutter!";
                crouchText.DisplayText = "Press S to crouch and activate warp pipes!";
                moveText.DisplayText = "Press A and D to move left and right respectively!";
            }
            #endregion
        }

        public override void OnWake()
        {
            InternalGame.CurrentState = Game.State.GUI;
            InternalGame.IsMouseVisible = true;
        }

        private void OnBack()
        {
            GUIManager.SetGUI("main");
        }
    }
}
