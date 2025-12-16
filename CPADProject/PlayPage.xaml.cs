using System;
using System.Collections.Generic;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;

using System.Text.Json;



namespace CPADProject;

public partial class PlayPage : ContentPage
{
    private Player player;
    private List<Enemy> enemies = new();
    private IDispatcherTimer gameTimer;

    private Drawing sky;
    private IDispatcherTimer skyTimer;
    private double skyProgress = 0;

    private IDispatcherTimer enemySpawnTimer;

    private int score = 0;
    private int lives = 3;
    private bool gameRun = false;

    private double canvasWidth;
    private double canvasHeight;
    private double lastPanX = 0;
    private double lastPanY = 0;

    public int gameScore
    {
        get { return score; }
        set
        {
            score = value;
            //OnPropertyChanged()?
        }
    }

    public PlayPage()
    {
        InitializeComponent();
        InitialiseTimersandGestures();
        sky = new Drawing();
        SkyView.Drawable = sky;

        StartSkyCycle();
    }

    private void StartSkyCycle()
    {
        skyTimer = Dispatcher.CreateTimer();
        skyTimer.Interval = TimeSpan.FromMilliseconds(16); // 60 FPS
        skyTimer.Tick += (s, e) =>
        {
            // Sky day-night cycle
            skyProgress += 0.001;

            if (skyProgress >= 1)
                skyProgress = 0;

            sky.daylight = skyProgress;

            // Road + trees + buildings scroll
            sky.scrollEffect += 4;   // adjust speed

            SkyView.Invalidate();
        };

        skyTimer.Start();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (width > 0 && height > 0)
        {
            canvasWidth = width;
            canvasHeight = height - 65; // Account for header
        }
    }

    private void startBtnClicked(object sender, EventArgs e)
    {
        StartGame();
    }

    private void StartGame()
    {
        if (gameRun) return;

        gameRun = true;
        score = 0;
        lives = 3;
        enemies.Clear();
        GameCanvas.Children.Clear();
        GameOverOverlay.IsVisible = false;
        StartButton.IsEnabled = false;
        gameTimer.Start();
        enemySpawnTimer.Start();

        player = new Player(canvasWidth / 2, canvasHeight / 2);
        GameCanvas.Children.Add(player.Visual);
        AbsoluteLayout.SetLayoutBounds(player.Visual,
            new Rect(player.X - player.Size / 2, player.Y - player.Size / 2, player.Size, player.Size));
    }

    public void InitialiseTimersandGestures()
    {
        // Add pan gesture for continuous movement
        var panGesture = new PanGestureRecognizer();
        panGesture.PanUpdated += OnPanUpdated;
        GameCanvas.GestureRecognizers.Add(panGesture);

        //// Setup game loop timer using DispatcherTimer (60 FPS)
        gameTimer = Dispatcher.CreateTimer();
        gameTimer.Interval = TimeSpan.FromMilliseconds(16);
        gameTimer.Tick += OnGameTick;
        gameTimer.IsRepeating = true;

        enemySpawnTimer = Dispatcher.CreateTimer();
        enemySpawnTimer.Interval = TimeSpan.FromSeconds(2); //enemy spawns every 2 seconds
        enemySpawnTimer.Tick += OnEnemySpawn;
        enemySpawnTimer.IsRepeating = true;

    }

    private void OnGameTick(object sender, EventArgs e)
    {
        if (!gameRun) return;

        // Update all enemies
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            enemies[i].Update(canvasWidth, canvasHeight);

            // Update enemy position
            AbsoluteLayout.SetLayoutBounds(enemies[i].Visual,
                new Rect(enemies[i].X - enemies[i].Size / 2,
                         enemies[i].Y - enemies[i].Size / 2,
                         enemies[i].Size, enemies[i].Size));

            // Check collision with player
            if (CheckCollision(player.X, player.Y, player.Size,
                             enemies[i].X, enemies[i].Y, enemies[i].Size))
            {
                GameCanvas.Children.Remove(enemies[i].Visual);
                enemies.RemoveAt(i);
                LoseLife();
                continue;
            }
        }
    }


    private void LoseLife()
    {
        lives--;
        UpdateUI();

        if (lives <= 0)
        {
            EndGame();
        }
    }

    private void UpdateUI()
    {
        LivesLabel.Text = $"Lives: {lives}";
    }

    private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        if (!gameRun) return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                //reset tracking when gesture starts
                lastPanX = e.TotalX;
                lastPanY = e.TotalY;
                break;

            case GestureStatus.Running:
                {
                    // Only move by the *change* in pan, not the total
                    // Divide by 2 to reduce sensitivity
                    double deltaX = (e.TotalX - lastPanX) / 2;
                    double deltaY = (e.TotalY - lastPanY) / 2;

                    lastPanX = e.TotalX;
                    lastPanY = e.TotalY;

                    double newX = player.X + deltaX;
                    double newY = player.Y + deltaY;

                    newX = Math.Clamp(newX, player.Size / 2, canvasWidth - player.Size / 2);
                    //newY = Math.Clamp(newY, player.Size / 2, canvasHeight - player.Size / 2);
                    MovePlayer(newX, newY);
                    break;
                }

            case GestureStatus.Completed:
                break;
        }
    }

    private void MovePlayer(double targetX, double targetY)
    {
        player.MoveTo(targetX, targetY);
        AbsoluteLayout.SetLayoutBounds(player.Visual,
            new Rect(player.X - player.Size / 2, player.Y - player.Size / 2, player.Size, player.Size));
    }

    private void OnEnemySpawn(object sender, EventArgs e)
    {
        if (!gameRun) return;
        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        Random rand = new Random();

        // match Drawing.cs road logic:
        double roadWidth = canvasWidth * 0.55;
        double roadLeft = (canvasWidth - roadWidth) / 2;
        double roadRight = roadLeft + roadWidth;

        // divide road into 3 equal lanes  
        double laneWidth = roadWidth / 3;

        int lane = rand.Next(3); // 0, 1, or 2

        double x = roadLeft + lane * laneWidth + laneWidth / 2;

        //bus spawn above screen  
        double y = 65;
        Enemy enemy = new Enemy(x, y);

        enemies.Add(enemy);
        GameCanvas.Children.Add(enemy.Visual);

        // position visually  
        AbsoluteLayout.SetLayoutBounds(enemy.Visual,
            new Rect(
                enemy.X - enemy.Size / 2,
                enemy.Y - enemy.Size,
                enemy.Size,
                enemy.Size * 2
            )
        );
    }

    private bool CheckCollision(double x1, double y1, double size1,
                               double x2, double y2, double size2)
    {
        //player hitbox
        double playerWidth = size1 * 0.8;
        double playerHeight = size1 * 1.3;
        double playerLeft = x1 - playerWidth / 2;
        double playerRight = x1 + playerWidth / 2;

        //extends up
        double playerTop = y1 - playerHeight * 0.7;
        double playerBottom = y1 + playerHeight * 0.3;

        //enemy hitbox
        double enemyWidth = size2;
        double enemyHeight = size2 * 2;
        double enemyLeft = x2 - enemyWidth / 2;
        double enemyRight = x2 + enemyWidth / 2;
        double enemyTop = y2 - enemyHeight;
        double enemyBottom = y2;

        return playerLeft < enemyRight &&
               playerRight > enemyLeft &&
               playerTop < enemyBottom &&
               playerBottom > enemyTop;
    }

    private void EndGame()
    {
        gameRun = false;
        gameTimer?.Stop();
        enemySpawnTimer?.Stop();

        GameOverOverlay.IsVisible = true;
    }



}