using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;

namespace Mono_Chess
{
    internal class Pieces
    {
        //Stores piece type and color
        private struct pieces
        {
            public pieces() //Initializer (ensures that whenever a new instance of struct is created it will have these values by default)
            { piececolor = pieceid = 0; }

            public int piececolor; //0 for white (default) 1 for black
            public int pieceid; //0 none, 1 pawn, 2 rook, 3 knight, 4 bishop, 5 queen, 6 king
        }

        //Monogame Vars
        private Texture2D chesspieces;
        private SoundEffect pick, place; //Store audio file with move sfx

        //Stores all positions of white sprites, for black sprites simply sum a new Rectangle(0, 426, 0, 0) to any of the following elements
        private Rectangle[] spritelocations = {
                                                new Rectangle(0, 0, 0, 0),        //White None      0
                                                new Rectangle(0, 0, 427, 427),    //White Pawn      1
                                                new Rectangle(427, 0, 427, 427),  //White Rook      2
                                                new Rectangle(854, 0, 427, 427),  //White Knight    3
                                                new Rectangle(1281, 0, 427, 427), //White Bishop    4
                                                new Rectangle(1708, 0, 427, 427), //White Queen     5
                                                new Rectangle(2129, 0, 427, 427), //White King      6
                                                
                                                new Rectangle(0, 426, 427, 427),    //Black Pawn    7
                                                new Rectangle(427, 426, 427, 427),  //Black Rook    8
                                                new Rectangle(854, 426, 427, 427),  //Black Knight  9
                                                new Rectangle(1281, 426, 427, 427), //Black Bishop  10
                                                new Rectangle(1708, 426, 427, 427), //Black Queen   11
                                                new Rectangle(2129, 426, 427, 427)  //Black King    12
                                              };

        //Window Vars
        private int width = 1920, height = 1440, xoffset, yoffset, selectedxoffset, selectedyoffset; //Render Target dimensions, x axis offset and y axis offset (based on aspect ratio), x axis and y axis offset when selected (based on aspect ratio)
        private float scale, selectedscale, inversescale; //Scale for pieces, scale when piece selected and inverse scale
        private Vector2 letterboxing; //Letterbox offset values (x and y)

        //Game Vars
        private pieces[,] board = new pieces[8, 8]; //Matrix representing game
        private int [,] legalmoves = new int[8, 8]; //Matrix containing legal moves (sent to Game1.cs so the tiles change color)
        private bool whiteturn = true; //White always goes first
        private int gamestate = 0; //1 white wins, 2 black wins, 0 none

        //Mouse Click Vars
        private MouseState lastclick, currentclick; //Store Previous and Next Click
        private int clicktimer, clickdelay; //Delay between clicks, clickdelay recieves constant value from Game1 (this way only Game1 has to be modified)

        public Pieces(int _clickdelay) //Initializer, merely stores delay between registered clicks and sets timer
        {
            clickdelay = _clickdelay;
            clicktimer = clickdelay;
            ResetBoard();
        }

        public void LoadContent(ContentManager content)
        {
            chesspieces = content.Load<Texture2D>("Chess_Pieces_Sprite"); //Load sprite containing all pieces
            pick = content.Load<SoundEffect>("Pickup"); //Load pickup sound effect
            place = content.Load<SoundEffect>("Place"); //Load place sound efefct
        }

        public (int[,] legalmoves, int gamestate) Update(GameTime gameTime, bool IsActive) //Recieves gametime to check click timer and bool that says if window is active or not
        {
            clicktimer -= gameTime.ElapsedGameTime.Milliseconds; //Calculate time between clicks

            lastclick = currentclick; //Store last click
            currentclick = Mouse.GetState(); //Store current click

            //Check if click cooldown has elapsed and if mouse 1 clicked
            if (clicktimer < 1 && lastclick.LeftButton == ButtonState.Released && currentclick.LeftButton == ButtonState.Pressed && IsActive)
            {
                //Vector2 scaledmouse = Vector2.Transform(new Vector2((currentclick.X - letterboxing.X), (currentclick.Y - letterboxing.Y)), inversematrix); //Legacy inverse scale (inverse matrix of identity matrix with normal scale
                //Multiply mouse x and y by inverse scale (subtract x and y letterboxing to remove offset)
                Vector2 scaledmouse = new Vector2( ((currentclick.X - letterboxing.X) * inversescale), ((currentclick.Y - letterboxing.Y) * inversescale) );

                //Calculate eight of height and screen to calculate tile yx index (x is always 1920 so no need to calculate, should move to other position so no constant reassignment).
                int xeighth = 240, yeighth = height / 8;

                //Calculate tile yx index
                int xpos = (int) (scaledmouse.X / xeighth), ypos = (int) (scaledmouse.Y / yeighth);
            }

            return (legalmoves, gamestate);
        }

