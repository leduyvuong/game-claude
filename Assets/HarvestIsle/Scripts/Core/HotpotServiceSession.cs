using System;
using System.Collections.Generic;

namespace HarvestIsle.Core
{
    // Orders are session-only. All stock, payment, scoring and progression remain in Core.
    public sealed class HotpotOrder
    {
        public int key,recipeId;
        public HotpotRecipe recipe;
        public HotpotGuest guest;
        public List<HotpotGuest> party=new List<HotpotGuest>();
        public bool answered,cooking,ready,served;
        public double patience,cookRemaining,readyFor;
    }
    public sealed class HotpotServiceSession
    {
        private readonly FarmModel model;
        private readonly int seed;
        private int ordinal;
        private double clock,staffClock;
        public readonly List<HotpotOrder> orders=new List<HotpotOrder>();
        public bool Rush {get;private set;}
        public int Wave {get;private set;}
        public int Combo {get;private set;}
        public int RushScore {get;private set;}
        public double WaveRemaining {get;private set;}
        private int waveServed;
        public string LastMessage {get;private set;}
        public HotpotServiceSession(FarmModel model,int seed)
        {this.model=model;this.seed=seed;clock=model.State.lastSeenUtc;}
        public double ArrivalSeconds
        {get{double interval=Rush?Math.Max(.8,8/(1+Wave*.15)):9+(HotpotRules.Hash(seed,ordinal,13)%7);if(model.HotpotModifier(4))interval*=1.3;return interval;}}
        public int CookingCount {get{return orders.FindAll(o=>o.cooking&&!o.served).Count;}}
        public bool HasPending {get{return orders.Exists(o=>!o.served);}}
        public HotpotOrder Find(int key){return orders.Find(o=>o.key==key);}
        public bool CanReserve(HotpotRecipe recipe,HotpotOrder replacing=null)
        {
            if(!HotpotRules.ValidRecipe(recipe))return false;
            var need=new Dictionary<string,int>();
            foreach(var order in orders)if(!order.served&&order!=replacing)AddNeeds(order.recipe,need);
            AddNeeds(recipe,need);
            foreach(var pair in need){var parts=pair.Key.Split(':');if(model.KitchenCount(parts[0],int.Parse(parts[1]))<pair.Value)return false;}return true;
        }
        private static void AddNeeds(HotpotRecipe recipe,Dictionary<string,int> needs)
        {
            var list=new List<HotpotPortion>(recipe.portions);list.Add(new HotpotPortion("base",1,1));list.Add(new HotpotPortion("seasoning",1+recipe.salt+recipe.heat,1));
            foreach(var p in list){string key=p.id+":"+p.stars;int count;needs.TryGetValue(key,out count);needs[key]=count+p.amount;}
        }
        public bool Seat(int key,bool inspector=false)
        {
            if(key<0||key>=104||Find(key)!=null||model.State.hotpot.progress.operatingDebt>0)return false;
            int floor=key/26,slot=key%26;
            if(floor>=model.State.hotpot.floors||!(floor==0&&slot<model.HotpotTables)&&!model.State.hotpot.furniture.Exists(f=>f.floor==floor&&f.slot==slot&&HotpotFurnitureCatalog.IsTable(f.kind)))return false;
            if(inspector&&(model.HotpotPromotionMissing()!=""||orders.Exists(o=>o.guest.inspector)))return false;
            var progress=model.State.hotpot.progress;
            var guest=HotpotRules.Guest(seed,ordinal,model.HotpotRank,progress.season,progress.branch,inspector);
            var party=new List<HotpotGuest>{guest};
            for(int i=1;i<guest.groupSize;i++)
            {
                var member=HotpotRules.Guest(seed,ordinal+i*197,model.HotpotRank,progress.season,progress.branch);
                member.archetype=guest.archetype;member.request=guest.request;member.budget=guest.budget;member.threshold=guest.threshold;party.Add(member);
            }
            HotpotRecipe best=null;int bestScore=-1;
            foreach(var recipe in model.State.hotpot.recipes)
            {
                if(!recipe.onMenu||!CanReserve(recipe))continue;
                int score=100;foreach(var member in party)score=Math.Min(score,HotpotRules.Satisfaction(recipe,member,model.HotpotRank));
                score-=(int)Math.Round((1-model.HotpotFatigueMultiplier(recipe,guest,clock))*30);
                if(guest.archetype==5&&HotpotRules.Specialty(recipe)>=0)score+=8;
                if(model.RecipePrice(recipe)>guest.budget)score-=20;
                if(score>bestScore){best=recipe;bestScore=score;}
            }
            if(best==null)return false;
            ordinal++;double patience=(guest.archetype==1?45:75)*(model.HotpotModifier(13)?.75:1)*(Rush?Math.Max(.08,Math.Pow(.97,Math.Min(1000,Wave))):1);
            orders.Add(new HotpotOrder{key=key,recipeId=best.id,recipe=HotpotRules.Copy(best),guest=guest,party=party,patience=patience,answered=guest.request==0});
            LastMessage=guest.Name+" gọi "+best.name+(guest.groupSize>1?" · nhóm "+guest.groupSize:"");return true;
        }
        public bool Answer(int key,bool accept)
        {
            var order=Find(key);if(order==null||order.answered||order.cooking||order.served)return false;
            if(!accept){orders.Remove(order);LastMessage="Đã từ chối yêu cầu; khách rời bàn, không mất nguyên liệu.";return true;}
            var modified=HotpotRules.Customize(order.recipe,order.guest.request);
            if(!CanReserve(modified,order)){LastMessage="Biến thể không hợp lệ hoặc kho không đủ. Có thể từ chối khách.";return false;}
            order.recipe=modified;order.answered=true;LastMessage="Đã nhận yêu cầu "+HotpotRules.RequestNames[order.guest.request]+".";return true;
        }
        public bool Cook(int key)
        {
            var order=Find(key);
            if(order==null||!order.answered||order.cooking||order.served||CookingCount>=model.HotpotBurners||!CanReserve(order.recipe,order))return false;
            if(order.recipe.technique==2&&model.EffectiveKitchenLevel<3){LastMessage="Cách đun nhanh cần bếp hiệu lực cấp 3.";return false;}
            order.cooking=true;order.cookRemaining=model.HotpotCookSeconds(order.recipe);LastMessage="Đang nấu "+order.recipe.name+".";return true;
        }
        public int Satisfaction(HotpotOrder order)
        {int score=100;foreach(var guest in order.party)score=Math.Min(score,HotpotRules.Satisfaction(order.recipe,guest,model.HotpotRank));return score;}
        public int Quote(HotpotOrder order)
        {
            var original=model.State.hotpot.recipes.Find(r=>r.id==order.recipeId);if(original==null)return 0;
            HotpotGuest worst=order.guest;int score=101;
            foreach(var guest in order.party){int value=HotpotRules.Satisfaction(order.recipe,guest,model.HotpotRank);if(value<score){score=value;worst=guest;}}
            int combo=Rush&&order.readyFor<=8?Math.Min(1000,Combo+1):0;
            return model.HotpotGuestPrice(original,order.recipe,worst,clock,combo);
        }
        public bool Serve(int key)
        {
            var order=Find(key);if(order==null||!order.ready||order.served)return false;
            var original=model.State.hotpot.recipes.Find(r=>r.id==order.recipeId);HotpotGuest worst=order.guest;int score=101;
            foreach(var guest in order.party){int value=HotpotRules.Satisfaction(order.recipe,guest,model.HotpotRank);if(value<score){score=value;worst=guest;}}
            int nextCombo=Rush&&order.readyFor<=8?Math.Min(1000,Combo+1):0;
            var result=model.ServeHotpotGuest(original,order.recipe,worst,clock,nextCombo);LastMessage=result.message;
            if(!result.success)return false;
            order.served=true;order.cooking=false;order.patience=12;Combo=nextCombo;
            if(order.guest.inspector){bool passed=model.CompleteHotpotInspection(score);LastMessage+=passed?" · Thanh tra duyệt lên hạng "+model.HotpotRank:" · Chưa đạt bài kiểm tra lên hạng.";}
            if(Rush){waveServed++;RushScore=HotpotRules.SafeInt((double)RushScore+score+Combo*5);}
            return true;
        }
        public void Move(int from,int to){var order=Find(from);if(order!=null&&Find(to)==null)order.key=to;}
        public bool StartRush()
        {if(Rush||HasPending)return false;Rush=true;Wave=1;WaveRemaining=90;RushScore=0;Combo=0;waveServed=0;LastMessage="Giờ cao điểm bắt đầu!";return true;}
        public void EndRush()
        {
            if(!Rush)return;Rush=false;var p=model.State.hotpot.progress;p.bestRushScore=Math.Max(p.bestRushScore,RushScore);
            LastMessage="Kết thúc cao điểm · "+RushScore+" điểm · sóng tốt nhất "+p.bestWave;
        }
        public void Tick(double seconds)
        {
            if(double.IsNaN(seconds)||double.IsInfinity(seconds)||seconds<=0)return;seconds=Math.Min(2,seconds);clock+=seconds;staffClock+=seconds;
            model.TickHotpotOperating(seconds);
            foreach(var order in new List<HotpotOrder>(orders))
            {
                order.patience-=seconds;
                if(order.patience<=0)
                {
                    if(!order.served){model.State.hotpot.progress.reputation=Math.Max(0,model.State.hotpot.progress.reputation-2);Combo=0;LastMessage=order.guest.Name+" đã hết kiên nhẫn.";}
                    orders.Remove(order);continue;
                }
                if(order.cooking&&!order.ready){order.cookRemaining-=seconds;if(order.cookRemaining<=0)order.ready=true;}
                if(order.ready&&!order.served)order.readyFor+=seconds;
            }
            var p=model.State.hotpot.progress;
            if(staffClock>=Math.Max(2,8-2*p.assistant))
            {
                staffClock=0;
                foreach(var order in new List<HotpotOrder>(orders))
                {
                    if(p.assistant>0&&!order.answered)Answer(order.key,true);
                    if(p.assistant>0&&!order.cooking&&!order.served)Cook(order.key);
                    if(p.waiter>0&&order.ready&&!order.served&&order.readyFor>=Math.Max(2,10-2*p.waiter))Serve(order.key);
                }
            }
            if(Rush)
            {
                WaveRemaining-=seconds;
                if(WaveRemaining<=0)
                {
                    int target=3+(int)Math.Min(97,Wave/2);
                    if(waveServed<target){EndRush();return;}
                    p.bestWave=Math.Max(p.bestWave,Wave);p.bestRushScore=Math.Max(p.bestRushScore,RushScore);
                    if(Wave>=HotpotRules.Limit){EndRush();return;}
                    Wave++;WaveRemaining=90;waveServed=0;LastMessage="Sóng "+Wave+" · mục tiêu "+(3+Math.Min(97,Wave/2))+" bàn.";
                }
            }
        }
    }
}
