# Poloniex Trading Dashboard

---------------------------------------

A WPF dashboard for trading on Poloniex Exchange.

I have used the Trading View widget for displaying the charts
This includes all the indicators and tools available on tradingview
** You cannot trade off the chart directly, merely a visual tool **

# Warning about API keys and cloning / forking this project
** GitIgnore App.config or don't upload your saved keys to github! Please take note!!!! **

# Usage
** Dummy API key loaded to get app running **
1. Open API key entry window, enter you own API keys from Poloniex
2. Save keys and restart app

![API Keys](https://github.com/ColossusFX/PoloniexTradingDashboard/blob/master/Screenshot_1.jpg?raw=true "API Key Entry")

3. Select Quote and Base currency to trade
4. Press the refresh button to fech the data 
** Trading view widget can take a while to load, be patient **

![Refresh](https://github.com/ColossusFX/PoloniexTradingDashboard/blob/master/Screenshot_2.jpg?raw=true "Refresh")

## Features
- Buy / Sell Market Order from top of order book (need to change this to use Tick Data like Poloniex Auto Trader)
- Buy / Sell Limit ordes
- Show past trades
- Show open ordes
- Tick data stream datagrid
- Shows balances
 
#### To Do
- This is a working demo I made to test all the functions of the PoloniexApi.Net project
- There are bugs, but it will works to trade manually with

#### Dependencies
Make sure to download, build and reference my fork of PoloniexApi.Net from my repo.

## Contribute
Check out the [contribution guidelines](CONTRIBUTING.md)
if you want to contribute to this project.

## License
[Apache 2.0](LICENSE)

## Thanks
Thanks to https://github.com/afhacker for helping
