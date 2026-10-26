using System;
using System.Collections.Generic;

namespace HarvestIsle.Core
{
    // Land reclamation, the hotpot shop economy and hired workers.
    // Pure domain rules: no Unity, fully covered by Tools/Tests.
    public sealed partial class FarmModel
    {
        // ----- Land parcels: the field grows south, one 6-plot row per parcel. -----
        public int ParcelsUnlocked { get { return (State.plots.Count - PlotCount) / Width; } }
        public static int ParcelCost(int parcel) { return parcel == 0 ? 100 : 280; }
        public static int ParcelLevel(int parcel) { return parcel == 0 ? 3 : 5; }
        public static int ParcelHarvestGoal(int parcel) { return parcel == 0 ? 10 : 30; }

        // Permanent reclamation missions use existing lifetime counters, including old saves.
        public bool ParcelMissionComplete(int parcel)
        {
            return parcel >= 0 && parcel < 2 && State.harvested >= ParcelHarvestGoal(parcel)
                && (parcel == 0 ? State.hasSold : State.development.ordersCompleted >= 5);
        }

        public string ParcelMissionProgress(int parcel)
        {
            if (parcel < 0 || parcel >= 2) return "Khu đất không hợp lệ.";
            int goal = ParcelHarvestGoal(parcel);
            return "Thu hoạch " + Math.Min(State.harvested, goal) + "/" + goal + " vụ · "
                + (parcel == 0 ? "Bán nông sản " + (State.hasSold ? "1/1" : "0/1") + " lần"
                : "Giao đơn hàng " + Math.Min(State.development.ordersCompleted, 5) + "/5");
        }

        public string ParcelUnlockBlocker(int parcel)
        {
            if (parcel < 0 || parcel >= 2) return "Khu đất không hợp lệ.";
            if (parcel < ParcelsUnlocked) return "Khu đất đã được khai hoang.";
            if (parcel > ParcelsUnlocked) return "Hãy khai hoang khu đất liền trước để mở khu này.";
            if (Level < ParcelLevel(parcel)) return "Cần đạt cấp " + ParcelLevel(parcel) + " để khai hoang khu đất này.";
            if (!ParcelMissionComplete(parcel)) return "Chưa hoàn thành nhiệm vụ: " + ParcelMissionProgress(parcel) + ".";
            if (State.coins < ParcelCost(parcel)) return "Cần thêm " + Money(ParcelCost(parcel) - State.coins) + " để khai hoang.";
            return "";
        }

        public bool CanUnlockParcel(int parcel) { return ParcelUnlockBlocker(parcel).Length == 0; }

        public ActionResult UnlockParcel()
        {
            if (State.plots.Count >= MaxPlots) return Fail("Đã khai hoang toàn bộ đất có thể mở.");
            string blocker = ParcelUnlockBlocker(ParcelsUnlocked);
            if (blocker.Length > 0) return Fail(blocker);
            int cost = ParcelCost(ParcelsUnlocked);
            State.coins -= cost;
            for (int i = 0; i < Width; i++) State.plots.Add(new PlotState());
            GainExperience(10);
            return Ok("Đã khai hoang 6 ô ruộng mới · tổng cộng " + State.plots.Count + " ô!");
        }

        // ----- Perennials: fruit trees stay planted and refruit; chopping frees the plot. -----
        public static bool IsPerennial(string id)
        { return IslandCropCatalog.Tree(id)||id == "mango" || id == "coconut" || id == "orange" || id == "lemon" || id == "banana" || id == "papaya"; }

        public ActionResult ChopPerennial(int index)
        {
            if (index < 0 || index >= State.plots.Count) return Fail("Ô đất không hợp lệ.");
            PlotState plot = State.plots[index];
            if (!IsPerennial(plot.cropId)) return Fail("Ô này không có cây lâu năm.");
            string name = CropCatalog.Find(plot.cropId).displayName;
            Island.pruning.RemoveAll(p=>p.plot==index);
            plot.secondPreviousCropId = plot.previousCropId;
            plot.previousCropId = plot.cropId;
            plot.cropId = ""; plot.plantedAt = 0; plot.watered = false; plot.harvests = 0;
            return Ok("Đã chặt cây " + name + " · ô đất sẵn sàng cho vụ mới.");
        }

