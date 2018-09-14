CREATE TABLE StockPrice
(
    Ticker char(4) NOT NULL,
    Price int NOT NULL,
    QuoteTime DateTime NOT NULL
)
WITH
(
    DISTRIBUTION = ROUND_ROBIN
)