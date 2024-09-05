using SFML.Graphics;
using SFML.System;
using SFML.Window;
using MazeGenerator;

Vector2i atlasSize = new(12, 21);
Vector2i gridSize = new(70, 70);
Texture tex = new("Resources/tilemap.png");

RenderWindow window = new(new VideoMode(1600, 1280), "Wave function collapse");
View view = new(new Vector2f(), new Vector2f(1600, 1280));
window.SetView(view);

Tilemap tilemap = new(tex, atlasSize, gridSize);

window.SetKeyRepeatEnabled(true);
window.Resized += (s, e) => window.SetView(new View(new Vector2f(), new Vector2f(e.Width, e.Height)));
window.Closed += (s, e) => window.Close();

bool isMovingCam = false;
Vector2f lastPos = new();
float zoom = 1.0f;

window.Resized += (s, e) => view.Size = new Vector2f(e.Width, e.Height) * zoom;

window.MouseWheelScrolled += (s, e) =>
{
    float multiplier = Math.Abs(e.Delta);
    float ratio = e.Delta < 0 ? 1.25f * multiplier : 0.8f / multiplier;
    zoom = Math.Clamp(zoom * ratio, 0.2f, 3.0f);
    view.Size = (Vector2f)window.Size * zoom;
};

window.MouseButtonPressed += (s, e) =>
{
    if (e.Button != Mouse.Button.Left)
        return;

    isMovingCam = true;
    lastPos = new Vector2f(e.X, e.Y);
};

window.MouseButtonReleased += (s, e) =>
{
    if (e.Button != Mouse.Button.Left)
        return;

    isMovingCam = false;
};

window.MouseMoved += (s, e) =>
{
    if (!isMovingCam)
        return;

    Vector2f currentPos = new Vector2f(e.X, e.Y);

    Vector2f offset = (lastPos - currentPos) * zoom;

    view.Move(offset);

    lastPos = currentPos;
};

window.KeyPressed += (s, e) =>
{
    if (e.Code == Keyboard.Key.Space)
    {
        if (e.Control)
        {
            for (int i = 0; i < 10; i++)
                tilemap.Tick();
        }
        else
        {
            tilemap.Tick();
        }
    }

    if (e.Code == Keyboard.Key.R)
        tilemap = new(tex, atlasSize, gridSize);


    if (e.Code == Keyboard.Key.PageDown)
    {
        view.Zoom(1.25f);
        window.SetView(view);
    }
    else if (e.Code == Keyboard.Key.PageUp)
    {
        view.Zoom(0.8f);
        window.SetView(view);
    }
};

while (window.IsOpen)
{
    window.Clear(new Color(135, 206, 235));
    window.DispatchEvents();
    window.SetView(view);

    window.Draw(tilemap);

    window.Display();
}
