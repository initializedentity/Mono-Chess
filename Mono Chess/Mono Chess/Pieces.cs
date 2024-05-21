using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;

namespace Mono_Chess
{
    public enum piece
    {
        none,
        whitepawn,
        whiterook,
        whiteknight,
        whitebishop,
        whitequeen,
        whiteking,
        blackpawn,
        blackrook,
        blackknight,
        blackbishop,
        blackqueen,
        blackking
    }

    internal class Pieces
    {
        private Texture2D chesspieces;
        private SoundEffect pick, place; //Store audio file with move sfx

        private piece[,] board = new piece[8,8]; //NOOOOOO YOU DON'T UNDERSTAND YOU CAN'T USE AN ENUM WITH AN INT, MUH ABSTRACTION, YOU MUST MAKE IT THE SAME OBJECT TYPE HOW ELSE AM I SUPPOSED THAT AN ENUM WITH AN INTEGER VALUE IS THE SAME AS A PRIMITIVE INT
        private int width = 1920, height = 1440, xoffset, yoffset, clicktimer, clickdelay, selectedxoffset, selectedyoffset; //real ones + offsets
        private float scale, selectedscale; //Scale for pieces
        private bool whiteturn = true; //White always goes first

        private Matrix inversematrix;
        private Vector2 letterboxing, intransit; //Recieves halved values since letterboxing beyond board is irrelevant, these are removed from cursor positioning before descaling (RenderTarget goes from start of board to end, no letterboxing). intransit stores piece in transit

        private MouseState lastclick, currentclick;

        public Pieces(int _clickdelay)
        {
            ResetBoard();

            clickdelay = _clickdelay;
            clicktimer = clickdelay;

            intransit.X = intransit.Y = -1;
        }

        public void LoadContent(ContentManager content)
        {
            chesspieces = content.Load<Texture2D>("Chess_Pieces_Sprite");
            pick = content.Load<SoundEffect>("Pickup");
            place = content.Load<SoundEffect>("Place");
        }

        public void Update(GameTime gameTime, bool IsActive)
        {
            clicktimer -= gameTime.ElapsedGameTime.Milliseconds;
            lastclick = currentclick;
            currentclick = Mouse.GetState();

            //Check if time between clicks has elapsed and valid left click
            if (clicktimer < 1 && lastclick.LeftButton == ButtonState.Released && currentclick.LeftButton == ButtonState.Pressed && IsActive)
            {
                //Figure out why math below needed
                Vector2 scaledmouse = Vector2.Transform(new Vector2((currentclick.X - letterboxing.X), (currentclick.Y - letterboxing.Y)), inversematrix);

                //This math I know, we simply figure out what an eight of both axis is, and then divide the positions by the eight, the value ROUNDED DOWN is the current xy position in the matrix :)
                int xeighth = width / 8, yeighth = height / 8;
                int xpos = (int)(scaledmouse.X / xeighth), ypos = (int)(scaledmouse.Y / yeighth);

                //Check if valid range (avoid invalid ranges < 0 and > 7)
                if (currentclick.X > letterboxing.X && currentclick.Y > letterboxing.Y && xpos < 8 && ypos < 8)
                {
                    //If piece in transit 
                    if (intransit.X >= 0 && intransit.Y >= 0)
                    {
                        board[ypos, xpos] = board[(int)intransit.Y, (int)intransit.X];

                        if (xpos != (int)intransit.X || ypos != (int)intransit.Y)
                        {
                            board[(int)intransit.Y, (int)intransit.X] = piece.none;
                            whiteturn = !whiteturn;
                        }

                        intransit.X = intransit.Y = -1;
                        place.Play();
                        //MOAR DEBUUUUUUUUUUUUUUG
                        /*Debug.WriteLine(board[ypos, xpos]);
                        Debug.WriteLine(board[(int)intransit.Y, (int)intransit.X]);*/
                    }

                    //If no piece in transit and piece not deselected, and currently attempting to select a piece of the same color as the players turn
                    else if (board[ypos, xpos] != piece.none && ((whiteturn && board[ypos, xpos] >= piece.whitepawn && board[ypos, xpos] <= piece.whiteking) || (!whiteturn && board[ypos, xpos] >= piece.blackpawn && board[ypos, xpos] <= piece.blackking)))
                    {
                        intransit.X = xpos;
                        intransit.Y = ypos;

                        pick.Play();
                    }
                }

                //MOAR DEBUG
                /*Debug.WriteLine("------------");
                Debug.WriteLine(xpos);
                Debug.WriteLine(ypos);
                Debug.WriteLine("------------");*/

                //Debug stuff
                /*Debug.WriteLine(currentclick.X + " | " + scaledmouse.X);
                Debug.WriteLine(currentclick.Y + " | " + scaledmouse.Y);*/

                //Once 300ms have elapsed
                clicktimer = clickdelay;
            }
        }