        // ----- Hotpot shop: broth plus farm produce, served for a cooking margin. -----
        public static readonly string[] HotpotBroths = { "Lẩu gà lá chanh", "Lẩu chua cay", "Lẩu nấm chay" };

        // Fame tiers (My Hotpot Story-style rank): more pots served means a busier,
        // pricier shop. Tier 1 at 15 pots, tier 2 at 40; each adds a dining table
        // and +10% on every bill.
        public int HotpotTier { get { int served=State.hotpot!=null&&State.hotpot.progress!=null&&State.hotpot.progress.branch>0?State.hotpot.progress.branchServed:State.hotpotServed;return served >= 40 ? 2 : served >= 15 ? 1 : 0; } }
        public int HotpotTables { get { return 4 + HotpotTier; } }

        public int HotpotPrice(IList<string> items)
        {
            int total = 0;
            foreach (string id in items) { CropSpec crop = CropCatalog.Find(id); if (crop != null) total += crop.sellPrice; }
            return (int)Math.Ceiling(total * 1.75 * (1 + 0.1 * HotpotTier)) + 2;
        }

        public ActionResult ServeHotpot(IList<string> items)
        {
            return Fail("Hãy nhập kho bếp và nấu thử công thức trước khi phục vụ.");
        }

        // ----- Workers: the worker house caps crew size and sets new hires' level. -----
        public static readonly string[] WorkerKinds = { "Tạp vụ", "Thợ trồng trọt", "Thợ chăn nuôi" };
        public static int WorkerHireCost(int kind) { return 40 + kind * 25; }
        public static int WorkerHouseCost(int level) { return level == 1 ? 60 : level == 2 ? 150 : 300; }
        public static int WorkerHouseWood(int level) { return level * 6; }
        public static int WorkerHouseOre(int level) { return (level - 1) * 3; }

        public ActionResult UpgradeWorkerHouse()
        {
            if (State.workerHouse >= 3) return Fail("Nhà nhân công đã đạt cấp tối đa.");
            int next = State.workerHouse + 1;
            int cost = WorkerHouseCost(next), wood = WorkerHouseWood(next), ore = WorkerHouseOre(next);
            if (State.coins < cost || State.development.wood < wood || State.development.ore < ore)
                return Fail("Cần " + Money(cost) + ", " + wood + " gỗ và " + ore + " quặng.");
            State.coins -= cost; State.development.wood -= wood; State.development.ore -= ore;
            State.workerHouse = next;
            GainExperience(10);
            return Ok("Nhà nhân công cấp " + next + " · " + CrewCapacity + " chỗ làm việc. Nhân công phổ thông mới thuê đạt cấp " + next + ".");
        }

        public ActionResult HireWorker(int kind)
        {
            if (kind < 0 || kind > 2) return Fail("Loại nhân công không hợp lệ.");
            if (State.workerHouse < 1) return Fail("Xây Nhà nhân công trước khi thuê người.");
            if (State.workers.Count >= State.workerHouse || AssignedCrew >= CrewCapacity)
                return Fail("Nhà đã đủ người (" + State.workers.Count + "/" + State.workerHouse + "). Nâng cấp để thuê thêm.");
            int cost = WorkerHireCost(kind);
            if (State.coins < cost) return Fail("Cần " + Money(cost) + " tiền cọc thuê " + WorkerKinds[kind] + ".");
            State.coins -= cost;
            State.workers.Add(new WorkerState { kind = kind, level = State.workerHouse });
            return Ok("Đã thuê " + WorkerKinds[kind] + " cấp " + State.workerHouse + ". Họ bắt đầu làm ngay!");
        }

