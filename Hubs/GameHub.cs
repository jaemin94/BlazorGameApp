using Microsoft.AspNetCore.SignalR;
using MyBlazorApp.Models;

namespace MyBlazorApp.Hubs;

// 서버에서 플레이어, 몬스터, 드랍, 퀘스트, 전직, 스킬을 관리하는 SignalR Hub
public class GameHub : Hub
{
    // 접속자 목록
    private static readonly Dictionary<string, PlayerState> Players = new();

    // 방+맵 기준 몬스터 목록
    private static readonly Dictionary<string, List<MonsterState>> RoomMonsters = new();

    // 방+맵 기준 드랍 아이템 목록
    private static readonly Dictionary<string, List<DroppedItem>> RoomDrops = new();

    // 퀘스트 원본 목록
    private static readonly List<QuestState> QuestDefinitions = new()
    {
        new QuestState
        {
            QuestId = "merchant_slime",
            QuestName = "상인의 부탁",
            Description = "슬라임 3마리를 처치하세요.",
            TargetMonsterType = MonsterType.Slime,
            RequiredCount = 3,
            RewardGold = 50,
            RewardExp = 30,
            RewardItemName = "초보자 검",
            RewardItemType = "Weapon",
            RewardAttackBonus = 3,
            RewardRarity = "Normal"
        },
        new QuestState
        {
            QuestId = "hunter_goblin",
            QuestName = "사냥꾼의 의뢰",
            Description = "고블린 2마리를 처치하세요.",
            TargetMonsterType = MonsterType.Goblin,
            RequiredCount = 2,
            RewardGold = 120,
            RewardExp = 80,
            RewardItemName = "고블린 갑옷",
            RewardItemType = "Armor",
            RewardDefenseBonus = 8,
            RewardRarity = "Rare"
        },
        new QuestState
        {
            QuestId = "boss_clear",
            QuestName = "보스 토벌",
            Description = "보스 몬스터 1마리를 처치하세요.",
            TargetMonsterType = MonsterType.Boss,
            RequiredCount = 1,
            RewardGold = 300,
            RewardExp = 200,
            RewardItemName = "용사의 검",
            RewardItemType = "Weapon",
            RewardAttackBonus = 30,
            RewardRarity = "Epic"
        }
    };

    private static string MapKey(string roomCode, MapType map) => $"{roomCode}_{map}";

    // ==============================
    // 게임 입장
    // ==============================
    public async Task JoinGame(string roomCode, string playerName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
        await Groups.AddToGroupAsync(Context.ConnectionId, MapKey(roomCode, MapType.Town));

        Players[Context.ConnectionId] = new PlayerState
        {
            ConnectionId = Context.ConnectionId,
            Name = playerName,
            RoomCode = roomCode,
            CurrentMap = MapType.Town,
            X = 250,
            Y = 300,
            Hp = 100,
            MaxHp = 100,
            Level = 1,
            Exp = 0,
            Gold = 0
        };

        EnsureMap(roomCode, MapType.Town, 1);
        EnsureMap(roomCode, MapType.Field, 1);
        EnsureMap(roomCode, MapType.BossRoom, 1);

        await SendRoomState(roomCode);
        await SendMapState(roomCode, MapType.Town);
    }

