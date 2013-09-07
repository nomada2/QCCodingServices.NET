/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals, V0.1
 * Created by Jared Broad
*/

/**********************************************************
* USING NAMESPACES
**********************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantConnect  {

    /******************************************************** 
    * QUANTCONNECT PROJECT LIBRARIES
    *********************************************************/
    using QuantConnect.Markets;
    using QuantConnect.Models;

    /******************************************************** 
    * CLASS DEFINITIONS
    *********************************************************/
    /// <summary>
    /// Interface for Algorithm Class Libraries
    /// </summary>
    public partial interface IAlgorithm {


        /******************************************************** 
        * INTERFACE PROPERTIES:
        *********************************************************/
        
        /// <summary>
        /// Equities Object Collection Class
        /// </summary>
        EquitiesManager Equities { 
            get; 
            set; 
        }


        /// <summary>
        /// Equities Portfolio Management Class:
        /// </summary>
        EquitiesPortfolioManager Portfolio { 
            get; 
            set; 
        }


        /// <summary>
        /// Equities Transaction Processing Class.
        /// </summary>
        EquitiesTransactionManager Transacions { 
            get; 
            set;
        }

        /// <summary>
        /// Set a public name for the algorithm.
        /// </summary>
        string Name {
            get;
            set;
        }

        /// <summary>
        /// Get the current date/time.
        /// </summary>
        DateTime Time {
            get;
        }

        /// <summary>
        /// Accessor for Filled Orders:
        /// </summary>
        Dictionary<int, Order> Orders {
            get;
        }


        /// <summary>
        /// Run Simulation Mode for the algorithm: Automatic, Parallel or Series.
        /// </summary>
        RunMode RunMode {
            get;
        }


        /// <summary>
        /// Indicator if the algorithm has been initialised already. When this is true cash and securities cannot be modified.
        /// </summary>
        bool Locked {
            get;
        }


        /******************************************************** 
        * INTERFACE METHODS
        *********************************************************/
        /// <summary>
        /// Initialise the Algorithm and Prepare Required Data:
        /// </summary>
        void Initialize();


        /// <summary>
        /// Update the algorithm calculations:
        /// </summary>
        /// <param name="symbols">Dictionary of TradeBar Data-Objects for this timeslice</param>
        void OnTradeBar(Dictionary<string, TradeBar> symbols);


        /// <summary>
        /// Handler for Tick Events
        /// </summary>
        /// <param name="tick">Tick Data Packet</param>
        void OnTick(Tick tick);


        /// <summary>
        /// Set the DateTime Frontier: This is the master time and is 
        /// </summary>
        /// <param name="time"></param>
        void SetDateTime(DateTime time);


        /// <summary>
        /// Set the run mode of the algorithm: series, parallel or automatic.
        /// </summary>
        /// <param name="mode">Run mode to select, default Automatic</param>
        void SetRunMode(RunMode mode = RunMode.Automatic);


        /// <summary>
        /// Set the algorithm as initialized and locked. No more cash or security changes.
        /// </summary>
        void SetLocked();


        /// <summary>
        /// Get a list of the debug messages sent so far.
        /// </summary>
        /// <returns>List of string debug messages.</returns>
        List<string> GetDebugMessages();


        /// <summary>
        /// Set a required MarketType-symbol and resolution for the simulator to prepare
        /// </summary>
        /// <param name="marketType">MarketType Enum: Equity, Commodity, FOREX or Future</param>
        /// <param name="symbol">Symbol Representation of the MarketType, e.g. AAPL</param>
        /// <param name="resolution">Resolution of the MarketType required: MarketData, Second or Minute</param>
        /// <param name="fillDataForward">If true, returns the last available data even if none in that timeslice.</param>
        void AddSecurity(MarketType marketType, string symbol, Resolution resolution, bool fillDataForward = true);



        /// <summary>
        /// Send an order to the transaction manager.
        /// </summary>
        /// <param name="symbol">Symbol we want to purchase</param>
        /// <param name="quantity">Quantity to buy, + is long, - short.</param>
        /// <param name="type">Market, Limit or Stop Order</param>
        /// <returns>Integer Order ID.</returns>
        int Order(string symbol, int quantity, OrderType type = OrderType.Market);


        /// <summary>
        /// Liquidate your portfolio holdings:
        /// </summary>
        /// <param name="symbolToLiquidate">Specific asset to liquidate, defaults to all.</param>
        /// <returns>list of order ids</returns>
        List<int> Liquidate(string symbolToLiquidate = "");


        /// <summary>
        /// Terminate the algorithm on exiting the current event processor. 
        /// If have holdings at the end of the algorithm/day they will be liquidated at market prices.
        /// If running a series analysis this command skips the current day (and doesn't liquidate).
        /// </summary>
        /// <param name="message">Exit message</param>
        void Quit(string message);


        /// <summary>
        /// Set the quit flag true / false.
        /// </summary>
        /// <param name="quit">When true quits the algorithm event loop for this day</param>
        void SetQuit(bool quit);

        /// <summary>
        /// Get the quit flag state. 
        /// </summary>
        /// <returns>Boolean quit flag</returns>
        bool GetQuit();
    }

}
