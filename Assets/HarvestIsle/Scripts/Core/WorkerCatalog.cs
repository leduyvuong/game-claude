using System;
using System.Collections.Generic;

namespace HarvestIsle.Core
{
    public sealed class WorkerSpec
    {
        public string id,name,cropId,portrait;
        // 0 crop specialist, 1 orchard, 2 irrigation, 3 soil care, 4 ranch.
        public int role,rarity;
        public int HirePrice {get{return rarity==1?120:rarity==2?360:rarity==3?900:1800;}}
        public int ShiftPrice {get{return rarity==1?4:rarity==2?9:rarity==3?18:30;}}
        public int DuplicateNotes {get{return rarity==1?1:rarity==2?3:rarity==3?8:20;}}
        public int Interval(int training){return Math.Max(4,(rarity==1?24:rarity==2?20:rarity==3?12:8)-4*training);}
        public int Skill(int training){return Math.Min(3,rarity-1+(training==2?1:0));}
        public string Specialty {get{return role==0?"Thu "+CropCatalog.Find(cropId).displayName:role==1?"Thu cây lâu năm":role==2?"Tưới cây / thoát úng":role==3?"Bắt sâu / chăm đất":"Thu sản phẩm / cho ăn";}}
    }
    public static class WorkerCatalog
    {
        public const int DrawPrice=30,ShiftJobs=10;
        public static readonly string[] Rarities={"Phổ thông","Lành nghề","Chuyên gia","Bậc thầy"};
        public static readonly int[] Rates={60,28,10,2};
        public static readonly WorkerSpec[] All=Build();
        public static WorkerSpec Find(string id){foreach(var w in All)if(w.id==id)return w;return null;}
        private static WorkerSpec[] Build()
        {
            // Stable IDs bind to crop IDs; names and tier choices are authored independently of saves.
            string[] juniors={"An Nhi","Bảo Nam","Bé Mận","Tấn Lộc","Diệu Vy","Minh Khang","Hồng Anh","Quốc Bảo","Thảo My","Đức Tài","Trúc Linh","Gia Hân","Ngọc Mai","Hữu Phúc","Khánh An","Thanh Thủy","Tuấn Kiệt","Liên Chi","Hoài Sơn","Bích Ngân","Việt Hùng","Hải Đăng","Tuyết Nhi","Phương Thảo","Nhật Minh","Kim Oanh","Thanh Tâm","Bảo Trân","Hoàng Long","Ánh Dương"};
            string[] seniors={"Cô Sáu Cà Rốt","Chú Bảy Bắp","Dì Năm Bí","Ông Ba Lúa","Cô Tám Cải","Chị Hồng Cà Chua","Chú Tư Hành","Cô Lan Đậu","Anh Hậu Dưa","Bác Mười Mía","Chị Hạnh Hà Lan","Cô Dâu Đỏ","Dì Cúc Vàng","Chú Chín Ớt","Cô Hương Sả","Dì Thủy Rau","Bác Tín Khoai","Bà Sen Hồng","Chú Lâm Nấm","Cô Ngọc Củ Cải","Bác Bình Bắp Cải","Ông Tư Lúa Mì","Bà Tuyết Trà","Chị Tỏi Trắng","Ông Năm Xoài","Chú Dừa Nước","Cô Cam Ngọt","Anh Chanh Xanh","Dì Chuối Mật","Bác Đu Đủ"};
            string masters="|rice|tomato|wheat|lotus|mango|";
            string experts="|corn|pumpkin|watermelon|sugarcane|strawberry|marigold|mushroom|snowtea|coconut|orange|";
            string youngMen="|Bảo Nam|Tấn Lộc|Minh Khang|Quốc Bảo|Đức Tài|Hữu Phúc|Tuấn Kiệt|Hoài Sơn|Việt Hùng|Hải Đăng|Nhật Minh|Hoàng Long|";
            var list=new List<WorkerSpec>();
            for(int i=0;i<CropCatalog.Original.Length;i++)
            {
                string id=CropCatalog.Original[i].id;
                list.Add(new WorkerSpec{id=id+"_junior",name=juniors[i],cropId=id,role=0,rarity=1,portrait=youngMen.Contains("|"+juniors[i]+"|")?"Farmer":"FarmerFemale"});
                list.Add(new WorkerSpec{id=id+"_senior",name=seniors[i],cropId=id,role=0,rarity=masters.Contains("|"+id+"|")?4:experts.Contains("|"+id+"|")?3:2,portrait=Portrait(seniors[i])});
            }
            string[][] teams={new[]{"Anh Vườn","Cô Sáu Miệt Vườn","Lão Vườn Bốn Mùa"},new[]{"Chị Mưa","Bác Thủy Lợi","Thầy Nước An Bình"},new[]{"Anh Đất","Cô Út Sinh Thái","Bà Đất Lành"},new[]{"Chị Mến","Chú Sáu Chuồng Trại","Ông Tám Chăn Nuôi"}};
            string[] ids={"orchard","irrigation","soil","ranch"};
            for(int role=1;role<=4;role++)for(int r=2;r<=4;r++)
                list.Add(new WorkerSpec{id=ids[role-1]+"_"+r,name=teams[role-1][r-2],cropId="",role=role,rarity=r,portrait=Portrait(teams[role-1][r-2])});
            return list.ToArray();
        }
        private static string Portrait(string name)
        {return name.StartsWith("Ông")||name.StartsWith("Bác")||name.StartsWith("Lão")?"ElderTu":name.StartsWith("Chú")||name.StartsWith("Anh")||name.StartsWith("Thầy")?"Farmer":"FarmerFemale";}
    }
    [Serializable] public sealed class SpecialistWorker
    {
        public string id;
        public int training,jobs,completed,cursor;
        public bool assigned,anyCrop;
        public double nextActionAt;
    }
    [Serializable] public sealed class WorkerReveal
    {
        public string id;
        public bool duplicate,revealed;
        public int notes;
    }
    [Serializable] public sealed class RecruitmentState
    {
        public List<SpecialistWorker> roster=new List<SpecialistWorker>();
        public List<WorkerReveal> reveal=new List<WorkerReveal>();
        public int serial,totalDraws,sinceExpert,sinceMaster,notes;
    }
    public sealed class WorkerAction
    {
        public string id,animalId;
        public int plot=-1;
        public FarmTool tool;
    }
}
