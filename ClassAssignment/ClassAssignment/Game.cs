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

namespace ClassAssignment
{
    /// <summary>
    /// The main game class to be initialized by XNA when the Yoshi's Island game starts up.
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game
    {
        /// <summary>
        /// A static boolean representing whether or not debug draws should be ran for collision.
        /// </summary>
        public static bool CollisionDebugEnabled = false;

        /// <summary>
        /// A static boolean representing whether or not the world debugger is enabled.
        /// </summary>
        public static bool WorldDebugEnabled = false;

        /// <summary>
        /// The dimensions at which the game will run at.
        /// </summary>
        public static Point WindowDimensions = new Point(800, 600);

        /// <summary>
        /// The graphics device used by the game.
        /// </summary>
        private GraphicsDeviceManager graphics;

        /// <summary>
        /// The sprite batch used by the game when drawing.
        /// </summary>
        private SpriteBatch spriteBatch;

        /// <summary>
        /// The primary player object to be used.
        /// </summary>
        public Yoshi.Player Player;

        /// <summary>
        /// A vector representing the direction and strength of gravity, a constant force
        /// applied to all controlled entities.
        /// </summary>
        public static Vector2 Gravity = new Vector2(0, 600);

        /// <summary>
        /// A list of collective goodies for the player.
        /// </summary>
        public List<RedCoin> Goodies;

        /// <summary>
        /// A list of all active game entities active in the scene.
        /// </summary>
        public List<ControlledEntity> Entities;

        /// <summary>
        /// The player's current score.
        /// </summary>
        public int Score;

        /// <summary>
        /// The current count of collected goodies. This is used to determine whether or not
        /// all of the coins have been collected.
        /// </summary>
        public int CollectedGoodies;

        /// <summary>
        /// Preloaded Arial sprite font.
        /// </summary>
        public SpriteFont Arial;

        /// <summary>
        /// The offset at which the game will be drawn at. This is used to simulate
        /// camera panning.
        /// </summary>
        public Vector2 DrawOffset;

        /// <summary>
        /// An enum that is a registry of all possible game states.
        /// </summary>
        public enum State
        {
            Play = 0,
            Death = 1,
            GUI = 2,
            Pause = 3,
        };

        /// <summary>
        /// A counter representing what level we are currently on.
        /// </summary>
        public int LevelCounter;

        /// <summary>
        /// What is the current game state?
        /// </summary>
        public State CurrentState;

        /// <summary>
        /// The tile coordinate to start the tile drawing at.
        /// </summary>
        private Point ViewableTilesStart;

        /// <summary>
        /// The tile coordinate to stop drawing the background at.
        /// </summary>
        private Point ViewableBackgroundTilesEnd;

        /// <summary>
        /// The tile coordinate to stop drawing the foreground at.
        /// </summary>
        private Point ViewableForegroundTilesEnd;
		
        /// <summary>
        /// The tile coordinate the player was known to be in last frame.
        /// </summary>
		private Point LastPlayerTileCoordinate;

        /// <summary>
        /// Main constructor for the game.
        /// </summary>
        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = WindowDimensions.X;
            graphics.PreferredBackBufferHeight = WindowDimensions.Y;
            graphics.ApplyChanges();

            Window.Title = "Booper Dooper";
        }

        /// <summary>
        /// Completely restarts the game. It resets the score, the level counter and loads
        /// the first level of the game.
        /// </summary>
        public void Restart()
        {
            Score = 0;
            LevelCounter = -1;
            this.AdvanceLevel();
        }

        /// <summary>
        /// Reloads the currently loaded level of the game. It takes away 500 points and
        /// re-runs the level load logic.
        /// </summary>
        public void ReloadLevel()
        {
            Score -= 500;
            MapManager.LoadMap(String.Format("Maps/{0}.txt", LevelCounter), "Maps/bg.txt");
        }

