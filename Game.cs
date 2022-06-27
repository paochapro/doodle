using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using MonoGame.Extended;

namespace Doodle;

using static Utils;

//////////////////////////////// Starting point
class MyGame : Game
{
    //More important stuff
    public static readonly Point screenSize = new(600, 800);
    const string gameName = "DoodleJump";
    const float defaultVolume = 0.3f;

    //Stuff
    static public GraphicsDeviceManager Graphics => graphics;
    static GraphicsDeviceManager graphics;
    SpriteBatch spriteBatch;
    public static MouseState mouse { get => Mouse.GetState(); }
    public static KeyboardState keys { get => Keyboard.GetState(); }
    static public Vector2 Camera;
    static public bool Debug { get; private set; } = false;
    static public bool God { get; private set; } = false;

    //Game
    static Player player;
    public static Player Player => player;

    const int startGenerateCount = 30;
    public static float DeathPit { get; private set; }

    public const int minDifficulty = 1;
    public const int maxDifficulty = 10;
    private static int Diffuculty { get; set; }

    public static float generateHeight;
    public static float diffucultyHeight;
    public const float startDiffucultyHeight = 2000;

    public const float difficultyMultiplier = 1.8f;

    static float highestY;
    static int score;

    DebugLine generateHeightLine;
    DebugLine deathpit;

    bool pressingE = false;
    bool pressingG = false;

    //Ui
    static readonly Rectangle upperBox = new(0, 0, MyGame.screenSize.X, percent(screenSize.Y, 7));
    static readonly Color upperBoxColor = new Color(0, 0, 0, 0.1f);
    static readonly Vector2 scorePosition = new Vector2(percent(screenSize.X, 3), percent(screenSize.Y, 2));

    //Generate stuff
    static private void Generate()
    {
        Platforms.GeneratedPlatformData data = Platforms.Generate();
        SpawnGroundItem(data.x, data.y, data.platformType); //Bonuses and springs
        SpawnObstacle(data.previousY, data.y); //Enemies and trap platforms
        generateHeight += -data.distance;
    }
    static private void SpawnGroundItem(int x, int y, Platforms.PlatformType platformType)
    {
        //0-nothing, 1-spring, 2-bonus
        int chance = Chance(100 - Springs.springChance - Bonuses.bonusChance, Springs.springChance, Bonuses.bonusChance);

        if (failedBonusSpawn)
        {
            chance = 2;
            failedBonusSpawn = false;
        }

        if (platformType != Platforms.PlatformType.Simple)
        {
            if (chance == 2)
            {
                failedBonusSpawn = true;
                print("failed bonus spawn");
            }

            return;
        }

        if (chance == 1) Springs.SpawnSpring(x, y);
        if (chance == 2) Bonuses.SpawnBonus(x, y);

    }
    static public void SpawnObstacle(int previousY, int currentY)
    {
        //0-nothing, 1-trap platform, 2-enemy
        int chance = Chance(100 - Enemies.trapChance - Enemies.enemyChance, Enemies.trapChance, Enemies.enemyChance);

        //If there was not enough distance between platforms for enemy to spawn
        //Try to spawn it again next time until its done
        if (failedEnemySpawn)
        {
            chance = 2;
            failedEnemySpawn = false;
        }

        //If nothing, dont spawn anything
        if (chance == 0) return;

        //If bonus is activated, dont spawn the enemies
        if (bonusActivated)
        {
            chance = 1;
        }

        //Not enough space?
        int obstacleHeight = chance == 1 ? Platform.height : Enemy.size.Y;
        int distanceBetween = previousY - currentY - Platform.height;

        if (distanceBetween < Platforms.minPossibleDistance + obstacleHeight)
        {
            if (chance == 2)
            {
                failedEnemySpawn = true;
                print("failed enemy spawn!");
            }

            return;
        }

        //Enemy
        if (chance == 2)
        {
            print("spawned enemy");
            Enemies.SpawnEnemy(previousY, currentY);
        }
        //Trap platform
        if (chance == 1) Platforms.AddTrap(previousY, currentY);
    }

    static bool failedEnemySpawn = false;
    static bool failedBonusSpawn = false;
    public static bool bonusActivated = true;

