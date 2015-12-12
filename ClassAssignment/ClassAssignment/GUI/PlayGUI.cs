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
    /// The PlayGUI is the GUI that is drawn when the player is currently playing the game --
    /// in control of the player object.
    /// </summary>
    class PlayGUI : GUI
    {
        /// <summary>
        /// The text element used to draw the score on the PlayGUI.
        /// </summary>
        private Elements.Text ScoreText;

        /// <summary>
        /// The GUI to be drawn when the player is playing the game.
        /// </summary>
        /// <param name="game">
        /// The game instance to associate this new PlayGUI with.
        /// </param>
        public PlayGUI(Game game) : base(game)
        {
            ScoreText = new Elements.Text(game, "fonts/Arial")
            {
                DisplayText = "Score: <UNSET>",
                Position = new Vector2(150, 50),
                BackgroundColor = Color.Black,
            };

            this.AddElement(ScoreText);
        }

        /// <summary>
        /// Called when the GUI is first set to be the active GUI.
        /// </summary>
        public override void OnWake()
        {
            base.OnWake();

            SoundManager.PauseMusic(false);
            InternalGame.CurrentState = Game.State.Play;

            InternalGame.IsMouseVisible = false;
            InternalGame.Player.BindControls();

            if (SoundManager.MusicName != "music" && SoundManager.MusicName != null)
            {
                SoundManager.StopMusic();
                SoundManager.SoundSource start = SoundManager.Play("levelstart");
                start.OnPlaybackEndResponder = this.OnLevelStartEnd;
            }
        }

        public override void Update(GameTime time)
        {
            base.Update(time);

            ScoreText.DisplayText = String.Format("Score: {0}", InternalGame.Score);

        }

        /// <summary>
        /// Callback method called when the level start sound ends playback.
        /// </summary>
        private void OnLevelStartEnd()
        {
            SoundManager.PlayMusic("music");
        }

        /// <summary>
        /// Method called when the PlayGUI is first set to no longer be the active GUI.
        /// </summary>
        public override void OnSleep()
        {
            base.OnSleep();

            InternalGame.Player.UnbindControls();
        }
    }
}
