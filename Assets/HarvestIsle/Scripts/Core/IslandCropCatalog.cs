using System;
using System.Collections.Generic;
namespace HarvestIsle.Core
{
    public static class IslandCropCatalog
    {
        public static readonly string[] Ids={"sweetpotato","potato","cassava","peanut","soybean","sesame","ginger","turmeric","galangal","pandan","basil","perilla","mint","cucumber","eggplant","okra","bittermelon","bottlegourd","loofah","passionfruit","dragonfruit","guava","lychee","longan","rambutan","mangosteen","durian","jackfruit","coffee","cacao"};
        public static readonly string[] Names={"Khoai lang","Khoai tây","Sắn","Lạc","Đậu nành","Mè","Gừng","Nghệ","Riềng","Lá dứa","Húng quế","Tía tô","Bạc hà","Dưa leo","Cà tím","Đậu bắp","Khổ qua","Bầu","Mướp","Chanh dây","Thanh long","Ổi","Vải","Nhãn","Chôm chôm","Măng cụt","Sầu riêng","Mít","Cà phê","Ca cao"};
        public static int Index(string id){return Array.IndexOf(Ids,id);}
        public static string DesignId(string id){int n=Index(id);return n<0?null:"C"+(n+1).ToString("00");}
        public static int Chapter(string id){int n=Index(id);return n<0?0:n==25||n==26?3:n>=19?2:1;}
        public static bool Leaf(string id){int n=Index(id);return n>=9&&n<=12;}
        public static bool Tree(string id){return Index(id)>=19;}
        public static string Group(string id)
        {int n=Index(id);return n<0?null:n==3||n==4?"Đậu":n<=2||n>=6&&n<=8?"Củ":n>=9&&n<=12||n==5?"Lá":"Quả";}
        public static CropSpec[] Append(CropSpec[] original)
        {
            var list=new List<CropSpec>(original);
            for(int i=0;i<Ids.Length;i++)
            {
                int chapter=Chapter(Ids[i]);double seconds=Leaf(Ids[i])?90+i*2:i>=19?420+(i%4)*60:i==2?420:180+(i%4)*30;
                int price=i>=19?18+(i%5)*2:i>=9&&i<=12?6:9+i%4;
                list.Add(new CropSpec(Ids[i],Names[i],"IX_C"+(i+1).ToString("00"),i>=19?30+i:price+2,price,3,seconds,chapter==1?3:chapter==2?7:10));
            }
            return list.ToArray();
        }
    }
}