    static public void Reset()
    {
        diffucultyHeight = startDiffucultyHeight * minDifficulty;
        Diffuculty = minDifficulty;
        highestY = 0;
        score = 0;
        generateHeight = 0;
        Camera.Y = 0;
        DeathPit = 999;

        failedEnemySpawn = false;
        failedBonusSpawn = false;
        bonusActivated = false;

        Platforms.Reset();
        Enemies.Reset();
        Bonuses.Reset();
        Springs.Clear();
        DebugLines.Clear();
        Event.ClearEvents();


        for (int i = 0; i < startGenerateCount; ++i)
            Generate();

        generateHeight /= 2;
    }

    protected override void Initialize()
    {
        Window.AllowUserResizing = false;
        Window.Title = gameName;
        IsMouseVisible = true;
        graphics.PreferredBackBufferWidth = screenSize.X;
        graphics.PreferredBackBufferHeight = screenSize.Y;
        graphics.ApplyChanges();

        SoundEffect.MasterVolume = defaultVolume;

        generateHeightLine = new DebugLine(new(0, 0), new(screenSize.X, 0));
        deathpit = new DebugLine(new(0, 0), new(screenSize.X, 0));

        base.Initialize();
    }

    private void DifficultyChange()
    {
        Bonuses.DifficultyChange(Diffuculty);
        Enemies.DifficultyChange(Diffuculty);
    }

    //Main
    protected override void Update(GameTime gameTime)
    {
        //Exit
        if (keys.IsKeyDown(Keys.Escape)) Exit();

        Controls();

        Ui.UpdateElements(mouse);
        Event.ExecuteEvents(gameTime);
        Entities.Update(gameTime);

        if (player.Rect.Y < highestY)
            highestY = player.Rect.Y;

        generateHeightLine.p1.Y = generateHeight;
        generateHeightLine.p2.Y = generateHeight;

        Camera.Y = highestY - center(screenSize.Y, player.Rect.Height) + percent(screenSize.Y, 5);
        DeathPit = Camera.Y + screenSize.Y;

        deathpit.p1.Y = DeathPit;
        deathpit.p2.Y = DeathPit;

        score = -(int)Math.Round(highestY / 10);

        if (-player.Rect.Y >= diffucultyHeight)
        {
            diffucultyHeight *= difficultyMultiplier;

            Platforms.DifficultyChange(Diffuculty);

            Diffuculty += 1;
            Diffuculty = clamp(Diffuculty, minDifficulty, maxDifficulty);

            print("Difficulty: " + Diffuculty);

            //new line

            DifficultyChange();
        }

        if (player.Rect.Y < generateHeight)
            Generate();

        base.Update(gameTime);
    }

    private void Controls()
    {
        //if (keys.IsKeyDown(Keys.H)) GeneratePlatforms();
        if (keys.IsKeyDown(Keys.E) && !pressingE) Debug = !Debug;
        if (keys.IsKeyDown(Keys.G) && !pressingG) God = !God;
        pressingE = keys.IsKeyDown(Keys.E);
        pressingG = keys.IsKeyDown(Keys.G);
    }

    private void DrawUi()
    {
        spriteBatch.FillRectangle(upperBox, upperBoxColor);
        spriteBatch.DrawString(Ui.Font, "Score: " + score.ToString(), scorePosition, Color.Black);

        if(God)
        {
            Vector2 measure = Ui.Font.MeasureString("Godmode ON");
            Vector2 position = new Vector2(screenSize.X - measure.X - percent(screenSize.X, 5), scorePosition.Y);
            spriteBatch.DrawString(Ui.Font, "Godmode ON", position, Color.Red);
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        graphics.GraphicsDevice.Clear(Color.LightYellow);

        spriteBatch.Begin();
        {
            Platforms.Draw(spriteBatch);
            Bonuses.Draw(spriteBatch);
            Springs.Draw(spriteBatch);
            DebugLines.Draw(spriteBatch);
            player.Draw(spriteBatch);
            Enemies.Draw(spriteBatch);

            DrawUi();

            Ui.DrawElements(spriteBatch);

            generateHeightLine.Draw(spriteBatch);
            deathpit.Draw(spriteBatch);
        }
        spriteBatch.End();

        base.Draw(gameTime);
    }
    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        MonoGame.Content = Content;
        Ui.Font = Content.Load<SpriteFont>("bahnschrift");

        player = new Player();

        Reset();
        Entities.Add(player);
    }
    private void CreateUi()
    {
    }
    //Setups

    public MyGame() : base()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
    }
}

class Program
{
    public static void Main()
    {
        using (MyGame game = new MyGame())
            game.Run();
    }
}