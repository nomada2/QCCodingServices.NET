/*
* QUANTCONNECT.COM - 
* QC.Algorithm - Base Class for Algorithm.
*/

/**********************************************************
* USING NAMESPACES
**********************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;


using QuantConnect.Markets;
using QuantConnect.Models;

namespace QuantConnect {

    /******************************************************** 
    * CLASS DEFINITIONS
    *********************************************************/
    public partial class QCAlgorithm : MarshalByRefObject, IAlgorithm {

        /******************************************************** 
        * CLASS PRIVATE VARIABLES
        *********************************************************/
        private DateTime _time = new DateTime();
        private RunMode _runMode = RunMode.Automatic;
        private bool _locked = false;
        private String _resolution = "";
        private bool _quit = false;
        private List<string> _debugMessages = new List<string>();
        private List<string> _errorMessages = new List<string>();

        /******************************************************** 
        * CLASS PUBLIC VARIABLES
        *********************************************************/
        public virtual EquitiesManager Equities { get; set; }
        public virtual EquitiesPortfolioManager Portfolio { get; set; }
        public virtual EquitiesTransactionManager Transacions { get; set; }

        /******************************************************** 
        * CLASS CONSTRUCTOR
        *********************************************************/
        /// <summary>
        /// Initialise the Algorithm
        /// </summary>
        public QCAlgorithm() { 
            //Initialise the Algorithm Helper Classes:
            Equities = new EquitiesManager();
            Portfolio = new EquitiesPortfolioManager(Equities, null);
            Transacions = new EquitiesTransactionManager(Equities, null);

            Portfolio.Transactions = Transacions;
            Transacions.Portfolio = Portfolio;

            //Initialise Error and Order Holders:
            Errors = new List<string>();

            //Initialise Algorithm RunMode to Automatic:
            _runMode = RunMode.Automatic;

            //Initialise to unlocked:
            _locked = false;
        }

        /******************************************************** 
        * CLASS PROPERTIES
        *********************************************************/
        /// <summary>
        /// Set a public name for the algorithm.
        /// </summary>
        public string Name {
            get;
            set;
        }

        /// <summary>
        /// Get the current date/time.
        /// </summary>
        public DateTime Time {
            get {
                return _time;
            }
        }


        /// <summary>
        /// Catchable Error List.
        /// </summary>
        public List<string> Errors {
            get;
            set;
        }


        /// <summary>
        /// Accessor for Filled Orders:
        /// </summary>
        public Dictionary<int, Order> Orders {
            get {
                return Transacions.ProcessedOrders;
            }
        }


        /// <summary>
        /// Simulation Server setup RunMode for the Algorithm: Automatic, Parallel or Series.
        /// </summary>
        public RunMode RunMode {
            get {
                return _runMode;
            }
        }


        /// <summary>
        /// Check if the algorithm is locked from any further init changes.
        /// </summary>
        public bool Locked {
            get {
                return _locked;
            }
        }



        /******************************************************** 
        * CLASS METHODS
        *********************************************************/
        
        /// <summary>
        /// Initialise the data and resolution requiredv 
        /// </summary>
        public virtual void Initialize() {
            //Setup Required Data
            throw new NotImplementedException("Please override the Intitialize() method");

        }


        /// <summary>
        /// New data routine: handle new data packets. Algorithm starts here..
        /// </summary>
        /// <param name="symbols">Dictionary of MarketData Objects</param>
        public virtual void OnTradeBar(Dictionary<string, TradeBar> symbols) {
            //Algorithm Implementation
            throw new NotImplementedException("Please override the Update() method");
        }


        /// <summary>
        /// Handle a new incoming Tick Packet:
        /// </summary>
        /// <param name="tick">Tick packets arrive one by one, as they generally don't line up in time. Ticks occuring at the same time arrive in the order you added to dictionary/</param>
        public virtual void OnTick(Tick tick) {
            //Algorithm Implementation
            throw new NotImplementedException("Please override the UpdateTick() method");
        }


        /// <summary>
        /// Set the current datetime frontier: the most forward looking tick so far. This is used by backend to advance time. Do not modify
        /// </summary>
        /// <param name="frontier">Current datetime.</param>
        public void SetDateTime(DateTime frontier) {
            this._time = frontier;
        }


        /// <summary>
        /// Set the RunMode for the Servers. If you are running an overnight algorithm, you must select series.
        /// Automatic will analyse the selected data, and if you selected only minute data we'll select series for you.
        /// </summary>
        /// <param name="mode">Enum RunMode with options Series, Parallel or Automatic. Automatic scans your requested symbols and resolutions and makes a decision on the fastest analysis</param>
        public void SetRunMode(RunMode mode) {
            if (!Locked) {
                this._runMode = mode;
            } else {
                throw new Exception("Algorithm.SetRunMode(): Cannot change run mode once initialized.");
            }
        }


        /// <summary>
        /// Set the requested balance to launch this algorithm
        /// </summary>
        /// <param name="startingCash">Minimum required cash</param>
        public void SetCash(decimal startingCash) {
            if (!Locked) {
                Portfolio.SetCash(startingCash);
            } else {
                throw new Exception("Algorithm.SetCash(): Cannot change cash available once initialized.");
            }
        }


        /// <summary>
        /// Lock the algorithm initialization to avoid messing with cash and data streams.
        /// </summary>
        public void SetLocked() {
            this._locked = true;
        }


        /// <summary>
        /// Get a list of the debug messages sent so far.
        /// </summary>
        /// <returns>List of debug string messages sent</returns>
        public List<string> GetDebugMessages() {
            return _debugMessages;
        }


        /// <summary>
        /// Add specified data to required list. QC will funnel this data to the handle data routine.
        /// </summary>
        /// <param name="marketType">MarketType Type: Equity, Commodity, Future or FOREX</param>
        /// <param name="symbol">Symbol Reference for the MarketType</param>
        /// <param name="resolution">Resolution of the Data Required</param>
        public void AddSecurity(MarketType marketType, string symbol, Resolution resolution = Resolution.Minute, bool fillDataForward = true) {

            try {
                if (!_locked) {
                    if (marketType != MarketType.Equity) {
                        throw new Exception("We only support equities at this time.");
                    }

                    if (_resolution != "" && _resolution != resolution.ToString()) {
                        throw new Exception("We can only accept one resolution at this time. Make all your datafeeds the lowest resolution you require.");
                    }

                    Equities.Add(symbol, resolution, fillDataForward);
                } else {
                    throw new Exception("Algorithm.AddSecurity(): Cannot add another security once initialized.");
                }

            } catch (Exception err) {
                Error("Algorithm.AddRequiredData(): " + err.Message);
            }
        }



        /// <summary>
        /// Submit a new order for quantity of symbol using type order.
        /// </summary>
        /// <param name="type">Buy/Sell Limit or Market Order Type.</param>
        /// <param name="symbol">Symbol of the MarketType Required.</param>
        /// <param name="quantity">Number of shares to request.</param>
        public int Order(string symbol, int quantity, OrderType type = OrderType.Market) {
            //Add an order to the transacion manager class:
            int orderId = -1;
            decimal price = 0;

            //Ordering 0 is useless.
            if (quantity == 0) {
                return orderId;
            }

            if (type != OrderType.Market) {
                throw new Exception("Algorithm.Order(): Currently only market orders supported");
            }

            //If we're not tracking this symbol: throw error:
            if (!Equities.ContainsKey(symbol)) {
                throw new Exception("Algorithm.Order(): You haven't requested " + symbol + " data. Add this with AddSecurity() in the Initialize() Method.");
            }

            //Set a temporary price for validating order for market orders:
            if (type == OrderType.Market) {
                price = Equities[symbol].Price;
            }

            try {
                orderId = Transacions.AddOrder(new Order(symbol, quantity, type, Time, price));

                if (orderId < 0) { 
                    //Order failed validaity checks and was rejected:
                    Debug("Algorithm.Order(): Order Rejected on " + Time.ToShortDateString() + " at " + Time.ToShortTimeString() + " -> " + OrderErrors.ErrorTypes[orderId]);
                }

            } catch (Exception err) {
                Error("Algorithm.Order(): Error sending order. " + err.Message);
            }
            return orderId;
        }



        /// <summary>
        /// Liquidate all holdings. Called at the end of day for tick-strategies.
        /// </summary>
        /// <returns>Array of order ids for liquidated symbols</returns>
        public List<int> Liquidate(string symbolToLiquidate = "") {
            List<int> orderIdList = new List<int>();

            foreach (string symbol in Equities.Keys) {
                //Send market order to liquidate if 1, we have stock, 2, symbol matches.
                if (Portfolio[symbol].HoldStock && (symbol == symbolToLiquidate || symbolToLiquidate == "")) {
                    int quantity = Portfolio[symbol].Quantity;
                    if (Portfolio[symbol].IsLong) {
                        quantity = -Portfolio[symbol].Quantity;
                    }
                    orderIdList.Add(Transacions.AddOrder(new Order(symbol, quantity, OrderType.Market, Time)));
                }
            }
            return orderIdList;
        }


        /// <summary>
        /// Send a debug message to the console:
        /// </summary>
        /// <param name="message">Message to send to debug console</param>
        public void Debug(string message) {
            _debugMessages.Add(message);
        }


        /// <summary>
        /// Send Error Message to the Console.
        /// </summary>
        /// <param name="message">Message to display in errors grid</param>
        public void Error(string message) {
            _errorMessages.Add(message);
        }


        /// <summary>
        /// Terminate the algorithm on exiting the current event processor. 
        /// If have holdings at the end of the algorithm/day they will be liquidated at market prices.
        /// If running a series analysis this command skips the current day (and doesn't liquidate).
        /// </summary>
        /// <param name="message">Exit message</param>
        public void Quit(string message) {
            Debug(message);
            _quit = true;
        }


        /// <summary>
        /// Check if the Quit Flag is Set:
        /// </summary>
        public void SetQuit(bool quit) {
            _quit = quit;
        }


        /// <summary>
        /// Get the quit flag state.
        /// </summary>
        /// <returns>Boolean true if set to quit event loop.</returns>
        public bool GetQuit() {
            return _quit;
        }

    } // End Algorithm Template

} // End QC Namespace
