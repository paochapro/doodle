using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

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
    static public bool Debug { get; private set; } = true;

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

    static private void GeneratePlatform()
    {
        int dist = Platforms.Generate();
        generateHeight += dist;
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

            DifficultyChange();
        }

        if (player.Rect.Y < generateHeight)
            GeneratePlatform();

        base.Update(gameTime);
    }

    private void Controls()
    {
        //if (keys.IsKeyDown(Keys.H)) GeneratePlatforms();
        if (keys.IsKeyDown(Keys.E) && !pressingE) Debug = !Debug;
        pressingE = keys.IsKeyDown(Keys.E);
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
        Rectangle rectButton = new Rectangle(15,15, 100, 30);
        Ui.Add(new Button(rectButton, ()=>Console.WriteLine("hi"), "Hello", 0));
    }
    //Setups
    static public void Reset()
    {
        diffucultyHeight = startDiffucultyHeight * minDifficulty;
        Diffuculty = minDifficulty;
        highestY = 0;
        score = 0;
        generateHeight = 0;
        Camera.Y = 0;
        DeathPit = 999;

        Platforms.Reset();
        Enemies.Reset();
        Bonuses.Reset();
        Springs.Clear();
        DebugLines.Clear();
        Event.ClearEvents();

        //Platforms.Add(new SpinningPlatform( new Vector2(screenSize.X/2-Platform.width/2, 0)));

        for (int i = 0; i < startGenerateCount; ++i)
            GeneratePlatform();

        generateHeight /= 2;
    }
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