        // ----- Pest catching: a free skill minigame clears infestations. -----
        public ActionResult CatchPests(int index, int caught)
        {
            if (index < 0 || index >= State.plots.Count || caught < 0 || caught > 3) return Fail("Kết quả bắt sâu không hợp lệ.");
            PlotState plot = State.plots[index];
            if (plot.pest < 1) return Fail("Ô này không có sâu.");
            if (caught < 2) return Fail("Sâu chạy mất (" + caught + "/3 con). Thử lại nhé!");
            plot.pest = 0;
            if (caught == 3) plot.treatmentProtection = Math.Max(plot.treatmentProtection, 30);
            GainExperience(4);
            return Ok(caught == 3 ? "Bắt sạch sâu! Cây được bảo vệ 30 giây." : "Đã đuổi sâu khỏi ô · hết phạt sản lượng.");
        }

        // ----- Achievements: milestones over existing counters, claimed once via a bitmask. -----
        public sealed class AchievementSpec
        {
            public readonly string name; public readonly int goal, reward;
            public AchievementSpec(string name, int goal, int reward) { this.name = name; this.goal = goal; this.reward = reward; }
        }
        public static readonly AchievementSpec[] Achievements = {
            new AchievementSpec("Nông dân tập sự · thu 10 vụ", 10, 50),
            new AchievementSpec("Vua mùa vàng · thu 100 vụ", 100, 200),
            new AchievementSpec("Thương vụ đầu tiên · bán hàng cho Hải Âu", 1, 20),
            new AchievementSpec("Đầu bếp lẩu · phục vụ 5 nồi", 5, 60),
            new AchievementSpec("Quán lẩu trứ danh · phục vụ 40 nồi", 40, 150),
            new AchievementSpec("Khai hoang · mở vùng đất đầu tiên", 1, 80),
            new AchievementSpec("Điền chủ · mở toàn bộ 36 ô", 2, 200),
            new AchievementSpec("Ông chủ nhỏ · thuê nhân công đầu tiên", 1, 60),
            new AchievementSpec("Xưởng bận rộn · 10 thành phẩm chế biến", 10, 100),
            new AchievementSpec("Bạn của Hải Âu · giao 10 đơn hàng", 10, 120)
        };
        public int AchievementProgress(int index)
        {
            switch (index)
            {
                case 0: case 1: return State.harvested;
                case 2: return State.hasSold ? 1 : 0;
                case 3: case 4: return State.hotpotServed;
                case 5: case 6: return ParcelsUnlocked;
                case 7: return State.workers.Count;
                case 8: return State.development.crafted;
                default: return State.development.ordersCompleted;
            }
        }
        public bool AchievementClaimed(int index) { return (State.achievementsClaimed & 1 << index) != 0; }
        public ActionResult ClaimAchievement(int index)
        {
            if (index < 0 || index >= Achievements.Length) return Fail("Thành tích không hợp lệ.");
            AchievementSpec spec = Achievements[index];
            if (AchievementClaimed(index)) return Fail("Đã nhận thưởng thành tích này rồi.");
            if (AchievementProgress(index) < spec.goal) return Fail("Chưa đạt: " + AchievementProgress(index) + "/" + spec.goal + ".");
            if ((long)State.coins + spec.reward > 1000000000) return Fail("Kho tiền đã đầy.");
            State.achievementsClaimed |= 1 << index;
            State.coins += spec.reward;
            GainExperience(15);
            return Ok("Thành tích \"" + spec.name + "\" · +" + Money(spec.reward) + " và 15 XP!");
        }

