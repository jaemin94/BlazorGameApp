namespace MyBlazorApp.Services;

// 스킬 위치 계산
public class GameSkillService
{
    public (int x, int y) GetFireballPosition(int playerX, int playerY, string direction)
    {
        int fireballX = playerX;
        int fireballY = playerY;
        const int distance = 80;

        if (direction == "up") fireballY -= distance;
        else if (direction == "down") fireballY += distance;
        else if (direction == "left") fireballX -= distance;
        else if (direction == "right") fireballX += distance;

        fireballX = Math.Clamp(fireballX, 24, 976);
        fireballY = Math.Clamp(fireballY, 24, 626);

        return (fireballX, fireballY);
    }
}
