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
    /// The map manager is a static class that manages loading and storage of the game maps that the player will both interact 
    /// with and see in the form of a background. It will spawn the player, enemies and all other entities using information registered
    /// by the CreateTileInformation method.
    /// </summary>
    public class MapManager
    {
        /// <summary>
        /// The game instance the tile manager is currently associated with.
        /// </summary>
        private static Game InternalGame;

        /// <summary>
        /// What is the current resolution of the map, in tiles?
        /// </summary>
        public static Point MapResolution;

        /// <summary>
        /// The foreground tilemap to render and interact with.
        /// </summary>
        public static TileInformation[,] ForegroundTiles;

        /// <summary>
        /// The background tilemap to render.
        /// </summary>
        public static TileInformation[,] BackgroundTiles;

        /// <summary>
        /// A dictionary of all tile characters to their respective textures.
        /// </summary>
        public static SortedDictionary<char, Texture2D> TileImages;

        /// <summary>
        /// A dictionary of all tile characters to their respective tile data.
        /// </summary>
        public static SortedDictionary<char, TileInformation> TileData;

        /// <summary>
        /// The window of tiles that is visible on the screen at any given time.
        /// </summary>
        public static Point ViewableTiles = new Point(Game.WindowDimensions.X / TileDimensions.X, Game.WindowDimensions.Y / TileDimensions.Y);
        
        /// <summary>
        /// An enumeration used to associate special functions with a tile such as setting the player spawn.
        /// </summary>
        public enum SPECIAL_FUNCTION
        {
            /// <summary>
            /// Do nothing.
            /// </summary>
            NOTHING = 0,

            /// <summary>
            /// Spawns a Koopa.
            /// </summary>
            SPAWN_KOOPA = 1,

            /// <summary>
            /// Spawns a collectible coin.
            /// </summary>
            SPAWN_COIN = 2,
            
            /// <summary>
            /// Spawns the player.
            /// </summary>
            SPAWN_PLAYER = 3,

            /// <summary>
            /// Advances the level if the player crouches on it.
            /// </summary>
            ADVANCE_LEVEL = 4,
        };

        /// <summary>
        /// This class represents information for each usable tile in the game.
        /// </summary>
        public class TileInformation
        {
            /// <summary>
            /// Is the tile solid?
            /// </summary>
            public bool Solid;

            /// <summary>
            /// Is the tile lethal?
            /// </summary>
            public bool Lethal;

            /// <summary>
            /// Does the tile remap to something else? null is no.
            /// </summary>
            public char? TileRemap;

            /// <summary>
            /// Does the tile perform some sort of special function?
            /// </summary>
            public SPECIAL_FUNCTION SpecialFunction;

            /// <summary>
            /// The animated sprite associated with this tile information.
            /// </summary>
            AnimatedSprite Graphic;
        
            /// <summary>
            /// Draws the tile the specified position.
            /// </summary>
            /// <param name="batch">
            /// The batch to draw to.
            /// </param>
            /// <param name="position">
            /// The position to draw at.
            /// </param>
            public void Draw(SpriteBatch batch, Vector2 position)
            {
                if (Graphic != null)
                    Graphic.Draw(batch, position);
            }

            /// <summary>
            /// Updates the tile information's associated animated sprite.
            /// </summary>
            /// <param name="time">
            /// The GameTime passed in by the game's main update method.
            /// </param>
            public void Update(GameTime time)
            {
                if (Graphic != null)
                    Graphic.Update(time);
            }

            /// <summary>
            /// Constructor accepting a game instance and a texture path.
            /// </summary>
            /// <param name="game">
            /// The game instance to associate with.
            /// </param>
            /// <param name="texturePath">
            /// The path of the texture to load. null if none.
            /// </param>
            public TileInformation(Game game, String texturePath)
            {
                TileRemap = null;

                if (texturePath != null)
                {
                    Graphic = new AnimatedSprite(game, texturePath, TileDimensions, null)
                    {
                        Drawn = true,
                        Updated = true,
                        MillisecondsPerFrame = 200,
                    };

                    Graphic.Initialize();
                }
            }
        };

        /// <summary>
        /// How many tiles we have altogether for the currently loaded map.
        /// </summary>
        public static int TileCount;

        /// <summary>
        /// A static point representing the dimensions of the tiles.
        /// </summary>
        public static Point TileDimensions
        {
            get
            {
                return new Point(50, 50);
            }
        }

        /// <summary>
        /// Gets the tile at the given tile coordinate. If the the location is out of binds, an empty tile is returned.
        /// </summary>
        /// <param name="position">
        /// The tile coordinate to lookup.
        /// </param>
        /// <returns>
        /// The corresponding tile at that coordinate. Returns an empty tile if the coordinate is out of range.
        /// </returns>
        public static TileInformation GetTile(Point position)
        {
            if (position.X < 0 || position.X >= MapResolution.X || position.Y < 0 || position.Y >= MapResolution.Y)
                return TileData['g'];

            return ForegroundTiles[position.X, position.Y];
        }

        /// <summary>
        /// Returns the corresponding map tile for the given position.
        /// </summary>
        /// <param name="position">
        /// The input position.
        /// </param>
        /// <returns>
        /// The tile x,y coordinates for the given position.
        /// </returns>
        public static Point PositionToTile(Vector2 position)
        {
            return new Point((int)Math.Floor(position.X) / TileDimensions.X, (int)Math.Floor(position.Y) / TileDimensions.Y);
        }

        /// <summary>
        /// Creates and initializes the tile manager by creating internal structures and loading textures to be used for
        /// the sprites.
        /// </summary>
        /// <param name="game">
        /// The game instance to associate the tile manager with.
        /// </param>
        public static void Create(Game game)
        {
            InternalGame = game;

            TileImages = new SortedDictionary<char, Texture2D>();
            TileData = new SortedDictionary<char, TileInformation>();

            for (char i = 'a'; i <= 'k'; i++)
                TileImages[i] = InternalGame.Content.Load<Texture2D>("Images/tile" + i);

            TileImages['q'] = InternalGame.Content.Load<Texture2D>("Images/tileq");

            CreateTileInformation();
        }

        /// <summary>
        /// A helper method that is called by the tile manager to create tile information to be used
        /// during the map load process.
        /// </summary>
        public static void CreateTileInformation()
        {
            #region Tile Information Initialization
            TileInformation stoneOne = new TileInformation(InternalGame, "Images/tileb")
            {
                Solid = true,
            };

            TileInformation stoneTwo = new TileInformation(InternalGame, "Images/tilec")
            {
                Solid = true,
            };

            TileInformation stoneThree = new TileInformation(InternalGame, "Images/tilee")
            {
                Solid = true,
            };

            TileInformation grassOne = new TileInformation(InternalGame, "Images/tilef")
            {
                Solid = true,
            };

            TileInformation blank = new TileInformation(InternalGame, "Images/tileg")
            {
                Solid = false,
            };

            TileInformation bridgeRight = new TileInformation(InternalGame, "Images/tileh")
            {
                Solid = true,
            };

            TileInformation bridgeCenter = new TileInformation(InternalGame, "Images/tilek")
            {
                Solid = true,
            };

            TileInformation bridgeLeft = new TileInformation(InternalGame, "Images/tileq")
            {
                Solid = true,
            };

            TileInformation spikes = new TileInformation(InternalGame, "Images/tilei")
            {
                Solid = true,
                Lethal = true,
            };

            TileInformation castleBG = new TileInformation(InternalGame, "Images/tilej")
            {
                Solid = false,
            };

            TileInformation inactivePipe = new TileInformation(InternalGame, "Images/tiled")
            {
                Solid = true,
            };

            TileInformation sign = new TileInformation(InternalGame, "Images/tilea")
            {
                Solid = false,
            };

            TileInformation lavaSurface = new TileInformation(InternalGame, "Images/tilew")
            {
                Solid = true,
                Lethal = true,
            };

            TileInformation lavaDepth = new TileInformation(InternalGame, "Images/tilex")
            {
                Solid = true,
                Lethal = true,
            };

            TileInformation advancePipe = new TileInformation(InternalGame, "Images/tiled")
            {
                Solid = true,
                SpecialFunction = SPECIAL_FUNCTION.ADVANCE_LEVEL,
            };


            TileData['a'] = sign;
            TileData['b'] = stoneOne;
            TileData['c'] = stoneTwo;
            TileData['d'] = inactivePipe;
            TileData['e'] = stoneThree;
            TileData['f'] = grassOne;
            TileData['g'] = blank;
            TileData[' '] = blank;
            TileData['i'] = spikes;
            TileData['j'] = castleBG;

            TileData['k'] = bridgeCenter;
            TileData['q'] = bridgeLeft;
            TileData['h'] = bridgeRight;
            TileData['w'] = lavaSurface;
            TileData['x'] = lavaDepth;
            TileData['.'] = advancePipe;

            // Spawning tiles
            TileInformation coinTile = new TileInformation(InternalGame, null)
            {
                SpecialFunction = SPECIAL_FUNCTION.SPAWN_COIN,
            };

            TileData['l'] = coinTile;

            TileInformation koopaTile = new TileInformation(InternalGame, null)
            {
                SpecialFunction = SPECIAL_FUNCTION.SPAWN_KOOPA,
            };

            TileData['m'] = koopaTile;

            TileInformation playerTile = new TileInformation(InternalGame, null)
            {
                SpecialFunction = SPECIAL_FUNCTION.SPAWN_PLAYER,
            };

            TileData['p'] = playerTile;
            #endregion
        }

        /// <summary>
        /// Updates the map manager by updating all animated tile information in the map. This
        /// advances the animation frame of all animated tiles.
        /// </summary>
        /// <param name="time">
        /// The GameTime instance passed in by the game's main update.
        /// </param>
        public static void Update(GameTime time)
        {
            foreach (KeyValuePair<char, TileInformation> current in TileData.ToList())
            {
                current.Value.Update(time);
            }
        }

        /// <summary>
        /// Destroys the map manager.
        /// </summary>
        public static void Destroy()
        {
            ForegroundTiles = null;
            BackgroundTiles = null;

            TileData = null;
            TileImages = null;
        }

        /// <summary>
        /// Draws the map manager to the screen. This is only the viewable sections of both the background and the foreground.
        /// </summary>
        /// <param name="batch">
        /// The sprite batch to draw to.
        /// </param>
        /// <param name="tileStart">
        /// The starting point of both the foreground and background tile maps to draw from.
        /// </param>
        /// <param name="backgroundEnd">
        /// The point at which the background stops drawing.
        /// </param>
        /// <param name="foregroundEnd">
        /// The point at which the foreground stops drawing.
        /// </param>
        public static void Draw(SpriteBatch batch, Point tileStart, Point backgroundEnd, Point foregroundEnd)
        {
            // Draw the background
            for (int tileY = tileStart.Y; tileY <= backgroundEnd.Y; tileY++)
                for (int tileX = tileStart.X; tileX <= backgroundEnd.X; tileX++)
                {
                    Vector2 drawLocation = new Vector2(tileX * TileDimensions.X, tileY * TileDimensions.Y) + InternalGame.DrawOffset;
                    BackgroundTiles[tileX, tileY].Draw(batch, drawLocation);
                }

            // Draw the foreground.
            for (int tileY = tileStart.Y; tileY <= foregroundEnd.Y; tileY++)
                for (int tileX = tileStart.X; tileX <= foregroundEnd.X; tileX++)
                {
                    Vector2 drawLocation = new Vector2(tileX * TileDimensions.X, tileY * TileDimensions.Y) + InternalGame.DrawOffset;
                    ForegroundTiles[tileX, tileY].Draw(batch, drawLocation);
                }
        }

        /// <summary>
        /// Loads and returns the map data from the map file desiginated by the path, spawning entities along the way
        /// if the map is supposed to be used as a foreground map.
        /// </summary>
        /// <param name="path">
        /// The path to the map file to load.
        /// </param>
        /// <param name="foreground">
        /// A boolean representing whether or not this map will be used in the foreground. If this is false, no enemies in
        /// the map data will be spawned.
        /// </param>
        /// <returns>
        /// A variable length 2D char array representing the loaded map data.
        /// </returns>
        public static TileInformation[,] GetMapData(String path, bool foreground)
        {
            System.IO.StreamReader fileReader = new System.IO.StreamReader(path);

            // We create a dynamic buffer to stick each line in temporarily.
            List<String> lineBuffer = new List<String>();

            int lineCount = 0;
            int lineWidth = 0;
            while (!fileReader.EndOfStream)
            {
                String currentLine = fileReader.ReadLine();
                lineBuffer.Add(currentLine);
                ++lineCount;

                if (lineWidth == 0)
                    lineWidth = currentLine.Length;
                else if (currentLine.Length != lineWidth)
                    throw new System.SystemException("TileManager: Input tilemap must keep the same width on all lines!");
            }

            fileReader.Close();

            // Create the tile map since we know our dimensions now
            TileInformation[,] result = new TileInformation[lineWidth, lineCount];

            // Blow through the saved buffer
            for (int y = 0; y < lineBuffer.Count; y++)
            {
                String currentLine = lineBuffer[y];

                for (int x = 0; x < currentLine.Length; x++)
                {
                    result[x, y] = TileData[currentLine[x]];

                    if (result[x, y].SpecialFunction != SPECIAL_FUNCTION.NOTHING)
                    {
                        if (foreground)
                            switch (result[x, y].SpecialFunction)
                            {
                                case SPECIAL_FUNCTION.SPAWN_COIN:
                                    {
                                        RedCoin goody = new RedCoin(InternalGame, "Images/red_coin")
                                        {
                                            Position = new Vector2(x * TileDimensions.X, y * TileDimensions.Y),
                                        };
                                        InternalGame.Goodies.Add(goody);

                                        break;
                                    }

                                case SPECIAL_FUNCTION.SPAWN_KOOPA:
                                    {
                                        Yoshi.Koopa koopa = new Yoshi.Koopa(InternalGame)
                                        {
                                            Position = new Vector2(x * TileDimensions.X, y * TileDimensions.Y),
                                        };
                                        koopa.Position -= new Vector2(0, 1);
                                        InternalGame.Entities.Add(koopa);

                                        break;
                                    }

                                case SPECIAL_FUNCTION.SPAWN_PLAYER:
                                    {
                                        InternalGame.Player.TileCoordinates = new Point(x, y);
                                        InternalGame.Player.Position -= new Vector2(0, 1);

                                        break;
                                    }
                            }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Loads a new map into the game, deleting all entities except for the primary player which is simply repositioned within the new game
        /// world when its ready.
        /// </summary>
        /// <param name="foregroundPath">
        /// The path to the map to be loaded for the main game interaction. This is the game world the player plays in.
        /// </param>
        /// <param name="backgroundPath">
        /// The path to the map to be loaded for the background. This map is simply rendered behind the foreground map.
        /// </param>
        public static void LoadMap(string foregroundPath, string backgroundPath)
        {
            InternalGame.CollectedGoodies = 0;

            InternalGame.Entities.Clear();
            InternalGame.Goodies.Clear();

            // Create the player
            InternalGame.Player = new Yoshi.Player(InternalGame)
            {
                Position = new Vector2(200, 100),
            };
            InternalGame.Entities.Add(InternalGame.Player);
            InternalGame.Player.BindControls();

            ForegroundTiles = GetMapData(foregroundPath, true);

            MapResolution = new Point(ForegroundTiles.GetUpperBound(0) + 1, ForegroundTiles.GetUpperBound(1) + 1);
            Console.WriteLine("TileManager: Loaded Map is {0}x{0}", MapResolution.X, MapResolution.Y);
            TileCount = MapResolution.X * MapResolution.Y;

            if (backgroundPath != null)
                BackgroundTiles = GetMapData(backgroundPath, false);
            else
            {
                BackgroundTiles = new TileInformation[MapResolution.X, MapResolution.Y];

                for (int y = 0; y < MapResolution.Y; y++)
                    for (int x = 0; x < MapResolution.X; x++)
                        BackgroundTiles[x, y] = TileData['g'];
            }

            foreach (RedCoin goody in InternalGame.Goodies)
                goody.Initialize();

            foreach (ControlledEntity entity in InternalGame.Entities)
                entity.Initialize();
        }
        
        /// <summary>
        /// Private constructor to prevent direct construction.
        /// </summary>
        private MapManager() { }
    }
}