        public void Draw(SpriteBatch _spriteBatch)
        {
            int horizontaloffset = 0, verticaloffset = 0; float finalscale = 0f; //Final offsets and scale, used to make code easier to read

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (/*condition to check if in transit*/ false) //NEEDS TO BE CHANGED
                    {
                        horizontaloffset = selectedxoffset;
                        verticaloffset = selectedyoffset;
                        finalscale = selectedscale;
                    }

                    else
                    {
                        horizontaloffset = xoffset;
                        verticaloffset = yoffset;
                        finalscale = scale;
                    }

                    _spriteBatch.Draw(
                                      chesspieces, //Sprite
                                      new Vector2( ((width / 8) * j) + horizontaloffset, ((height / 8) * i) - verticaloffset ), //Offset, when dealing with axis we always divide by eight, only when dealing with both simultaneously do we divide by 64
                                      spritelocations[ board[i, j].pieceid + (6 * board[i,j].piececolor) ], //Use array of Rectangles to get sprite location and dimension (pieceid + (6 * piececolor)) where pieceid 0->6 and piececolor 0->1
                                      Color.White, //No color masking
                                      0f, //Rotation
                                      Vector2.Zero, //Center of Rotation
                                      finalscale, //Scale
                                      SpriteEffects.None, //Sprite effects
                                      0f //Layer depth
                                     );
                }
            }
        }
        
        public void UpdateResolution(int _width, int _height, float _inversescale, Vector2 _letterboxing) //Good enough
        {
            width = _width; //Store Render Target width
            height = _height; //Store Render Target height
            inversescale = _inversescale; //Store inverse scale
            letterboxing = _letterboxing; //Store letterboxing offset

            //inversematrix = _inversematrix; //Legacy, here for documentation purposes

            //Update Offset
            switch ( ((int)(((float)width / (float)height) * 10)) ) //Calculate aspect ratio
            {
                case 16: // 16:10
                    xoffset = 45;
                    yoffset = 6;
                    scale = 0.35f;

                    selectedxoffset = 26;
                    selectedyoffset = 30;
                    selectedscale = 0.45f;

                    break;

                case 17: //16:9
                    xoffset = 55;
                    yoffset = 0;
                    scale = 0.3f;

                    selectedxoffset = 36;
                    selectedyoffset = 24;
                    selectedscale = 0.41f;

                    break;

                default: //4:3
                    xoffset = 32;
                    yoffset = 4;
                    scale = 0.4f;

                    selectedxoffset = 14;
                    selectedyoffset = 24;
                    selectedscale = 0.51f;

                    break;
            }
        }

        public void ResetBoard()
        {
            clicktimer = clickdelay; //Click timer
            whiteturn = true; //Whose turn it is
            gamestate = 0; //Checkmate bool
            //intransit.X = intransit.Y = -1; //Piece in transit (probably gonna be deleted)

            //Set main pieces (rook -> king)
            board[0, 0].pieceid = board[7, 0].pieceid = 2;
            board[0, 1].pieceid = board[7, 1].pieceid = 3;
            board[0, 2].pieceid = board[7, 2].pieceid = 4;
            board[0, 3].pieceid = board[7, 3].pieceid = 5;
            board[0, 4].pieceid = board[7, 4].pieceid = 6;
            board[0, 5].pieceid = board[7, 5].pieceid = 4;
            board[0, 6].pieceid = board[7, 6].pieceid = 3;
            board[0, 7].pieceid = board[7, 7].pieceid = 2;

            //Set pawns and their respective color (also the main pieces colors)
            for (int i = 0; i < 8; i++)
            {
                board[1, i].pieceid = board[6, i].pieceid = 1; //Assign pawns

                board[0, i].piececolor = board[1, i].piececolor = 1; //Assign piece color for blacks (others and pawns)
                board[7, i].piececolor = board[6, i].piececolor = 0; //Assign piece color for whites (others and pawns)
            }

            //Empty remaining tiles
            for (int i = 2; i < 6; i++) //Starting from the second line (line 0 and 1 have placed pieces)
            {
                for (int j = 0; j < 8; j++) //For every column
                {
                    board[i, j].pieceid = 0; //Set piece to none
                    board[i, j].piececolor = 0; //0 means white (default)
                }
            }

            ResetLegalMoves();
        }

        public void ResetLegalMoves()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                    legalmoves[i, j] = 0;
            }
        }

        public void LegalMoves(int x, int y)
        {
            
        }
    }
}
