using Microsoft.AspNetCore.SignalR;
using MyBlazorApp.Models;

namespace MyBlazorApp.Hubs;

// 서버에서 플레이어, 몬스터, 드랍, 퀘스트, 상점, 전직, 스킬을 관리하는 Hub
public class GameHub : Hub
{
    private static readonly Dictionary<string, PlayerState> Players = new();
    private static readonly Dictionary<string, List<MonsterState>> RoomMonsters = new();
    private static readonly Dictionary<string, List<DroppedItem>> RoomDrops = new();

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
        }
    };

    private static readonly List<ShopItem> ShopItems = new()
    {
        new ShopItem { ShopType = BuildingType.PotionShop, Name = "작은 포션", ItemType = "Consumable", Price = 20, Rarity = "Normal" },
        new ShopItem { ShopType = BuildingType.PotionShop, Name = "중간 포션", ItemType = "Consumable", Price = 60, Rarity = "Rare" },

        new ShopItem { ShopType = BuildingType.WeaponShop, Name = "낡은 검", ItemType = "Weapon", Price = 100, AttackBonus = 5, Rarity = "Normal" },
        new ShopItem { ShopType = BuildingType.WeaponShop, Name = "초보자 지팡이", ItemType = "Weapon", Price = 150, AttackBonus = 4, Rarity = "Normal" },
        new ShopItem { ShopType = BuildingType.WeaponShop, Name = "초보 활", ItemType = "Weapon", Price = 180, AttackBonus = 7, Rarity = "Normal" },
        new ShopItem { ShopType = BuildingType.WeaponShop, Name = "단검", ItemType = "Weapon", Price = 160, AttackBonus = 6, Rarity = "Normal" },
        new ShopItem { ShopType = BuildingType.WeaponShop, Name = "고블린 검", ItemType = "Weapon", Price = 300, AttackBonus = 12, Rarity = "Rare" },

        new ShopItem { ShopType = BuildingType.ArmorShop, Name = "가죽 갑옷", ItemType = "Armor", Price = 120, DefenseBonus = 4, Rarity = "Normal" },
        new ShopItem { ShopType = BuildingType.ArmorShop, Name = "철 갑옷", ItemType = "Armor", Price = 350, DefenseBonus = 10, Rarity = "Rare" }
    };

    private static string MapKey(string roomCode, MapType map) => $"{roomCode}_{map}";

    // 게임 입장
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
            Gold = 100
        };

        EnsureMap(roomCode, MapType.Town, 1);
        EnsureMap(roomCode, MapType.Field, 1);
        EnsureMap(roomCode, MapType.BossRoom, 1);

        await SendRoomState(roomCode);
        await SendMapState(roomCode, MapType.Town);
    }

    // 맵 이동
    public async Task ChangeMap(MapType targetMap, int targetX, int targetY)
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player)) return;

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

    // 플레이어 위치 동기화
    public async Task MovePlayer(int x, int y)
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player)) return;
        player.X = x;
        player.Y = y;
        await SendRoomState(player.RoomCode);
    }

    // 전직. NPC를 통해 Game.razor에서 호출함
    public async Task ChangeJob(JobType job)
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player)) return;
        if (player.Job != JobType.Beginner) return;

        if (job == JobType.Mage && player.Level < 8) return;
        if ((job == JobType.Warrior || job == JobType.Thief || job == JobType.Archer) && player.Level < 10) return;
        if (job != JobType.Mage && job != JobType.Warrior && job != JobType.Thief && job != JobType.Archer) return;

        player.Job = job;
        player.SkillPoint += 1;
        await SendRoomState(player.RoomCode);
    }

    // 스킬 배우기/레벨업
    public async Task LearnOrLevelUpSkill(string skillId)
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player)) return;
        if (!player.IsJobChanged) return;
        if (player.SkillPoint <= 0) return;

        var skill = SkillDatabase.GetSkill(skillId);
        if (skill == null) return;
        if (skill.RequiredJob != player.Job) return;
        if (player.Level < skill.RequiredLevel) return;

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
            if (learned.Level >= skill.MaxLevel) return;
            learned.Level++;
            player.SkillPoint--;
        }

        await SendRoomState(player.RoomCode);
    }

    // 배운 스킬만 퀵슬롯 등록 가능
    public async Task SetSkillSlot(int slot, string skillId)
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player)) return;
        if (slot < 1 || slot > 9) return;
        if (!player.LearnedSkills.Any(s => s.SkillId == skillId)) return;

        var targetSlot = player.SkillSlots.FirstOrDefault(s => s.Slot == slot);
        if (targetSlot == null)
        {
            targetSlot = new SkillSlot { Slot = slot };
            player.SkillSlots.Add(targetSlot);
        }

        targetSlot.SkillId = skillId;
        await SendRoomState(player.RoomCode);
    }

    // ===============================
    // 퀵슬롯 해제
    // 스킬창에서 X 버튼을 누르면 해당 슬롯을 비움
    // ===============================
    public async Task RemoveSkillSlot(int slot)
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player)) return;
        if (slot < 1 || slot > 9) return;

        var targetSlot = player.SkillSlots.FirstOrDefault(s => s.Slot == slot);

        // 슬롯 객체가 없으면 새로 만들어 빈 슬롯으로 유지
        if (targetSlot == null)
        {
            targetSlot = new SkillSlot { Slot = slot, SkillId = "" };
            player.SkillSlots.Add(targetSlot);
        }
        else
        {
            targetSlot.SkillId = "";
        }

        await SendRoomState(player.RoomCode);
    }

    // 슬롯 번호로 스킬 사용
    public async Task UseSkillFromSlot(int slot)
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player)) return;

        var slotInfo = player.SkillSlots.FirstOrDefault(s => s.Slot == slot);
        if (slotInfo == null || string.IsNullOrWhiteSpace(slotInfo.SkillId)) return;

        var learned = player.LearnedSkills.FirstOrDefault(s => s.SkillId == slotInfo.SkillId);
        if (learned == null) return;

        var skill = SkillDatabase.GetSkill(slotInfo.SkillId);
        if (skill == null) return;

        if (skill.SkillId == "mage_heal")
        {
            int healAmount = player.HealPower + 20 + learned.Level * 12;
            player.Hp = Math.Min(player.MaxHp, player.Hp + healAmount);
        }
        else
        {
            UseDamageSkill(player, skill, learned.Level);
        }

        await SendRoomState(player.RoomCode);
        await SendMapState(player.RoomCode, player.CurrentMap);
    }

    // 기본 공격
    public async Task AttackMonster()
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player)) return;

        var monsters = GetRoomMonsters(player.RoomCode, player.CurrentMap);
        var target = monsters.OrderBy(m => GetDistance(player.X, player.Y, m.X, m.Y)).FirstOrDefault();
        if (target == null) return;
        if (GetDistance(player.X, player.Y, target.X, target.Y) > 80) return;

        int damage = GetBasicAttackDamage(player);
        target.Hp -= damage;
        if (target.Hp <= 0) KillMonster(player, monsters, target);

        await SendRoomState(player.RoomCode);
        await SendMapState(player.RoomCode, player.CurrentMap);
    }

    // 캐릭터 외형 변경
    // 캐릭터 외형 변경
    // 얼굴 이모지는 사용하지 않고 CSS 픽셀 얼굴로 렌더링합니다.
    public async Task UpdateCharacterAppearance(
        string avatarPreset,
        string skinColor,
        string hairColor,
        string outfitColor,
        string hairStyle,
        string outfitStyle,
        string accessory)
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player)) return;

        var allowedAvatar = new[] { "beginner_01", "beginner_02", "warrior_01", "warrior_02", "mage_01", "mage_02", "thief_01", "thief_02", "archer_01", "archer_02" };
        var allowedSkin = new[] { "#f2c7a5", "#d8a47f", "#8d5524", "#ffe0bd", "#b7795f" };
        var allowedHair = new[] { "#2b1b12", "#7c2d12", "#facc15", "#111827", "#f8fafc", "#78350f", "#991b1b" };
        var allowedOutfit = new[] { "#2563eb", "#16a34a", "#dc2626", "#7c3aed", "#f97316", "#0f172a", "#92400e", "#0891b2" };
        var allowedHairStyle = new[] { "short", "long", "spiky", "bob", "ponytail", "hood" };
        var allowedOutfitStyle = new[] { "adventurer", "armor", "robe", "leather", "ranger" };
        var allowedAccessory = new[] { "none", "cape", "pauldron", "hood", "scarf" };

        if (allowedAvatar.Contains(avatarPreset)) player.AvatarPreset = avatarPreset;
        if (allowedSkin.Contains(skinColor)) player.SkinColor = skinColor;
        if (allowedHair.Contains(hairColor)) player.HairColor = hairColor;
        if (allowedOutfit.Contains(outfitColor)) player.OutfitColor = outfitColor;
        if (allowedHairStyle.Contains(hairStyle)) player.HairStyle = hairStyle;
        if (allowedOutfitStyle.Contains(outfitStyle)) player.OutfitStyle = outfitStyle;
        if (allowedAccessory.Contains(accessory)) player.Accessory = accessory;

        await SendRoomState(player.RoomCode);
    }

    // 상점 구매
    public async Task BuyShopItem(string itemName)
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player)) return;

        var shopItem = ShopItems.FirstOrDefault(i => i.Name == itemName);
        if (shopItem == null) return;
        if (player.Gold < shopItem.Price) return;

        player.Gold -= shopItem.Price;
        AddItem(player, shopItem.Name, 1, shopItem.ItemType, shopItem.AttackBonus, shopItem.DefenseBonus, shopItem.Rarity, shopItem.Price);

        await SendRoomState(player.RoomCode);
    }

    // 인벤토리 아이템 판매. 판매가는 구매가의 50% 기준
    public async Task SellInventoryItem(string itemName)
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player)) return;

        var item = player.Inventory.FirstOrDefault(i => i.Name == itemName);
        if (item == null || item.Count <= 0) return;

        // 장착 중인 장비는 판매 방지
        if (player.Weapon?.Name == item.Name || player.Armor?.Name == item.Name) return;

        int sellPrice = item.SellPrice;
        player.Gold += sellPrice;

        item.Count--;
        if (item.Count <= 0) player.Inventory.Remove(item);

        await SendRoomState(player.RoomCode);
    }

    // 포션 사용
    public async Task UseInventoryItem(string itemName)
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player)) return;

        var item = player.Inventory.FirstOrDefault(i => i.Name == itemName);
        if (item == null || item.Count <= 0) return;

        if (item.ItemType == "Consumable")
        {
            if (player.Hp >= player.MaxHp) return;

            int heal = item.Name == "중간 포션" ? 100 : 40;
            player.Hp = Math.Min(player.MaxHp, player.Hp + heal);
            item.Count--;
            if (item.Count <= 0) player.Inventory.Remove(item);
        }

        await SendRoomState(player.RoomCode);
    }

    // 장비 장착
    public async Task EquipItem(string itemName)
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player)) return;

        var item = player.Inventory.FirstOrDefault(i => i.Name == itemName);
        if (item == null) return;

        if (item.ItemType == "Weapon")
        {
            player.Weapon = new EquipmentItem
            {
                Name = item.Name,
                Type = "Weapon",
                AttackBonus = item.AttackBonus,
                DefenseBonus = item.DefenseBonus,
                Rarity = item.Rarity,
                BuyPrice = item.BuyPrice
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
                Rarity = item.Rarity,
                BuyPrice = item.BuyPrice
            };
        }

        await SendRoomState(player.RoomCode);
    }

    // 장비 해제
    public async Task UnequipItem(string equipmentType)
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player)) return;
        if (equipmentType == "Weapon") player.Weapon = null;
        else if (equipmentType == "Armor") player.Armor = null;
        await SendRoomState(player.RoomCode);
    }

    // 스탯 찍기
    public async Task AddStat(string statName)
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player)) return;
        if (player.StatPoint <= 0) return;

        if (statName == "STR") player.Str++;
        else if (statName == "DEF") player.Def++;
        else if (statName == "INT") player.Int++;
        else return;

        player.StatPoint--;
        await SendRoomState(player.RoomCode);
    }

    // 퀘스트 받기
    public async Task AcceptQuest(string questId)
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player)) return;
        var quest = QuestDefinitions.FirstOrDefault(q => q.QuestId == questId);
        if (quest == null) return;
        if (player.Quests.Any(q => q.QuestId == questId)) return;

        player.Quests.Add(new PlayerQuestState { QuestId = questId });
        await SendRoomState(player.RoomCode);
    }

    // 퀘스트 보상
    public async Task ReceiveQuestReward(string questId)
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player)) return;

        var quest = QuestDefinitions.FirstOrDefault(q => q.QuestId == questId);
        var playerQuest = player.Quests.FirstOrDefault(q => q.QuestId == questId);
        if (quest == null || playerQuest == null) return;
        if (!playerQuest.Completed || playerQuest.RewardReceived) return;

        player.Gold += quest.RewardGold;
        AddExp(player, quest.RewardExp);
        if (!string.IsNullOrWhiteSpace(quest.RewardItemName))
        {
            AddItem(player, quest.RewardItemName, 1, quest.RewardItemType, quest.RewardAttackBonus, quest.RewardDefenseBonus, quest.RewardRarity, 100);
        }

        playerQuest.RewardReceived = true;
        await SendRoomState(player.RoomCode);
    }

    // 아이템 줍기
    public async Task PickupItem()
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player)) return;

        var drops = GetRoomDrops(player.RoomCode, player.CurrentMap);
        var item = drops.FirstOrDefault(d => GetDistance(player.X, player.Y, d.X, d.Y) <= 80);
        if (item == null) return;

        if (item.IsGold) player.Gold += item.GoldAmount;
        else AddItem(player, item.Name, 1, item.ItemType, item.AttackBonus, item.DefenseBonus, item.Rarity, item.BuyPrice);

        drops.Remove(item);
        await SendRoomState(player.RoomCode);
        await SendMapState(player.RoomCode, player.CurrentMap);
    }

    // 몬스터 AI 갱신
    public async Task UpdateMonsters()
    {
        foreach (var playerGroup in Players.Values.GroupBy(p => new { p.RoomCode, p.CurrentMap }))
        {
            string roomCode = playerGroup.Key.RoomCode;
            MapType map = playerGroup.Key.CurrentMap;
            if (map == MapType.Town) continue;

            var monsters = GetRoomMonsters(roomCode, map);
            var playersInMap = playerGroup.ToList();

            foreach (var monster in monsters)
            {
                var target = playersInMap.OrderBy(p => GetDistance(monster.X, monster.Y, p.X, p.Y)).FirstOrDefault();
                if (target == null) continue;

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

    private static void UseDamageSkill(PlayerState player, SkillDefinition skill, int skillLevel)
    {
        var monsters = GetRoomMonsters(player.RoomCode, player.CurrentMap);
        int targetCount = skill.GetTargetCount(skillLevel);
        int hitCount = skill.GetHitCount(skillLevel);
        int range = skill.GetRange(skillLevel);
        int damage = GetSkillDamage(player, skill, skillLevel);

        var targets = monsters
            .Where(m => GetDistance(player.X, player.Y, m.X, m.Y) <= range)
            .OrderBy(m => GetDistance(player.X, player.Y, m.X, m.Y))
            .Take(targetCount)
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


    private static int GetBasicAttackDamage(PlayerState player)
    {
        // 기존 단순 AttackPower보다 타격감이 느껴지도록 스탯/장비 반영치를 강화합니다.
        int weaponBonus = player.Weapon?.AttackBonus ?? 0;
        int rawDamage = player.BaseAttackPower + player.Str * 3 + weaponBonus * 2;
        int variance = Random.Shared.Next(-2, 4);
        return Math.Max(1, rawDamage + variance);
    }

    private static int GetSkillDamage(PlayerState player, SkillDefinition skill, int skillLevel)
    {
        // 스킬은 기본 공격보다 확실히 강하게 체감되도록 배율을 적용합니다.
        int weaponBonus = player.Weapon?.AttackBonus ?? 0;
        int mainStatBonus = player.Job == JobType.Mage ? player.MagicPower : player.Str * 3;
        int baseDamage = skill.GetDamage(skillLevel);
        int damage = baseDamage + player.BaseAttackPower + weaponBonus * 2 + mainStatBonus;

        if (player.Job == JobType.Mage)
            damage += player.MagicPower;

        int variance = Random.Shared.Next(-3, 6);
        return Math.Max(1, damage + variance);
    }

    private static void MoveMonsterTowardTarget(MonsterState monster, PlayerState target)
    {
        const int speed = 4;
        int dx = monster.X < target.X ? speed : monster.X > target.X ? -speed : 0;
        int dy = monster.Y < target.Y ? speed : monster.Y > target.Y ? -speed : 0;

        bool moved = false;
        if (!IsBlockedTile(monster.X + dx, monster.Y + dy)) { monster.X += dx; monster.Y += dy; moved = true; }
        if (!moved && dx != 0 && !IsBlockedTile(monster.X + dx, monster.Y)) { monster.X += dx; moved = true; }
        if (!moved && dy != 0 && !IsBlockedTile(monster.X, monster.Y + dy)) { monster.Y += dy; moved = true; }

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
            if (playerQuest.Completed) continue;
            var quest = QuestDefinitions.FirstOrDefault(q => q.QuestId == playerQuest.QuestId);
            if (quest == null || quest.TargetMonsterType != killedType) continue;

            playerQuest.CurrentCount++;
            if (playerQuest.CurrentCount >= quest.RequiredCount)
            {
                playerQuest.CurrentCount = quest.RequiredCount;
                playerQuest.Completed = true;
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

            // 전직 후 레벨업마다 스킬포인트 1 지급
            if (player.IsJobChanged) player.SkillPoint += 1;
        }
    }

    private static void CreateDrop(string roomCode, MapType map, int x, int y, MonsterType monsterType)
    {
        var drops = GetRoomDrops(roomCode, map);
        int roll = Random.Shared.Next(1, 101);

        if (monsterType == MonsterType.Boss)
        {
            if (roll <= 40) drops.Add(new DroppedItem { Name = "보스의 대검", ItemType = "Weapon", AttackBonus = 35, Rarity = "Epic", X = x, Y = y, BuyPrice = 800 });
            else if (roll <= 75) drops.Add(new DroppedItem { Name = "보스의 갑옷", ItemType = "Armor", DefenseBonus = 20, Rarity = "Epic", X = x, Y = y, BuyPrice = 800 });
            else drops.Add(new DroppedItem { Name = "Gold", GoldAmount = Random.Shared.Next(100, 201), X = x, Y = y, Rarity = "Epic" });
            return;
        }

        if (monsterType == MonsterType.Goblin)
        {
            if (roll <= 25) drops.Add(new DroppedItem { Name = "고블린 검", ItemType = "Weapon", AttackBonus = 12, Rarity = "Rare", X = x, Y = y, BuyPrice = 300 });
            else if (roll <= 45) drops.Add(new DroppedItem { Name = "고블린 갑옷", ItemType = "Armor", DefenseBonus = 8, Rarity = "Rare", X = x, Y = y, BuyPrice = 300 });
            else if (roll <= 70) drops.Add(new DroppedItem { Name = "작은 포션", ItemType = "Consumable", X = x, Y = y, BuyPrice = 20 });
            else drops.Add(new DroppedItem { Name = "Gold", GoldAmount = Random.Shared.Next(20, 51), X = x, Y = y });
            return;
        }

        if (roll <= 25) drops.Add(new DroppedItem { Name = "작은 포션", ItemType = "Consumable", X = x, Y = y, BuyPrice = 20 });
        else if (roll <= 45) drops.Add(new DroppedItem { Name = monsterType == MonsterType.Bat ? "박쥐 날개" : "슬라임 젤리", ItemType = "Material", X = x, Y = y, BuyPrice = 10 });
        else if (roll <= 65) drops.Add(new DroppedItem { Name = "낡은 검", ItemType = "Weapon", AttackBonus = 5, Rarity = "Normal", X = x, Y = y, BuyPrice = 100 });
        else if (roll <= 80) drops.Add(new DroppedItem { Name = "가죽 갑옷", ItemType = "Armor", DefenseBonus = 4, Rarity = "Normal", X = x, Y = y, BuyPrice = 120 });
        else drops.Add(new DroppedItem { Name = "Gold", GoldAmount = Random.Shared.Next(10, 31), X = x, Y = y });
    }

    private static void AddItem(PlayerState player, string itemName, int count, string itemType = "Material", int attackBonus = 0, int defenseBonus = 0, string rarity = "Normal", int buyPrice = 10)
    {
        var item = player.Inventory.FirstOrDefault(i =>
            i.Name == itemName && i.ItemType == itemType && i.AttackBonus == attackBonus && i.DefenseBonus == defenseBonus && i.Rarity == rarity);

        if (item == null)
        {
            player.Inventory.Add(new InventoryItem
            {
                Name = itemName,
                Count = count,
                ItemType = itemType,
                AttackBonus = attackBonus,
                DefenseBonus = defenseBonus,
                Rarity = rarity,
                BuyPrice = buyPrice
            });
        }
        else
        {
            item.Count += count;
            if (item.BuyPrice <= 0) item.BuyPrice = buyPrice;
        }
    }

    private static void EnsureMap(string roomCode, MapType map, int playerLevel)
    {
        var monsterKey = MapKey(roomCode, map);
        var dropKey = MapKey(roomCode, map);

        if (!RoomDrops.ContainsKey(dropKey)) RoomDrops[dropKey] = new List<DroppedItem>();
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

        var spawnPoints = new List<(int x, int y)> { (850, 250), (850, 350), (700, 500), (300, 500), (200, 350) };
        var point = spawnPoints[Random.Shared.Next(spawnPoints.Count)];
        int roll = Random.Shared.Next(1, 101);

        if (playerLevel >= 3 && roll <= 40)
        {
            return new MonsterState { Type = MonsterType.Goblin, Name = "고블린", X = point.x, Y = point.y, MaxHp = 80, Hp = 80, AttackDamage = 10, ExpReward = 30, GoldReward = 25 };
        }

        if (playerLevel >= 2 && roll <= 65)
        {
            return new MonsterState { Type = MonsterType.Bat, Name = "박쥐", X = point.x, Y = point.y, MaxHp = 45, Hp = 45, AttackDamage = 7, ExpReward = 18, GoldReward = 15 };
        }

        return new MonsterState { Type = MonsterType.Slime, Name = "슬라임", X = point.x, Y = point.y, MaxHp = 35, Hp = 35, AttackDamage = 5, ExpReward = 10, GoldReward = 10 };
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
        if (!RoomMonsters.ContainsKey(key)) RoomMonsters[key] = new List<MonsterState>();
        return RoomMonsters[key];
    }

    private static List<DroppedItem> GetRoomDrops(string roomCode, MapType map)
    {
        var key = MapKey(roomCode, map);
        if (!RoomDrops.ContainsKey(key)) RoomDrops[key] = new List<DroppedItem>();
        return RoomDrops[key];
    }

    private static double GetDistance(int x1, int y1, int x2, int y2)
    {
        return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
    }

    private static bool IsBlockedTile(int x, int y)
    {
        if (x < 24 || x > 976 || y < 24 || y > 626) return true;
        int tileX = x / 50;
        int tileY = y / 50;
        if (tileX <= 0 || tileX >= 19 || tileY <= 0 || tileY >= 12) return true;
        if (tileY == 2 && tileX >= 4 && tileX <= 8) return true;
        if (tileY == 8 && tileX >= 12 && tileX <= 15) return true;
        return false;
    }
}
