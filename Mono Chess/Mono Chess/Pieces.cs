using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mono_Chess
{
    internal class Pieces
    {
        //Stores piece type and color
        private struct pieces
        {
            public pieces() //Initializer (ensures that whenever a new instance of struct is created it will have these values by default)
            { piececolor = pieceid = 0; }

            public int piececolor; //0 for none 1 for white 2 for black
            public int pieceid; //0 none, 1 pawn, 2 rook, 3 knight, 4 bishop, 5 queen, 6 king
        }

        //Monogame Vars
        private Texture2D chesspieces;
        private SoundEffect pick, place; //Store audio file with move sfx

        //Window Vars
        private int width = 1920, height = 1440, xoffset, yoffset, selectedxoffset, selectedyoffset; //Render Target dimensions, x axis offset and y axis offset (based on aspect ratio), x axis and y axis offset when selected (based on aspect ratio)
        private float scale, selectedscale, inversescale; //Scale for pieces, scale when piece selected and inverse scale
        private Vector2 letterboxing; //Letterbox offset values (x and y)

        //Stores all positions of white sprites, for black sprites simply sum a new Rectangle(0, 426, 0, 0) to any of the following elements
        private Rectangle[] spritelocations = {
                                                new Rectangle(2129, 0, 427, 427),    //White Pawn      0
                                                new Rectangle(1708, 0, 427, 427),    //White Rook      1
                                                new Rectangle(1281, 0, 427, 427),    //White Knight    2
                                                new Rectangle(854, 0, 427, 427),     //White Bishop    3
                                                new Rectangle(427, 0, 427, 427),     //White Queen     4
                                                new Rectangle(0, 0, 427, 427),       //White King      5
                                                
                                                new Rectangle(2129, 426, 427, 427),  //Black Pawn      6
                                                new Rectangle(1708, 426, 427, 427),  //Black Rook      7
                                                new Rectangle(1281, 426, 427, 427),  //Black Knight    8
                                                new Rectangle(854, 426, 427, 427),   //Black Bishop    9
                                                new Rectangle(427, 426, 427, 427),   //Black Queen     10
                                                new Rectangle(0, 426, 427, 427)      //Black King      11
                                              };


        //Mouse Input Vars
        private MouseState lastclick, currentclick; //Store Previous and Next Click
        private int clicktimer, clickdelay; //Delay between clicks, clickdelay recieves constant value from Game1 (this way only Game1 has to be modified)

        //Game Vars
        private pieces[,] board = new pieces[8, 8]; //Matrix representing game
        private int [,] legalmoves = new int[8, 8]; //Matrix containing legal moves (sent to Game1.cs so the tiles change color)
        private int gamestate = 0, colorturn = 1; //1 white check, 2 black check, 3 white checkmate, 4 black checkmate, 0 none | White always goes first
        private int[] selectedpiece = { -1, -1 };  //Piece that's currently being moved
        private int[] whiteking = { -1, -1}, blackking = { -1, -1}; //Positions of both kings (necessary to verify if in check or check mate)

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

        public (int[,] legalmoves, int gamestate, bool pawnpromotion) Update(GameTime gameTime, bool IsActive) //Recieves gametime to check click timer and bool that says if window is active or not
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

                if (ypos >= 0 && ypos < 8 && xpos >= 0 && xpos < 8)
                {
                    if ( selectedpiece[0] == -1 && board[ypos, xpos].piececolor == colorturn && (gamestate == 0 || (gamestate > 0 && board[ypos, xpos].pieceid == 6)) ) //No need to check both dimensions, we manually set them, so if one is -1 then they both are, we check that the player choses a piece of the correct turn, if a king is in check then the player can only move the king, if not then he can move whatever piece he wants
                    {
                        ResetLegalMoves(true);
                        selectedpiece[0] = ypos; selectedpiece[1] = xpos;

                        if (board[ypos, xpos].pieceid == 6) //Check if king can move, if he can't it's checkmate for him
                            KingLegalMoves(ypos, xpos);

                        else //See legalmoves for non king piece
                            NonKingLegalMoves(ypos, xpos);

                        pick.Play();
                    }

                    //Placed before placing condition to hijack placing in same position and handle it safely (not nuke the fucking piece with a blackhole)
                    else if (ypos == selectedpiece[0] && xpos == selectedpiece[1])
                    { selectedpiece[0] = selectedpiece[1] = -1; ResetLegalMoves(true); place.Play(); } //If moving to same place, simlpy deselect

                    //FUCK IT WE SHORT CIRCUIT EVALUATING THIS ONE, CAN'T LIVE IN FEAR
                    //else if (selectedpiece[0] >= 0 && board[selectedpiece[0], selectedpiece[1]].piececolor != board[ypos, xpos].piececolor) //Kept for debugging legalmoves, still here cuz idk lmao
                    else if (selectedpiece[0] >= 0 && legalmoves[ypos, xpos] == 1)
                    {
                        board[ypos, xpos].pieceid = board[ selectedpiece[0], selectedpiece[1] ].pieceid; //Store piece id
                        board[ypos, xpos].piececolor = board[ selectedpiece[0], selectedpiece[1] ].piececolor; //Store piece color

                        board[ selectedpiece[0], selectedpiece[1] ].pieceid = board[ selectedpiece[0], selectedpiece[1] ].piececolor = 0; //Clear old spot
                        selectedpiece[0] = selectedpiece[1] = -1; //Empty selected piece array (will be reused if pawn promotion occurs)
                        colorturn = (colorturn == 1 ? 2 : 1); //Switch turn (white turn then black turn, if not white turn then white turn), ternary expression cuz -> I ain't playing C#'s game

                        if (board[ypos, xpos].pieceid == 1 && (ypos == 0 || ypos == 7)) //If pawn was moved, we immediately return since there's nothing left to do
                        { selectedpiece[0] = ypos; selectedpiece[1] = xpos; return (legalmoves, gamestate, true); }

                        else if (board[ypos, xpos].pieceid == 6) //If instead the moved piece is a king, then update the correspondent var (WE MAKE USAGE OF YPOS AND XPOS SINCE THE INDEXES ON SELECTEDPIECE ARRAY ARE OUTDATED (PIECE IS MOVED IN CODE ABOVE))
                        {
                            if (board[ypos, xpos].piececolor == 1) //Update White King Location
                            { whiteking[0] = ypos; whiteking[1] = xpos; }

                            else //Update Black King Location
                            { blackking[0] = ypos; blackking[1] = xpos; }
                        }

                        if (board[ypos, xpos].piececolor == 1) //Verify if opposite king is in check or checkmate (White piece checks Black)
                            KingLegalMoves(blackking[0], blackking[1]);

                        else //(Black piece checks white)
                            KingLegalMoves(whiteking[0], whiteking[1]);

                        place.Play(); //Play drop sound effect

                        ResetLegalMoves(true);
                    }
                }
            }

            return (legalmoves, gamestate, false);
        }

        //Promotes pawn and returns (ADD KINGLEGALAMOVES)
        public void PromotePawn(int piece)
        {
            board[ selectedpiece[0], selectedpiece[1] ].pieceid = piece;

            if (board[selectedpiece[0], selectedpiece[1]].piececolor == 1)
                KingLegalMoves(blackking[0], blackking[1]);

            else
                KingLegalMoves(whiteking[0], whiteking[1]);
            
            selectedpiece[0] = selectedpiece[1] = -1;
            ResetLegalMoves(true);
            place.Play(); //Play drop sound effect
            return;
        }

        public void Draw(SpriteBatch _spriteBatch)
        {
            int horizontaloffset = 0, verticaloffset = 0; float finalscale = 0f; //Final offsets and scale, used to make code easier to read

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (board[i, j].pieceid == 0)
                        continue;

                    else if (i == selectedpiece[0] && j == selectedpiece[1])
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
                                      spritelocations[ (board[i, j].pieceid - 1) + (6 * (board[i, j].piececolor - 1)) ], //Use array of Rectangles to get sprite location and dimension pieceid - 1 since pieceid 0 is none and array of rectangle 0 is white pawn, piececolor - 1 since color 0 is none and 1 is white and 2 is black
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
            colorturn = 1;//Whose turn it is
            gamestate = 0; //Checkmate bool

            selectedpiece[0] = selectedpiece[1] = -1; //Reset selected piece
            whiteking[1] = blackking[1] = 4; //Both kings are on the fourth column from the left
            whiteking[0] = 7; //White king is in last line
            blackking[0] = 0; //Black king is in first line

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

                board[0, i].piececolor = board[1, i].piececolor = 2; //Assign piece color for blacks (others and pawns)
                board[7, i].piececolor = board[6, i].piececolor = 1; //Assign piece color for whites (others and pawns)
            }

            //Empty remaining tiles
            for (int i = 2; i < 6; i++) //Starting from the second line (line 0 and 1 have placed pieces)
            {
                for (int j = 0; j < 8; j++) //For every column
                    board[i, j].pieceid = board[i, j].piececolor = 0; //0 means none piece and color
            }

            ResetLegalMoves(false);
        }

        private void ResetLegalMoves(bool preservecheck)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (!preservecheck) //If we don't wish to preserve checks, then we just reset the entire legalmoves board
                        legalmoves[i, j] = 0;

                    else if (legalmoves[i, j] != -1) //Otherwise if we do wish to preserve it, we clear the board, with the exception of the check tiles
                        legalmoves[i, j] = 0;

                    //OLD CODE
                    /*if (preservecheck && legalmoves[i, j] != -1) //If we wish to preserve check and the current tile is in check then we skip it
                        continue;

                    else //If we don't wish to preserve the tile or the current tile isn't in check, we clear it
                        legalmoves[i, j] = 0;*/
                }
            }
        }

        private void NonKingLegalMoves(int y, int x)
        {
            switch (board[y, x].pieceid)
            {
                case 1: //Pawn
                    int direction = (board[y, x].piececolor == 1 ? -1 : 1); //direction (would use enums for pieceid too if I didn't have to do a fucking explicit cast) is used to swap direction based on piececolor, it's only relevant for pawns since they move in a specific direction, other pieces move in any direction but within specific rules (such as king only moving 1 tile or bishops only moving diagonally)
                    legalmoves[y, x] = 1; //MultiDirection function handles placing on same spot, so we declare it manually on pawns AND kings, comment on the right is bs (kings do not need it as they iterate over themselves)

                    if (y + direction < 8)
                    {
                        for (int i = (x > 0 ? (x - 1) : 0), count = 0; i < 8 && count < 3; i++, count++)
                        {
                            if (board[y + direction, i].pieceid != 0 && i != x && board[y + direction, i].piececolor != board[y, x].piececolor) //REPEATED COMMENT FOR EXTRA CLARITY: Also direction only changes its value when dealing with pawns, since those move on the y axis in a specific direction (up or down, hence when initializing the for loop in pawns we use -1 to ensure we check the left tile the center tile and right tile, and not skip a tile due to random addition/subtraction)
                                legalmoves[y + direction, i] = 1;

                            else if (i == x && board[y + direction, i].pieceid == 0)
                                legalmoves[y + direction, x] = 1;

                        }

                        if ((y == 1 || y == 6) && board[(y + direction), x].pieceid == 0 && board[(y + direction * 2), x].pieceid == 0)
                            legalmoves[y + (direction * 2), x] = 1;
                    }

                    break;

                case 5: //Queen
                case 4: //Bishop
                    for (int i = y, j = x; i < 8 && j < 8 && ValidMultiAxisMovement(i, j, y, x); i++, j++) ;
                    for (int i = y, j = x; i >= 0 && j >= 0 && ValidMultiAxisMovement(i, j, y, x); i--, j--) ;

                    for (int i = y, j = x; i < 8 && j >= 0 && ValidMultiAxisMovement(i, j, y, x); i++, j--) ;
                    for (int i = y, j = x; i >= 0 && j < 8 && ValidMultiAxisMovement(i, j, y, x); i--, j++) ;

                    //C# compliant
                    if (board[y, x].pieceid == 5) //If queen jump to next case as well
                        goto case 2;

                    break;

                    //C compliant
                    //if(board[y, x].pieceid == 4)
                    //    break;

                case 2: //Rook (Ok Rowley, now hit the second tower)
                    for (int i = y; i < 8 && ValidMultiAxisMovement(i, x, y, x); i++) ;
                    for (int i = y; i >= 0 && ValidMultiAxisMovement(i, x, y, x); i--) ;

                    for (int i = x; i < 8 && ValidMultiAxisMovement(y, i, y, x); i++) ;
                    for (int i = x; i >= 0 && ValidMultiAxisMovement(y, i, y, x); i--) ;

                    break;

                case 3: //Knight (Honse)
                    legalmoves[y, x] = 1; //Make original position valid

                    if ((y + 2) < 8 && (x + 1) < 8)
                        ValidMultiAxisMovement(y + 2, x + 1, y, x);

                    if ((y + 2) < 8 && x - 1 >= 0)
                        ValidMultiAxisMovement(y + 2, x - 1, y, x);

                    if ((y - 2) >= 0 && (x + 1) < 8)
                        ValidMultiAxisMovement(y - 2, x + 1, y, x);

                    if ((y - 2) >= 0 && (x - 1) >= 0)
                        ValidMultiAxisMovement(y - 2, x - 1, y, x);

                    break;
            }
        }

        private bool ValidMultiAxisMovement(int newy, int newx, int oldy, int oldx) //Added old coordinates parameters here so we don't need ternary expressions when initializing our for loop since we'll handle it, also makes placing in the same spot possible, pawn and king do it on their specific cases. Used to only pass new y x and piececolor
        {
            //Debug.WriteLine(newy + " " + newx + "\n" + oldy + " " + oldx);
            if (board[newy, newx].pieceid == 0 || (newy == oldy && newx == oldx))
            { legalmoves[newy, newx] = 1; return true; }

            else if (board[newy, newx].piececolor == board[oldy, oldx].piececolor)
                return false;

            else
            { legalmoves[newy, newx] = 1; return false; }
        }

        private void KingLegalMoves(int kingy, int kingx) //Make it so that when a king is selected we only check the opposite color and that we don't allow the king to move into position if position already taken
        {
            //Iterate over board here (check if tile is empty then || piece of same color, since theres 32 pieces (half the board) and the number of pieces goes down as game goes on so more empty spaces)
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (board[i, j].piececolor == 0 || board[i, j].piececolor == board[kingy, kingx].piececolor)
                        continue;

                    NonKingLegalMoves(i, j);
                }
            }

            //Iterate over king square (king can move 1 in any direction)
            int[,] kinglegalmoves = new int[8, 8]; bool canmove = false; //We store the king's moves in a separate matrix which we then copy onto our original matrix, we also have a var to determine if king can move or not, we assume he can't until told otherwise
            for (int i = (kingy > 0 ? (kingy - 1) : 0), ycount = (kingy == 0 || kingy == 7 ? 1 : 0); i < 8 && ycount < 3; i++, ycount++) //Added ternary expression on ycount, used to be an if within the first for scope, but it was pointless to check this every loop, so it is a ternay expression in the for initializer
            {
                for (int j = (kingx > 0 ? (kingx - 1) : 0), xcount = 0; j < 8 && xcount < 3; j++, xcount++)
                {
                    if (board[i, j].piececolor != board[kingy, kingx].piececolor && legalmoves[i, j] != 1) //Will not iterate over itself, moron
                    { kinglegalmoves[i, j] = 1; canmove = true; }

                    else if (board[i, j].piececolor != board[kingy, kingx].piececolor && board[i, j].pieceid != 0)
                    { kinglegalmoves[i, j] = 1; canmove = true; }
                }
            }

            if (legalmoves[kingy, kingx] == 0) //If no piece can go to king's current position then it is safe (we assume no one is in check or checkmate until told otherwise by subsequent ifs)
            { gamestate = 0;  kinglegalmoves[kingy, kingx] = 1; canmove = true; }

            else //King is in check
            { gamestate = board[kingy, kingx].piececolor; kinglegalmoves[kingy, kingx] = -1; }

            if (!canmove) //Check is in checkmate
                gamestate = board[kingy, kingx].piececolor + 2;

            legalmoves = kinglegalmoves; //Store kings moves

            return;
        }
    }
}

