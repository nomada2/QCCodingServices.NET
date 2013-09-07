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

namespace qc  {

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
        string sName {
            get;
            set;
        }

        /// <summary>
        /// Get the current date/time.
        /// </summary>
        DateTime dtTime {
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
        RunMode eRunMode {
            get;
            set;
        }


        /// <summary>
        /// Indicator if the algorithm has been initialised already. When this is true cash and securities cannot be modified.
        /// </summary>
        bool bLocked {
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
        /// Update the algorithm calculations
        /// </summary>
        /// <param name="lData">New data we've passed into cache</param>
        void OnTradeBar(Dictionary<string, TradeBar> lData);


        /// <summary>
        /// Handler for Tick Events
        /// </summary>
        /// <param name="cTick">Tick Data Packet</param>
        void OnTick(Tick cTick);


        /// <summary>
        /// Set the DateTime Frontier: This is the master time and is 
        /// </summary>
        /// <param name="dtTime"></param>
        void SetDateTime(DateTime dtTime);


        /// <summary>
        /// Set the run mode of the algorithm: series, parallel or automatic.
        /// </summary>
        /// <param name="eMode">Run mode to select, default Automatic</param>
        void SetRunMode(RunMode eMode = RunMode.Automatic);



        /// <summary>
        /// Set the algorithm as initialized and locked. No more cash or security changes.
        /// </summary>
        void SetLocked();


        /// <summary>
        /// Set a required MarketType-symbol and resolution for the simulator to prepare
        /// </summary>
        /// <param name="eMarketType">MarketType Enum: Equity, Commodity, FOREX or Future</param>
        /// <param name="sSymbol">Symbol Representation of the MarketType, e.g. AAPL</param>
        /// <param name="eResolution">Resolution of the MarketType required: MarketData, Second or Minute</param>
        void AddSecurity(MarketType eMarketType, string sSymbol, Resolution eResolution, bool bFillDataForward = true);



        /// <summary>
        /// Send an order to the transaction manager.
        /// </summary>
        /// <param name="sSymbol">Symbol we want to purchase</param>
        /// <param name="iQuantity">Quantity to buy, + is long, - short.</param>
        /// <param name="eType">Market, Limit or Stop Order</param>
        /// <returns>Integer Order ID.</returns>
        int Order(string sSymbol, int iQuantity, OrderType eType = OrderType.Market);


        /// <summary>
        /// Liquidate your portfolio holdings:
        /// </summary>
        /// <param name="sSymbolToLiquidate">Specific asset to liquidate, defaults to all.</param>
        /// <returns>list of order ids</returns>
        List<int> Liquidate(string sSymbolToLiquidate = "");
    }

}
