using Microsoft.AspNetCore.SignalR.Client;

namespace MyBlazorApp.Services;

// 클라이언트에서 주기적으로 서버 몬스터 AI 갱신 요청
public class GameAiService
{
    private PeriodicTimer? monsterTimer;
    private CancellationTokenSource? monsterCts;
    private bool started = false;

    public void Start(HubConnection hubConnection)
    {
        if (started) return;
        started = true;
        monsterCts = new CancellationTokenSource();
        monsterTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(300));

        _ = Task.Run(async () =>
        {
            try
            {
                while (await monsterTimer.WaitForNextTickAsync(monsterCts.Token))
                {
                    if (hubConnection.State == HubConnectionState.Connected)
                        await hubConnection.SendAsync("UpdateMonsters");
                }
            }
            catch { }
        });
    }

    public void Stop()
    {
        started = false;
        monsterCts?.Cancel();
        monsterCts?.Dispose();
        monsterTimer?.Dispose();
        monsterCts = null;
        monsterTimer = null;
    }
}