        public void Draw(SpriteBatch _spriteBatch)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    _spriteBatch.Draw(
                                      chesspieces,
                                      new Vector2(((width / 8) * j) + ((intransit.X == j && intransit.Y == i) ? selectedxoffset : xoffset), ((height / 8) * i) - ((intransit.X == j && intransit.Y == i) ? selectedyoffset : yoffset)),
                                      SpritePiece(board[i, j]),
                                      Color.White,
                                      0f,
                                      Vector2.Zero,
                                      //Needs adjusting
                                      ((intransit.X == j && intransit.Y == i) ? selectedscale : scale),
                                      SpriteEffects.None,
                                      0f
                                     );
                }
            }
        }

        private Rectangle SpritePiece(piece spriteid)
        {
            return spriteid switch
            {
                //pawns should be 2131 but was changed to 2129 so it reads until 2560 and sprite doesn't stretch
                piece.whitepawn => new Rectangle(2129, 0, 427, 427),
                piece.whiterook => new Rectangle(1708, 0, 427, 427),
                piece.whiteknight => new Rectangle(1281, 0, 427, 427),
                piece.whitebishop => new Rectangle(854, 0, 427, 427),
                piece.whitequeen => new Rectangle(427, 0, 427, 427),
                piece.whiteking => new Rectangle(0, 0, 427, 427),

                //For blacks we start reading at 426, technically it should be 427, but sprite has odd height meaning it's not perfectly divisble by 2
                piece.blackpawn => new Rectangle(2129, 426, 427, 427),
                piece.blackrook => new Rectangle(1708, 426, 427, 427),
                piece.blackknight => new Rectangle(1281, 426, 427, 427),
                piece.blackbishop => new Rectangle(854, 426, 427, 427),
                piece.blackqueen => new Rectangle(427, 426, 427, 427),
                piece.blackking => new Rectangle(0, 426, 427, 427),
                _ => new Rectangle(0, 0, 0, 0),
            };
        }

        public void UpdateResolution(int _width, int _height, Matrix _inversematrix, Vector2 _letterboxing)
        {
            width = _width;
            height = _height;
            inversematrix = _inversematrix;
            letterboxing = _letterboxing;

            //Update Offset
            switch (((int)(((float)width / (float)height) * 10)))
            {
                case 16:
                    xoffset = 45;
                    yoffset = 6;
                    scale = 0.35f;
                    selectedxoffset = 26;
                    selectedyoffset = 30;
                    selectedscale = 0.45f;
                    break;

                case 17:
                    xoffset = 55;
                    yoffset = 0;
                    scale = 0.3f;
                    selectedxoffset = 36;
                    selectedyoffset = 24;
                    selectedscale = 0.41f;
                    break;

                default:
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
            //Sad Violin Noises
            board[0, 0] = piece.blackrook;
            board[0, 1] = piece.blackknight;
            board[0, 2] = piece.blackbishop;
            board[0, 3] = piece.blackqueen;
            board[0, 4] = piece.blackking;
            board[0, 5] = piece.blackbishop;
            board[0, 6] = piece.blackknight;
            board[0, 7] = piece.blackrook;

            board[7, 0] = piece.whiterook;
            board[7, 1] = piece.whiteknight;
            board[7, 2] = piece.whitebishop;
            board[7, 3] = piece.whitequeen;
            board[7, 4] = piece.whiteking;
            board[7, 5] = piece.whitebishop;
            board[7, 6] = piece.whiteknight;
            board[7, 7] = piece.whiterook;

            for (int i = 1; i < 7; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    //FUCKING C# BULLSHIT STOP FORCING ME TO CONVERT INTS TO BOOL, OR USE KEYWORDS LIKE IS INSTEAD OF == AND WHY THE FUCK CAN'T I USE ASSIGNMENTS AS STATEMENTS SAYING X = 1 IS LITERALLY A FUCKING STATEMENT
                    //((i == 1 || i == 6) ? board[i, j] = piece.pawn : board[i, j] = piece.none);
                    switch (i)
                    {
                        case 1:
                            board[i, j] = piece.blackpawn;
                            break;

                        case 6:
                            board[i, j] = piece.whitepawn;
                            break;

                        default:
                            board[i, j] = piece.none;
                            break;
                    }
                }
            }
        }
    }
}
