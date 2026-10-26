using System;
using System.Text;
using System.Web.Script.Serialization;
using HarvestIsle.Core;

public sealed class WorkerRequest
{
    public FarmState farm;
    public string kind,id;
    public int index,tool,job;
    public bool useCorn;
    public double now,elapsed;
}
internal static class CoreWorker
{
    private static int Main()
    {
        Console.InputEncoding=Encoding.UTF8;Console.OutputEncoding=new UTF8Encoding(false);
        var json=new JavaScriptSerializer{MaxJsonLength=1024*1024};
        try
        {
            var request=json.Deserialize<WorkerRequest>(Console.In.ReadToEnd());
            var model=new FarmModel(request.farm??FarmModel.NewState(request.now));
            model.ObserveTime(request.now);
            model.AdvanceEcology(Math.Max(0,Math.Min(5,request.elapsed)));
            ActionResult result;
            switch(request.kind)
            {
                case "new":case "state":result=new ActionResult(true,"Đã đồng bộ với máy chủ.");break;
                case "plot":
                    if(request.tool<0||request.tool>6||request.tool==2||request.tool==3)result=new ActionResult(false,"Công cụ không hợp lệ.");
                    else result=model.Apply(request.index,(FarmTool)request.tool,request.id,request.now);break;
                case "harvest":result=model.Harvest(request.index,1,request.now);break;
                case "sell":result=model.SellAll();break;
                case "sellStock":result=model.SellStock(request.id,request.tool,request.job);break;
                case "choiceOrder":result=model.FulfillChoiceOrder(request.index,request.job);break;
                case "build":result=model.UpgradeBuilding(request.id);break;
                case "produce":result=model.StartProduction(request.id,request.now);break;
                case "collect":result=model.CollectProduction(request.job,request.now);break;
                case "sellProduct":result=model.SellProduct(request.id);break;
                case "order":result=model.FulfillOrder();break;
                case "daily":result=model.ClaimDaily(request.now);break;
                case "chapter":result=model.ClaimChapter();break;
                case "nursery":result=model.StartNursery(request.id,request.now);break;
                case "greenhouse":result=model.ToggleGreenhouse(request.index);break;
                case "careProduct":result=model.UseCareProduct(request.id,request.index);break;
                case "buyAnimal":result=model.BuyAnimal(request.id);break;
                case "feedAnimal":result=model.FeedAnimal(request.id,request.useCorn,request.now);break;
                case "collectAnimal":result=model.CollectAnimal(request.id,request.now);break;
                default:result=new ActionResult(false,"Lệnh không được hỗ trợ.");break;
            }
            string reason;if(!FarmModel.Validate(model.State,out reason))throw new InvalidOperationException(reason);
            Console.Write(json.Serialize(new {success=result.success,message=result.message,farm=model.State,serverTime=request.now}));
            return 0;
        }
        catch(Exception e){Console.Error.WriteLine(e.GetType().Name+": "+e.Message);return 1;}
    }
}
