namespace MyBlazorApp.Services;

// 키보드 이동/대쉬 처리 서비스
public class GameInputService
{
    public string LastDirection { get; private set; } = "down";

    public void SetDirection(string direction)
    {
        LastDirection = direction;
    }

    public (int x, int y) Move(int x, int y, int moveX, int moveY, string direction)
    {
        LastDirection = direction;
        x += moveX;
        y += moveY;
        return ClampPosition(x, y);
    }

    public (int x, int y) Dash(int x, int y)
    {
        const int dashDistance = 80;
        if (LastDirection == "up") y -= dashDistance;
        else if (LastDirection == "down") y += dashDistance;
        else if (LastDirection == "left") x -= dashDistance;
        else if (LastDirection == "right") x += dashDistance;
        return ClampPosition(x, y);
    }

    public (int x, int y) ClampPosition(int x, int y)
    {
        const int mapWidth = 1000;
        const int mapHeight = 650;
        const int playerHalfSize = 24;
        x = Math.Clamp(x, playerHalfSize, mapWidth - playerHalfSize);
        y = Math.Clamp(y, playerHalfSize, mapHeight - playerHalfSize);
        return (x, y);
    }
}
