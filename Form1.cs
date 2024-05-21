using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;

namespace StreetFightClone
{
    public partial class Form1 : Form
    {
        Image player;
        Image background;
        Image drum;
        Image fireball;

        int playerX = 0;
        int playerY = 300;

        int drumX = 360;
        int drumY = 340;

        int fireballX;
        int fireballY;

        int drumMoveTime = 0;
        int actionStrength = 0;
        int endFrame = 0;
        int backgroundPosition = 0;
        int totalFrame = 0;
        int bg_number = 0;

        float num;

        bool goleft, goright;
        bool directionPressed;
        bool playerAction;
        bool shotFireBall;

        List<string> backgound_image = new List<string>();
        public Form1()
        {
            InitializeComponent();
            SetUpForm();
        }

        private void GameTimerEvent(object sender, EventArgs e)
        {
            this.Invalidate();
            ImageAnimator.UpdateFrames(); 
            MovePlayerBackGround();
            CheckPunchHit();

            if (playerAction)
            {
                if(num < totalFrame)
                {
                    num += 0.5f;
                }
            }
            if(num == totalFrame)
            {
                ResetPlayer();
            }
            //fire ball instructor
            if (shotFireBall)
            {
                fireballX += 10;
                CheckHadokenHit();
            }
            if (fireballX > this.ClientSize.Width)
            {
                shotFireBall = false;
            }
            if(!shotFireBall && num > endFrame && drumMoveTime == 0 && actionStrength == 30)
            {
                ShootHadoken();
            }
            
            if(drumMoveTime > 0)
            {
                drumMoveTime--;
                drumX += 10;
                drum = Image.FromFile("hitdrum.png");
            }
            else
            {
                drum = Image.FromFile("drum.png");
                drumMoveTime = 0;
            }
            //reset drum when the drum too far away from the player
            if(drumX > this.ClientSize.Width)
            {
                drumMoveTime = 0;
                drumX = 300;
            }
        }

        private void KeyIsDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Left && !directionPressed)
            {
                MovePlayerAnimation("left");
            }
            if(e.KeyCode == Keys.Right && !directionPressed)
            {
                MovePlayerAnimation("right");
            }
        }

        private void KeyIsUp(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.X)
            {
                if(bg_number < backgound_image.Count - 1)
                {
                    bg_number++;
                }
                else
                {
                    bg_number = 0;
                }
                background = Image.FromFile(backgound_image[bg_number]);
                backgroundPosition = 0;
                drumMoveTime = 0;
                drumX = 350;
            }
            if(e.KeyCode == Keys.Right || e.KeyCode == Keys.Left)
            {
                goleft = false;
                goright = false;
                directionPressed = false;
                ResetPlayer();
            }
            if(e.KeyCode == Keys.A && !playerAction && !goleft && !goright)
            {
                SetPlayerAction("punch2.gif", 2);
            }
            if(e.KeyCode == Keys.S && !playerAction && !goleft && !goright)
            {
                SetPlayerAction("punch1.gif", 5);
            }
            if(e.KeyCode == Keys.D && !playerAction && !goleft && !goright)
            {
                SetPlayerAction("fireball.gif", 30);
            }
        }

        private void PaintFormEvent(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(background, new Point(backgroundPosition, 0));
            e.Graphics.DrawImage(player, new Point(playerX, playerY));
            e.Graphics.DrawImage(drum, new Point(drumX, drumY));

            if (shotFireBall)
            {
                e.Graphics.DrawImage(fireball, new Point(fireballX, fireballY));
            }
        }
        private void SetUpForm()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            backgound_image = Directory.GetFiles("background", "*.jpg").ToList();
            background = Image.FromFile("background.jpg");
            player = Image.FromFile("standing.gif");
            drum = Image.FromFile("drum.png");
            SetUpAnimation();
        }
        private void SetUpAnimation()
        {
            ImageAnimator.Animate(player, this.OnFrameChangedHandler);
            FrameDimension dimentions = new FrameDimension(player.FrameDimensionsList[0]);
            totalFrame = player.GetFrameCount(dimentions);
            endFrame = totalFrame - 3;
        }

        private void OnFrameChangedHandler(object sender, EventArgs e)
        {
            this.Invalidate();  
        }

        private void MovePlayerBackGround()
        {
            if (goleft)
            {
                if (playerX > 0)
                {
                    playerX -= 6;
                }
                if(backgroundPosition < 0 && playerX < 100)
                {
                    backgroundPosition += 5;
                    drumX += 5;
                }
            }
            if (goright)
            {
                if(playerX + player.Width < this.ClientSize.Width)
                {
                    playerX += 6;
                }
                if (backgroundPosition + background.Width > this.ClientSize.Width + 5 && playerX > 650)
                {
                    backgroundPosition -= 5;
                    drumX -= 5;
                }
            }
        }
        private void MovePlayerAnimation(string direction)
        {
            if(direction == "left")
            {
                goleft = true;
                player = Image.FromFile("backwards.gif");
            }
            if(direction == "right")
            {
                goright = true;
                player = Image.FromFile("forwards.gif");
            }
            directionPressed = true;
            playerAction = false;
            SetUpAnimation();
        }
        private void ResetPlayer()
        {
            player = Image.FromFile("standing.gif");
            SetUpAnimation();
            num = 0;
            playerAction = false;
        }
        private void SetPlayerAction(string animation, int strength)
        {
            player = Image.FromFile(animation);
            actionStrength = strength;
            SetUpAnimation();
            playerAction = true;
        }
        private void ShootHadoken()
        {
            fireball = Image.FromFile("FireBallFinal.gif");
            ImageAnimator.Animate(fireball,this.OnFrameChangedHandler);
            // 2 bieu thuc la de Hadoken xuat hien tren nguoi cua Ryu, explain later
            fireballX = playerX + player.Width - 50 ; 
            fireballY = playerY - 33;
            shotFireBall = true;
        }
        private void CheckHadokenHit()
        {
            bool collision = DetectCollision(fireballX, fireballY, fireball.Width, fireball.Height, drumX, drumY, drum.Width, drum.Height);
            if (collision)
            {
                drumMoveTime = actionStrength;
                shotFireBall = false; //khi Hadoken ban trung drum thi Hadoken bien mat
            }
        }
        private void CheckPunchHit()
        {
            bool collision = DetectCollision(playerX, playerY, player.Width, player.Height, drumX, drumY, drum.Width, drum.Height);
            if(collision && playerAction && num > endFrame)
            {
                drumMoveTime = actionStrength;
            }
        }
        private bool DetectCollision(int object1X, int object1Y, int object1Height, int object1Width, int object2X, int object2Y, int object2Height, int object2Width)
        {
            if(object1X + object1Width <= object2X || object1X >= object2Width + object2X || object1Y + object1Height <= object2Y || object1Y >= object2Height + object2Y)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}

// Later guys, peace