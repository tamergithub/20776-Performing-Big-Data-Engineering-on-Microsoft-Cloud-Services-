install.packages("tidyverse")
library(tidyverse)

dataPath <- ("D:/DemoFiles/Mod06/Demo 3")
inFile <- file.path(dataPath, "StockPriceData.csv")
outFile <- file.path(dataPath, "StockPriceData.xdf")
stockDataXDF <- rxImport(inFile, outFile, overwrite = TRUE)
head(stockDataXDF)
rxGetVarInfo(stockDataXDF)

refactoredStockDataXDF <- rxFactors(stockDataXDF, c("ticker"))
head(refactoredStockDataXDF)
rxGetVarInfo(refactoredStockDataXDF)

rxSummary(price~ticker, data = refactoredStockDataXDF)

results <- refactoredStockDataXDF %>%
group_by(ticker) %>%
summarize(open = first(price), lowest = min(price), highest = max(price), close = last(price))

resultsFile <- file.path(dataPath, "StockPriceSummary.csv")
write.csv(results, file = resultsFile)

refactoredStockDataXDF %>%
mutate(seconds = unclass(as.POSIXct(quotetime))) %>%
filter(ticker == "SKGG") %>%
arrange(seconds)

dataPath <- ("D:/DemoFiles/Mod06/Demo 3")
inFile <- file.path(dataPath, "StockPriceData.csv")
stockData <- rxImport(inFile, outFile = NULL)
colnames(stockData) <- c("ticker", "price", "quotetime")
refactoredStockData <- rxFactors(stockData, c("ticker"))
sortedData <- refactoredStockData[order(refactoredStockData[,3]),]
openingPrices <- aggregate(price ~ ticker, sortedData, head)
closingPrices <- aggregate(price ~ ticker, sortedData, tail)
lowestPrices <- aggregate(price ~ ticker, sortedData, min)
highestPrices <- aggregate(price ~ ticker, sortedData, max)


outputToUSQL <- data.frame(openingPrices[1], openingPrices[[2]][,1], closingPrices[[2]][,6], lowestPrices[2], highestPrices[2])
colnames(outputToUSQL) <- c("ticker", "openingPrice", "closingPrice", "lowestPrice", "highestPrice")
