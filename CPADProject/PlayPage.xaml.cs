using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using System;
using System.Collections.Generic;
using System.Text.Json;
using static CPADProject.Pickups;



namespace CPADProject;

public partial class PlayPage : ContentPage
{
    private Player player;
    private List<Enemy> enemies = new();
    private List<Pickups> coin = new();
    private List<Pickups> fuel = new();
    private IDispatcherTimer gameTimer;

    private Drawing sky;
    private IDispatcherTimer skyTimer;
    private double skyProgress = 0;

    private IDispatcherTimer enemySpawnTimer;
    private IDispatcherTimer pickupSpawnTimer;
    private IDispatcherTimer fuelEffectTimer;

    private int score = 0;
    private int lives = 3;
    private bool gameRun = false;
    int distance = 0;

    private double canvasWidth;
    private double canvasHeight;
    private double lastPanX = 0;
    private double lastPanY = 0;
    private double moveSpeed = 1;


    public int gameScore
    {
        get { return score; }
        set
        {
            score = value;
            //OnPropertyChanged();
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
            sky.scrollEffect += 4 * moveSpeed;   // adjust speed

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
        pickupSpawnTimer.Start();

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

        pickupSpawnTimer = Dispatcher.CreateTimer();
        pickupSpawnTimer.Interval = TimeSpan.FromSeconds(5);
        pickupSpawnTimer.Tick += OnPickupSpawn;
        pickupSpawnTimer.IsRepeating = true;



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

        for (int i = coin.Count - 1; i >= 0; i--)
        {
            coin[i].Update(canvasWidth, canvasHeight);

            AbsoluteLayout.SetLayoutBounds(coin[i].Visual,
                new Rect(
                    coin[i].X - coin[i].Size / 2,
                    coin[i].Y - coin[i].Size / 2,
                    coin[i].Size,
                    coin[i].Size
                )
            );

            if (CheckCollision(player.X, player.Y, player.Size,
                               coin[i].X, coin[i].Y, coin[i].Size))
            {
                GameCanvas.Children.Remove(coin[i].Visual);
                coin.RemoveAt(i);
                GetCoins();
            }
        }

        for (int i = fuel.Count - 1; i >= 0; i--)
        {
            fuel[i].Update(canvasWidth, canvasHeight);

            AbsoluteLayout.SetLayoutBounds(fuel[i].Visual,
                new Rect(
                    fuel[i].X - fuel[i].Size / 2,
                    fuel[i].Y - fuel[i].Size / 2,
                    fuel[i].Size,
                    fuel[i].Size
                )
            );

            if (CheckCollision(player.X, player.Y, player.Size,
                               fuel[i].X, fuel[i].Y, fuel[i].Size))
            {
                GameCanvas.Children.Remove(fuel[i].Visual);
                fuel.RemoveAt(i);
                GetFuel();
            }
        }

        distance++;
        if (distance % 10 == 0) UpdateUI();
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

    private void GetCoins()
    {
        score += 10;
        UpdateUI();
    }

    private void GetFuel()
    {
        moveSpeed = 2.0; // boosted speed

        fuelEffectTimer?.Stop();
        fuelEffectTimer = Dispatcher.CreateTimer();
        fuelEffectTimer.Interval = TimeSpan.FromSeconds(5);

        fuelEffectTimer.Tick += (s, e) =>
        {
            moveSpeed = 1.0; // back to normal
            fuelEffectTimer.Stop();
        };

        fuelEffectTimer.Start();
    }

    private void UpdateUI()
    {
        LivesLabel.Text = $"Lives: {lives}";
        ScoreLabel.Text = $"Score: {score}";
        DistanceLabel.Text = $"Distance: {distance} m";
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
        return Math.Abs(x1 - x2) < (size1 + size2) * 0.20 &&
               Math.Abs(y1 - y2) < (size1 + size2) * 0.55;
    }

    private void OnPickupSpawn(object sender, EventArgs e)
    {
        if (!gameRun) return;

        if (Random.Shared.NextDouble() < 0.7)
            SpawnCoin();   // 70%
        else
            SpawnFuel();   // 30%
    }

    private void SpawnCoin()
    {
        Random rand = new Random();

        //match Drawing.cs road logic:
        double roadWidth = canvasWidth * 0.55;
        double roadLeft = (canvasWidth - roadWidth) / 2;
        double roadRight = roadLeft + roadWidth;

        //divide road into 3 equal lanes  
        double laneWidth = roadWidth / 3;

        int lane = rand.Next(3); // 0, 1, or 2

        double x = roadLeft + lane * laneWidth + laneWidth / 2;

        //bus spawn above screen  
        double y = 65;
        Pickups coins = new Pickups(x, y, PickupType.Coin);

        coin.Add(coins);
        GameCanvas.Children.Add(coins.Visual);

        //position visually  
        AbsoluteLayout.SetLayoutBounds(coins.Visual,
            new Rect(
                coins.X - coins.Size / 2,
                coins.Y - coins.Size,
                coins.Size,
                coins.Size * 2
            )
        );
    }


    private void SpawnFuel()
    {
        Random rand = new Random();

        //match Drawing.cs road logic:
        double roadWidth = canvasWidth * 0.55;
        double roadLeft = (canvasWidth - roadWidth) / 2;
        double roadRight = roadLeft + roadWidth;

        //divide road into 3 equal lanes  
        double laneWidth = roadWidth / 3;

        int lane = rand.Next(3); // 0, 1, or 2

        double x = roadLeft + lane * laneWidth + laneWidth / 2;

        //bus spawn above screen  
        double y = 65;
        Pickups getFuel = new Pickups(x, y, PickupType.Fuel);

        fuel.Add(getFuel);
        GameCanvas.Children.Add(getFuel.Visual);

        //position visually  
        AbsoluteLayout.SetLayoutBounds(getFuel.Visual,
            new Rect(
                getFuel.X - getFuel.Size / 2,
                getFuel.Y - getFuel.Size,
                getFuel.Size,
                getFuel.Size * 2
            )
        );
    }


    private void EndGame()
    {
        gameRun = false;
        gameTimer?.Stop();
        enemySpawnTimer?.Stop();
        pickupSpawnTimer?.Stop();

        GameOverOverlay.IsVisible = true;
    }



}