/*
 * MISSING RULES:
 * 
 * - CANNOT MOVE ANY PIECE OTHER THAN KING WHEN IN CHECK (RULES STATE THAT OTHER PIECES MAY BE MOVED TO BLOCK THE CHECKMATE OR CAPTURE THE CHECKING PIECE)
 * - PIECES CAN MOVE AND LEAVE THE KING VULNERABLE TO CHECK (OBVIOUSLY YOU CAN'T MAKE A MOVE THAT LEAVES YOUR OWN KING IN CHECK)
 *
 */

/*
private void LegalMoves(int y, int x)
{
    //int swain = 0; //The swain variable is a flag that swaps between addition and subtraction inside the bishop and rook for loops.
    switch (board[y, x].pieceid)
    {
        case 1: //Pawn
            int direction = (board[y, x].piececolor == 1 ? -1 : 1); //direction is variable for clarity (would use enums for pieceid too if I didn't have to do a fucking explicit cast), it's only relevant for pawns since they move in a specific direction, other pieces move in any direction but within specific rules (such as king only moving 1 tile or bishops only moving diagonally)
            legalmoves[y, x] = 1; //MultiDirection function handles placing on same spot, so we declare it manually on pawns and kings

            if (y + direction < 8)
            {
                for (int i = (x > 0 ? (x - 1) : 0), count = 0; i < 8 && count < 3; i++, count++)
                {
                    if (board[y + direction, i].pieceid != 0 && i != x && board[y + direction, i].piececolor != board[y, x].piececolor) //REPEATED COMMENT FOR EXTRA CLARITY: Also direction only changes its value when dealing with pawns, since those move on the y axis in a specific direction (up or down, hence when initializing the for loop in pawns we use -1 to ensure we check the left tile the center tile and right tile, and not skip a tile due to random addition/subtraction)
                        legalmoves[y + direction, i] = 1;

                    else if (i == x && board[y + direction, i].pieceid == 0)
                        legalmoves[y + direction, x] = 1;

                }

                if ((y == 1 || y == 6) && board[(y + direction), x].pieceid == 0 && board[(y + direction * 2), x].pieceid == 0)
                    legalmoves[y + (direction * 2), x] = 1;
            }

            break;

        case 6: //King
            legalmoves[y, x] = 1;

            for (int i = (y > 0 ? y - 1 : 0), ycount = (i >= 0 ? 0 : 1); i < 8 && ycount < 3; i++, ycount++)
            {
                for (int j = (x > 0 ? x - 1 : 0), xcount = 0; j < 8 && xcount < 3; j++, xcount++)
                {
                    //if (board[i, j].pieceid == 0 || board[i, j].piececolor != board[y, x].piececolor)
                    if (board[i, j].piececolor != board[y, x].piececolor)
                        legalmoves[i, j] = 1;
                }
            }

            break;

        case 5: //Queen
        case 4: //Bishop (Kinda gross, needs to be cleaned up) (unfinished bc tired and knight will use similar logic, so I don't wanna reuse shit)
            for (int i = (y < 7 ? (y + 1) : y), j = (x < 7 ? (x + 1) : x); swain < 2; i += (swain < 1 ? 1 : -1), j += (swain < 1 ? 1 : -1))
            {
                if (i < 0 || j < 0 || j > 7 || i > 7 || board[i, x].piececolor == board[y, x].piececolor)
                { swain++; i = (y > 0 ? y : 1); j = (x > 0 ? x : 1); }

                else if (board[i, j].pieceid == 0)
                    legalmoves[i, j] = 1;

                //else if (board[i, x].piececolor != board[y, x].piececolor)
                else
                { legalmoves[i, j] = 1; swain++; i = (y > 0 ? y : 1); j = (x > 0 ? x : 1); }
            }

            if (board[y, x].pieceid == 5)
            { swain = 0; goto case 2; }

            break;

            //If either y or x can't be added or subtracted we return values over the condition limit to terminate, as we need to 1 down and right simultaneously, not individually like in the rook
            for (int i = (y < 7 ? (y + 1) : 8), j = (x < 7 ? (x + 1) : 8); i < 8 && j < 8 && ValidMultiAxisMovement(i, j, board[y, x].piececolor); i += 1, j += 1) ;
            for (int i = (y > 0 ? (y - 1) : -1), j = (x > 0 ? (x - 1) : -1); i >= 0 && j >= 0 && ValidMultiAxisMovement(i, j, board[y, x].piececolor); i -= 1, j -= 1) ;

            for (int i = (y < 7 ? (y + 1) : 8), j = (x > 0 ? (x - 1) : -1); i < 8 && j >= 0 && ValidMultiAxisMovement(i, j, board[y, x].piececolor); i += 1, j -= 1) ;
            for (int i = (y > 0 ? (y - 1) : -1), j = (x < 7 ? (x + 1) : 8); i >= 0 && j < 8 && ValidMultiAxisMovement(i, j, board[y, x].piececolor); i -= 1, j += 1) ;

            for (int i = y, j = x; i < 8 && j < 8 && ValidMultiAxisMovement(i, j, y, x); i++, j++) ;
            for (int i = y, j = x; i >= 0 && j >= 0 && ValidMultiAxisMovement(i, j, y, x); i--, j--) ;

            for (int i = y, j = x; i < 8 && j >= 0 && ValidMultiAxisMovement(i, j, y, x); i++, j--) ;
            for (int i = y, j = x; i >= 0 && j < 8 && ValidMultiAxisMovement(i, j, y, x); i--, j++) ;

            if (board[y, x].pieceid == 5)
                goto case 2;

            break;

        case 2: //Rook (Ok Rowley, now hit the second tower)
            for (int i = (y < 7 ? (y + 1) : y); i < 8; i++)
            {
                if (board[i, x].pieceid == 0)
                    legalmoves[i, x] = 1;

                else if (board[i, x].piececolor == board[y, x].piececolor)
                    break;

                else
                { legalmoves[i, x] = 1; break; }
            }

            for (int i = (y > 0 ? (y - 1) : y); i > 0; i--)
            {
                if (board[i, x].pieceid == 0)
                    legalmoves[i, x] = 1;

                else if (board[i, x].piececolor == board[y, x].piececolor)
                    break;

                else
                { legalmoves[i, x] = 1; break; }
            }

            for (int i = (x < 7 ? (x + 1) : x); i < 8; i++)
            {
                if (board[y, i].pieceid == 0)
                    legalmoves[i, x] = 1;

                else if (board[y, i].piececolor == board[y, x].piececolor)
                    break;

                else
                { legalmoves[y, i] = 1; break; }
            }

            for (int i = (x > 0 ? (x - 1) : x); i > 0; i--)
            {
                if (board[y, i].pieceid == 0)
                    legalmoves[y, i] = 1;

                else if (board[y, i].piececolor == board[y, x].piececolor)
                    break;

                else
                { legalmoves[y, i] = 1; break; }
            }

            break;

    for (int i = (y < 7 ? (y + 1) : y); swain < 2; i += (swain < 1 ? 1 : -1))
            {
                if (i < 0 || i > 7 || board[i, x].piececolor == board[y, x].piececolor)
                { swain++; i = (y > 0 ? y : 1); }

                else if (board[i, x].pieceid == 0)
                    legalmoves[i, x] = 1;

                //else if (board[i, x].piececolor != board[y, x].piececolor)
                else
                { legalmoves[i, x] = 1; swain++; i = (y > 0 ? y : 1); }
            }

            swain = 0;
            for (int i = (x < 7 ? (x + 1) : x); swain < 2; i += (swain < 1 ? 1 : -1))
            {
                if (i < 0 || i > 7 || board[y, i].piececolor == board[y, x].piececolor)
                { swain++; i = (x > 0 ? x : 1); }

                else if (board[y, i].pieceid == 0)
                    legalmoves[y, i] = 1;

                //else if (board[y, i].piececolor != board[y, x].piececolor)
                else
                { legalmoves[y, i] = 1; swain++; i = (x > 0 ? x : 1); }
            }

            break;

            for (int i = (y < 7 ? (y + 1) : y); i < 8 && ValidMultiAxisMovement(i, x, board[y, x].piececolor); i++) ;
            for (int i = (y > 0 ? (y - 1) : y); i >= 0 && ValidMultiAxisMovement(i, x, board[y, x].piececolor); i--) ;

            for (int i = (x < 7 ? (x + 1) : x); i < 8 && ValidMultiAxisMovement(y, i, board[y, x].piececolor); i++) ;
            for (int i = (x > 0 ? (x - 1) : x); i >= 0 && ValidMultiAxisMovement(y, i, board[y, x].piececolor); i--) ;

            for (int i = y; i < 8 && ValidMultiAxisMovement(i, x, y, x); i++) ;
            for (int i = y; i >= 0 && ValidMultiAxisMovement(i, x, y, x); i--) ;

            for (int i = x; i < 8 && ValidMultiAxisMovement(i, x, y, x); i++) ;
            for (int i = x; i >= 0 && ValidMultiAxisMovement(i, x, y, x); i--) ;

            break;

        case 3: //Knight (Honse)
            if (y + 2 < 8 && x + 1 < 8)
                ValidMultiAxisMovement(y + 2, x + 1, y, x);

            if (y + 2 < 8 && x - 1 >= 0)
                ValidMultiAxisMovement(y + 2, x + 1, y, x);

            if (y - 2 >= 0 && x + 1 < 8)
                ValidMultiAxisMovement(y + 2, x + 1, y, x);

            if (y - 2 >= 0 && x - 1 >= 0)
                ValidMultiAxisMovement(y + 2, x + 1, y, x);

            break;
    }
}
*/