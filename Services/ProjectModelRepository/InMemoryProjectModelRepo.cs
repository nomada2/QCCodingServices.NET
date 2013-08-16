using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuantConnect.Server.Autocomplete.Models;

namespace QuantConnect.Server.Autocomplete.Services.ProjectModelRepository
{
    public class InMemoryProjectModelRepo : IProjectModelRepository
    {
        private static ProjectModel[] projects;

        static InMemoryProjectModelRepo()
        {
            #region Data

            // Yeah -- So I know there's only a single project in this array of projects.
            // It may not seem very useful now, but this should be good enough to prove the idea...
            projects = new ProjectModel[]
            {
                new ProjectModel()
                {
                    OwnerUserId = 478,
                    ProjectId = 125,
                    Name = "Project X",
                    Children = new List<IProjectItemModel>()
                    {
                        new ProjectDirectoryModel()
                        {
                            ProjectId = 125,
                            Id = 262,
                            Name = "Math",
                            Children = new List<IProjectItemModel>()
                            {
                                new ProjectFileModel()
                                {
                                    ProjectId = 125,
                                    Id = 264,
                                    Name = "RollingAverage.cs",
                                    Content = "/// <summary>\r\n///    Basic Template v0.1 :: Rolling Average\r\n/// </summary>    \r\nusing System;\r\nusing System.Collections;\r\nusing System.Collections.Generic;\r\n\r\nnamespace qc {\r\n\r\n    /// <summary>\r\n    /// Example Helper Class: Basic Math Routines.\r\n    /// Using the QCS you can create subfolders, classes. \r\n    /// All the code is compiled into your algorithm.\r\n    /// </summary>    \r\n    public partial class MathAverage {\r\n\r\n        public int iSamplePeriod = 10;\r\n        public decimal dCurrentAverage = 0;\r\n\r\n        /// <summary>\r\n        /// Example helper class: Add a new sample to a rolling average.\r\n        /// </summary>\r\n        /// <param name=\"dNewSample\">Decimal new sample value</param>\r\n        /// <returns>decimal current rolling average.</returns>\r\n        public static decimal RollingAverage(decimal dNewSample) {\r\n\r\n            Random cRand = new Random();\r\n            return dNewSample * ((decimal)cRand.NextDouble());\r\n        \r\n        }\r\n\r\n    }\r\n}"
                                }
                            }
                        },
                        new ProjectFileModel()
                        {
                            ProjectId = 125,
                            Id = 263,
                            Name = "Main.cs",
                            Content = "/// \r\n///    Basic Template v0.1\r\n///     \r\nusing System;\r\nusing System.Collections;\r\nusing System.Collections.Generic;\r\n\r\nnamespace qc {\r\n\r\n    ///\r\n    /// Your Algorithm Class Name:\r\n    /// This can be anything you like but it must extend the QCAlgorithm Class.\r\n    ///     \r\n    public partial class BasicTemplateAlgorithm : QCAlgorithm {\r\n    \r\n        /// \r\n        /// Initialize the data and resolution you require for your strategy:\r\n        /// -> AddSecurity(MarketType.Equity, string Symbol, Resolution)\r\n        ///    MarketType.Equity is the only type supported for now.\r\n        ///    Resolution can be: Resolution.Tick, .Second or .Minute\r\n        /// -> Use this section to set up your algorithm.\r\n        /// \r\n        public override void Initialize() {\r\n            AddSecurity(MarketType.Equity, \"AAPL\", Resolution.Second); \r\n        }\r\n\r\n        /// \r\n        /// Handle a New TradeBar Event\r\n        ///    -> This event is fired to recieve new data from Minute or Second events.\r\n        /// -> Data arrives in a Dictionary, indexed by the stock symbol.\r\n        /// -> The TradeBar Class contains the properties dHigh, dLow, dOpen, dClose (\"dPrice alias\"), iVolume and dtTime\r\n        //  -> You can only have one resolution per symbol, i.e. Apple Tick or Second, but not both.\r\n        /// \r\n        /// Dictionary of TradeBar Class Data Packets\r\n        public override void OnTradeBar(Dictionary<string, TradeBar> lData) {\r\n            if (Portfolio.bHoldStock == false) {\r\n                Order(\"AAPL\", 10, OrderType.Market);\r\n            }\r\n        }\r\n\r\n\r\n        /// \r\n        /// Handle a New Tick Event\r\n        ///    -> Tick events are almost identical to tradebars but arrive one by one, for all stocks, through this function.\r\n        /// -> You probably want to check cTick.sSymbol to see which symbol the tick belongs to.\r\n        /// -> Tick Data Class has properties: eTickType (TickType.Trade or .Quote), DateTime dtTime of tick\r\n        //    -> If a trade tick: has the properties: decimal dPrice, int iQuantity\r\n        //  -> If a quote tick\" has the properties: decimal dBidPrice, dAskPrice, int iAskQuantity, iBidQuantity, string sBidMarket, sAskMarket \r\n        /// \r\n        /// \r\n        public override void OnTick(Tick cTick) {\r\n            if (Portfolio.bHoldStock == false) {\r\n                Order(\"AAPL\", 10, OrderType.Market);\r\n            }\r\n        }\r\n    }\r\n}"
                        },
                        //new ProjectFileModel()
                        //{
                        //    ProjectId = 125,
                        //    Id = 265,
                        //    Name = "BrowsableAttributeTest.cs",
                        //    Content = "using System;\r\nusing System.ComponentModel;\r\npublic class FooBar\r\n{\r\n    [EditorBrowsable(EditorBrowsableState.Always)]\r\n    public int BrowsableTest { get; set; }\r\n\r\n    [EditorBrowsable(EditorBrowsableState.Never)]\r\n    public int NotBrowsableTest { get; set; }\r\n\r\n    public void DoSomethingPointless(int i)\r\n    {\r\n        NotBrowsableTest = i;\r\n        if (i > 0)\r\n        {\r\n            NotBrowsableTest --;\r\n        	(new FooBar()).DoSomethingPointless(NotBrowsableTest-1);\r\n            DoSomethingPointless(NotBrowsableTest);\r\n        }\r\n    }\r\n}\r\n"
                        //}
                        new ProjectFileModel()
                        {
                            ProjectId = 125,
                            Id = 266,
                            Name = "TestClass.cs",
                            Content = @"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace qcx
{
    public interface IQC
    {
        int Integer { get; }
    }

    public abstract class QcBase
    {
        /// <summary>
        /// an int
        /// </summary>
        int integer = 1;

        /// <summary>
        /// An Integer
        /// </summary>
        protected int Integer
        {
            get { return integer; }
            set { integer = value; }
        }

        /// <summary>
        /// Of course, you know which double we're going to return...
        /// </summary>
        /// <returns>PI</returns>
        protected virtual double GetDouble()
        {
            return 3.1415926;
        }

        /// <summary>
        /// Multiplicatoin is fun!!
        /// </summary>
        /// <param name=""multiplicand"">Could have been factor</param>
        /// <returns>The product</returns>
        public double Multiply(double multiplicand)
        {
            return multiplicand * GetDouble();
        }

        /// <summary>
        /// A 2-factor mulication fn
        /// </summary>
        /// <param name=""multiplier"">Could have been factor1</param>
        /// <param name=""multiplicand"">Could have been factor2</param>
        /// <returns>The product</returns>
        public double Multiply(double multiplier, double multiplicand)
        {
            return multiplier * multiplicand * GetDouble();
        }
    }

    /// <summary>
    /// A delegate
    /// </summary>
    public delegate void QcImplement();
    
    /// <summary>
    /// A static class with a type param
    /// </summary>
    /// <typeparam name=""T"">The obligatory type param</typeparam>
    public static class StaticClass<T>
    {
        /// <summary>
        /// A static ToString method.  Not actually very helpful.
        /// </summary>
        /// <param name=""value""></param>
        /// <returns></returns>
        public static string ToString(T value)
        {
            return value.ToString();
        }
    }

    /// <summary>
    /// The test class
    /// </summary>
    public class FooBar : QcBase
    {
        /// <summary>
        /// string value
        /// </summary>
        public string StrValue { get; set; }

        /// <summary>
        /// The overriding method
        /// </summary>
        /// <returns></returns>
        protected override double GetDouble()
        {
            return 3.14159265359;
        }

        /// <summary>
        /// Integral addition
        /// </summary>
        /// <param name=""i""></param>
        /// <returns></returns>
        public int Add(int i)
        {
            return i + Integer;
        }
    }
}
"
                        }
                    }
                }
            };


            #endregion
        }

