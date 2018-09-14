CREATE TABLE dbo.StockPrice2
(
    Ticker CHAR(4) NOT NULL,
	Price INT NOT NULL,
	QuoteTime DATETIME NOT NULL
)
WITH
(
    DISTRIBUTION = HASH(Ticker)
)
GO

INSERT INTO dbo.StockPrice2
SELECT *
FROM dbo.StockPrice
GO

SELECT Ticker, MAX(Price) AS HighestPrice
FROM dbo.StockPrice2
GROUP BY Ticker
ORDER BY HighestPrice DESC
GO