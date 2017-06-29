using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Jojatekok.PoloniexAPI;
using Jojatekok.PoloniexAPI.MarketTools;
using MahApps.Metro.IconPacks;
using CefSharp;
using System.Threading;
using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;

namespace PoloniexDemo
{
    public sealed partial class MainWindow
    {
        public PoloniexClient PoloniexClient { get; set; }

        // Set Symbol 1 from combobox      
        private string symbol1;
        // Symbol 2 from combo box selction
        private string symbol2;
        private MarketPeriod marketseries;
        private double IBS;
        double range_adr;
        private double priceLimit;
        private double quantity;
        private double currentTickerPrice;
        private string orderID;

        //Min order size
        private double minOrderSize = 0.001;


        

        public MainWindow()
        {

            InitializeComponent();

            //PoloniexClient = new PoloniexClient(ApiKeys.PublicKey, ApiKeys.PrivateKey);
            PoloniexClient = new PoloniexClient(Properties.Settings.Default.PublicKey, Properties.Settings.Default.PrivateKey);
            Application.Current.Properties.Add("PoloniexClient", PoloniexClient);
            //PoloniexClient = (PoloniexClient)Application.Current.Properties["PoloniexClient"];

            PoloniexClient.Live.Start();

            QuoteSymbolSelect.SelectionChanged += QuoteSymbolSelect_SelectionChanged;
            BaseSymbolSelect.SelectionChanged += BaseSymbolSelect_SelectionChanged;
            
        }

        //***************************************************************************************

        #region API Keys
        //Set API Keys
        private void APIKeySetButton_Click(object sender, RoutedEventArgs e)
        {
            var ApiKeyWindow = new ApiKeyWindow();
            ApiKeyWindow.Show();
        }
        #endregion

        //***************************************************************************************

        #region Refresh Data
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            // Refresh all data
            RefreshAll();
        }

        private async void RefreshAll()
        {
           // await GetHistorialData();

            
            DisplayCurrentPrice(symbol1, symbol2);

            OrderSymbolBox();

            //DisplayCurrentPrice();

            //Calculate IBS
            //IBSCalculation();
            //ADR(marketseries);

            //Orders
            await GetOpenTrades();
            await GetOpenOrders();

            OpenOrdersDataGrid.Items.Refresh();

            // Balance
            await GetBalanceQuote();
            await GetBalanceBTC();

            //Browser
            BrowserNavigate();
        }
        #endregion

        //***************************************************************************************

        #region TV Widget
        // Navigate to TV Widget
        private void BrowserNavigate()
        {
            LoadTradingViewWidget();
        }

