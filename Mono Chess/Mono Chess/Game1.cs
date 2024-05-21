using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace Mono_Chess
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D tile;
        private Pieces pieces;

        private int realwidth = 1920, realheight = 1440, keytimer;
        private RenderTarget2D _renderTarget; //2D Texture that we draw to instead instead of screen (no othere file needs access, it is set on instance of _graphics and any related classes obey it)
        private Rectangle _renderDestination; //Rectangle that contains the starting X and Y positions to draw _renderTarget, and the scaling of _renderTarget
        private bool resizing = false;

        private const int KEYPRESSDELAY = 200;
        KeyboardState lastkey, currentkey;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);

            // CODE BELOW IS IRRELEVANT

                //BOTH ASSIGNMENTS ARE THE SAME. THE GraphicsDevice MEMBER OF GAME1 IS EQUAL TO THE member of _graphics (https://stackoverflow.com/questions/13552199/difference-between-game1-graphicsdevice-and-graphics-graphicsdevice) Console.WriteLine(ReferenceEquals(graphics.GraphicsDevice, this.GraphicsDevice)); USE Game.GraphicsDevice NOT _graphics
                /*width = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                height = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

                //GraphicsDevice is null here, so we set the preferred window height and width in the initialize method (only relevant if starting windowed, otherwise windowsizechanged event handles fullscreen rez)
                width = _graphics.GraphicsDevice.DisplayMode.Width;
                height = _graphics.GraphicsDevice.DisplayMode.Height;

                _graphics.PreferredBackBufferWidth = realwidth;
                _graphics.PreferredBackBufferHeight = realheight;*/

            // CODE ABOVE IS IRRELEVANT

            //_graphics.IsFullScreen = true;
            _graphics.HardwareModeSwitch = false; //HardwareModeSwitch 1 = real fullscreen (changes graphics card resolution), 0 = fake fullscreen (Borderless)

            Window.AllowUserResizing = true;
            Window.AllowAltF4 = true;
            Window.Title = "Mono Chess";
            Window.ClientSizeChanged += WindowSizeChanged; //+= a function

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            //My vars
            keytimer = KEYPRESSDELAY;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            // CODE BELOW IS IRRELEVANT

                //Determine aspect ratio of window
                /*switch( ((int)(((float)GraphicsDevice.Viewport.Width / (float)GraphicsDevice.Viewport.Height) * 10)) )
                {
                    case 16:
                        realwidth = 1920;
                        realheight = 1200;
                        break;

                    case 17:
                        realwidth = 1920;
                        realwidth = 1080;
                        break;

                    default:
                        realwidth = 1920;
                        realheight = 1440;
                        break;
                }

                //Set window res to half desktop res (windowed mode debug, when done this is commented)
                _graphics.PreferredBackBufferWidth = (realwidth);
                _graphics.PreferredBackBufferHeight = (realheight);
                _graphics.ApplyChanges();*/

            //CODE ABOVE IS IRRELEVANT

            //If starting program windowed, code below needed (sets default window size and invokes WindowSizeChanged that sends necessary data to the pieces class
            pieces = new Pieces(KEYPRESSDELAY);

            _graphics.PreferredBackBufferWidth = (GraphicsDevice.Adapter.CurrentDisplayMode.Width / 2);
            _graphics.PreferredBackBufferHeight = (GraphicsDevice.Adapter.CurrentDisplayMode.Height / 2);
            _graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // TODO: use this.Content to load your game content here
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            tile = new Texture2D(GraphicsDevice, 1, 1); //Tile pixel, later gets scaled to 1/64 of the screen size (8 * 8 = 64)
            tile.SetData(new[] { Color.White } );

            //Create render target, set it to use the graphics card we're using, and the native width and height (determined by aspect ratio on init)
            _renderTarget = new RenderTarget2D(GraphicsDevice, realwidth, realheight);
            ScalePositionRenderTarget();

            pieces.LoadContent(Content);
            _graphics.ApplyChanges();
        }

        protected override void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            lastkey = currentkey;
            currentkey = Keyboard.GetState();

            keytimer -= (int) gameTime.ElapsedGameTime.TotalMilliseconds;

            //Switch between fullscree and windowed mode (fullscreen default) (this.IsActive)
            if (keytimer < 1 && lastkey.IsKeyUp(Keys.F11) && currentkey.IsKeyDown(Keys.F11) && IsActive)
            {
                if (_graphics.IsFullScreen)
                {
                    _graphics.IsFullScreen = false;
                    _graphics.PreferredBackBufferWidth = (GraphicsDevice.Adapter.CurrentDisplayMode.Width / 2);
                    _graphics.PreferredBackBufferHeight = (GraphicsDevice.Adapter.CurrentDisplayMode.Height / 2);
                }

                else
                    _graphics.IsFullScreen = true;

                _graphics.ApplyChanges();

                keytimer = KEYPRESSDELAY;
            }

            else if (keytimer < 1 && lastkey.IsKeyUp(Keys.R) && currentkey.IsKeyDown(Keys.R))
            {
                pieces.ResetBoard();
                keytimer = KEYPRESSDELAY;
            }

            pieces.Update(gameTime, IsActive);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            //transformMatrix: Matrix.CreateScale((Window.ClientBounds.Width / realwidth), (Window.ClientBounds.Height / realheight), 0f) (inside .Begin)

            //Render to _renderTarget
            GraphicsDevice.SetRenderTarget(_renderTarget);

            //Enable sampling so scaling looks nicer
            _spriteBatch.Begin(samplerState: SamplerState.AnisotropicClamp);

            for (int j = 0; j < 8; j++)
            {
                for (int i = 0; i < 8; i++)
                {
                    _spriteBatch.Draw(  
                                      tile, 
                                      new Vector2((realwidth / 8) * i, (realheight / 8) * j),
                                      null, //new Rectangle(0, 0, width / 8, height / 8), //x pos, y pos, width, height. Initial attempt at rendering board
                                      (!Convert.ToBoolean(((j + i) % 2)) ? Color.Beige : Color.Sienna), //(!Convert.ToBoolean((j % 2)) ? (!Convert.ToBoolean((i % 2)) ? Color.White : Color.Purple) : (!Convert.ToBoolean((i % 2)) ? Color.Purple : Color.White)), <- OLD VERSION (Hate that I saw this in a vid and did not figure it out by myself)
                                      0f,
                                      Vector2.Zero,
                                      (realwidth * realheight) / 8, //(REALWIDTH * REALHEIGHT) / 8 (1/64 of the width height screen ratio (8 * 8 = 64))
                                      SpriteEffects.None,
                                      0f
                                     ); 
                }
            }

            pieces.Draw(_spriteBatch);
            _spriteBatch.End();

            //Set to default render target (screen)
            GraphicsDevice.SetRenderTarget(null);

            _spriteBatch.Begin(samplerState: SamplerState.AnisotropicClamp);
            //Vector2.Zero was replaced by our Rectangle that gives us the scaling and starting positions (see documentation)
            _spriteBatch.Draw(_renderTarget, _renderDestination, Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void ScalePositionRenderTarget()
        {
            //Get scales for axis (same shit we've done with previous method)
            float xscale = (float)((float)GraphicsDevice.Viewport.Width / (float)_renderTarget.Width);
            float yscale = (float)((float)GraphicsDevice.Viewport.Height / (float)_renderTarget.Height);

            //Kinda like the if we had, but better
            float scale = Math.Min(xscale, yscale); //Determine smallest scale to ensure both scales fit (ie if width scale ends up too big then the height might be higher than viewport which leads to clipping)

            //Destination not Target since Rectangle will scale Target
            _renderDestination.Width = (int)(_renderTarget.Width * scale);
            _renderDestination.Height = (int)(_renderTarget.Height * scale);

            //Starting point to draw Target (Our scaling might not cover the whole screen, when that happens the bottom and right side would be empty, if we calculate the difference between the real resolution and scaled resolution we would get the same result, but at the top and left, if we divide by 2 we split the empty area between all the corners)
            _renderDestination.X = ((GraphicsDevice.Viewport.Width - _renderDestination.Width) / 2);
            _renderDestination.Y = ((GraphicsDevice.Viewport.Height - _renderDestination.Height) / 2);

            //Create scale and do a matrix invert idk also send a Vector2 to store letterboxing offset from top and left (letterboxing offset after board is irrelevant). Figure out why math below needed
            pieces.UpdateResolution(realwidth, realheight, Matrix.Invert(Matrix.CreateScale(scale, scale , 1.0f)), new Vector2(_renderDestination.X, _renderDestination.Y));

            //What a minor spelling mistake does to a mf
            //Debug.WriteLine("X: " + _renderDestination.Width + " | " + xscale + " | " + realwidth);
            //Debug.WriteLine("Y: " + _renderDestination.Height + " | " + yscale + " | " + realheight);
        }

        //Event method
        private void WindowSizeChanged(Object sender, EventArgs e)
        {
            //Verify window to see if new supported aspect ratio
            if (!resizing && Window.ClientBounds.Width > 0 && Window.ClientBounds.Height > 0)
            {
                resizing = true; //Prevent window from calculating nonstop when user is draggin window

                switch( ((int)(((float)GraphicsDevice.Viewport.Width / (float)GraphicsDevice.Viewport.Height) * 10)) )
                {
                    case 16:
                        realwidth = 1920;
                        realheight = 1200;
                        break;

                    case 17:
                        realwidth = 1920;
                        realheight = 1080;
                        break;

                    default:
                        realwidth = 1920;
                        realheight = 1440;
                        break;
                }

                //Create new RenderTarget with new aspect ratio
                _renderTarget = new RenderTarget2D(GraphicsDevice, realwidth, realheight);

                ScalePositionRenderTarget();
                resizing = false;
            }
        }
    }
}

