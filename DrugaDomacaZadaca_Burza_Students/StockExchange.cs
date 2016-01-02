using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DrugaDomacaZadaca_Burza
{
    public static class Factory
    {
        public static IStockExchange CreateStockExchange()
        {
            return new StockExchange();
        }
    }

    public class StockExchange : IStockExchange
    {
        CurrentStocks currentStocks = new CurrentStocks();
        CurrentIndexes currentIndexes = new CurrentIndexes();
        CurrentPortfolio currentPortfolio = new CurrentPortfolio();

        public void ListStock(string inStockName, long inNumberOfShares, decimal inInitialPrice, DateTime inTimeStamp)
        {
            currentStocks.addStock(inStockName, inNumberOfShares, inInitialPrice, inTimeStamp);
        }

        public void DelistStock(string inStockName)
        {
            currentStocks.deleteStock(inStockName);
            currentIndexes.removeStock(inStockName);
            currentPortfolio.removeStock(inStockName);
        }

        public bool StockExists(string inStockName)
        {
            return currentStocks.stockWithNameExists(inStockName);
        }

        public int NumberOfStocks()
        {
            return currentStocks.numberOfStocks();
        }

        public void SetStockPrice(string inStockName, DateTime inIimeStamp, decimal inStockValue)
        {
            Stock stock = currentStocks.getStock(inStockName);
            stock.setPrice(inIimeStamp, inStockValue);
        }

        public decimal GetStockPrice(string inStockName, DateTime inTimeStamp)
        {
            Stock stock = currentStocks.getStock(inStockName);
            return stock.getPrice(inTimeStamp);
        }

        public decimal GetInitialStockPrice(string inStockName)
        {
            Stock stock = currentStocks.getStock(inStockName);
            return stock.getInitialPrice();
        }

        public decimal GetLastStockPrice(string inStockName)
        {
            Stock stock = currentStocks.getStock(inStockName);
            return stock.getLastPrice();
        }

        public void CreateIndex(string inIndexName, IndexTypes inIndexType)
        {
            currentIndexes.addIndex(inIndexName, inIndexType);
        }

        public void AddStockToIndex(string inIndexName, string inStockName)
        {
            Stock stock = currentStocks.getStock(inStockName);
            Index index = currentIndexes.getIndex(inIndexName);

            index.addStock(stock);
        }

        public void RemoveStockFromIndex(string inIndexName, string inStockName)
        {
            Stock stock = currentStocks.getStock(inStockName);
            Index index = currentIndexes.getIndex(inIndexName);

            index.removeStock(stock);
        }

        public bool IsStockPartOfIndex(string inIndexName, string inStockName)
        {
            Stock stock = currentStocks.getStock(inStockName);
            Index index = currentIndexes.getIndex(inIndexName);

            return index.hasStock(stock);
        }

        public decimal GetIndexValue(string inIndexName, DateTime inTimeStamp)
        {
            Index index = currentIndexes.getIndex(inIndexName);

            return index.getValue();
        }

        public bool IndexExists(string inIndexName)
        {
            return currentIndexes.checkIfIndexExists(inIndexName);
        }

        public int NumberOfIndices()
        {
            return currentIndexes.numberOfIndicies();
        }

        public int NumberOfStocksInIndex(string inIndexName)
        {
            Index index = currentIndexes.getIndex(inIndexName);
            return index.numberOfStocks();
        }

        public void CreatePortfolio(string inPortfolioID)
        {
            currentPortfolio.addPortfolio(inPortfolioID);
        }

        public void AddStockToPortfolio(string inPortfolioID, string inStockName, int numberOfShares)
        {
            Stock stock = currentStocks.getStock(inStockName);

            long numberOfAvailable = currentPortfolio.numberOfStockAvailable(stock);

            if (numberOfAvailable >= numberOfShares)
            {
                Portfolio portfolio = currentPortfolio.getPortfolio(inPortfolioID);
                portfolio.addStock(stock, numberOfShares);
            }
            else
            {
                throw new StockExchangeException("Not enaught stocks available");
            }

        }

        public void RemoveStockFromPortfolio(string inPortfolioID, string inStockName, int numberOfShares)
        {
            Stock stock = currentStocks.getStock(inStockName);
            Portfolio portfolio = currentPortfolio.getPortfolio(inPortfolioID);

            portfolio.removeStock(stock, numberOfShares);
        }

        public void RemoveStockFromPortfolio(string inPortfolioID, string inStockName)
        {
            Stock stock = currentStocks.getStock(inStockName);
            Portfolio portfolio = currentPortfolio.getPortfolio(inPortfolioID);

            portfolio.removeStock(stock);
        }

        public int NumberOfPortfolios()
        {
            return currentPortfolio.numberOfPortfolios();
        }

        public int NumberOfStocksInPortfolio(string inPortfolioID)
        {
            Portfolio portfolio = currentPortfolio.getPortfolio(inPortfolioID);
            return portfolio.numberOfStocks();
        }

        public bool PortfolioExists(string inPortfolioID)
        {
            return currentPortfolio.checkIfPortfolioExsists(inPortfolioID);
        }

        public bool IsStockPartOfPortfolio(string inPortfolioID, string inStockName)
        {
            Stock stock = currentStocks.getStock(inStockName);
            Portfolio portfolio = currentPortfolio.getPortfolio(inPortfolioID);

            return portfolio.checkIfStockExists(stock);
        }

        public int NumberOfSharesOfStockInPortfolio(string inPortfolioID, string inStockName)
        {
            Portfolio portfolio = currentPortfolio.getPortfolio(inPortfolioID);

            return portfolio.numberOfShares(inStockName);
        }

        public decimal GetPortfolioValue(string inPortfolioID, DateTime timeStamp)
        {
            Portfolio portfolio = currentPortfolio.getPortfolio(inPortfolioID);
            return portfolio.totalValue(DateTime.Now);
        }

        public decimal GetPortfolioPercentChangeInValueForMonth(string inPortfolioID, int Year, int Month)
        {
            Portfolio portfolio = currentPortfolio.getPortfolio(inPortfolioID);
            return portfolio.getMonthlyChange(Year, Month);
        }
    }

    class Stock : IEquatable<Stock>
    {
        public String name;
        public long numberOfShares;
        public List<StockPrice> prices;

        public Stock(string name, long numberOfShares, List<StockPrice> prices)
        {
            this.name = name;
            this.numberOfShares = numberOfShares;
            this.prices = prices;
        }

        public bool checkName(string name)
        {
            return this.name.ToLower() == name.ToLower();
        }

        public Decimal getPrice(DateTime timeStamp)
        {
            if (prices.Count < 1)
            {
                throw new StockExchangeException("Stock has 0 prices");
            }

            for (int i = 0; i < prices.Count; i++)
            {
                if (prices[i].timeStamp > timeStamp)
                {
                    if (i == 0)
                    {
                        throw new StockExchangeException("Stock does not have defined price for given datetime");
                    }
                    else
                    {
                        return prices[i - 1].price;
                    }
                }
            }

            return prices.Last().price;
        }

        public Decimal getInitialPrice()
        {
            if (prices.Count == 0)
            {
                throw new StockExchangeException("Stock does not have initial price");
            }

            return prices[0].price;
        }

        public Decimal getLastPrice()
        {
            if (prices.Count == 0)
            {
                throw new StockExchangeException("Stock does not have prices");
            }

            return prices.Last().price;
        }

        public void setPrice(DateTime timeStamp, Decimal price)
        {
            if (checkIfPriceForTimeExists(timeStamp))
            {
                throw new StockExchangeException("Time stamp exists");
            }

            prices.Add(
                new StockPrice(price, timeStamp)
            );

            prices.Sort();
        }

        private bool checkIfPriceForTimeExists(DateTime timeStamp)
        {
            if (prices.Where(x => Math.Abs((x.timeStamp - timeStamp).TotalMilliseconds) < 1).Count() > 0)
            {
                return true;
            }

            return false;
        }

        // IEqutable

        public override string ToString()
        {
            return name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Stock objAsPart = obj as Stock;
            if (objAsPart == null) return false;
            else return Equals(objAsPart);
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public bool Equals(Stock other)
        {
            if (other == null) return false;
            return (name.ToLower().Equals(other.name.ToLower()));
        }

        public Decimal getTotalPrice()
        {
            return numberOfShares * getPrice(DateTime.Now);
        }

    }

    class StockPrice : IEquatable<StockPrice>, IComparable<StockPrice>
    {
        public Decimal price;
        public DateTime timeStamp;

        public StockPrice(decimal price, DateTime timeStamp)
        {
            this.price = price;
            this.timeStamp = timeStamp;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            StockPrice objAsPart = obj as StockPrice;
            if (objAsPart == null) return false;
            else return Equals(objAsPart);
        }

        public int SortByTimeStampAscending(DateTime timeStamp1, DateTime timeStamp2)
        {
            return timeStamp1.CompareTo(timeStamp2);
        }

        // Default comparer for Part type.
        public int CompareTo(StockPrice comparePart)
        {
            // A null value means that this object is greater.
            if (comparePart == null)
                return 1;

            else
                return this.timeStamp.CompareTo(comparePart.timeStamp);
        }

        public override int GetHashCode()
        {
            return timeStamp.GetHashCode();
        }

        public bool Equals(StockPrice other)
        {
            if (other == null) return false;
            return (this.timeStamp.Equals(other.timeStamp));
        }
    }

    class CurrentStocks
    {
        private List<Stock> stocks;

        public CurrentStocks()
        {
            stocks = new List<Stock>();
        }

        public void addStock(string name, long numberOfShares, Decimal price, DateTime timeStamp)
        {

            if (stockWithNameExists(name))
            {
                throw new StockExchangeException("Stock with name " + name + " already exists");
            }

            if (numberOfShares < 1)
            {
                throw new StockExchangeException("Stock cannot have less than 1 share");
            }

            if (price <= 0)
            {
                throw new StockExchangeException("Stock must have possitive price");
            }

            stocks.Add(
                new Stock(name: name,
                numberOfShares: numberOfShares,

                prices: new List<StockPrice>(){
                    new StockPrice(
                        price: price,
                        timeStamp: timeStamp)
                })
            );
        }

        public void deleteStock(string name)
        {
            stocks.RemoveAt(getStockIndex(name));
        }

        public bool stockWithNameExists(string name)
        {
            if (stocks.Where(x => x.checkName(name)).Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int numberOfStocks()
        {
            return stocks.Count;
        }


        private int getStockIndex(string name)
        {
            for (int i = 0; i < stocks.Count(); i++)
            {
                if (stocks[i].name.ToLower() == name.ToLower())
                {
                    return i;
                }
            }

            throw new StockExchangeException("Stock does not exist");
        }

        public Stock getStock(string name)
        {
            for (int i = 0; i < stocks.Count(); i++)
            {
                if (stocks[i].checkName(name))
                {
                    return stocks[i];
                }
            }

            throw new StockExchangeException("Stock does not exist");
        }
    }


    class Index
    {
        public string name;
        public IndexTypes type;
        public List<Stock> stocks;

        public Index(string name, IndexTypes type)
        {
            this.name = name;
            this.type = type;
            this.stocks = new List<Stock>();
        }

        public void addStock(Stock stock)
        {
            if (hasStock(stock))
            {
                throw new StockExchangeException("Index " + name + " already contains stock " + stock.name);
            }

            stocks.Add(stock);
        }

        public void removeStock(Stock stock)
        {
            if (!hasStock(stock))
            {
                throw new StockExchangeException("Index " + name + " does not contain stock " + stock.name);
            }

            stocks.Remove(stock);
        }

        public bool hasStock(Stock stock)
        {
            foreach (Stock stock2 in stocks)
            {
                if (stock.checkName(stock2.name))
                {
                    return true;
                }
            }

            return false;
        }

        public int numberOfStocks()
        {
            return stocks.Count;
        }

        public decimal getValue()
        {
            decimal value = 0;

            if (type == IndexTypes.AVERAGE)
            {
                long numberOfStocks = 0;

                foreach (Stock stock in stocks)
                {
                    value += stock.getTotalPrice();
                    numberOfStocks += stock.numberOfShares;
                }

                value = value / numberOfStocks;
            }
            else
            {
                decimal totalVal = totalValue();

                foreach (Stock stock in stocks)
                {
                    value += stock.getPrice(DateTime.Now) * (stock.getTotalPrice() / totalVal);
                }
            }

            return decimal.Round(value, 3);
        }

        private Decimal totalValue()
        {
            Decimal value = 0;

            foreach (Stock stock in stocks)
            {
                value += stock.getTotalPrice();
            }

            return value;
        }
    }



    class CurrentIndexes
    {
        public List<Index> indexes = new List<Index>();

        public void addIndex(string name, IndexTypes type)
        {

            if (checkIfIndexExists(name))
            {
                throw new StockExchangeException("Index with name " + name + " already exists");
            }

            indexes.Add(
                new Index(name, type)
            );
        }

        public Index getIndex(string name)
        {
            foreach (Index index in indexes)
            {
                if (index.name.ToLower() == name.ToLower())
                {
                    return index;
                }
            }

            throw new StockExchangeException("Index with name " + name + " does not exist");
        }

        public bool checkIfIndexExists(string name)
        {
            foreach (Index index in indexes)
            {
                if (index.name.ToLower() == name.ToLower())
                {
                    return true;
                }
            }

            return false;

        }

        public int numberOfIndicies()
        {
            return indexes.Count;
        }

        public void removeStock(string name)
        {
            for (int i = 0; i < indexes.Count; i++)
            {
                for (int j = 0; j < indexes[i].stocks.Count; j++)
                {
                    if (indexes[i].stocks[j].checkName(name))
                    {
                        indexes[i].stocks.RemoveAt(j);
                        break;
                    }
                }
            }
        }
    }



    class Portfolio
    {
        public string id;
        public List<StockWithCount> stocks;

        public Portfolio(string id)
        {
            this.id = id;
            stocks = new List<StockWithCount>();
        }

        public void addStock(Stock stock, int number)
        {
            StockWithCount stockWithCount = new StockWithCount(stock, 0);

            for (int i = 0; i < stocks.Count; i++)
            {
                if (stocks[i].stock.checkName(stock.name))
                {
                    stockWithCount = stocks[i];
                    stocks.RemoveAt(i);
                    break;
                }
            }

            stockWithCount.count += number;

            stocks.Add(stockWithCount);
        }

        public void removeStock(Stock stock, int number)
        {
            StockWithCount? stock2 = null;

            for (int i = 0; i < stocks.Count; i++)
            {
                if (stocks[i].stock.checkName(stock.name))
                {
                    stock2 = stocks[i];
                    stocks.RemoveAt(i);
                    break;
                }
            }

            if (stock2 != null)
            {
                stocks.Add(
                    new StockWithCount(
                         ((StockWithCount)stock2).stock,
                         ((StockWithCount)stock2).count - number
                        )
                    );
            }
            else
            {
                throw new StockExchangeException("Stock does not exists in portfolio");
            }

            if ((((StockWithCount)stock2).count - number) > 0)
            {
                return;
            }
            else if ((((StockWithCount)stock2).count - number) == 0)
            {
                for (int i = 0; i < stocks.Count; i++)
                {
                    if (stocks[i].stock.checkName(stock.name))
                    {
                        stocks.RemoveAt(i);
                    }
                }
            }
            else
            {
                throw new StockExchangeException("Cannot remove more stocks than are in portfolio");
            }

        }

        public void removeStock(Stock stock)
        {
            for (int i = 0; i < stocks.Count; i++)
            {
                if (stocks[i].stock.checkName(stock.name))
                {
                    stocks.RemoveAt(i);
                    return;
                }
            }

            throw new StockExchangeException("Stock does not exist in portfolio");
        }

        public bool checkIfStockExists(Stock stock)
        {
            foreach (StockWithCount stock2 in stocks)
            {
                if (stock2.stock.checkName(stock.name))
                {
                    return true;
                }
            }

            return false;
        }

        public int numberOfStocks()
        {
            return stocks.Count;
        }

        public int numberOfShares(string name)
        {
            StockWithCount stock = getStock(name);

            return stock.count;
        }

        public Decimal totalValue(DateTime dateTime)
        {
            Decimal value = 0;

            foreach (StockWithCount stockWithCount in stocks)
            {
                value += stockWithCount.stock.getPrice(dateTime) * stockWithCount.count;
            }

            return decimal.Round(value, 3);
        }

        public Decimal getMonthlyChange(int Year, int Month)
        {
            Decimal firstPrice = totalValue(new DateTime(Year, Month, 1, 0, 0, 0));
            Decimal secondPrice = totalValue(new DateTime(Year, Month, DateTime.DaysInMonth(Year, Month), 23, 59, 59, 999));

            return decimal.Round((secondPrice / firstPrice) * 100);
        }

        private StockWithCount getStock(string name)
        {
            foreach (StockWithCount stock in stocks)
            {
                if (stock.stock.checkName(name))
                {
                    return stock;
                }
            }

            throw new StockExchangeException("Portfolio does not conaint stock");
        }

    }

    struct StockWithCount
    {
        public Stock stock;
        public int count;

        public StockWithCount(Stock stock, int count)
        {
            this.stock = stock;
            this.count = count;
        }
    }


    class CurrentPortfolio
    {
        List<Portfolio> portfolios = new List<Portfolio>();

        public void addPortfolio(string name)
        {

            if (checkIfPortfolioExsists(name))
            {
                throw new StockExchangeException("Portfolio with name " + name + "already exists");
            }

            portfolios.Add(
                new Portfolio(name)
                    );
        }

        public Portfolio getPortfolio(string name)
        {
            foreach (Portfolio portfolio in portfolios)
            {
                if (portfolio.id == name)
                {
                    return portfolio;
                }
            }

            throw new StockExchangeException("Portfolio with name " + name + "does not exists");
        }

        public bool checkIfPortfolioExsists(string name)
        {

            foreach (Portfolio portfolio in portfolios)
            {
                if (portfolio.id == name)
                {
                    return true;
                }
            }

            return false;

        }

        public long numberOfStockAvailable(Stock stock)
        {
            int num = 0;

            foreach (Portfolio portfolio in portfolios)
            {
                foreach (StockWithCount stockWithCount in portfolio.stocks)
                {
                    if (stockWithCount.stock.checkName(stock.name))
                    {
                        num += stockWithCount.count;
                    }
                }
            }

            return stock.numberOfShares - num;
        }

        public int numberOfPortfolios()
        {
            return portfolios.Count;
        }

        public void removeStock(string name)
        {
            for (int i = 0; i < portfolios.Count; i++)
            {
                for (int j = 0; j < portfolios[i].stocks.Count; j++)
                {
                    if (portfolios[i].stocks[j].stock.checkName(name))
                    {
                        portfolios[i].stocks.RemoveAt(j);
                        break;
                    }
                }
            }
        }
    }
}