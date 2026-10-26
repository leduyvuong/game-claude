using System;
using System.Collections.Generic;

namespace HarvestIsle.Core
{
    public sealed partial class FarmModel
    {
        public int HotpotRank {get{return State.hotpot.progress.rank;}}
        public int KitchenLevel {get{return State.hotpot.progress.kitchenLevel;}}
        public bool HotpotModifier(int id){return Array.IndexOf(HotpotRules.Modifiers(State.hotpot.progress.season),id)>=0;}
        public int EffectiveKitchenLevel {get{return HotpotModifier(3)?Math.Min(4,KitchenLevel):KitchenLevel;}}
        public int HotpotBurners {get{return new[]{1,1,2,4,4,4,4,6}[EffectiveKitchenLevel-1];}}
        public int HotpotRecentTaste
        {get{var list=State.hotpot.progress.recentScores;if(list.Count==0)return 0;int sum=0;foreach(int score in list)sum+=score;return sum/list.Count;}}
        public int HotpotSpace
        {
            get{int points=HotpotTables*3;var kinds=new HashSet<int>();foreach(var item in State.hotpot.furniture)
                points+=(HotpotFurnitureCatalog.IsTable(item.kind)?3:2)+(kinds.Add(item.kind)?Math.Min(10,1+HotpotRules.FurniturePrices[item.kind]/100):0);return points;}
        }
        public int RequiredHotpotSpace {get{return 12+(int)Math.Min(300,5L*HotpotRank);}}
        public int KitchenUpgradeCost {get{return KitchenLevel>=8?0:HotpotRules.SafeInt(100*Math.Pow(2,KitchenLevel-1));}}
        public double HotpotCookSeconds(HotpotRecipe recipe)
        {return Math.Max(3,(recipe.technique==2?9:recipe.technique==1?16:20)*(1-.08*(EffectiveKitchenLevel-1))*(1-.08*HotpotRules.Mastery(recipe)));}
        public ActionResult UpgradeHotpotKitchen()
        {
            if(KitchenLevel>=8)return Fail("Bếp đã đạt cấp 8.");
            if(HotpotRank<KitchenLevel)return Fail("Cần hạng quán "+KitchenLevel+" để nâng bếp.");
            int cost=KitchenUpgradeCost;if(State.coins<cost)return Fail("Cần "+Money(cost)+".");
            State.coins-=cost;State.hotpot.progress.kitchenLevel++;return Ok("Bếp cấp "+KitchenLevel+" · "+HotpotBurners+" nồi song song.");
        }
        public static int SupplierUnlockRank(string id)
        {return id=="abalone"?6:id=="lobster"?5:id=="goat"||id=="crab"?3:id=="squid"||id=="mussels"?2:1;}
        public int KitchenUnitPrice(string id)
        {
            var ingredient=HotpotRules.Find(id);if(ingredient==null)return 0;
            if(ingredient.farm)return ingredient.price;
            int week=(int)Math.Min(int.MaxValue,State.lastSeenUtc/604800);
            double multiplier=.85+(HotpotRules.Hash(week,Array.IndexOf(HotpotRules.Ingredients,ingredient),29)%31)*.01;
            if(HotpotModifier(1)&&HotpotRules.IsSeafood(id))multiplier*=2;
            if(HotpotModifier(11)&&id=="beef")multiplier*=1.5;
            if(HotpotModifier(15)&&id=="seasoning")multiplier*=2;
            if(HotpotModifier(16)&&id=="base")multiplier*=2;
            if(HotpotModifier(20)&&!HotpotRules.IsMeat(id))multiplier*=.8;
            int branch=State.hotpot.progress.branch%4;
            if(branch==1&&HotpotRules.IsSeafood(id)||branch==2&&(id=="goat"||id=="herbs"))multiplier*=.75;
            return Math.Max(1,(int)Math.Ceiling(ingredient.price*multiplier));
        }
        private void DiscoverHotpot(HotpotRecipe recipe)
        {
            string key=HotpotRules.DiscoveryKey(recipe);var list=State.hotpot.progress.discoveries;
            if(!list.Contains(key)&&list.Count<2048)list.Add(key);
            int dish=HotpotRules.Specialty(recipe);if(dish>=0)recipe.name=HotpotRules.SpecialtyNames[dish];
        }
        private void RecordHotpotSale(HotpotRecipe recipe,int satisfaction,int revenue)
        {
            var p=State.hotpot.progress;
            recipe.timesServed=Math.Min(HotpotRules.Limit,recipe.timesServed+1);
            p.branchServed=Math.Min(HotpotRules.Limit,p.branchServed+1);
            p.seasonRevenue=HotpotRules.SafeInt((double)p.seasonRevenue+revenue);
            if(satisfaction>=85)p.seasonStars=Math.Min(HotpotRules.Limit,p.seasonStars+1);
            p.recentScores.Add(satisfaction);if(p.recentScores.Count>20)p.recentScores.RemoveAt(0);
        }
        public string HotpotPromotionMissing()
        {
            var p=State.hotpot.progress;
            if(p.rank>=HotpotRules.Limit)return "Đã chạm giới hạn số của bản lưu.";
            if(p.branchServed<HotpotRules.RankPots(p.rank))return "Cần "+HotpotRules.RankPots(p.rank)+" nồi tại chi nhánh này.";
            if(p.recentScores.Count<5||HotpotRecentTaste<HotpotRules.RankTaste(p.rank))return "Cần ít nhất 5 đánh giá gần đây, trung bình ≥ "+HotpotRules.RankTaste(p.rank)+".";
            if(HotpotSpace<RequiredHotpotSpace)return "Cần "+RequiredHotpotSpace+" điểm không gian: thêm bàn và đồ trang trí khác nhau.";
            return "";
        }
        internal bool CompleteHotpotInspection(int satisfaction)
        {
            if(HotpotPromotionMissing()!=""||satisfaction<HotpotRules.RankTaste(HotpotRank))return false;
            State.hotpot.progress.rank++;GainExperience(25);return true;
        }
        public double HotpotFatigueMultiplier(HotpotRecipe recipe,HotpotGuest guest,double now)
        {
            var p=State.hotpot.progress;var fatigue=p.fatigue.Find(f=>f.archetype==guest.archetype);
            if(fatigue==null||fatigue.recipeId!=recipe.id)return 1;
            double recovery=Math.Max(0,now-fatigue.lastServed)/60*.1;
            return Math.Min(1,Math.Max(.2,Math.Pow(.85,fatigue.streak))+recovery+p.cashier*.04);
        }
        public int HotpotGuestPrice(HotpotRecipe recipe,HotpotRecipe served,HotpotGuest guest,double now,int combo=0)
        {
            int satisfaction=HotpotRules.Satisfaction(served,guest,HotpotRank);
            double price=Math.Min(guest.budget,RecipePrice(served)*(guest.archetype==6?3:1));
            if(satisfaction<guest.threshold)price*=Math.Max(.25,satisfaction/(double)Math.Max(1,guest.threshold)*.75);
            price*=HotpotFatigueMultiplier(recipe,guest,now);
            if(HotpotModifier(4)&&served.heat>=2)price*=1.2;
            if(HotpotModifier(9)&&!served.portions.Exists(p=>HotpotRules.IsMeat(p.id)))price*=1.15;
            if(HotpotModifier(10)&&served.portions.Exists(p=>p.id=="mushroom"))price*=1.15;
            if(HotpotModifier(17)&&recipe.timesServed<10)price*=1.2;
            if(HotpotModifier(18))price*=1+.08*HotpotRules.Mastery(recipe);
            price*=1+Math.Min(20,Math.Max(0,combo))*(HotpotModifier(23)?.04:.02);
            return Math.Min(guest.budget,HotpotRules.SafeInt(price));
        }
        internal ActionResult ServeHotpotGuest(HotpotRecipe original,HotpotRecipe served,HotpotGuest guest,double now,int combo)
        {
            if(original==null||!State.hotpot.recipes.Contains(original)||!original.onMenu)return Fail("Món đã ngừng bán.");
            if(State.hotpot.progress.operatingDebt>0)return Fail("Thanh toán chi phí vận hành trước khi phục vụ.");
            string missing=KitchenMissing(served);if(missing!="")return Fail(missing);
            int revenue=HotpotGuestPrice(original,served,guest,now,combo),score=HotpotRules.Satisfaction(served,guest,HotpotRank);
            if(State.coins>HotpotRules.Limit-revenue||State.hotpotServed>=HotpotRules.Limit||State.hotpot.reviews>=HotpotRules.Limit)return Fail("Kho tiền hoặc lượt phục vụ đã đầy.");
            ConsumeRecipe(served);State.coins+=revenue;State.hotpotServed++;State.hotpot.reviews++;State.hotpot.tasteTotal+=HotpotRules.Taste(served).Score;GainExperience(6);
            RecordHotpotSale(original,score,revenue);
            var p=State.hotpot.progress;p.reputation=Math.Max(0,Math.Min(100,p.reputation+(score>=guest.threshold?1:-3)));
            var fatigue=p.fatigue.Find(f=>f.archetype==guest.archetype);
            if(fatigue==null){fatigue=new HotpotFatigue{archetype=guest.archetype};p.fatigue.Add(fatigue);}
            fatigue.streak=fatigue.recipeId==original.id&&now-fatigue.lastServed<600?Math.Min(1000,fatigue.streak+1):1;fatigue.recipeId=original.id;fatigue.lastServed=now;
            return Ok(guest.Name+" · hài lòng "+score+"/100 · +"+Money(revenue));
        }
        public void AdvanceHotpotSeason(double now)
        {
            if(!IsFinite(now)||now<0||now>253402300799)return;var p=State.hotpot.progress;
            if(p.seasonStarted==0)p.seasonStarted=now;
            if(now<p.seasonStarted+1209600)return;
            int elapsed=HotpotRules.SafeInt(Math.Floor((now-p.seasonStarted)/1209600));
            p.season=HotpotRules.SafeInt((double)p.season+elapsed);p.seasonStarted+=elapsed*1209600d;p.seasonRevenue=0;p.seasonStars=0;
        }
        public ActionResult ClaimHotpotSeason()
        {
            var p=State.hotpot.progress;int target=HotpotRules.SeasonTarget(p.season),stars=10+(int)Math.Min(990,5L*p.season);
            if(p.seasonRevenue<target||p.seasonStars<stars)return Fail("Mùa cần "+Money(target)+" doanh thu và "+stars+" nồi hài lòng ≥85.");
            int reward=Math.Max(100,target/5);if(State.coins>HotpotRules.Limit-reward||p.season>=HotpotRules.Limit||p.seasonClaims>=HotpotRules.Limit)return Fail("Đã đạt giới hạn thưởng.");
            State.coins+=reward;p.season++;p.seasonClaims++;p.seasonRevenue=0;p.seasonStars=0;p.seasonStarted=Math.Max(1,State.lastSeenUtc);GainExperience(30);
            return Ok("Hoàn thành mùa! +"+Money(reward)+" · mở mùa "+(p.season+1));
        }
        public int HotpotOperatingCost
        {get{var p=State.hotpot.progress;return HotpotRules.SafeInt((2+2d*HotpotRank+4*(p.waiter+p.assistant+p.cashier))*(HotpotModifier(19)?1.5:1));}}
        public void TickHotpotOperating(double seconds)
        {
            if(!IsFinite(seconds)||seconds<=0)return;var p=State.hotpot.progress;
            p.activeSeconds+=Math.Min(600,seconds);
            while(p.activeSeconds>=300){p.activeSeconds-=300;int cost=HotpotOperatingCost,paid=Math.Min(State.coins,cost);State.coins-=paid;p.operatingDebt=HotpotRules.SafeInt((double)p.operatingDebt+cost-paid);}
        }
        public ActionResult PayHotpotDebt()
        {var p=State.hotpot.progress;if(p.operatingDebt==0)return Fail("Không có khoản chưa thanh toán.");if(State.coins<p.operatingDebt)return Fail("Chưa đủ tiền thanh toán.");State.coins-=p.operatingDebt;p.operatingDebt=0;return Ok("Đã thanh toán chi phí vận hành.");}
        public ActionResult UpgradeHotpotStaff(int role)
        {
            if(role<0||role>2)return Fail("Nhân viên không hợp lệ.");var p=State.hotpot.progress;
            int level=role==0?p.waiter:role==1?p.assistant:p.cashier;
            if(State.workerHouse<level+1)return Fail("Nâng Nhà nhân công lên cấp "+(level+1)+" trước khi thuê/nâng nhân viên quán.");
            int roles=(p.waiter>0?1:0)+(p.assistant>0?1:0)+(p.cashier>0?1:0);
            if(level==0&&roles>=State.workerHouse)return Fail("Nhà nhân công cấp "+State.workerHouse+" chỉ hỗ trợ "+State.workerHouse+" vị trí tại quán.");
            if(level>=3||HotpotRank<level+2)return Fail("Cần hạng "+(level+2)+"; nhân viên tối đa cấp 3.");
            int cost=150*(level+1)*(role+1);if(State.coins<cost)return Fail("Cần "+Money(cost)+".");
            State.coins-=cost;if(role==0)p.waiter++;else if(role==1)p.assistant++;else p.cashier++;
            return Ok("Đã thuê/nâng nhân viên. Lương nằm trong phí vận hành 5 phút.");
        }
        public ActionResult SetHotpotOffline(bool enabled,double now)
        {
            var p=State.hotpot.progress;if(!IsFinite(now)||now<0||now>253402300799)return Fail("Thời gian không hợp lệ.");
            if(enabled&&(p.waiter==0||p.assistant==0||p.cashier==0))return Fail("Cần đủ phục vụ, phụ bếp và thu ngân.");
            AdvanceHotpotSeason(now);p.offlineEnabled=enabled;p.lastBusinessUtc=Math.Max(now,p.lastBusinessUtc);return Ok(enabled?"Nhân viên bán tối đa 48 nồi trong 8 giờ vắng mặt, dùng kho thật và tính phí.":"Đã tắt bán khi vắng mặt.");
        }
        public ActionResult SettleHotpotOffline(double now)
        {
            if(!IsFinite(now)||now<0||now>253402300799)return Fail("Thời gian không hợp lệ.");var p=State.hotpot.progress;
            double elapsed=Math.Max(0,Math.Min(28800,now-p.lastBusinessUtc));bool eligible=p.lastBusinessUtc>0&&p.offlineEnabled&&p.waiter>0&&p.assistant>0&&p.cashier>0;
            p.lastBusinessUtc=Math.Max(p.lastBusinessUtc,now);AdvanceHotpotSeason(now);
            elapsed=Math.Min(elapsed,Math.Max(0,now-p.seasonStarted));
            if(!eligible||elapsed<600)return Ok("Quán đã mở cửa.");
            int count=0,before=State.coins;
            for(int i=0;i<(int)(elapsed/600)&&i<48;i++)
            {
                if(p.operatingDebt>0)break;
                var guest=HotpotRules.Guest((int)Math.Min(int.MaxValue,now/600),i,HotpotRank,p.season,p.branch);
                var menu=State.hotpot.recipes.FindAll(r=>r.onMenu&&KitchenMissing(HotpotRules.Customize(r,guest.request))=="");if(menu.Count==0)break;
                var recipe=menu[i%menu.Count];var served=HotpotRules.Customize(recipe,guest.request);
                int fee=HotpotRules.SafeInt(2d*HotpotOperatingCost);if(State.coins<fee)break;
                var result=ServeHotpotGuest(recipe,served,guest,now-elapsed+(i+1)*600,0);if(!result.success)break;
                State.coins-=fee;count++;
            }
            return Ok("Nhân viên đã bán "+count+" nồi · chênh lệch sau phí "+Money(State.coins-before)+".");
        }
        public int HotpotBranchCost {get{return HotpotRules.SafeInt(2000d*(State.hotpot.progress.branch+1));}}
        public ActionResult OpenHotpotBranch()
        {
            var p=State.hotpot.progress;
            if(HotpotRank<8||p.seasonClaims<1)return Fail("Cần hạng 8 và hoàn thành ít nhất một mùa.");
            if(p.operatingDebt>0||p.branch>=HotpotRules.Limit||State.coins<HotpotBranchCost)return Fail("Cần thanh toán chi phí và đủ tiền mở chi nhánh.");
            State.coins-=HotpotBranchCost;p.branch++;p.rank=1;p.branchServed=0;p.kitchenLevel=1;p.reputation=50;p.recentScores.Clear();p.fatigue.Clear();p.activeSeconds=0;
            State.hotpot.floors=1;State.hotpot.furniture.Clear();
            return Ok("Mở chi nhánh "+HotpotRules.BranchNames[p.branch%4]+". Giữ công thức, tay nghề, kho và nhân viên; xây lại quán.");
        }
    }
}
