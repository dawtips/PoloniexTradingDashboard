
namespace PoloniexDemo
{
    class Widget
    {
        public string htmlWidget =
                @"
<!DOCTYPE html>
<html lang='en' xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta charset = 'utf-8'/>
    <meta http - equiv = 'X-UA-Compatible' content = 'edge'/>
    <title></title>
</head>
<body>
    <!--TradingView Widget BEGIN-->
    <script type='text/javascript' src='https://d33t3vvu2t2yu5.cloudfront.net/tv.js'></script>
    <script type='text/javascript'>
        new TradingView.widget({
            'symbol': 'POLONIEX:INSERTSYMBOL',
            'width': 1110,
            'height': 640,            
            'interval': 'D',
            'timezone': 'Etc/UTC',
            'theme': 'White',
            'style': '1',
            'locale': 'en',
            'toolbar_bg': '#f1f3f6',
            'enable_publishing': true,
            'withdateranges': true,
            'hide_side_toolbar': false,
            'allow_symbol_change': true,
            'hideideas': true,
            'studies': [
                'MASimple@tv-basicstudies'
            ],
            'show_popup_button': true,
            'popup_width': '1000',
            'popup_height': '650'
        });
    </script>
    <!-- TradingView Widget END -->
</body>
</html>";

        //'toolbar_bg': 'rgba(0,0,0,1)',    
        //'toolbar_bg': '#f1f3f6',
    }
}
