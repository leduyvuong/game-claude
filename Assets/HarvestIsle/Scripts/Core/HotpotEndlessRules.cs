using System;
using System.Collections.Generic;

namespace HarvestIsle.Core
{
    [Serializable] public sealed class HotpotFatigue
    {public int archetype,recipeId,streak;public double lastServed;}
    [Serializable] public sealed class HotpotProgress
    {
        public int rank=1,kitchenLevel=1,branch,branchServed,reputation=50;
        public int season,seasonRevenue,seasonStars,seasonClaims,bestWave,bestRushScore;
        public int waiter,assistant,cashier,operatingDebt;
        public double seasonStarted,lastBusinessUtc,activeSeconds;
        public bool offlineEnabled;
        public List<int> recentScores=new List<int>();
        public List<string> discoveries=new List<string>();
        public List<HotpotFatigue> fatigue=new List<HotpotFatigue>();
        public bool initialized;
    }
    public sealed class HotpotGuest
    {
        public int archetype,sweet,salt,sour,heat,budget,threshold,request,groupSize=1;
        public bool vegetarian,inspector;
        public string Name {get{return HotpotRules.GuestNames[archetype];}}
    }
    public static partial class HotpotRules
    {
        public const int Limit=1000000000;
        public static readonly string[] GuestNames={"Sinh viên","Dân văn phòng","Gia đình","Khách chay","Sành ăn","Du khách","VIP","Khách địa phương","Người tập luyện","Cặp đôi","Khách đoàn","Thanh tra"};
        public static readonly string[] RequestNames={"Đúng công thức","Thêm cay","Ít mặn","Không hành"};
        public static readonly string[] BranchNames={"Vườn nhà","Bãi biển","Phố núi","Chợ đêm"};
        public static readonly string[] SpecialtyNames={"Lẩu Thái","Lẩu Tứ Xuyên","Lẩu cà chua","Lẩu nấm","Lẩu hải sản","Lẩu bò","Lẩu gà lá é","Lẩu cá chua cay","Lẩu dê","Lẩu cua đồng","Sukiyaki","Shabu-shabu"};
        public static readonly string[] ModifierNames={"Tuần lẩu chay","Bão biển","Khách đoàn","Kiểm định bếp","Mưa dầm","Hội sinh viên","Tuần du lịch","Khách VIP","Lễ hội cay","Mùa rau xanh","Mùa nấm","Tuần thịt bò","Đêm gia đình","Khách vội","Ngày thanh đạm","Giá gia vị tăng","Giá nước nền tăng","Món mới lên ngôi","Thưởng tay nghề","Phí mặt bằng tăng","Khuyến mãi rau","Lẩu ngọt mùa hè","Khách sành ăn","Lễ hội tiền tip"};
        public static int Hash(int a,int b,int c)
        {unchecked{uint x=(uint)a*747796405u+(uint)b*2891336453u+(uint)c*277803737u+12345;x=(x^(x>>16))*2246822519u;x^=x>>13;return (int)(x&0x7fffffff);}}
        public static int SafeInt(double value){return (int)Math.Max(0,Math.Min(Limit,Math.Ceiling(value)));}
        public static int RankPots(int rank){return SafeInt(15*Math.Pow(1.5,Math.Min(60,Math.Max(0,rank-1))));}
        public static int RankTaste(int rank){return 55+(int)Math.Min(40,4L*Math.Max(1,rank));}
        public static int SeasonTarget(int season){return SafeInt(1000*Math.Pow(1.18,Math.Min(100,Math.Max(0,season))));}
        public static int Mastery(HotpotRecipe r){return r.timesServed>=200?3:r.timesServed>=50?2:r.timesServed>=10?1:0;}
        public static int[] Modifiers(int season)
        {
            var pool=new List<int>();for(int i=0;i<ModifierNames.Length;i++)pool.Add(i);
            int count=2+Hash(season,81,2)%3;var result=new int[count];
            for(int i=0;i<count;i++){int index=Hash(season,i,41)%pool.Count;result[i]=pool[index];pool.RemoveAt(index);}return result;
        }
        public static bool IsMeat(string id)
        {return id=="beef"||id=="chickenMeat"||id=="fish"||id=="shrimp"||id=="lobster"||id=="abalone"||id=="pork"||id=="squid"||id=="mussels"||id=="meatballs"||id=="goat"||id=="crab";}
        public static bool IsSeafood(string id){return id=="fish"||id=="shrimp"||id=="lobster"||id=="abalone"||id=="squid"||id=="mussels"||id=="crab";}
        private static HotpotIngredient[] ExpandIngredients(HotpotIngredient[] original)
        {
            var list=new List<HotpotIngredient>(original);
            foreach(var crop in CropCatalog.All)
                if(crop.id!="cassava"&&!list.Exists(s=>s.id==crop.id))list.Add(new HotpotIngredient(crop.id,crop.displayName,true,crop.sellPrice,
                    crop.id=="sugarcane"||crop.id=="banana"||crop.id=="mango"?3:1,crop.id=="garlic"?3:0,crop.id=="lemon"||crop.id=="orange"?3:0,1));
            list.Add(new HotpotIngredient("honey","Mật ong",true,18,4,0,0,0));
            foreach(var item in IslandExpansionCatalog.Items)
                if(item.edible&&item.buy==0&&item.id!="ix_noodles"&&item.id!="ix_soytofu")list.Add(new HotpotIngredient(item.id,item.name,true,Math.Max(1,item.sell),item.id.Contains("candy")||item.id.Contains("chocolate")?3:1,item.id.Contains("spice")?2:0,item.id.Contains("pickle")?2:0,1));
            foreach(var product in DevelopmentCatalog.Recipes)
                if(product.id!="herbal"&&product.id!="compost")list.Add(new HotpotIngredient(product.id,product.name,true,product.price,product.id=="sugar"||product.id=="jam"?4:1,IsMeat(product.id)?4:product.id=="cheese"?3:1,0,IsMeat(product.id)?0:1));
            string[] ids={"pork","squid","mussels","meatballs","tofu","tofuSkin","eggNoodles","riceNoodles","herbs","goat","crab"};
            string[] names={"Thịt ba chỉ","Mực","Vẹm","Viên thả lẩu","Đậu phụ","Tàu hũ ky","Mì trứng","Bún gạo","Lá é và rau thơm","Thịt dê","Cua đồng"};
            int[] prices={17,28,24,15,9,12,8,7,5,32,23};
            for(int i=0;i<ids.Length;i++)if(!list.Exists(s=>s.id==ids[i]))list.Add(new HotpotIngredient(ids[i],names[i],false,prices[i],0,IsMeat(ids[i])?4:1,0,IsMeat(ids[i])?0:1));
            return list.ToArray();
        }
        public static int Specialty(HotpotRecipe r)
        {
            if(!ValidRecipe(r))return -1;
            Func<string,bool> has=id=>r.portions.Exists(p=>p.id==id);
            if(r.style==1&&r.broth==1&&has("shrimp")&&has("lemongrass"))return 0;
            if(r.style==1&&r.broth==1&&r.heat>=2&&has("beef")&&has("chili"))return 1;
            if(r.style==1&&r.broth==1&&has("tomato")&&has("eggs"))return 2;
            if(r.style==0&&r.broth==0&&has("mushroom")&&has("greens"))return 3;
            if(r.style==0&&r.broth==0&&has("shrimp")&&has("fish")&&has("corn"))return 4;
            if(r.style==2&&r.broth==2&&has("beef")&&has("onion"))return 5;
            if(r.style==0&&r.broth==0&&has("chickenMeat")&&has("herbs")&&has("mushroom"))return 6;
            if(r.style==1&&r.broth==1&&has("fish")&&has("tomato"))return 7;
            if(r.style==2&&r.broth==2&&has("goat")&&has("taro"))return 8;
            if(r.style==1&&r.broth==1&&has("crab")&&has("tomato"))return 9;
            if(r.style==2&&r.broth==2&&has("beef")&&has("tofu")&&has("mushroom"))return 10;
            if(r.style==0&&r.broth==0&&has("beef")&&has("cabbage"))return 11;
            return -1;
        }
        public static string DiscoveryKey(HotpotRecipe r)
        {
            int specialty=Specialty(r);if(specialty>=0)return "special:"+specialty;
            var main=r.portions[0];foreach(var p in r.portions)if(p.amount>main.amount||(p.amount==main.amount&&String.CompareOrdinal(p.id,main.id)<0))main=p;
            return "house:"+r.broth+":"+r.style+":"+main.id;
        }
        public static string DiscoveryName(HotpotRecipe r)
        {int specialty=Specialty(r);if(specialty>=0)return SpecialtyNames[specialty];var key=DiscoveryKey(r).Split(':');return "Lẩu "+Find(key[3]).name.ToLowerInvariant()+" "+Styles[r.style].ToLowerInvariant();}
        public static HotpotRecipe Customize(HotpotRecipe r,int request)
        {
            var copy=Copy(r);if(request==1)copy.heat=Math.Min(3,copy.heat+1);
            if(request==2)copy.salt=Math.Max(0,copy.salt-1);
            if(request==3)copy.portions.RemoveAll(p=>p.id=="onion");return copy;
        }
        public static HotpotGuest Guest(int seed,int ordinal,int rank,int season,int branch,bool inspector=false)
        {
            int available=Math.Min(11,3+Math.Max(1,rank)),type=Hash(seed,ordinal,rank)%available;
            int[] mods=Modifiers(season);
            if(Array.IndexOf(mods,0)>=0&&Hash(seed,ordinal,2)%2==0)type=3;
            if(Array.IndexOf(mods,6)>=0||branch%4==1)if(Hash(seed,ordinal,3)%3==0)type=5;
            if(Array.IndexOf(mods,7)>=0&&rank>=4&&Hash(seed,ordinal,4)%3==0)type=6;
            if(Array.IndexOf(mods,5)>=0)type=0;
            if(Array.IndexOf(mods,22)>=0&&rank>=3)type=4;
            if(inspector)type=11;
            int style=Hash(seed,ordinal,9)%3;
            var guest=new HotpotGuest{archetype=type,sweet=Hash(seed,ordinal,7)%3,sour=style==1?1:0,salt=style==2?2:1,heat=style==1?2:0,
                vegetarian=type==3,budget=SafeInt((type==0?100:type==6?2500:400)*(1+.2*Math.Min(100000,rank))),
                threshold=50+(int)Math.Min(45,3L*rank),request=Hash(seed,ordinal,8)%4,inspector=inspector};
            if(type==6)guest.threshold=Math.Max(90,guest.threshold);
            if(inspector){guest.threshold=RankTaste(rank);guest.request=0;guest.budget=Limit;}
            if(type==2||type==10||Array.IndexOf(mods,2)>=0||Array.IndexOf(mods,12)>=0)guest.groupSize=2+Hash(seed,ordinal,6)%2;
            if(inspector)guest.groupSize=1;
            if(Array.IndexOf(mods,8)>=0)guest.heat=2;
            if(Array.IndexOf(mods,14)>=0)guest.salt=0;
            if(Array.IndexOf(mods,21)>=0)guest.sweet=2;
            return guest;
        }
        public static int Satisfaction(HotpotRecipe r,HotpotGuest guest,int rank)
        {
            if(!ValidRecipe(r))return 0;
            int total=0,sweet=0,sour=0;bool meat=false;
            foreach(var p in r.portions){var s=Find(p.id);total+=p.amount;sweet+=s.sweet*p.amount;sour+=s.sour*p.amount;meat|=IsMeat(p.id);}
            int tolerance=Math.Max(1,12-Math.Min(12,rank));
            int delta=Math.Abs(r.salt-guest.salt)+Math.Abs(r.heat-guest.heat)+Math.Abs(Math.Min(3,sweet/total)-guest.sweet)+Math.Abs(Math.Min(3,sour/total)-guest.sour);
            return Math.Max(0,Taste(r).Score-4*Math.Max(0,delta-tolerance)-(guest.vegetarian&&meat?15:0));
        }
        public static bool ValidateProgress(HotpotProgress p)
        {
            if(EmptyProgress(p))return true;
            if(p.rank<1||p.rank>Limit||p.kitchenLevel<1||p.kitchenLevel>8||p.branch<0||p.branch>Limit||p.branchServed<0||p.branchServed>Limit||p.reputation<0||p.reputation>100||
                p.season<0||p.season>Limit||p.seasonRevenue<0||p.seasonRevenue>Limit||p.seasonStars<0||p.seasonStars>Limit||p.seasonClaims<0||p.seasonClaims>Limit||p.bestWave<0||p.bestWave>Limit||p.bestRushScore<0||p.bestRushScore>Limit||
                p.waiter<0||p.waiter>3||p.assistant<0||p.assistant>3||p.cashier<0||p.cashier>3||p.operatingDebt<0||p.operatingDebt>Limit||!FiniteTime(p.seasonStarted)||!FiniteTime(p.lastBusinessUtc)||!FiniteTime(p.activeSeconds)||p.activeSeconds>=300||
                p.recentScores==null||p.recentScores.Count>20||p.discoveries==null||p.discoveries.Count>2048||p.fatigue==null||p.fatigue.Count>12)return false;
            foreach(int score in p.recentScores)if(score<0||score>100)return false;
            var keys=new HashSet<string>();foreach(string key in p.discoveries)if(!ValidDiscovery(key)||!keys.Add(key))return false;
            var types=new HashSet<int>();foreach(var f in p.fatigue)if(f==null||f.archetype<0||f.archetype>11||f.recipeId<1||f.recipeId>=1000000||f.streak<1||f.streak>1000||!FiniteTime(f.lastServed)||!types.Add(f.archetype))return false;
            return true;
        }
        public static bool EmptyProgress(HotpotProgress p)
        {
            return p==null||!p.initialized&&((p.rank==0&&p.kitchenLevel==0&&p.reputation==0)||(p.rank==1&&p.kitchenLevel==1&&p.reputation==50))&&p.branch==0&&p.branchServed==0&&p.season==0&&p.seasonRevenue==0&&p.seasonStars==0&&p.seasonClaims==0&&p.bestWave==0&&p.bestRushScore==0&&p.waiter==0&&p.assistant==0&&p.cashier==0&&p.operatingDebt==0&&p.seasonStarted==0&&p.lastBusinessUtc==0&&p.activeSeconds==0&&!p.offlineEnabled&&(p.recentScores==null||p.recentScores.Count==0)&&(p.discoveries==null||p.discoveries.Count==0)&&(p.fatigue==null||p.fatigue.Count==0);
        }
        private static bool FiniteTime(double value){return !double.IsNaN(value)&&!double.IsInfinity(value)&&value>=0&&value<=253402300799;}
        private static bool ValidDiscovery(string key)
        {
            if(key==null)return false;string[] pieces=key.Split(':');int a,b;
            if(pieces.Length==2&&pieces[0]=="special")return int.TryParse(pieces[1],out a)&&a>=0&&a<12;
            return pieces.Length==4&&pieces[0]=="house"&&int.TryParse(pieces[1],out a)&&a>=0&&a<3&&int.TryParse(pieces[2],out b)&&b>=0&&b<3&&Find(pieces[3])!=null&&pieces[3]!="base"&&pieces[3]!="seasoning";
        }
    }
}