        //public static ProjectFileModel GetFile(int userId, int projectId, int fileId)
        //{
        //    var project = GetProject(userId, projectId);
        //    if (project == null)
        //        return null;

        //    var file = project.FindFile(fileId);
        //    return file;
        //}

        //public static ProjectModel GetProject(int userId, int projectId)
        //{
        //    return projects.FirstOrDefault(x => x.OwnerUserId == userId && x.ProjectId == projectId);
        //}

        public ProjectModel GetProject(int userId, int projectId)
        {
#if true
            return projects.FirstOrDefault(x => x.OwnerUserId == userId && x.ProjectId == projectId);
#else
            // Note: there's only one project in this repo:  project 478 for user 125
            var projectDto = MockWebServiceUtility.LoadProject(userId, projectId);
            if (projectDto == null)
                throw new Exception("Could not find project in internal repository.");

            ProjectModel projectModel = ProjectModelConverters.FromDtoToModel(projectDto);
            return projectModel;
#endif
        }

        public void SaveFileContent(int userId, int projectId, int fileId, string fileContent)
        {
            ProjectModel project = GetProject(userId, projectId);
            if (project == null)
                throw new Exception(string.Format("Project {0} could not be found for user {1}.  Saving a file for a new project is not yet supported", projectId, userId));

            ProjectFileModel fileModel = project.FindFile(fileId);
            if (fileModel == null)
                throw new Exception(string.Format("File {0} could not be found within project {1} for user {2}.  " +
                                                  "Saving a new file in an existing project is not yet supported", 
                    fileId, projectId, userId));

            fileModel.Content = fileContent;
        }

    }
}
