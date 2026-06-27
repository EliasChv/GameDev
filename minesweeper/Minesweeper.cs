using Godot;
using System;
using System.Collections.Generic;

public class Tile
{
    public int Number { set; get; }
    public bool Bomb { set; get; }
    public bool Flag { set; get; }
    public bool Hidden { set; get; }
}


public partial class Minesweeper : Control
{
    [Export] public Font font;
    public List<List<Tile>> Board = new();
    private int Zoom = 1;
    private Vector2I revealAllTiles = Vector2I.Zero;
    private bool startRevealAllTiles = false;
    private float time = 0;
    private int _bombsToPlace; // New
    private bool _firstClickMade = false; // New

    private void ToggleFlag(Vector2I position)
    {
        Tile tile = Board[position.Y][position.X];
        if (!tile.Hidden) return; // New
        tile.Flag = !tile.Flag;
    }

    public override void _Ready()
    {
        foreach (MyButton button in GetNode<HBoxContainer>("HBoxContainer").GetChildren())
        {
            button.Pressed += () =>
            {
                ZoomGame(button.ButtonType);
            };
        }
        GetNode<MyButton>("Button").Pressed += () =>
        {
            GetParent().GetNode<VBoxContainer>("VBoxContainer").Visible = true;
        };
        SetProcess(false);
    }

    public override void _Process(double delta)
    {
        if (startRevealAllTiles)
        {
            time += (float)delta;
            if (time > (1 / 225f)) 
            {
                RevealAllTiles();
                time = 0;
            }
        }

        QueueRedraw();
    }

