# MetroBin

Simple [Feedbin](https://github.com/feedbin/feedbin-api) client for Windows Phone 8.1. I didn't have time to finish it before Microsoft ditched WP8.1 platform for UWP in Windows 10. 

![MyImage](https://github.com/revanmj/FeedbinWP/raw/master/metrobin0.jpeg) ![MyImage](https://github.com/revanmj/FeedbinWP/raw/master/metrobin1.jpeg)

Since I won't be updating this one, I decided to publish code for archive purposes (quality isn't that great, so I wouldn't try learning from it - for example code responsible for handling SQLite and Feedbin queries is in one class instead of two or three).

App can download and display list of articles (ArticleListPage) that can be opened in separate view (ArticlePage) which supports sharing a link, marking item as starred and parsing article via [Readability service](https://www.readability.com/developers/api/parser) in order to get its full contents.