        // Once browser has finished loading
        private void Browser_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            LoadTradingViewWidget();
        }

        // TV Widget
        private void LoadTradingViewWidget()
        {
            // Load widget code
            var widget = new Widget();

            // Combine symbol 1 & 2
            string symbolString = symbol2 + symbol1;

            // Replace symbol in widget HTML with symbolcode
            string htmlstring = widget.htmlWidget.Replace("INSERTSYMBOL", symbolString);

            // Load widget code with webbrowser
            WebBrowser.LoadHtml(htmlstring, "http://www.example.com/");
        }
        #endregion

        //***************************************************************************************

        #region Currency Pair Method
        // Get Currency pair from symbol strings
        public CurrencyPair GetSymbolCode(string symbol1, string symbol2)
        {
            CurrencyPair symbol = new CurrencyPair(symbol1, symbol2);
            return symbol;
        }
        #endregion

        //***************************************************************************************

        #region Ticker Data
        public async Task GetTickerData()
        {
            await PoloniexClient.Live.SubscribeToTickerAsync();

            PoloniexClient.Live.OnTickerChanged += Live_OnTickerChanged;
        }

        private void Live_OnTickerChanged(object sender, TickerChangedEventArgs ticker)
        {
            CurrencyPair symbol = GetSymbolCode(symbol1, symbol2);

            if (ticker.CurrencyPair == symbol)

                TickerTextBlock.Dispatcher.Invoke(new Action(() =>
                {
                    TickerData.Items.Add(ticker.MarketData);
                    TickerData.Items.Refresh();
                    TickerTextBlock.Text = ticker.MarketData.PriceLast.ToStringNormalized();
                    currentTickerPrice = ticker.MarketData.PriceLast;
                }));
        }

        public async void StartTicker_Click(object sender, RoutedEventArgs e)
        {
            var textblock = sender as TextBlock;
            await GetTickerData();
        }

        private void StopTicker_Click(object sender, RoutedEventArgs e)
        {
            PoloniexClient.Live.Stop();
            TickerData.Items.Clear();
        }
        #endregion

        //***************************************************************************************

        //Get Trades
        public async Task GetOpenTrades()
        {
            CurrencyPair symbol = GetSymbolCode(symbol1, symbol2);

            DateTime startdate = new DateTime(2017, 1, 1);
            DateTime enddate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 0);

            var opentrades = await PoloniexClient.Trading.GetTradesAsync(symbol, startdate, enddate);

            OpenTradesDataGrid.Items.Clear();

            foreach (var t in opentrades)
            {
                // Populate Grid with Open Trades
                OpenTradesDataGrid.Items.Add(t);

                if (t.Type == OrderType.Buy && t.PricePerCoin > currentTickerPrice || t.Type == OrderType.Sell && t.PricePerCoin < currentTickerPrice)
                {
                    // Set row to red if price is under current price
                    OpenTradesDataGrid.RowBackground = System.Windows.Media.Brushes.IndianRed;
                }
                else if (t.Type == OrderType.Buy && t.PricePerCoin < currentTickerPrice || t.Type == OrderType.Sell && t.PricePerCoin > currentTickerPrice)
                {
                    // Set row to green if price is over current price
                    OpenTradesDataGrid.RowBackground = System.Windows.Media.Brushes.ForestGreen;
                }
            }

            OpenTradesDataGrid.Items.Refresh();
        }


        #region Order Processing
        //Get Trades
        public async Task GetOpenOrders()
        {
            CurrencyPair symbol = GetSymbolCode(symbol1, symbol2);

            var openorders = await PoloniexClient.Trading.GetOpenOrdersAsync(symbol);

            OpenOrdersDataGrid.Items.Clear();

            foreach (var o in openorders)
            {
                // Populate Grid with Open Trades
                OpenOrdersDataGrid.Items.Add(o);
            }

            OpenOrdersDataGrid.Items.Refresh();
        }

        //Close Order
        public async Task CloseOpenOrder(string orderID)
        {
            // Get/Set Symbol Code
            CurrencyPair symbol = GetSymbolCode(symbol1, symbol2);

            // Close order using order ID
            var closeorder = await PoloniexClient.Trading.DeleteOrderAsync(symbol, Convert.ToUInt64(orderID));

        }

        private void OpenOrdersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get orders dataGrid Item
            object item = OpenOrdersDataGrid.SelectedItem;

            // Get orders datagrid cell #4 - Order ID
            var orderAmount = (OpenOrdersDataGrid.SelectedCells[0].Column.GetCellContent(item) as TextBlock).Text;

            orderID = (OpenOrdersDataGrid.SelectedCells[4].Column.GetCellContent(item) as TextBlock).Text;
            // Show message box to confirm order cancellation
            MessageBox.Show(orderAmount + " " + symbol2 + symbol1 + " " + "OrderID = " + orderID, "Order Selected");
        }

        // Close button click
        private async void CloseOrder_Click(object sender, RoutedEventArgs e)
        {
            object item = OpenOrdersDataGrid.SelectedItem;
            var orderAmount = (OpenOrdersDataGrid.SelectedCells[0].Column.GetCellContent(item) as TextBlock).Text;

            // Warning about closing order
            MessageBoxResult result = MessageBox.Show(this, "Do you close order of " + orderAmount + " " + symbol2 + symbol1,
            "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

            if (result == MessageBoxResult.OK)

            {
                // Close selected open order
                await CloseOpenOrder(orderID);
                OpenOrdersDataGrid.Items.Refresh();
            }
        }
        #endregion

        //***************************************************************************************

        #region Submit Orders
        // Method to get market data
        private async Task<IDictionary<CurrencyPair, IMarketData>> GetMarketInfo()
        {
            return await PoloniexClient.Markets.GetSummaryAsync();
        }

        // Execute Limit Order
        public async Task ExecuteLimitOrder(string symbol1, string symbol2, OrderType orderType)
        {
            CurrencyPair symbol = GetSymbolCode(symbol1, symbol2);

            if (orderType == OrderType.Buy)
            {
                var ExecuteBuyOrder = await PoloniexClient.Trading.PostOrderAsync(symbol, OrderType.Buy, priceLimit, quantity);
            }
            else if (orderType == OrderType.Sell)
            {
                var ExecuteSellOrder = await PoloniexClient.Trading.PostOrderAsync(symbol, OrderType.Sell, priceLimit, quantity);
            }
        }

        // Execute Market Order from top of order book
        public async Task ExecuteMarketOrder(string symbol1, string symbol2, OrderType orderType, double quantity)
        {
            CurrencyPair symbol = GetSymbolCode(symbol1, symbol2);
            IDictionary<CurrencyPair, IMarketData> marketinfo = await GetMarketInfo();

            if (orderType == OrderType.Buy)
            {
                double currentPrice = CurrentMarketPrice(symbol1, symbol2, OrderType.Buy, marketinfo);
                var ExecuteBuyOrder = await PoloniexClient.Trading.PostOrderAsync(symbol, OrderType.Buy, currentPrice, quantity);

            }
            else if (orderType == OrderType.Sell)
            {
                double currentPrice = CurrentMarketPrice(symbol1, symbol2, OrderType.Sell, marketinfo);
                var ExecuteSellOrder = await PoloniexClient.Trading.PostOrderAsync(symbol, OrderType.Sell, currentPrice, quantity);
            }
        }
        #endregion

        //***************************************************************************************

        //Get Top OrderBook Quote
        private static double CurrentMarketPrice(string symbol1, string symbol2, OrderType orderType, IDictionary<CurrencyPair, IMarketData> marketinfo)
        {
            double price = 0;

            foreach (var m in marketinfo)
            {
                if (m.Key.BaseCurrency == symbol1 && m.Key.QuoteCurrency == symbol2)
                {
                    if (orderType == OrderType.Buy)
                    {
                        price = m.Value.OrderTopBuy;
                    }
                    else if (orderType == OrderType.Sell)
                    {
                        price = m.Value.OrderTopBuy;
                    }
                }
            }
            return price;
        }

        //***************************************************************************************

        //Zero balance checkbox - Checkced
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Handle(sender as CheckBox);
        }

        //Zero balance checkbox - Uncheckced
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Handle(sender as CheckBox);
        }

        //
        void Handle(CheckBox checkBox)
        {
            // Refresh balance
            BalanceGrid.Items.Refresh();
        }

        //***************************************************************************************

        #region Display Balances and Current Price
        // Get and show current market price in textbox - Not used for calculations
        private async void DisplayCurrentPrice(string symbol1, string symbol2)
        {
            IDictionary<CurrencyPair, IMarketData> marketinfo = await GetMarketInfo();

            double currentPrice = CurrentMarketPrice(symbol1, symbol2, OrderType.Buy, marketinfo);

            Price.Text = currentPrice.ToStringNormalized();
        }

        // Get / Set and return BTC balance
        private async Task<double> GetBalanceBTC()
        {
            double balanceBTC = 0;
            var wallet = await PoloniexClient.Wallet.GetBalancesAsync();

            foreach (var b in wallet)
            {
                if (b.Key == symbol1)
                    BalanceDisplay.Text = b.Value.BitcoinValue.ToStringNormalized();
                balanceBTC = b.Value.BitcoinValue;
            }
            return balanceBTC;

        }

        // Get / Set and return Quote balance
        private async Task<double> GetBalanceQuote()
        {
            double balanceQuote = 0;
            double totalAll = 0;
            var wallet = await PoloniexClient.Wallet.GetBalancesAsync();

            BalanceGrid.Items.Clear();

            // Show quote balances in Balance Datagrid
            foreach (var b in wallet)
            {
                if (ShowZeroBalance.IsChecked == true)
                    BalanceGrid.Items.Add(b);

                else if (ShowZeroBalance.IsChecked == false)
                {
                    if (b.Value.QuoteAvailable > 0)
                        BalanceGrid.Items.Add(b);
                    totalAll += b.Value.BitcoinValue;
                    TotalBalanceAll.Text = totalAll.ToString();
                }

            }

            BalanceGrid.Items.Refresh();

            foreach (var b in wallet)
            {
                if (b.Key == symbol2)
                {
                    BalanceDisplayQuote.Text = b.Value.QuoteAvailable.ToString();
                    QuoteValue.Text = b.Value.BitcoinValue.ToStringNormalized();
                    balanceQuote = b.Value.BitcoinValue;
                }
            }
            return balanceQuote;

        }

        // Get deposit address
        private async void GetDepositAddress(string symbol)
        {
            var wallet = await PoloniexClient.Wallet.GetDepositAddressesAsync();

            foreach (var w in wallet)
            {
                if (w.Key == symbol)
                {
                    BalanceDisplay.Text = w.Value;
                }
            }
        } 
        #endregion

        //***************************************************************************************

        private void OrderSymbolBox()
        {
            // ... Set Symbols for trading panel

            CostPerCoin_Base.Clear();
            CostPerCoin_Base_Limit.Clear();
            Quantity_Quote.Clear();
            TotalCost_Base.Clear();

            CostPerCoin_Base.Text = symbol1;
            CostPerCoin_Base_Limit.Text = symbol1;
            Quantity_Quote.Text = symbol2;
            TotalCost_Base.Text = symbol1;

        }

        //***************************************************************************************

        //MarketSeries ComboBox
        private void MarketSeriesSelect_Loaded(object sender, RoutedEventArgs e)
        {
            MarketSeriesSelect.Items.Add(MarketPeriod.Minutes5);
            MarketSeriesSelect.Items.Add(MarketPeriod.Minutes15);
            MarketSeriesSelect.Items.Add(MarketPeriod.Minutes30);
            MarketSeriesSelect.Items.Add(MarketPeriod.Hours2);
            MarketSeriesSelect.Items.Add(MarketPeriod.Hours4);
            MarketSeriesSelect.Items.Add(MarketPeriod.Day);
                
            // ... Get the ComboBox reference.
            var MarketSeriesSelect_ComboBox = sender as ComboBox;

            // ... Make the first item selected.
            MarketSeriesSelect_ComboBox.SelectedIndex = 0;

        }

        //MarketSeries ComboBox Selection
        private void MarketSeriesSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // ... Get the ComboBox.
            var comboBox = sender as ComboBox;
            // ... Set SelectedItem MarketSeries
            marketseries = (MarketPeriod)(comboBox.SelectedItem);
        }


        //***************************************************************************************


        #region Setting Symbols

        private async void SymbolSelect_Loaded(object sender, RoutedEventArgs e)
        {
            await SetSymbols();
        }

        private async Task SetSymbols()
        {
            IDictionary<CurrencyPair, IMarketData> markets = await GetMarketInfo();

            BaseSymbolSelect.Items.Clear();
            foreach (var baseSymbol in (markets.OrderBy(i => i.Key.BaseCurrency)))
            {
                if (symbol2 == null)
                    symbol2 = "ETH";
                else
                if (baseSymbol.Key.QuoteCurrency == symbol2)

                    BaseSymbolSelect.Items.Add(baseSymbol.Key.BaseCurrency);
            }
            BaseSymbolSelect.Items.Refresh();

            BaseSymbolSelect.SelectedIndex = 0;


            QuoteSymbolSelect.Items.Clear();
            foreach (var quoteSymbol in (markets.OrderBy(i => i.Key.QuoteCurrency)))
            {
                if (symbol1 == null)
                    symbol1 = "BTC";
                else
                if (quoteSymbol.Key.BaseCurrency == symbol1)

                    QuoteSymbolSelect.Items.Add(quoteSymbol.Key.QuoteCurrency);
            }

            QuoteSymbolSelect.Items.Refresh();

            // Set Default Symbol 2 to ETH
            QuoteSymbolSelect.SelectedIndex = 17;

            symbol1 = BaseSymbolSelect.SelectedItem as string;
            symbol2 = QuoteSymbolSelect.SelectedItem as string;

        }

        //Symbol changed
        void BaseSymbolSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveSymbols(sender as ComboBox);            
        }

        void QuoteSymbolSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveSymbols(sender as ComboBox);            
        }

        private void SaveSymbols(ComboBox comboBox)
        {           
            symbol1 = BaseSymbolSelect.SelectedItem as string;
            symbol2 = QuoteSymbolSelect.SelectedItem as string;
        }

        #endregion

        //***************************************************************************************

        #region TradePanel
        void PriceCalculation_TextChanged(object sender, TextChangedEventArgs e)
        {
            TradePanelCalculator(sender as TextBox);
        }

        private void TradePanelCalculator(TextBox textbox)
        {
            // Do Calculations here
            try
            {
                if (Quantity.Text == string.Empty)
                {
                    Quantity.Text = "0";
                }
                else

                    quantity = double.Parse(Quantity.Text);

                if (LimitPrice.Text == string.Empty)
                {
                    LimitPrice.Text = "0";
                }
                else

                    priceLimit = double.Parse(LimitPrice.Text);

                double total = priceLimit * quantity;

                if (total < 0.001)
                {
                    Total.Text = "0.001";
                    //total = 0;
                    Total.Foreground = System.Windows.Media.Brushes.IndianRed;
                }
                else
                {
                    Total.Text = total.ToString();
                    //Quantity.Text = (double.Parse(Total.Text) / double.Parse(LimitPrice.Text)).ToString();
                    Total.Foreground = System.Windows.Media.Brushes.Black;
                }

            }
            catch (NullReferenceException)
            {
            }

        }

        private void Sell_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Sell_Button.Dispatcher.Invoke(new Action(async () =>
                {
                    await ExecuteMarketOrder(symbol1, symbol2, OrderType.Sell, double.Parse(Quantity.Text));
                }));

            }
            catch (Exception error)
            {
                MessageBox.Show("Order Error" + error.Message);
            }
        }

        private void Buy_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Buy_Button.Dispatcher.Invoke(new Action(async () =>
                {
                    await ExecuteMarketOrder(symbol1, symbol2, OrderType.Buy, double.Parse(Quantity.Text));
                }));

            }
            catch (Exception error)
            {
                MessageBox.Show("Order Error" + error.Message);
            }
        }

        private void SellLimit_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SellLimit_Button.Dispatcher.Invoke(new Action(async () =>
                {
                    await ExecuteLimitOrder(symbol1, symbol2, OrderType.Sell);
                }));

                OpenOrdersDataGrid.Items.Refresh();

            }
            catch (Exception error)
            {
                MessageBox.Show("Order Error" + error.Message);
            }
        }

        private void BuyLimit_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BuyLimit_Button.Dispatcher.Invoke(new Action(async () =>
                {
                    await ExecuteLimitOrder(symbol1, symbol2, OrderType.Buy);
                }));

                OpenOrdersDataGrid.Items.Refresh();

            }
            catch (Exception error)
            {
                MessageBox.Show("Order Error" + error.Message);
            }
        }
        #endregion
    

        //***************************************************************************************


        #region Strategies

        public async Task GetHistorialData()
        {
            CurrencyPair symbol = GetSymbolCode(symbol1, symbol2);

            DateTime startdate = new DateTime(2017, 1, 1);
            DateTime enddate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 0);

            var canldeinfo = await PoloniexClient.Markets.GetChartDataAsync(symbol, marketseries, startdate, enddate);
            var candleindex = canldeinfo.Count() - 1;

            //string closeprices = String.Format("{0} {1}", canldeinfo[candleindex].Close, canldeinfo[candleindex].Time);

            //var v = canldeinfo[candleindex].Time.ToString();

            // Last 20 Closes
            //for (int i = candleindex - 1; i >= candleindex - 20; i--)
            //{
            //string closeprices = String.Format("{0}", canldeinfo[i].Close.ToStringNormalized());
            // Add to Textblock
            //ClosePriceTextBlock.AppendText(closeprices += Environment.NewLine);
            //}             
        }

        //public async Task IBSStrategy()
        public async void IBSCalculation()
        {
            CurrencyPair symbol = GetSymbolCode(symbol1, symbol2);

            DateTime startdate = new DateTime(2017, 1, 1);
            DateTime enddate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 0);

            var canldeinfo = await PoloniexClient.Markets.GetChartDataAsync(symbol, marketseries, startdate, enddate);
            var candleindex = canldeinfo.Count() - 1;

            //string closeprices = String.Format("{0}", canldeinfo[i].Close.ToStringNormalized());

            IBS = ((canldeinfo[candleindex].Close - canldeinfo[candleindex - 1].Low) / (canldeinfo[candleindex].High - canldeinfo[candleindex].Low)) * 100;

            //IBS_Value.Text = Math.Round(IBS, 0).ToString() + " %";

            // ADR
            for (int i = candleindex; i >= candleindex - 20; i--)
            {
                range_adr += (canldeinfo[i].High - canldeinfo[i].Low);
            }

            range_adr /= candleindex - 20;
            range_adr = Math.Round(range_adr, 10);

            //ADRValue.Text = range_adr.ToStringNormalized();
        }

        // Percentage Change
        public async Task PercentChange(MarketPeriod marketseries)
        {
            CurrencyPair symbol = GetSymbolCode(symbol1, symbol2);

            double balance = await GetBalanceQuote();

            DateTime startdate = new DateTime(2017, 1, 1);
            DateTime enddate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 0);

            var CandleSeries = await PoloniexClient.Markets.GetChartDataAsync(symbol, marketseries, startdate, enddate);
            var index = CandleSeries.Count() - 1;

            //Percent Drop Calc
            double currentBarRange = CandleSeries[index].High - CandleSeries[index].Low;

            double percentChange = (currentBarRange * 100) / CandleSeries[index].Close;

            //double percentRange = ((CandleSeries[index].Close - CandleSeries[index-1].Close) / CandleSeries[index].Close) * 100;

            //Buy
            try
            {
                if (CandleSeries[index].Close > CandleSeries[index].Open && percentChange >= 1)
                {

                    Sell_Button.Dispatcher.Invoke(new Action(async () =>
                    {
                        await ExecuteMarketOrder(symbol1, symbol2, OrderType.Buy, double.Parse(Quantity.Text));
                    }));
                }

                else if (CandleSeries[index].Close < CandleSeries[index].Open && percentChange >= 10)
                {
                    if (balance > minOrderSize)

                    {

                        Sell_Button.Dispatcher.Invoke(new Action(async () =>
                        {
                            await ExecuteMarketOrder(symbol1, symbol2, OrderType.Sell, double.Parse(Quantity.Text));
                        }));
                    }
                }
            }
            catch (Exception error)
            {
                MessageBox.Show("Order Error" + error.Message);
            }
        }
        

        // ADR Calculation

        public async void ADR(MarketPeriod marketseries)
        {
            CurrencyPair symbol = GetSymbolCode(symbol1, symbol2);
            DateTime startdate = new DateTime(2017, 1, 1);
            DateTime enddate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 0);

            var canldeinfo = await PoloniexClient.Markets.GetChartDataAsync(symbol, marketseries, startdate, enddate);
            var candleindex = canldeinfo.Count() - 1;

        }

        //timer.Interval = TimeSpan.FromMinutes(Convert.ToDouble(marketseries));

        /*
            public async void IBSStrategy()
        {

            if (IBS < 10)
            {
                //Send buy order
                await Task.Run(() => ExecuteMarketOrder(symbol1, symbol2, OrderType.Buy, quantity));
            }
            else if (IBS > 90)
            {
                //Send sell order
                await Task.Run(() => ExecuteMarketOrder(symbol1, symbol2, OrderType.Sell, quantity));
            }

        }*/

        #endregion

        //private async void IBS_AutoTrade_Click(object sender, RoutedEventArgs e)
        //{
        // }
    }
}