    // ==============================
    // 전직
    // 8레벨: 마법사
    // 10레벨: 전사/도적/궁수
    // 전직 성공 시 스킬포인트 +1
    // ==============================
    public async Task ChangeJob(JobType job)
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player))
            return;

        if (player.Job != JobType.Beginner)
            return;

        if (job == JobType.Mage)
        {
            if (player.Level < 8)
                return;
        }
        else if (job == JobType.Warrior || job == JobType.Thief || job == JobType.Archer)
        {
            if (player.Level < 10)
                return;
        }
        else
        {
            return;
        }

        player.Job = job;
        player.SkillPoint += 1;

        await SendRoomState(player.RoomCode);
    }

    // ==============================
    // 맵 이동
    // ==============================
    public async Task ChangeMap(MapType targetMap, int targetX, int targetY)
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player))
            return;

        var oldMap = player.CurrentMap;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, MapKey(player.RoomCode, oldMap));

        player.CurrentMap = targetMap;
        player.X = targetX;
        player.Y = targetY;

        await Groups.AddToGroupAsync(Context.ConnectionId, MapKey(player.RoomCode, targetMap));

        EnsureMap(player.RoomCode, targetMap, player.Level);

        await SendRoomState(player.RoomCode);
        await SendMapState(player.RoomCode, oldMap);
        await SendMapState(player.RoomCode, targetMap);
    }

    public async Task MovePlayer(int x, int y)
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player))
            return;

        player.X = x;
        player.Y = y;

        await SendRoomState(player.RoomCode);
    }

    // ==============================
    // 기본 공격
    // ==============================
    public async Task AttackMonster()
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player))
            return;

        var monsters = GetRoomMonsters(player.RoomCode, player.CurrentMap);

        var target = monsters
            .OrderBy(m => GetDistance(player.X, player.Y, m.X, m.Y))
            .FirstOrDefault();

        if (target == null)
            return;

        if (GetDistance(player.X, player.Y, target.X, target.Y) > 80)
            return;

        target.Hp -= player.AttackPower;

        if (target.Hp <= 0)
            KillMonster(player, monsters, target);

        await SendRoomState(player.RoomCode);
        await SendMapState(player.RoomCode, player.CurrentMap);
    }

    // ==============================
    // 스킬 배우기 / 스킬 레벨업
    // 전직 이후부터 가능
    // 최대 레벨 10
    // ==============================
    public async Task LearnOrLevelUpSkill(string skillId)
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player))
            return;

        if (!player.IsJobChanged)
            return;

        if (player.SkillPoint <= 0)
            return;

        var skill = SkillDatabase.GetSkill(skillId);
        if (skill == null)
            return;

        if (skill.RequiredJob != player.Job)
            return;

        if (player.Level < skill.RequiredLevel)
            return;

        var learned = player.LearnedSkills.FirstOrDefault(s => s.SkillId == skillId);

        if (learned == null)
        {
            player.LearnedSkills.Add(new PlayerSkillState
            {
                SkillId = skillId,
                Level = 1,
                MaxLevel = skill.MaxLevel
            });

            player.SkillPoint--;
        }
        else
        {
            if (learned.Level >= skill.MaxLevel)
                return;

            learned.Level++;
            player.SkillPoint--;
        }

        await SendRoomState(player.RoomCode);
    }

    // ==============================
    // 퀵슬롯 등록
    // 배운 스킬만 등록 가능
    // ==============================
    public async Task SetSkillSlot(int slot, string skillId)
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player))
            return;

        if (slot < 1 || slot > 9)
            return;

        if (!player.LearnedSkills.Any(s => s.SkillId == skillId))
            return;

        var targetSlot = player.SkillSlots.FirstOrDefault(s => s.Slot == slot);
        if (targetSlot == null)
        {
            targetSlot = new SkillSlot { Slot = slot };
            player.SkillSlots.Add(targetSlot);
        }

        targetSlot.SkillId = skillId;

        await SendRoomState(player.RoomCode);
    }

    // ==============================
    // 퀵슬롯 번호로 스킬 사용
    // ==============================
    public async Task UseSkillFromSlot(int slot)
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player))
            return;

        var slotInfo = player.SkillSlots.FirstOrDefault(s => s.Slot == slot);
        if (slotInfo == null || string.IsNullOrWhiteSpace(slotInfo.SkillId))
            return;

        var learned = player.LearnedSkills.FirstOrDefault(s => s.SkillId == slotInfo.SkillId);
        if (learned == null)
            return;

        var skill = SkillDatabase.GetSkill(slotInfo.SkillId);
        if (skill == null)
            return;

        if (skill.SkillId == "basic_strike")
            UseDamageSkill(player, skill, learned.Level, 90, false);
        else if (skill.SkillId == "mage_fireball")
            UseDamageSkill(player, skill, learned.Level, skill.GetRange(learned.Level), true);
        else if (skill.SkillId == "mage_heal")
            UseHealSkill(player, learned.Level);
        else if (skill.SkillId == "warrior_spin")
            UseDamageSkill(player, skill, learned.Level, skill.GetRange(learned.Level), true);
        else if (skill.SkillId == "thief_double_slash")
            UseDamageSkill(player, skill, learned.Level, skill.GetRange(learned.Level), false);
        else if (skill.SkillId == "archer_power_shot")
            UseDamageSkill(player, skill, learned.Level, skill.GetRange(learned.Level), true);

        await SendRoomState(player.RoomCode);
        await SendMapState(player.RoomCode, player.CurrentMap);
    }

    private static void UseDamageSkill(PlayerState player, SkillDefinition skill, int skillLevel, int range, bool multipleTarget)
    {
        var monsters = GetRoomMonsters(player.RoomCode, player.CurrentMap);

        int targetCount = skill.GetTargetCount(skillLevel);
        int hitCount = skill.GetHitCount(skillLevel);
        int damage = skill.GetDamage(skillLevel) + player.AttackPower;

        if (player.Job == JobType.Mage)
            damage += player.MagicPower;

        var targets = monsters
            .Where(m => GetDistance(player.X, player.Y, m.X, m.Y) <= range)
            .OrderBy(m => GetDistance(player.X, player.Y, m.X, m.Y))
            .Take(multipleTarget ? targetCount : 1)
            .ToList();

        foreach (var monster in targets)
        {
            for (int i = 0; i < hitCount; i++)
            {
                monster.Hp -= damage;

                if (monster.Hp <= 0)
                {
                    KillMonster(player, monsters, monster);
                    break;
                }
            }
        }
    }

    private static void UseHealSkill(PlayerState player, int skillLevel)
    {
        int healAmount = player.HealPower + 20 + skillLevel * 12;
        player.Hp = Math.Min(player.MaxHp, player.Hp + healAmount);
    }

    // 예전 Game.razor와의 호환용. 새 Game.razor는 UseSkillFromSlot을 사용함.
    public async Task HealPlayer()
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player))
            return;

        player.Hp = Math.Min(player.MaxHp, player.Hp + player.HealPower);
        await SendRoomState(player.RoomCode);
    }

    // ==============================
    // 상점 / 인벤 / 장비
    // ==============================
    public async Task BuyPotion()
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player))
            return;

        const int price = 20;
        if (player.Gold < price)
            return;

        player.Gold -= price;
        AddItem(player, "작은 포션", 1, "Consumable");

        await SendRoomState(player.RoomCode);
    }

    public async Task UseInventoryItem(string itemName)
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player))
            return;

        var item = player.Inventory.FirstOrDefault(i => i.Name == itemName);
        if (item == null || item.Count <= 0)
            return;

        if (item.Name == "작은 포션")
        {
            if (player.Hp >= player.MaxHp)
                return;

            item.Count--;
            player.Hp = Math.Min(player.MaxHp, player.Hp + 40);

            if (item.Count <= 0)
                player.Inventory.Remove(item);
        }

        await SendRoomState(player.RoomCode);
    }

    public async Task EquipItem(string itemName)
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player))
            return;

        var item = player.Inventory.FirstOrDefault(i => i.Name == itemName);
        if (item == null)
            return;

        if (item.ItemType == "Weapon")
        {
            player.Weapon = new EquipmentItem
            {
                Name = item.Name,
                Type = "Weapon",
                AttackBonus = item.AttackBonus,
                DefenseBonus = item.DefenseBonus,
                Rarity = item.Rarity
            };
        }
        else if (item.ItemType == "Armor")
        {
            player.Armor = new EquipmentItem
            {
                Name = item.Name,
                Type = "Armor",
                AttackBonus = item.AttackBonus,
                DefenseBonus = item.DefenseBonus,
                Rarity = item.Rarity
            };
        }

        await SendRoomState(player.RoomCode);
    }

    public async Task UnequipItem(string equipmentType)
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player))
            return;

        if (equipmentType == "Weapon")
            player.Weapon = null;
        else if (equipmentType == "Armor")
            player.Armor = null;

        await SendRoomState(player.RoomCode);
    }

    public async Task AddStat(string statName)
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player))
            return;

        if (player.StatPoint <= 0)
            return;

        if (statName == "STR") player.Str++;
        else if (statName == "DEF") player.Def++;
        else if (statName == "INT") player.Int++;
        else return;

        player.StatPoint--;
        await SendRoomState(player.RoomCode);
    }

    // ==============================
    // 퀘스트
    // ==============================
    public async Task AcceptQuest(string questId)
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player))
            return;

        var quest = QuestDefinitions.FirstOrDefault(q => q.QuestId == questId);
        if (quest == null)
            return;

        if (player.Quests.Any(q => q.QuestId == questId))
            return;

        player.Quests.Add(new PlayerQuestState { QuestId = questId });

        if (questId == "merchant_slime")
        {
            player.QuestAccepted = true;
            player.QuestCompleted = false;
            player.QuestRewardReceived = false;
        }

        await SendRoomState(player.RoomCode);
    }

    public async Task ReceiveQuestReward(string questId)
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player))
            return;

        var quest = QuestDefinitions.FirstOrDefault(q => q.QuestId == questId);
        var playerQuest = player.Quests.FirstOrDefault(q => q.QuestId == questId);

        if (quest == null || playerQuest == null)
            return;

        if (!playerQuest.Completed || playerQuest.RewardReceived)
            return;

        player.Gold += quest.RewardGold;
        AddExp(player, quest.RewardExp);

        if (!string.IsNullOrWhiteSpace(quest.RewardItemName))
        {
            AddItem(player, quest.RewardItemName, 1, quest.RewardItemType, quest.RewardAttackBonus, quest.RewardDefenseBonus, quest.RewardRarity);
        }

        playerQuest.RewardReceived = true;

        if (questId == "merchant_slime")
            player.QuestRewardReceived = true;

        await SendRoomState(player.RoomCode);
    }

    public async Task PickupItem()
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player))
            return;

        var drops = GetRoomDrops(player.RoomCode, player.CurrentMap);
        var item = drops.FirstOrDefault(d => GetDistance(player.X, player.Y, d.X, d.Y) <= 80);

        if (item == null)
            return;

        if (item.IsGold)
            player.Gold += item.GoldAmount;
        else
            AddItem(player, item.Name, 1, item.ItemType, item.AttackBonus, item.DefenseBonus, item.Rarity);

        drops.Remove(item);

        await SendRoomState(player.RoomCode);
        await SendMapState(player.RoomCode, player.CurrentMap);
    }

    // ==============================
    // 몬스터 AI
    // ==============================
    public async Task UpdateMonsters()
    {
        foreach (var playerGroup in Players.Values.GroupBy(p => new { p.RoomCode, p.CurrentMap }))
        {
            string roomCode = playerGroup.Key.RoomCode;
            MapType map = playerGroup.Key.CurrentMap;

            if (map == MapType.Town)
                continue;

            var monsters = GetRoomMonsters(roomCode, map);
            var playersInMap = playerGroup.ToList();

            foreach (var monster in monsters)
            {
                var target = playersInMap
                    .OrderBy(p => GetDistance(monster.X, monster.Y, p.X, p.Y))
                    .FirstOrDefault();

                if (target == null)
                    continue;

                double distance = GetDistance(monster.X, monster.Y, target.X, target.Y);

                if (distance <= 70)
                {
                    if ((DateTime.Now - monster.LastAttackTime).TotalMilliseconds >= 1000)
                    {
                        int damage = Math.Max(1, monster.AttackDamage - target.Defense);
                        target.Hp -= damage;
                        monster.LastAttackTime = DateTime.Now;

                        if (target.Hp <= 0)
                        {
                            target.Hp = target.MaxHp;
                            target.CurrentMap = MapType.Town;
                            target.X = 250;
                            target.Y = 300;
                        }
                    }

                    continue;
                }

                MoveMonsterTowardTarget(monster, target);
            }

            await SendRoomState(roomCode);
            await SendMapState(roomCode, map);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Players.TryGetValue(Context.ConnectionId, out var player))
        {
            Players.Remove(Context.ConnectionId);
            await SendRoomState(player.RoomCode);
            await SendMapState(player.RoomCode, player.CurrentMap);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private static void MoveMonsterTowardTarget(MonsterState monster, PlayerState target)
    {
        const int speed = 4;

        int dx = 0;
        int dy = 0;

        if (monster.X < target.X) dx = speed;
        else if (monster.X > target.X) dx = -speed;

        if (monster.Y < target.Y) dy = speed;
        else if (monster.Y > target.Y) dy = -speed;

        bool moved = false;

        if (!IsBlockedTile(monster.X + dx, monster.Y + dy))
        {
            monster.X += dx;
            monster.Y += dy;
            moved = true;
        }

        if (!moved && dx != 0 && !IsBlockedTile(monster.X + dx, monster.Y))
        {
            monster.X += dx;
            moved = true;
        }

        if (!moved && dy != 0 && !IsBlockedTile(monster.X, monster.Y + dy))
        {
            monster.Y += dy;
            moved = true;
        }

        if (!moved)
        {
            var sideMoves = new List<(int x, int y)>
            {
                (monster.X + speed, monster.Y),
                (monster.X - speed, monster.Y),
                (monster.X, monster.Y + speed),
                (monster.X, monster.Y - speed)
            };

            var possibleMove = sideMoves
                .Where(p => !IsBlockedTile(p.x, p.y))
                .OrderBy(p => GetDistance(p.x, p.y, target.X, target.Y))
                .FirstOrDefault();

            if (possibleMove != default)
            {
                monster.X = possibleMove.x;
                monster.Y = possibleMove.y;
            }
        }
    }

    private static void UpdateQuestProgress(PlayerState player, MonsterType killedType)
    {
        foreach (var playerQuest in player.Quests)
        {
            if (playerQuest.Completed)
                continue;

            var quest = QuestDefinitions.FirstOrDefault(q => q.QuestId == playerQuest.QuestId);
            if (quest == null || quest.TargetMonsterType != killedType)
                continue;

            playerQuest.CurrentCount++;

            if (playerQuest.CurrentCount >= quest.RequiredCount)
            {
                playerQuest.CurrentCount = quest.RequiredCount;
                playerQuest.Completed = true;

                if (quest.QuestId == "merchant_slime")
                    player.QuestCompleted = true;
            }
        }
    }

    private static void KillMonster(PlayerState player, List<MonsterState> monsters, MonsterState monster)
    {
        monsters.Remove(monster);

        AddExp(player, monster.ExpReward);
        player.Gold += monster.GoldReward;

        if (monster.Type == MonsterType.Slime) player.SlimeKillCount++;
        else if (monster.Type == MonsterType.Bat) player.BatKillCount++;
        else if (monster.Type == MonsterType.Goblin) player.GoblinKillCount++;
        else if (monster.Type == MonsterType.Boss) player.BossKillCount++;

        UpdateQuestProgress(player, monster.Type);
        CreateDrop(player.RoomCode, player.CurrentMap, monster.X, monster.Y, monster.Type);

        monsters.Add(CreateMonster(player.Level, player.CurrentMap));
    }

    private static void AddExp(PlayerState player, int exp)
    {
        player.Exp += exp;

        while (player.Exp >= player.Level * 30)
        {
            player.Exp -= player.Level * 30;
            player.Level++;
            player.MaxHp += 20;
            player.Hp = player.MaxHp;
            player.StatPoint += 3;

            // 전직 이후부터 레벨업마다 스킬포인트 +1
            if (player.IsJobChanged)
                player.SkillPoint += 1;
        }
    }

    private static void CreateDrop(string roomCode, MapType map, int x, int y, MonsterType monsterType)
    {
        var drops = GetRoomDrops(roomCode, map);
        int roll = Random.Shared.Next(1, 101);

        if (monsterType == MonsterType.Boss)
        {
            if (roll <= 40)
                drops.Add(new DroppedItem { Name = "보스의 대검", ItemType = "Weapon", AttackBonus = 35, Rarity = "Epic", X = x, Y = y });
            else if (roll <= 75)
                drops.Add(new DroppedItem { Name = "보스의 갑옷", ItemType = "Armor", DefenseBonus = 20, Rarity = "Epic", X = x, Y = y });
            else
                drops.Add(new DroppedItem { Name = "Gold", GoldAmount = Random.Shared.Next(100, 201), X = x, Y = y, Rarity = "Epic" });

            return;
        }

        if (monsterType == MonsterType.Goblin)
        {
            if (roll <= 25)
                drops.Add(new DroppedItem { Name = "고블린 검", ItemType = "Weapon", AttackBonus = 12, Rarity = "Rare", X = x, Y = y });
            else if (roll <= 45)
                drops.Add(new DroppedItem { Name = "고블린 갑옷", ItemType = "Armor", DefenseBonus = 8, Rarity = "Rare", X = x, Y = y });
            else if (roll <= 70)
                drops.Add(new DroppedItem { Name = "작은 포션", ItemType = "Consumable", X = x, Y = y });
            else
                drops.Add(new DroppedItem { Name = "Gold", GoldAmount = Random.Shared.Next(20, 51), X = x, Y = y });

            return;
        }

        if (roll <= 25)
            drops.Add(new DroppedItem { Name = "작은 포션", ItemType = "Consumable", X = x, Y = y });
        else if (roll <= 45)
            drops.Add(new DroppedItem { Name = monsterType == MonsterType.Bat ? "박쥐 날개" : "슬라임 젤리", ItemType = "Material", X = x, Y = y });
        else if (roll <= 65)
            drops.Add(new DroppedItem { Name = "낡은 검", ItemType = "Weapon", AttackBonus = 5, Rarity = "Normal", X = x, Y = y });
        else if (roll <= 80)
            drops.Add(new DroppedItem { Name = "가죽 갑옷", ItemType = "Armor", DefenseBonus = 4, Rarity = "Normal", X = x, Y = y });
        else
            drops.Add(new DroppedItem { Name = "Gold", GoldAmount = Random.Shared.Next(10, 31), X = x, Y = y });
    }

    private static void AddItem(PlayerState player, string itemName, int count, string itemType = "Material", int attackBonus = 0, int defenseBonus = 0, string rarity = "Normal")
    {
        var item = player.Inventory.FirstOrDefault(i =>
            i.Name == itemName &&
            i.ItemType == itemType &&
            i.AttackBonus == attackBonus &&
            i.DefenseBonus == defenseBonus &&
            i.Rarity == rarity);

        if (item == null)
        {
            player.Inventory.Add(new InventoryItem
            {
                Name = itemName,
                Count = count,
                ItemType = itemType,
                AttackBonus = attackBonus,
                DefenseBonus = defenseBonus,
                Rarity = rarity
            });
        }
        else
        {
            item.Count += count;
        }
    }

    private static void EnsureMap(string roomCode, MapType map, int playerLevel)
    {
        var monsterKey = MapKey(roomCode, map);
        var dropKey = MapKey(roomCode, map);

        if (!RoomDrops.ContainsKey(dropKey))
            RoomDrops[dropKey] = new List<DroppedItem>();

        if (!RoomMonsters.ContainsKey(monsterKey))
        {
            RoomMonsters[monsterKey] = new List<MonsterState>();

            if (map == MapType.Field)
            {
                RoomMonsters[monsterKey].Add(CreateMonster(playerLevel, map));
                RoomMonsters[monsterKey].Add(CreateMonster(playerLevel, map));
            }
            else if (map == MapType.BossRoom)
            {
                RoomMonsters[monsterKey].Add(CreateMonster(playerLevel, map));
            }
        }
    }

    private static MonsterState CreateMonster(int playerLevel, MapType map)
    {
        if (map == MapType.BossRoom)
        {
            return new MonsterState
            {
                Type = MonsterType.Boss,
                Name = "보스 몬스터",
                X = 500,
                Y = 300,
                MaxHp = 250,
                Hp = 250,
                AttackDamage = 18,
                ExpReward = 100,
                GoldReward = 100
            };
        }

        var spawnPoints = new List<(int x, int y)>
        {
            (850, 250), (850, 350), (700, 500), (300, 500), (200, 350)
        };

        var point = spawnPoints[Random.Shared.Next(spawnPoints.Count)];
        int roll = Random.Shared.Next(1, 101);

        if (playerLevel >= 3 && roll <= 40)
        {
            return new MonsterState
            {
                Type = MonsterType.Goblin,
                Name = "고블린",
                X = point.x,
                Y = point.y,
                MaxHp = 80,
                Hp = 80,
                AttackDamage = 10,
                ExpReward = 30,
                GoldReward = 25
            };
        }

        if (playerLevel >= 2 && roll <= 65)
        {
            return new MonsterState
            {
                Type = MonsterType.Bat,
                Name = "박쥐",
                X = point.x,
                Y = point.y,
                MaxHp = 45,
                Hp = 45,
                AttackDamage = 7,
                ExpReward = 18,
                GoldReward = 15
            };
        }

        return new MonsterState
        {
            Type = MonsterType.Slime,
            Name = "슬라임",
            X = point.x,
            Y = point.y,
            MaxHp = 35,
            Hp = 35,
            AttackDamage = 5,
            ExpReward = 10,
            GoldReward = 10
        };
    }

    private async Task SendRoomState(string roomCode)
    {
        await Clients.Group(roomCode).SendAsync("UpdatePlayers", Players.Values.Where(p => p.RoomCode == roomCode).ToList());
    }

    private async Task SendMapState(string roomCode, MapType map)
    {
        await Clients.Group(MapKey(roomCode, map)).SendAsync("UpdateMonsters", GetRoomMonsters(roomCode, map));
        await Clients.Group(MapKey(roomCode, map)).SendAsync("UpdateDrops", GetRoomDrops(roomCode, map));
    }

    private static List<MonsterState> GetRoomMonsters(string roomCode, MapType map)
    {
        var key = MapKey(roomCode, map);
        if (!RoomMonsters.ContainsKey(key))
            RoomMonsters[key] = new List<MonsterState>();
        return RoomMonsters[key];
    }

    private static List<DroppedItem> GetRoomDrops(string roomCode, MapType map)
    {
        var key = MapKey(roomCode, map);
        if (!RoomDrops.ContainsKey(key))
            RoomDrops[key] = new List<DroppedItem>();
        return RoomDrops[key];
    }

    private static double GetDistance(int x1, int y1, int x2, int y2)
    {
        return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
    }

    private static bool IsBlockedTile(int x, int y)
    {
        if (x < 24 || x > 976 || y < 24 || y > 626)
            return true;

        int tileX = x / 50;
        int tileY = y / 50;

        if (tileX <= 0 || tileX >= 19 || tileY <= 0 || tileY >= 12)
            return true;

        if (tileY == 2 && tileX >= 4 && tileX <= 8)
            return true;

        if (tileY == 8 && tileX >= 12 && tileX <= 15)
            return true;

        return false;
    }
}