        // ----- Weekly quests: three rotating goals seeded by the UTC week index. -----
        public void EnsureQuests(double now)
        {
            int week = (int)(now / 604800);
            if (week < 0 || State.questWeek == week && State.quests.Count == 3) return;
            State.questWeek = week;
            State.quests.Clear();
            for (int i = 0; i < 3; i++)
            {
                int kind = (week * 7 + i * 13 + i) % 4;
                int step = (week + i) % 3;
                int target = kind == 0 ? 12 + step * 6 : kind == 1 ? 4 + step * 3 : kind == 2 ? 5 + step * 4 : 3 + step * 2;
                State.quests.Add(new QuestState { kind = kind, target = target, baseline = QuestCounter(kind), claimed = false });
            }
        }
        private int QuestCounter(int kind)
        {
            return kind == 0 ? State.harvested : kind == 1 ? State.hotpotServed
                : kind == 2 ? State.development.crafted : State.development.ordersCompleted;
        }
        public static int QuestReward(QuestState quest)
        { return quest.target * (quest.kind == 0 ? 6 : quest.kind == 1 ? 25 : quest.kind == 2 ? 18 : 30); }
        public static string QuestName(QuestState quest)
        {
            return quest.kind == 0 ? "Thu hoạch " + quest.target + " vụ" : quest.kind == 1 ? "Phục vụ " + quest.target + " nồi lẩu"
                : quest.kind == 2 ? "Chế " + quest.target + " thành phẩm" : "Giao " + quest.target + " đơn hàng";
        }
        public int QuestProgress(int index)
        {
            if (index < 0 || index >= State.quests.Count) return 0;
            QuestState quest = State.quests[index];
            return Math.Max(0, Math.Min(quest.target, QuestCounter(quest.kind) - quest.baseline));
        }
        public ActionResult ClaimQuest(int index)
        {
            if (index < 0 || index >= State.quests.Count) return Fail("Nhiệm vụ không hợp lệ.");
            QuestState quest = State.quests[index];
            if (quest.claimed) return Fail("Nhiệm vụ này đã nhận thưởng trong tuần.");
            if (QuestProgress(index) < quest.target) return Fail("Chưa xong: " + QuestProgress(index) + "/" + quest.target + ".");
            int reward = QuestReward(quest);
            if ((long)State.coins + reward > 1000000000) return Fail("Kho tiền đã đầy.");
            quest.claimed = true;
            State.coins += reward;
            GainExperience(20);
            return Ok("Hoàn thành \"" + QuestName(quest) + "\" · +" + Money(reward) + " và 20 XP!");
        }

        // ----- Firefly Festival: a recurring boom window (2 of every 10 minutes). -----
        public static bool FestivalActive(double now)
        { return now > 0 && (long)(now / 120) % 5 == 4; }
        public static double FestivalRemaining(double now)
        { return FestivalActive(now) ? 120 - now % 120 : 0; }

        // One pass: each worker performs at most one action it is qualified for.
        // Level 1 only handles small chores; higher levels take on the skilled work.
        public int RunWorkers(double now)
        {
            now = ObserveTime(now);
            LastWorkerActions.Clear();
            int done = RunSpecialists(now);
            foreach (WorkerState worker in State.workers)
            {
                if (worker.kind <= 1)
                {
                    int thirsty = -1, mature = -1;
                    for (int i = 0; i < State.plots.Count; i++)
                    {
                        PlotState plot = State.plots[i];
                        if (string.IsNullOrEmpty(plot.cropId)) continue;
                        if (thirsty < 0 && !plot.watered && Progress(i, now) < 1) thirsty = i;
                        if (mature < 0 && Progress(i, now) >= 1) mature = i;
                    }
                    if (worker.kind == 1 && worker.level >= 2 && mature >= 0)
                    { if (Harvest(mature, 1, now).success) { done++; continue; } }
                    if (thirsty >= 0)
                    {
                        PlotState plot = State.plots[thirsty];
                        plot.watered = true; plot.soilWater = Math.Max(plot.soilWater, .25);
                        State.hasWatered = true; done++;
                    }
                }
                else
                {
                    foreach (string id in new[] { "chicken", "cow", "duck", "pig" })
                    {
                        LivestockState animal = Animal(id);
                        if (animal.producing && AnimalRemaining(id, now) <= 0 && CollectAnimal(id, now).success) { done++; break; }
                        if (!animal.producing && animal.count > 0 && worker.level >= 2 && Count("corn") > 0 &&
                            FeedAnimal(id, true, now).success) { done++; break; }
                    }
                }
            }
            return done;
        }
    }
}