    public override void _Draw()
    {
        int tileSize = 16 * Zoom;
        Vector2I screenSize = DisplayServer.WindowGetSize();
        Vector2 centerBoard = new Vector2(screenSize.X / 2 - Board.Count * tileSize / 2, screenSize.Y / 2 - Board.Count * tileSize / 2);

        for (int y = 0; y < Board.Count; y++)
        {
            for (int x = 0; x < Board[y].Count; x++)
            {
                Tile tile = Board[y][x];

                DrawRect(
                    new Rect2(new Vector2(centerBoard.X + x * tileSize + 2, centerBoard.Y + y * tileSize + 2), new Vector2(tileSize - 2, tileSize - 2)),
                    tile.Bomb && !tile.Hidden ? new Color(0.545098f, 0, 0, 1) : new Color(0.745098f, 0.745098f, 0.745098f, 1),
                    filled: true
                );

                if (tile.Flag && tile.Hidden)
                {
                    Vector2 stringSize = font.GetStringSize("F", HorizontalAlignment.Center, -1, 8 * Zoom);
                    Vector2 drawPos = new Vector2(centerBoard.X + x * tileSize + tileSize / 2 - stringSize.X / 2,
                                                  centerBoard.Y + y * tileSize + tileSize / 2 + stringSize.Y / 2.5f);
                    DrawString(font, drawPos, "F", HorizontalAlignment.Center, -1, 8 * Zoom, new Color(1, 0, 0)); // Red "F"
                }
// Change with F if u want flags 🚩!!
                if (!tile.Hidden && !tile.Bomb && tile.Number > 0)
                {
                    string numberChar = tile.Number.ToString();
                    Vector2 stringSize = font.GetStringSize(numberChar, HorizontalAlignment.Center, -1, 8 * Zoom);
                    Vector2 drawPos = new Vector2(centerBoard.X + x * tileSize + tileSize / 2 - stringSize.X / 2, centerBoard.Y + y * tileSize + tileSize / 2 + stringSize.Y / 2.5f);
                    DrawString(font, drawPos, numberChar, HorizontalAlignment.Center, -1, 8 * Zoom);
                }
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseInput && mouseInput.Pressed)
        {
            int tileSize = 16 * Zoom;
            Vector2I screenSize = DisplayServer.WindowGetSize();
            Vector2 centerBoard = new Vector2(screenSize.X / 2 - Board.Count * tileSize / 2, screenSize.Y / 2 - Board.Count * tileSize / 2);
            Vector2I tileIndex = (Vector2I)((mouseInput.Position - centerBoard) / tileSize).Floor();

            if (tileIndex.X >= 0 && tileIndex.X < 15 && tileIndex.Y >= 0 && tileIndex.Y < 15)
            {
                if (mouseInput.ButtonIndex == MouseButton.Left)
                {
        
                    if (!_firstClickMade)
                    {
                        InitializeBoardAndBombs(tileIndex); // Initialize 
                        _firstClickMade = true;
                    }
                    RevealTile(tileIndex);
                }
                else if (mouseInput.ButtonIndex == MouseButton.Right)
                {
                    ToggleFlag(tileIndex);
                }
            }
        }
    }

    public void SetupGame(int bombs)
    {
        Board = new();
        revealAllTiles = Vector2I.Zero;
        startRevealAllTiles = false;
        time = 0;
        _bombsToPlace = bombs; 
        _firstClickMade = false; 

        for (int i = 0; i < 15; i++)
        {
            Board.Add(new List<Tile>());
            for (int j = 0; j < 15; j++)
            {
                Board[i].Add(new Tile()
                {
                    Number = 0,
                    Bomb = false,
                    Flag = false,
                    Hidden = true
                });
            }
        }

        SetProcess(true);
    }

    private void InitializeBoardAndBombs(Vector2I firstClickPosition)
    {
        int bombsPlaced = 0;
        while (bombsPlaced < _bombsToPlace)
        {
            int x = GD.RandRange(0, 14);
            int y = GD.RandRange(0, 14);

            if (Math.Abs(x - firstClickPosition.X) <= 1 && Math.Abs(y - firstClickPosition.Y) <= 1) continue;
            if (Board[y][x].Bomb) continue;

            Board[y][x].Bomb = true;
            bombsPlaced++;
        }

        for (int y = 0; y < 15; y++)
        {
            for (int x = 0; x < 15; x++)
            {
                if (Board[y][x].Bomb) continue;

                int bombCount = 0;
                for (int k = -1; k <= 1; k++)
                {
                    for (int h = -1; h <= 1; h++)
                    {
                        if (k == 0 && h == 0) continue; 

                        int neighborX = x + k;
                        int neighborY = y + h;

                        if (neighborX >= 0 && neighborX < 15 && neighborY >= 0 && neighborY < 15)
                        {
                            if (Board[neighborY][neighborX].Bomb)
                            {
                                bombCount++;
                            }
                        }
                    }
                }
                Board[y][x].Number = bombCount;
            }
        }
    }

    private void RevealTile(Vector2I position)
    {
        Tile tile = Board[position.Y][position.X];

        if (tile.Flag) return;
        if (!tile.Hidden) return;

        if (tile.Bomb)
        {
            RevealAllTiles();
            startRevealAllTiles = true;
            return;
        }

        tile.Hidden = false;

        if (tile.Number == 0)
        {
            for (int k = -1; k <= 1; k++)
            {
                for (int h = -1; h <= 1; h++)
                {
                    if (k == 0 && h == 0) continue;

                    int neighborX = position.X + k;
                    int neighborY = position.Y + h;

                    if (neighborX >= 0 && neighborX < 15 && neighborY >= 0 && neighborY < 15)
                    {
                        RevealTile(new Vector2I(neighborX, neighborY));
                    }
                }
            }
        }
    }

    private void RevealAllTiles()
    {
        if (revealAllTiles.Y < Board.Count && revealAllTiles.X < Board[revealAllTiles.Y].Count)
        {
            Board[revealAllTiles.Y][revealAllTiles.X].Hidden = false;
        }

        revealAllTiles.X++;
        if (revealAllTiles.X >= 15)
        {
            revealAllTiles.X = 0;
            revealAllTiles.Y++;
        }

        if (revealAllTiles.Y >= 15)
        {
            startRevealAllTiles = false;
        }
    }

    private void ZoomGame(MyButton.ButtonTypeEnum buttonType)
    {
        switch (buttonType)
        {
            case MyButton.ButtonTypeEnum.ZoomIn:
                Zoom = Math.Clamp(Zoom + 1, 1, 10);
                break;
            case MyButton.ButtonTypeEnum.ZoomOut:
                Zoom = Math.Clamp(Zoom - 1, 1, 10);
                break;
        }
    }
}