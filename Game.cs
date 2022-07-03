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
    public static readonly Point screenSize = new(600, 800); //600,800
    const string gameName = "DoodleJump";
    const float defaultVolume = 0.3f;

    //General stuff
    static public GraphicsDeviceManager Graphics => graphics;
    static GraphicsDeviceManager graphics;
    static SpriteBatch spriteBatch;
    public static MouseState mouse { get => Mouse.GetState(); }
    public static KeyboardState keys { get => Keyboard.GetState(); }

    static private Vector2 camera;
    static public Vector2 Camera => camera;

    static public bool Debug { get; private set; } = false;
    static public bool God { get; private set; } = false;

    public enum GameState { Menu, Game, Death }

    private static GameState gameStateVariable; //never use this
    public static GameState gameState
    {
        get => gameStateVariable;
        set
        {
            gameStateVariable = value;
            UI.CurrentLayer = Convert.ToInt32(gameState);
        }
    }

    static readonly Dictionary<GameState, Action> drawMethods = new()
    {
        [GameState.Menu] = DrawMenu,
        [GameState.Game] = DrawGame,
        [GameState.Death] = DrawDeathUI
    };

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
    static int deathCount = 0;

    //Scores
    static int score;
    static int highscore = 0;
    const int maxTopScores = 10;
    static int[] scores = new int[maxTopScores];

    const string statsPath = "Content/stats.txt";

    static Texture2D background;
    static int bgStartY;
    static int bgEndY;

    //Debug
    static DebugLine generateHeightLine;
    static DebugLine deathpit;
    static bool pressingE = false;
    static bool pressingG = false;

    //Ui
    static readonly Rectangle upperBox = new(0, 0, screenSize.X, percent(screenSize.Y, 7));
    static readonly Color upperBoxColor = new Color(0, 0, 0, 0.1f);
    static readonly Vector2 scorePosition = new Vector2(percent(upperBox.Width, 3), percent(upperBox.Height, 25));

    static bool failedEnemySpawn = false;
    static bool failedBonusSpawn = false;
    public static bool bonusActivated = true;

    static int test = 0;

    //Generating
    static private void Generate()
    {
        Platforms.GeneratedPlatformData data = Platforms.Generate();
        SpawnGroundItem(data.x, data.y, data.platformType); //Bonuses and springs
        SpawnObstacle(data.previousY, data.y, data.previousX); //Enemies and trap platforms
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
    static public void SpawnObstacle(int previousY, int currentY, int x)
    {
        //0-nothing, 1-trap platform, 2-enemy
        int chance = Chance(100 - Enemies.trapChance - Enemies.enemyChance, Enemies.trapChance, Enemies.enemyChance);

        //If there was not enough distance between platforms for enemy to spawn
        //Try to spawn it again next time until its done
        if (failedEnemySpawn) {
            chance = 2;
            failedEnemySpawn = false;
        }

        //If nothing, dont spawn anything
        if (chance == 0) return;

        //If bonus is activated, dont spawn the enemies
        if (bonusActivated) chance = 1;

        //Not enough space?
        int minDistance = 0;
        int obstacleHeight = 0;

        if(chance == 1) {
            minDistance = Platforms.minPossibleDistance;
            obstacleHeight = Platform.height;
        }
        else {
            minDistance = Enemies.minPossibleDistance;
            obstacleHeight = Enemy.height;
        }

        int distanceBetween = previousY - currentY - obstacleHeight;
        if (distanceBetween < minDistance + obstacleHeight)
        {
            if (chance == 2) {
                failedEnemySpawn = true;
                print("failed enemy spawn!");
            }
            return;
        }

        //Enemy
        if (chance == 2)
        {
            print("spawned enemy");
            Enemies.SpawnEnemy(previousY, currentY, x);
        }
        //Trap platform
        if (chance == 1) Platforms.AddTrap(previousY, currentY);
    }
    static private void DifficultyChange()
    {
        Bonuses.DifficultyChange(Diffuculty);
        Enemies.DifficultyChange(Diffuculty);
        Platforms.DifficultyChange(Diffuculty);
    }

    //Initialization
    static private void Reset()
    {
        diffucultyHeight = startDiffucultyHeight * minDifficulty;
        Diffuculty = minDifficulty;
        highestY = 0;
        score = 0;
        generateHeight = 0;
        DeathPit = 999;
        ResetBackground();

        failedEnemySpawn = false;
        failedBonusSpawn = false;
        bonusActivated = false;
        God = false;

        Platforms.Reset();
        Enemies.Reset();
        Bonuses.Reset();
        Springs.Clear();
        DebugLines.Clear();
        Scorelines.Clear();
        Event.ClearEvents();
    }

    static private void ResetBackground()
    {
        camera = Vector2.Zero;
        bgStartY = (int)Camera.Y;
        bgEndY = ((int)Camera.Y + screenSize.Y) * 2;
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        MonoGame.Content = Content;
        UI.Font = Content.Load<SpriteFont>("bahnschrift");
        background = MonoGame.LoadTexture("background");

        CreateUi();
        player = new Player();
        Entities.Add(player);

        Reset();

        if (!File.Exists(statsPath))
        {
            print("file created");
            ResetGlobalScores();
        }

        scores = File.ReadAllLines(statsPath)
                        .Select(score => int.Parse(score))
                        .ToArray();

        highscore = scores[^1];
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

    //Main
    protected override void Update(GameTime gameTime)
    {
        //Exit
        if (keys.IsKeyDown(Keys.Escape)) Exit();

        Controls();

        UI.UpdateElements(mouse);
        Event.ExecuteEvents(gameTime);

        if(gameState == GameState.Game)
            Entities.Update(gameTime);

        if (gameState != GameState.Game)
        {
            base.Update(gameTime);
            return;
        }

        if (player.Rect.Y < highestY)
            highestY = player.Rect.Y;

        generateHeightLine.p1.Y = generateHeight;
        generateHeightLine.p2.Y = generateHeight;

        camera.Y = highestY - center(screenSize.Y, player.Rect.Height) + percent(screenSize.Y, 5);
        DeathPit = Camera.Y + screenSize.Y;

        deathpit.p1.Y = DeathPit;
        deathpit.p2.Y = DeathPit;

        score = -(int)Math.Round(highestY / 10);

        if (Camera.Y < bgStartY)
        {
            bgStartY -= screenSize.Y;
            bgEndY -= screenSize.Y;
        }

        if (-player.Rect.Y >= diffucultyHeight)
        {
            diffucultyHeight *= difficultyMultiplier;

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

    static private void Controls()
    {
        //if (keys.IsKeyDown(Keys.H)) GeneratePlatforms();
        if (keys.IsKeyDown(Keys.E) && !pressingE) Debug = !Debug;
        if (keys.IsKeyDown(Keys.G) && !pressingG) God = !God;
        pressingE = keys.IsKeyDown(Keys.E);
        pressingG = keys.IsKeyDown(Keys.G);
    }

    //Draw
    static private void DrawGame()
    {
        Platforms.Draw(spriteBatch);
        Scorelines.Draw(spriteBatch);
        Bonuses.Draw(spriteBatch);
        Springs.Draw(spriteBatch);
        DebugLines.Draw(spriteBatch);
        player.Draw(spriteBatch);
        Enemies.Draw(spriteBatch);

        generateHeightLine.Draw(spriteBatch);
        deathpit.Draw(spriteBatch);

        spriteBatch.FillRectangle(upperBox, upperBoxColor);
        spriteBatch.DrawString(UI.Font, score.ToString(), scorePosition, Color.Black);

        if(God)
        {
            Vector2 measure = UI.Font.MeasureString("Godmode ON");
            Vector2 position = new Vector2(screenSize.X - measure.X - percent(screenSize.X, 5), scorePosition.Y);
            spriteBatch.DrawString(UI.Font, "Godmode ON", position, Color.Red);
        }
    }

    static private void DrawDeathUI()
    {
        string scoreText = "Score: " + score;
        string highscoreText = "Highscore: " + highscore;

        Vector2 measureScore = UI.Font.MeasureString(scoreText);
        Vector2 measureHighscore = UI.Font.MeasureString(highscoreText);

        Vector2 scorePos = new Vector2(center(screenSize.X, measureScore.X), center(screenSize.Y, measureScore.Y));
        Vector2 highscorePos = new Vector2(center(screenSize.X, measureHighscore.X), scorePos.Y + measureScore.Y + percent(screenSize.Y, 1));

        spriteBatch.DrawString(UI.Font, scoreText, scorePos, Color.Black);
        spriteBatch.DrawString(UI.Font, highscoreText, highscorePos, Color.Black);
    }

    static private void DrawMenu()
    {
        const int scoreSizeY = 25;
        int scoreX = percent(screenSize.X, 2);

        Vector2 pos = new(scoreX, percent(screenSize.X, 2));
        spriteBatch.DrawString(UI.Font, "Top scores: ", pos, Color.Black);

        int scoreY = (int)pos.Y + scoreSizeY;

        for(int i=1; i < maxTopScores+1; ++i)
        {
            spriteBatch.DrawString(UI.Font, i + ". " + scores[^i].ToString(), new Vector2(scoreX, scoreY), Color.Black);
            scoreY += scoreSizeY;
        }
    }

    static private void DrawBackground()
    {
        for (int x = 0; x <= screenSize.X; x += background.Width)
        {
            for (int y = bgStartY; y <= bgEndY; y += background.Height)
            {
                spriteBatch.Draw(background, new Vector2(x, y) - Camera, Color.LightYellow);
            }
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        graphics.GraphicsDevice.Clear(Color.LightYellow);

        spriteBatch.Begin();
        {
            DrawBackground();
            UI.DrawElements(spriteBatch);

            drawMethods[gameState].Invoke();
        }
        spriteBatch.End();

        base.Draw(gameTime);
    }

    //Initialize game states
    static private void StartGame()
    {
        gameState = GameState.Game;
        Reset();

        foreach (int score in scores.Where(i => i > 0))
        {
            bool highest = false;
            if (score == scores.Max()) highest = true;
            Scorelines.Add(new Scoreline(score, highest));
        }

        for (int i = 0; i < startGenerateCount; ++i)
            Generate();

        generateHeight /= 2;
    }

    static private void StartMenu()
    {
        ResetBackground();
        gameState = GameState.Menu;
    }

    static public void Death()
    {
        ResetBackground();
        gameState = GameState.Death;

        deathCount++;

        if (score > highscore)
            highscore = score;

        AddScore();
    }

    //Other
    static private void AddScore()
    {
        //If scores already has this score or there isn't smaller score than this one, skip
        if (scores.Contains(score) || 
            score <= scores.Min()) 
            return;

        //Write score to the file
        scores[Array.IndexOf(scores, scores.Min())] = score;
        Array.Sort(scores);
        File.WriteAllLines(statsPath, scores.Select(score => score.ToString()));
    }

    static private void ResetGlobalScores()
    {
        File.WriteAllText(statsPath, null);
        for (int i = 0; i < maxTopScores; ++i)
        {
            File.AppendAllText(statsPath, "0\n");
        }
        highscore = 0;

        Array.Clear(scores);
    }

    static private void CreateUi()
    {
        Point buttonSize = new(150,50);
        Rectangle rectPlay = new(center(screenSize.X, buttonSize.X), center(screenSize.Y, buttonSize.Y), buttonSize.X, buttonSize.Y);
        Rectangle rectScores = new(screenSize.X - buttonSize.X - percent(screenSize.X, 3), percent(screenSize.Y, 2), buttonSize.X, buttonSize.Y);
        Rectangle rectMenu = new(center(screenSize.X, buttonSize.X), center(screenSize.Y, buttonSize.Y) + percent(screenSize.Y, 25), buttonSize.X, buttonSize.Y);
        Rectangle rectRestart = rectMenu;
        rectRestart.Y = rectMenu.Y - buttonSize.Y - percent(screenSize.X, 2);

        UI.Add(new Button(rectPlay, StartGame, "Play", 0));
        UI.Add(new Button(rectScores, ResetGlobalScores, "Reset scores", 0));
        UI.Add(new Button(rectMenu, StartMenu, "Menu", 2));
        UI.Add(new Button(rectRestart, StartGame, "Restart", 2));

        var volumeChange = (int v) => { SoundEffect.MasterVolume = (float)v / 100; };

        Point sliderSize = new(410, Slider.sizeY);
        UI.Add(new Slider(new Point(center(screenSize.X, sliderSize.X), screenSize.Y - percent(screenSize.Y, 5) - sliderSize.Y), "Volume:", volumeChange, 0));
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

class Scorelines : Group<Scoreline>  {}

class Scoreline : Entity
{
    static readonly Texture2D scorelineTexture = MonoGame.LoadTexture("scoreline");
    static readonly Point size = new Point(54, 12);
    static readonly float x = MyGame.screenSize.X - size.X;

    float score;

    bool highest = false;

    public Scoreline(float score, bool highest)
        : base( new RectangleF( new Vector2(x, -(score * 10) ), size), scorelineTexture)
    {
        this.score = score;
        this.highest = highest;
    }

    public override void Update(GameTime gameTime) {}

    public override void Draw(SpriteBatch spriteBatch)
    {
        Rectangle final = (Rectangle)rectangle;
        final.Location -= MyGame.Camera.ToPoint();

        float scoreScale = 0.8f;

        string scoreText = score.ToString();
        Vector2 scorePos = new Vector2(final.X + 6, final.Y - UI.Font.MeasureString(scoreText).Y * scoreScale);

        spriteBatch.DrawString(UI.Font, scoreText, scorePos, Color.Black, 0f, Vector2.Zero, scoreScale, SpriteEffects.None, 0);
        spriteBatch.Draw(texture, final, highest ? Color.Orange : Color.DarkCyan);
    }

    public override void Destroy() => Scorelines.Destroy(groupID);
}