/*
 * What we need:
 * 
 * - Background Image                           | DONE
 * - Sprites for pieces white and black         | DONE
 * - Sfx for picking up piece and placing it    | 
 * 
 * Desired behaviour:
 * 
 * - When a piece is picked up, showcase possible moves.
 * - If user clicks anywere but a valid move, we simply put the piece back down. | DONE
 * - Undo function that can undo all moves from current point in time.
 * - Redo function that can redo all moves so long as no new one has been made.
 * - When a piece goes to the same place as another, the moving piece erases the old one. | DONE
 * - When a pawn reaches the other side of the board (9 for white, 1 for black), he can choose to become a queen, bishop, rook (tower) or knight (honse). (Basically anything other than a king or a pawn)
 * - Special moves: Castling, En Passant (5 for white, 4 for black), and the above mentioned.
 * - Check fucntion that restricts the player to only moving the king.
 * - Check mate function that ends the game.
 * 
 * En Passant:
 * 
 * - If a black piece moves to the same spot as a white pawn is in(5 only) or a white piece moves to the spam spot as a black one (4 only), the player can capture it
 *   by moving to the square above said pawn.
 *  
 * Castling:
 * 
 * - If rook or king move, castling is disabled.
 * - King cannot be in check.
 * - Can't Castle King onto a check.
 * - No pieces between rook and king.
*/