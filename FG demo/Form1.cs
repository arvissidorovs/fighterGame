
using System.Drawing.Imaging;
using System.Reflection.Metadata.Ecma335;

namespace FG_demo
{
    public partial class Form1 : Form
    {
        Image player;
        Image background;
        Image cake;
        Image gun;

        int playerX = 0;
        int playerY = 250;
        int cakeX = 450;
        int cakeY = 250;

        int gunX;
        int gunY;

        int cakeMoveTime = 0;
        int actionStrenght = 0;
        int endFrame = 0;
        int backgroundPosition = 0;
        int totalFrame = 0;
        int bg_number = 0;

        float num = 0;

        bool goLeft, goRight;
        bool directionPressed;
        bool playingAction;
        bool shotGun;

        int cakeHealth = 7;

        List<string> background_images = new List<string>();
        public Form1()
        {
            InitializeComponent();
            SetUpForm();
        }

        private void KeyIsDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left && !directionPressed)
            {
                MovePlayerAnimation("left");
            }
            if (e.KeyCode == Keys.Right && !directionPressed)
            {
                MovePlayerAnimation("right");
            }
        }

        private void KeyIsUp(object sender, KeyEventArgs e)
        {

            if(e.KeyCode == Keys.Right || e.KeyCode == Keys.Left) {
                goLeft = false;
                goRight = false;
                directionPressed = false;
                ResetPlayer();
            }
            if (e.KeyCode== Keys.A && !playingAction && !goLeft && !goRight)
            {
               SetPlayerAction("punch.gif", 2);
            }
            if (e.KeyCode == Keys.D && !playingAction && !goLeft && !goRight && !shotGun)
            {
                SetPlayerAction("gun.gif", 6);
            }
        }

        private void FormPaintEvent(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(background, new Point(backgroundPosition, 0));
            e.Graphics.DrawImage(cake, new Point(cakeX, cakeY));
            e.Graphics.DrawImage(player, new Point(playerX, playerY));

            if(shotGun)
            {
                e.Graphics.DrawImage(gun, new Point(gunX,gunY));
            }
        }

        private void GameTimeEvent(object sender, EventArgs e)
        {
            this.Invalidate();
            ImageAnimator.UpdateFrames();
            MovePlayerAndBackground();
            CheckPunchHit();

            if (playingAction)
            {
                if (num < totalFrame)
                {
                    num += 0.5f; //50:20
                }
            }
            if (num==totalFrame)
            {
                ResetPlayer();
            }
            //gun bullet instructions for time
            if(shotGun)
            {
                gunX += 20;//speed of bullet
                CheckGunHit();
            }

            if(gunX > this.ClientSize.Width)
            {
                shotGun = false;
            }
            if (!shotGun && num > endFrame & cakeMoveTime==0 && actionStrenght==6)
            {
                ShootGun();
            }

            if (cakeMoveTime > 0)
            {
                cakeMoveTime--;
                cakeX += 10;
                cake = Image.FromFile("cake2.png");

            }
            else if (cakeHealth <= 0)
            {
                cake = Image.FromFile("cake2.png");

            }
            else
            {
                cake = Image.FromFile("cake.png");
                cakeMoveTime = 0;
            }

        }
        private void SetUpForm()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint, true);

            background_images = Directory.GetFiles("background", "*.jpg").ToList();
            background = Image.FromFile(background_images[bg_number]);
            player = Image.FromFile("walk.gif");
            cake = Image.FromFile("cake.png");
            SetUpAnimation();
        }
        private void SetUpAnimation()
        {
            ImageAnimator.Animate(player, this.OnFrameChangedHandler);
            FrameDimension dimentions = new FrameDimension(player.FrameDimensionsList[0]);
            totalFrame = player.GetFrameCount(dimentions);
            endFrame = totalFrame - 3;//23:00
        }

        private void OnFrameChangedHandler(object? sender, EventArgs e)
        {
            this.Invalidate();
        }

        private void MovePlayerAndBackground()
        {
            if (goLeft)
            {
                if (playerX > 0)
                {
                    playerX -= 8;//35:20 speed
                }

                if (backgroundPosition < 0 && playerX < 100)
                {
                    backgroundPosition += 6;
                    cakeX += 6;//eh
                }
            }
            if(goRight)
            { 
                if (playerX + player.Width < this.ClientSize.Width/2) //borderrs
                {
                    playerX += 6; 
                }
                if (backgroundPosition +background.Width > this.ClientSize.Width +6 && playerX > 60) ///moving boarders
                {
                    backgroundPosition -= 6;
                    cakeX -= 6;
                }
            }
        }
        private void MovePlayerAnimation(string direction)
        {
            if (direction == "left")
            {
                goLeft = true;
                player = Image.FromFile("walk.gif");//chang forward
            }
            if (direction == "right")
            {
                goRight = true;
                player = Image.FromFile("walk.gif");//chang backw
            }

            directionPressed = true;
            playingAction = false;
            SetUpAnimation();
        }
        private void ResetPlayer()
        {
            player = Image.FromFile("walk.gif"); //standing
            SetUpAnimation() ;
            num = 0;
            playingAction = false;
        }
        private void SetPlayerAction(string animation, int strenght)
        {
            player = Image.FromFile(animation);
            actionStrenght= strenght;
            SetUpAnimation();
            playingAction= true;
        }
        private void ShootGun()
        {
            gun = Image.FromFile("bullet.gif");
            ImageAnimator.Animate(gun, this.OnFrameChangedHandler);
            gunX = playerX + player.Width - 50; //arm lenght
            gunY = playerY + 15 ;/////bul Y
            shotGun = true;

        }
        private void CheckPunchHit()
        {
            bool collision = DetectCollision(playerX, playerY, player.Width, player.Height,
                cakeX, cakeY, cake.Width, cake.Height);

            if (collision && playingAction && num > endFrame)
            {
                cakeMoveTime = actionStrenght;
            }
        }
        private void CheckGunHit()
        {
            if (cakeHealth > 0)
            {
                bool collision = DetectCollision(gunX, gunY, gun.Width, gun.Height,
                    cakeX, cakeY, cake.Width, cake.Height);

                if (collision)
                {
                    cakeHealth -= actionStrenght;
                    if (cakeHealth <= 0)
                    {
                        cakeHealth = 0;
                    }
                    cakeMoveTime = actionStrenght;
                    shotGun = false;
                }
            }
        }
        private bool DetectCollision(int object1X, int object1Y, int object1Width, int object1Height, //obj2 = cake
            int object2X, int object2Y, int object2Width, int object2Height)
        {
            if (object1X + object1Width <= object2X || object1X >= object2X + object2Width ||
              object1Y + object1Height <= object2Y || object1Y >= object2Y + object2Height)
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