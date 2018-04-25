using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Core;
namespace Hitchhikers
{
    // [HubName("signalRChatHub")]
    public class SignalRChatHub
    {
        // var hubConnection = new HubConnection("http://www.contoso.com/");
        // IHubProxy stockTickerHubProxy = hubConnection.CreateHubProxy("StockTickerHub");

        // public IHubProxy StockTickerHubProxy { get => stockTickerHubProxy; set => stockTickerHubProxy = value; }

        // stockTickerHubProxy.On<Stock>("UpdateStockPrice", stock => Console.WriteLine("Stock update for {0} new price {1}", stock.Symbol, stock.Price));
        // await hubConnection.Start();
    }
}