        /// <summary>
        /// Advances the player up by one level. It loads the new level if available, and if there
        /// is none, then the player is considered to have completed the game.
        /// </summary>
        public void AdvanceLevel()
        {
            try
            {
                MapManager.LoadMap(String.Format("Maps/{0}.txt", ++LevelCounter), "Maps/bg.txt");
            }
            catch (System.IO.FileNotFoundException)
            {
                GUI.GUIManager.SetGUI("end");
            }
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            CurrentState = State.GUI;
            DrawOffset = new Vector2(0, 0);

            TimerManager.Create();
            Arial = Content.Load<SpriteFont>("Fonts/Arial");

            #region Sound Initialization
            SoundManager.Create(this);
            SoundManager.Load("Sounds/pause", "pause");
            SoundManager.Load("Sounds/red_coin", "redcoin");
            SoundManager.Load("Sounds/all_coins", "allcoins");
            SoundManager.Load("Sounds/yoshi_flutter", "yoshiflutter");
            SoundManager.Load("Sounds/yoshi_jump", "yoshijump");
            SoundManager.Load("Sounds/yoshi_ceiling", "yoshiceiling");
            SoundManager.Load("Sounds/gameover", "gameover");
            SoundManager.Load("Sounds/levelstart", "levelstart");
            SoundManager.Load("Sounds/music", "music");
            SoundManager.Load("Sounds/demo", "demo");
            SoundManager.Load("Sounds/goal", "goal");
            SoundManager.Load("Sounds/yoshi_push", "yoshipush");
            SoundManager.Load("Sounds/yoshi_stomp", "yoshistomp");
            SoundManager.Load("Sounds/yoshi_tongue", "yoshitongue");
            SoundManager.Load("Sounds/enemy_die", "enemydie");
            #endregion

            #region Input Initialization
            InputManager.Create();

            if (GamePad.GetState(0).IsConnected)
            {
                InputManager.UseKeyboard = false;
                InputManager.UseController = true;
            }
            else
            {
                InputManager.UseKeyboard = true;
                InputManager.UseController = false;
            }

            InputManager.SetKeyResponder(Keys.F12, Game.ToggleCollisionDebug);
            InputManager.SetKeyResponder(Keys.F11, Game.ToggleStateDebug);
            InputManager.SetKeyResponder(Keys.F10, Game.ToggleWorldDebug);
            InputManager.SetKeyResponder(Keys.F9, Game.ToggledPhysicsDebug);
            InputManager.SetKeyResponder(Keys.F8, Game.ToggleGUIDebug);
            InputManager.SetKeyResponder(Keys.F7, Game.ToggleSoundDebug);

            #endregion

            #region GUI Initialization
            GUI.GUIManager.Create(this);

            GUI.DeathGUI deathGUI = new GUI.DeathGUI(this);
            GUI.GUIManager.AddGUI(deathGUI, "death");
            GUI.PlayGUI playGUI = new GUI.PlayGUI(this);
            GUI.GUIManager.AddGUI(playGUI, "play");
            GUI.MainGUI mainGUI = new GUI.MainGUI(this);
            GUI.GUIManager.AddGUI(mainGUI, "main");
            GUI.InstructionsGUI instructionsGUI = new GUI.InstructionsGUI(this);
            GUI.GUIManager.AddGUI(instructionsGUI, "instructions");
            GUI.CreditsGUI creditsGUI = new GUI.CreditsGUI(this);
            GUI.GUIManager.AddGUI(creditsGUI, "credits");
            GUI.PauseGUI pauseGUI = new GUI.PauseGUI(this);
            GUI.GUIManager.AddGUI(pauseGUI, "pause");
            GUI.EndGUI endGUI = new GUI.EndGUI(this);
            GUI.GUIManager.AddGUI(endGUI, "end");

            GUI.GUIManager.Initialize();
            #endregion

            #region Misc. Initialization
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Goodies = new List<RedCoin>();
            Entities = new List<ControlledEntity>();

            MapManager.Create(this);

            #endregion

            MapManager.LoadMap("Maps/0.txt", "Maps/bg.txt");

            GUI.GUIManager.SetGUI("main");
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            InputManager.Update(gameTime);
            TimerManager.Update(gameTime);

            // Compute gravitational forces
            float deltaSeconds = (float)gameTime.ElapsedGameTime.Milliseconds / 1000;

            GUI.GUIManager.Update(gameTime);

            #region Game State Switch
            switch (CurrentState)
            {
                case State.Death:
                {
                        MapManager.Update(gameTime);

                        Player.Dead = true;

                        GUI.GUIManager.SetGUI("death");
                        break;
                }
                case State.Play:
                    {
                        MapManager.Update(gameTime);

                        Vector2 gravityDelta = Gravity * deltaSeconds;

                        Player.Dead = false;

                        GUI.GUIManager.SetGUI("play");

                        // Grab goodies!
                        foreach (RedCoin goody in Goodies)
                        {
                            goody.Update(gameTime);

                            if (goody.Drawn && goody.Rectangle.Intersects((Rectangle)Player.Rectangle))
                            {
                                goody.Drawn = false;

                                ++CollectedGoodies;
                                if (CollectedGoodies >= Goodies.Count)
                                    SoundManager.Play("allcoins");
                                else
                                    SoundManager.Play("redcoin");

                                Score += 100;
                            }
                        }

                        foreach (ControlledEntity entity in Entities)
                        {
                            entity.Update(gameTime);

                            if (!entity.Grounded)
                                entity.Velocity += gravityDelta;
                        }

                        break;
                    }
            }
            #endregion

            #region Pseudo Camera Translations
            DrawOffset = -Player.Position;
            DrawOffset = new Vector2(WindowDimensions.X / 2 + DrawOffset.X, WindowDimensions.Y / 2 + DrawOffset.Y);

            float bottomOfMap = -(MapManager.TileDimensions.Y * MapManager.ForegroundTiles.GetUpperBound(1));
            bottomOfMap += MapManager.TileDimensions.Y * 11;

            DrawOffset = new Vector2(DrawOffset.X > 0 ? 0 : DrawOffset.X,
                                    DrawOffset.Y < bottomOfMap ? bottomOfMap : DrawOffset.Y);
            #endregion

            #region Viewable Tile Calculations
            Point playerTile = MapManager.PositionToTile(Player.Center);

            // Only update the viewable tiles if the player has actually changed tiles.
            if (playerTile != LastPlayerTileCoordinate)
            {
                LastPlayerTileCoordinate = playerTile;

                // Calculate where the player is: The value returned by PositionToTile are not contrained to valid positions on the map, so we clamp it here.
                playerTile = new Point(playerTile.X < 0 ? 0 : playerTile.X, playerTile.Y < 0 ? 0 : playerTile.Y);
                playerTile = new Point(playerTile.X > MapManager.ForegroundTiles.GetUpperBound(0) ? MapManager.ForegroundTiles.GetUpperBound(0) : playerTile.X,
                    playerTile.Y > MapManager.ForegroundTiles.GetUpperBound(1) ? MapManager.ForegroundTiles.GetUpperBound(1) : playerTile.Y);

                // Calculate where the render starts for both the foreground and background
                ViewableTilesStart = new Point(playerTile.X - (MapManager.ViewableTiles.X / 2), playerTile.Y - (MapManager.ViewableTiles.Y / 2));
                ViewableTilesStart = new Point(ViewableTilesStart.X <= 0 ? 0 : ViewableTilesStart.X - 1, ViewableTilesStart.Y <= 0 ? 0 : ViewableTilesStart.Y - 1);

                // Calculate where the render ends for both the foreground and background.
                ViewableBackgroundTilesEnd = new Point(ViewableTilesStart.X + MapManager.ViewableTiles.X, ViewableTilesStart.Y + MapManager.ViewableTiles.Y);
                ViewableBackgroundTilesEnd = new Point(ViewableBackgroundTilesEnd.X >= MapManager.BackgroundTiles.GetUpperBound(0) ? MapManager.BackgroundTiles.GetUpperBound(0) : ViewableBackgroundTilesEnd.X + 1,
                    ViewableBackgroundTilesEnd.Y >= MapManager.BackgroundTiles.GetUpperBound(1) ? MapManager.BackgroundTiles.GetUpperBound(1) : ViewableBackgroundTilesEnd.Y + 1);

                ViewableForegroundTilesEnd = new Point(ViewableTilesStart.X + MapManager.ViewableTiles.X, ViewableTilesStart.Y + MapManager.ViewableTiles.Y);
                ViewableForegroundTilesEnd = new Point(ViewableForegroundTilesEnd.X >= MapManager.ForegroundTiles.GetUpperBound(0) ? MapManager.ForegroundTiles.GetUpperBound(0) : ViewableForegroundTilesEnd.X + 1,
                    ViewableForegroundTilesEnd.Y >= MapManager.ForegroundTiles.GetUpperBound(1) ? MapManager.ForegroundTiles.GetUpperBound(1) : ViewableForegroundTilesEnd.Y + 1);

                // FIXME: Busted drawing logic in maps where foreground size is distinctly different from background size, or potentially just smaller sizes period.

                // Ensure that the tile draw logic coincides with the camera locking logic
                if (ViewableBackgroundTilesEnd.Y == MapManager.BackgroundTiles.GetUpperBound(1))
                    ViewableTilesStart.Y = (ViewableForegroundTilesEnd.Y - MapManager.ViewableTiles.Y) - 2;
                else if (ViewableForegroundTilesEnd.Y == MapManager.ForegroundTiles.GetUpperBound(1))
                    ViewableTilesStart.Y = (ViewableBackgroundTilesEnd.Y - MapManager.ViewableTiles.Y) - 2;
            }
            #endregion

            #region Collision Box Code
            foreach (ControlledEntity first in Entities)
            {
                if (!first.Updated)
                    continue;

                foreach(ControlledEntity second in Entities)
                {
                    if (first == second || !second.Updated)
                        continue;

                    foreach (ControlledEntity.CollisionInformation firstinfo in first.CollisionBoxes)
                        foreach (ControlledEntity.CollisionInformation secondinfo in second.CollisionBoxes)
                            if (firstinfo.Box.Intersects(secondinfo.Box))
                            {
                                if (firstinfo.CollisionResponder != null)
                                    firstinfo.CollisionResponder(secondinfo);
                                if (secondinfo.CollisionResponder != null)
                                    secondinfo.CollisionResponder(firstinfo);
                            }
                }
            }
            #endregion

            SoundManager.Update();
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            MapManager.Draw(spriteBatch, ViewableTilesStart, ViewableBackgroundTilesEnd, ViewableForegroundTilesEnd);

            #region Game State Switch
            // Draw the others
            switch (CurrentState)
            {
                case State.Pause:
                case State.Play:
                case State.Death:
                    {
                        foreach (RedCoin goody in Goodies)
                            goody.Draw(spriteBatch);

                        foreach (ControlledEntity entity in Entities)
                            entity.Draw(spriteBatch);

                        break;
                    }
            }
            #endregion

            GUI.GUIManager.Draw(spriteBatch);

            switch (CurrentState)
            {
                case State.Pause:
                case State.Death:
                    {
                        Player.Draw(spriteBatch);
                        break;
                    }
              
            }

            if (Game.WorldDebugEnabled)
            {
                spriteBatch.DrawString(this.Arial, String.Format("Entities: {0}", this.Entities.Count()), new Vector2(20, 20), Color.Red);
                spriteBatch.DrawString(this.Arial, String.Format("Gravity: {0:f2}, {1:f2}", Gravity.X, Gravity.Y), new Vector2(20, 40), Color.Red);
                spriteBatch.DrawString(this.Arial, String.Format("Map Size: {0}x{1}", MapManager.ForegroundTiles.GetUpperBound(0), MapManager.ForegroundTiles.GetUpperBound(1)), 
                    new Vector2(20, 60), Color.Red);

                spriteBatch.DrawString(this.Arial, String.Format("Tile Start: {0}, {1}", this.ViewableTilesStart.X, this.ViewableTilesStart.Y),
                     new Vector2(20, 80), Color.Red);

                spriteBatch.DrawString(this.Arial, String.Format("Foreground End: {0}, {1}", this.ViewableForegroundTilesEnd.X, this.ViewableForegroundTilesEnd.Y),
                    new Vector2(20, 100), Color.Red);
                spriteBatch.DrawString(this.Arial, String.Format("Background End: {0}, {1}", this.ViewableBackgroundTilesEnd.X, this.ViewableBackgroundTilesEnd.Y),
                    new Vector2(20, 120), Color.Red);
            }

            SoundManager.Draw(spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        #region Keyboard Responders (Debugging)
        /// <summary>
        /// Static method called when the player toggles the physics debugger.
        /// </summary>
        /// <param name="pressed">
        /// A boolean representing whether or not the button is currently pressed.
        /// </param>
        private static void ToggleCollisionDebug(bool pressed)
        {
            if (pressed)
                Game.CollisionDebugEnabled = !Game.CollisionDebugEnabled;
        }

        /// <summary>
        /// Static method called wehn the player toggles the state debugger.
        /// </summary>
        /// <param name="pressed">
        /// A boolean representing whether or not the button is currently pressed.
        /// </param>
        private static void ToggleStateDebug(bool pressed)
        {
            if (pressed)
                ControlledEntity.StateDebugEnabled = !ControlledEntity.StateDebugEnabled;
        }

        /// <summary>
        /// Static method called wehn the player toggles the world debugger.
        /// </summary>
        /// <param name="pressed">
        /// A boolean representing whether or not the button is currently pressed.
        /// </param>
        private static void ToggleWorldDebug(bool pressed)
        {
            if (pressed)
                Game.WorldDebugEnabled = !Game.WorldDebugEnabled;
        }

        /// <summary>
        /// Static method called wehn the player toggles the physics debugger.
        /// </summary>
        /// <param name="pressed">
        /// A boolean representing whether or not the button is currently pressed.
        /// </param>
        private static void ToggledPhysicsDebug(bool pressed)
        {
            if (pressed)
                ControlledEntity.PhysicsDebugEnabled = !ControlledEntity.PhysicsDebugEnabled;
        }

        /// <summary>
        /// Static method called wehn the player toggles the GUI debugger.
        /// </summary>
        /// <param name="pressed">
        /// A boolean representing whether or not the button is currently pressed.
        /// </param>
        private static void ToggleGUIDebug(bool pressed)
        {
            if (pressed)
                GUI.GUIManager.GUIDebuggerEnabled = !GUI.GUIManager.GUIDebuggerEnabled;
        }

        /// <summary>
        /// Static method called wehn the player toggles the sound debugger.
        /// </summary>
        /// <param name="pressed">
        /// A boolean representing whether or not the button is currently pressed.
        /// </param>
        private static void ToggleSoundDebug(bool pressed)
        {
            if (pressed)
                SoundManager.SoundDebuggerEnabled = !SoundManager.SoundDebuggerEnabled;
        }
        #endregion
    }
}