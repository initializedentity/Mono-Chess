using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace Mono_Chess
{
    public class Game1 : Game
    {
        //Monogame Variables
        private GraphicsDeviceManager _graphics; //Graphics Manager
        private SpriteBatch _spriteBatch; //Sprite Batch
        private Texture2D tile; //Single Pixel Texture for Tile
        private SpriteFont font; //Font Sprite for Game Over Screen
        private Song backgroundmusic; //Load background music
        private float volume = 0.25f; //Volume

        //Render Variables
        private int realwidth = 1920, realheight = 1440; //Store rendertarget width and height
        private RenderTarget2D _renderTarget; //2D Texture that we draw to instead instead of screen (no othere file needs access, it is set on instance of _graphics and any related classes obey it)
        private Rectangle _renderDestination; //Rectangle that contains the starting X and Y positions to draw _renderTarget, and the scaling of _renderTarget
        private bool resizing = false; //Used in WindowSizeChanged event method to stop event from resizing window if it's in the process of being resized

        //Input Variables
        private const int KEYPRESSDELAY = 200; //Milisec delay between keypresses
        private int keytimer = KEYPRESSDELAY; //Count time until next keypress valid
        KeyboardState lastkey, currentkey; //Store keypress states

        //Game State Variables
        public int[,] boardtiles = new int[8, 8]; //Matrix used to determine 
        private int gamestate = 0; //0 none, 1 white won, 2 black won
        private Pieces pieces; //Instance of Pieces Class (class that manages the pieces on the board)
        private bool pawnpromotion = false; //Stop updating Pieces until user promotes pawn
        

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this); //Graphics Manager Instance

            //_graphics.IsFullScreen = true; //Start in fullscreen
            _graphics.HardwareModeSwitch = false; //HardwareModeSwitch 1 = real fullscreen (changes graphics card resolution), 0 = fake fullscreen (Borderless)

            Window.AllowUserResizing = true; //Allow user to manually resize window
            Window.AllowAltF4 = true; //Allow Alt F4 to close window
            Window.Title = "Mono Chess"; //Set Window Title
            Window.ClientSizeChanged += WindowSizeChanged; //Subscribe WindowSizeChanged method to ClientSizeChanged event

            Content.RootDirectory = "Content"; //Main Directory used to load assets
            IsMouseVisible = true; //Set mouse visibility to true
        }

        protected override void Initialize()
        {
            //Code Below is needed if starting in windowed mode, if not remove it
            pieces = new Pieces(KEYPRESSDELAY); //Create new instance of class Pieces

            _graphics.PreferredBackBufferWidth = (GraphicsDevice.Adapter.CurrentDisplayMode.Width / 2); //Set width to half the monitor resolution
            _graphics.PreferredBackBufferHeight = (GraphicsDevice.Adapter.CurrentDisplayMode.Height / 2); //Set height to half the monitor height
            _graphics.ApplyChanges(); //Apply Changes

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice); //New instance of spriteBatch, used to draw on the screen
            
            tile = new Texture2D(GraphicsDevice, 1, 1); //Texture used to create the tiles, single pixel gets scaled to 1/64 of the screen (8 * 8 = 64)
            tile.SetData(new[] { Color.White }); //Set Pixel color masking to White (no masking)

            font = Content.Load<SpriteFont>("GameOver"); //Load font
            backgroundmusic = Content.Load<Song>("Dreamy Flashback");
            MediaPlayer.Play(backgroundmusic);
            MediaPlayer.Volume = volume;
            MediaPlayer.IsRepeating = true;

            //Create render target, set it to use the graphics card we're using, and the native width and height (determined by aspect ratio on init)
            _renderTarget = new RenderTarget2D(GraphicsDevice, realwidth, realheight);
            ScalePositionRenderTarget(); //Calculate Scaling of RenderTarget (RenderDestination Rectangle)

            pieces.LoadContent(Content); //Load content for Pieces class
        }

        protected override void Update(GameTime gameTime)
        {
            lastkey = currentkey; //Store last keypress
            currentkey = Keyboard.GetState(); //Store current keypress

            keytimer -= (int)gameTime.ElapsedGameTime.Milliseconds; //Calculate if time between keypress has elapsed 

            //foreach (Keys key in currentkey.GetPressedKeys())
            //{ Debug.WriteLine(key.ToString()); }

            //If ESC pressed exit game
            if (keytimer < 1 && currentkey.IsKeyDown(Keys.Escape) && IsActive)
                Exit();

            else if (keytimer < 1 && currentkey.IsKeyDown(Keys.Add) && IsActive)
            { volume += (volume < 1.0f ? 0.05f : 0); MediaPlayer.Volume = volume; }

            else if (keytimer < 1 && currentkey.IsKeyDown(Keys.OemMinus) && IsActive)
            { volume -= (volume > 0.10f ? 0.05f : 0); MediaPlayer.Volume = volume; }

            else if (keytimer < 1 && lastkey.IsKeyUp(Keys.M) && currentkey.IsKeyDown(Keys.M) && IsActive)
            {
                if (MediaPlayer.State == MediaState.Paused)
                    MediaPlayer.Resume();

                else
                    MediaPlayer.Pause();
            }

            //If R key pressed, reset game state
            else if (keytimer < 1 && lastkey.IsKeyUp(Keys.R) && currentkey.IsKeyDown(Keys.R) && IsActive)
            { pieces.ResetBoard(); keytimer = KEYPRESSDELAY; }

            //All ifs verify if keytimer is up. This one checks the F11 key and goes fullscreen or windowed accordingly
            else if (keytimer < 1 && lastkey.IsKeyUp(Keys.F11) && currentkey.IsKeyDown(Keys.F11) && IsActive)
            {
                if (_graphics.IsFullScreen) //If fullscreen
                {
                    _graphics.IsFullScreen = false; //Go windowed
                    _graphics.PreferredBackBufferWidth = (GraphicsDevice.Adapter.CurrentDisplayMode.Width / 2); //Set width to half monitor resolution
                    _graphics.PreferredBackBufferHeight = (GraphicsDevice.Adapter.CurrentDisplayMode.Height / 2); //Same with height
                }

                else //If Windowed
                    _graphics.IsFullScreen = true; //Go fullscreen

                keytimer = KEYPRESSDELAY; //Reset key timer
                _graphics.ApplyChanges(); //Apply changes
            }

            if (!pawnpromotion)
            {
                //Apparently this isn't a hack and is legit... Still looks fucking disgusting tho.
                var boardelements = pieces.Update(gameTime, IsActive); //Get tuple from Pieces Update
                boardtiles = boardelements.legalmoves; //Store matrix containing all legal moves and tiles containing kings in check
                gamestate = boardelements.gamestate; //Store gamestate
                pawnpromotion = boardelements.pawnpromotion; //Determine if pawnpromotion moment
            }

            //Promote to Rook and reset pawnpromotion flag
            else if (keytimer < 1 && lastkey.IsKeyUp(Keys.D1) && currentkey.IsKeyDown(Keys.D1) && IsActive)
            { pieces.PromotePawn(2); pawnpromotion = false; }

            //Promote to Knight and reset pawnpromotion flag
            else if (keytimer < 1 && lastkey.IsKeyUp(Keys.D2) && currentkey.IsKeyDown(Keys.D2) && IsActive)
            { pieces.PromotePawn(3); pawnpromotion = false; }

            //Promote to Bishop and reset pawnpromotion flag
            else if (keytimer < 1 && lastkey.IsKeyUp(Keys.D3) && currentkey.IsKeyDown(Keys.D3) && IsActive)
            { pieces.PromotePawn(4); pawnpromotion = false; }

            //Promote to Queen and reset pawnpromotion flag
            else if (keytimer < 1 && lastkey.IsKeyUp(Keys.D4) && currentkey.IsKeyDown(Keys.D4) && IsActive)
            { pieces.PromotePawn(5); pawnpromotion = false; }

            base.Update(gameTime);
        }

        //transformMatrix: Matrix.CreateScale((Window.ClientBounds.Width / realwidth), (Window.ClientBounds.Height / realheight), 0f) (inside.Begin) //Code kept purely for documentation purposes

        protected override void Draw(GameTime gameTime)
        {
            if (gamestate < 3) //If not game over
            {
                //Set render target to our RenderTarget
                GraphicsDevice.SetRenderTarget(_renderTarget);

                //Clear _renderTarget
                GraphicsDevice.Clear(Color.Black);

                //Enable sampling so scaling looks nicer
                _spriteBatch.Begin(samplerState: SamplerState.AnisotropicClamp);

                Color tilecolor = Color.White; //Set default color

                for (int j = 0; j < 8; j++) //For every line
                {
                    for (int i = 0; i < 8; i++) //For every column
                    {
                        //Check which color to use (if line and column are even or if line and column are odd. This works because on odd lines we want odd columns with the color even
                        //columns would have on an even line. Also because a sum between even or odd numbers is an even number). Used to be ternary expression, turned to if for clarity.
                        if ( ((j + i) % 2) == 0 )
                        {
                            tilecolor = Color.Beige; ///Default color

                            if (boardtiles[j, i] > 0) //If movable tile
                                tilecolor = Color.DeepSkyBlue;

                            else if (boardtiles[j, i] < 0) //If tile where King is, is in Check
                                tilecolor = Color.Red;
                        }

                        else //If line and column are both not either even or odd (aka sum between them is odd number)
                        {
                            tilecolor = Color.Sienna; //Default color

                            if (boardtiles[j, i] > 0) //If movable tile
                                tilecolor = Color.RoyalBlue;

                            else if (boardtiles[j, i] < 0) //If tile where king is, is in check
                                tilecolor = Color.DarkRed;
                        }

                        //Draw tile
                        _spriteBatch.Draw(
                                          tile, //Texture
                                          new Vector2((realwidth / 8) * i, (realheight / 8) * j), //Position
                                          null, //Originally was Rectangle that would stretch texture
                                          tilecolor, //Color
                                          0f, //Rotation
                                          Vector2.Zero, //Center of Rotation
                                          (realwidth * realheight) / 64, //Scaling (Remember 8 * 8 = 64, therefore we want ours to scale up to 1/64 of the screen)
                                          SpriteEffects.None, //Effects
                                          0f //Layer Depth
                                          );
                    }
                }

                pieces.Draw(_spriteBatch); //Draw Pieces
                _spriteBatch.End(); //Finish Drawing to RenderTarget

                //Set to default render target (Window)
                GraphicsDevice.SetRenderTarget(null);

                //Clear window
                GraphicsDevice.Clear(Color.Black);

                //Enable sampling
                _spriteBatch.Begin(samplerState: SamplerState.AnisotropicClamp);
                _spriteBatch.Draw(_renderTarget, _renderDestination, Color.White); //Draw Render Target
                _spriteBatch.End(); //Finish Drawing to Window

                base.Draw(gameTime);
            }

            else //If game over
            {
                //Render to _renderTarget
                GraphicsDevice.SetRenderTarget(_renderTarget);

                //Clear _renderTarget
                GraphicsDevice.Clear(Color.Black);

                //Enable sampling so scaling looks nicer
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

                _spriteBatch.DrawString(
                                        font, //Sprite texture
                                        "    Game Over.\n" + (gamestate < 4 ? "     Black " : "    White ") + "Won!\n\n" + "Press R to Restart.", //String "Game Over\nWhite/Black Won!\n\nPress R to Restart"
                                        new Vector2((realwidth / 2) - 320, (realheight / 2) - 280), // Position on screen to draw (constant as there was no time to debug. Seems stable
                                        Color.White, //Color mask (none)
                                        0f, //Rotation
                                        Vector2.Zero, //Center of rotation
                                        2, //Scale to be aplied to text
                                        SpriteEffects.None, //Sprite effect
                                        0f //Layer Depth
                                        );

                _spriteBatch.End(); //Finish drawing to Render Target

                //Set to default render target (Window)
                GraphicsDevice.SetRenderTarget(null);

                //Clear window
                GraphicsDevice.Clear(Color.Black);

                //Enable sampling so scaling looks nicer
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                _spriteBatch.Draw(_renderTarget, _renderDestination, Color.White); //Draw to Window
                _spriteBatch.End(); //Finish Drawing

                base.Draw(gameTime);
            }
        }

        private void ScalePositionRenderTarget()
        {
            //Calculate scale between axis
            float xscale = (float) ( (float) GraphicsDevice.Viewport.Width / (float) _renderTarget.Width );
            float yscale = (float) ( (float) GraphicsDevice.Viewport.Height / (float) _renderTarget.Height );
            
            //Store smallest scale so entire board fits on screen
            float scale = Math.Min(xscale, yscale);

            //Calculate new dimensions of Render Destination (follows window dimensions)
            _renderDestination.Width = (int) (_renderTarget.Width * scale);
            _renderDestination.Height = (int) (_renderTarget.Height * scale);

            //Calculate where to start drawing on the window (Render Destination might not cover entire window, so we calculate the difference and divide it by two, centering the content on the window)
            _renderDestination.X = ( (GraphicsDevice.Viewport.Width - _renderDestination.Width) / 2 );
            _renderDestination.Y = ( (GraphicsDevice.Viewport.Height - _renderDestination.Height) / 2 );

            float inversescale = 0f;

            if (yscale < xscale) //Once again use smallest scale
                inversescale = (float) ( (float) _renderTarget.Height / (float) GraphicsDevice.Viewport.Height ); //Height smallest

            else
                inversescale = (float) ( (float) _renderTarget.Width / (float) GraphicsDevice.Viewport.Width); //Width smallest

            //Pass values necessary to Pieces class (Render Target width and height, inverse scale, and board position offset)
            pieces.UpdateResolution(realwidth, realheight, inversescale, new Vector2(_renderDestination.X, _renderDestination.Y));
        }

        private void WindowSizeChanged(Object sender, EventArgs e) //Event called when window size changes
        {
            //Verify window to see if new supported aspect ratio
            if (!resizing && Window.ClientBounds.Width > 0 && Window.ClientBounds.Height > 0) //If window not already being resized and window dimensions are larger than 0
            {
                resizing = true; //Window is currently being resized

                switch ( (int) (( (float) GraphicsDevice.Viewport.Width / (float) GraphicsDevice.Viewport.Height ) * 10) ) //Calculate window aspect ratio
                {
                    case 16: //16:10
                        realwidth = 1920;
                        realheight = 1200;

                        break;

                    case 17: //16:9
                        realwidth = 1920;
                        realheight = 1080;

                        break;

                    default: //4:3
                        realwidth = 1920;
                        realheight = 1440;

                        break;
                }

                //Create new RenderTarget with new aspect ratio
                _renderTarget = new RenderTarget2D(GraphicsDevice, realwidth, realheight);

                ScalePositionRenderTarget(); //Calculate Scaling of RenderTarget (RenderDestination Rectangle)
                resizing = false; //Finished resizing
            }
        }
    }
}