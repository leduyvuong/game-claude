using System;
using HarvestIsle.Core;
internal static partial class FarmModelTests
{
    private static void TradingTests()
    {
        Test("Selective sale preserves other items and quality buckets",delegate{
            var f=Farm();f.State.inventory.Find(x=>x.cropId=="carrot").count=7;f.State.fineGoods.Add(new InventoryEntry("carrot",5));f.State.fishStock=8;
            int before=f.State.coins;int quote=f.StockQuote("carrot",2,2);
            Assert(f.SellStock("carrot",2,2).success,"Sale failed");Equal(before+quote,f.State.coins);Equal(7,f.Count("carrot"));Equal(3,f.CountFine("carrot"));Equal(8,f.State.fishStock);
            Assert(!f.SellStock("carrot",2,4).success,"Oversale accepted");Equal(3,f.CountFine("carrot"));Assert(!f.SellStock("carrot",0,1).success,"Invalid grade accepted");Assert(!f.SellStock("unknown",1,1).success,"Invalid item accepted");Assert(!f.SellStock("fish",1,-1).success,"Negative quantity accepted");
            f.State.coins=1000000000;Assert(!f.SellStock("fish",1,1).success,"Wallet cap ignored");Equal(8,f.State.fishStock);
        });
        Test("Selectable orders reject stale boards and preserve stock on failure",delegate{
            var f=Farm();int serial=f.State.development.ordersCompleted;string item=f.ChoiceOrderItem(1);int needed=f.ChoiceOrderQuantity(1);
            Assert(!f.FulfillChoiceOrder(1,serial).success,"Empty stock accepted");f.State.inventory.Find(x=>x.cropId==item).count=needed;
            Assert(f.FulfillChoiceOrder(1,serial).success,"Ready order failed");Equal(0,f.ItemCount(item));Equal(serial+1,f.State.development.ordersCompleted);Assert(!f.FulfillChoiceOrder(1,serial).success,"Duplicate order accepted");
        });
    }
}
