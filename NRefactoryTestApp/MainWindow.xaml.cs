using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NRefactoryTestApp.ViewModels;
//using NRefactoryTestApp.models;
using Newtonsoft.Json;
using ServiceStack.WebHost.Endpoints;
using QuantConnect.CodingServices;
using QuantConnect.CodingServices.Models;
using QuantConnect.CodingServices.Services.ProjectModelRepository;

namespace NRefactoryTestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Logger.SetOutputViews(LogTextBox, CodeCompletionOptionsListView);
            //NRefactoryUtils.LoadReferencesInBackground();
            Loaded += OnLoaded;
        }

        private class AppHost : AppHostBase
        {
            public AppHost() : base("test", new Assembly[]{typeof(AppHost).Assembly})
            {}

            public override void Configure(Funq.Container container)
            {
                container.RegisterAutoWiredAs<InMemoryProjectModelRepo, IProjectModelRepository>();
                //container.Register(new InMemoryProjectModelRepo());
                //container.RegisterAs<InMemoryProjectModelRepo>();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Loaded -= OnLoaded;

            AppHost appHost = new AppHost();
            appHost.Init();
     
            //EndpointHost.AppHost.Register(new InMemoryProjectModelRepo());
            var projectModelRepo = EndpointHost.AppHost.TryResolve<IProjectModelRepository>();
            var projectModel = projectModelRepo.GetProject(478, 174);
            var projectDto = ProjectModelConverters.FromModelToDto(projectModel);
            var projectVm = ProjectMVVMConverters.FromModelToViewModel(projectDto);
            ProjectView.DataContext = projectVm;
        }

        //private void LoadProjectButton_Click(object sender, RoutedEventArgs e)
        //{
        //    ProjectView.DataContext = CreateStaticProjectViewModel();
        //}

        //private ProjectViewModel CreateStaticProjectViewModel()
        //{
        //    ProjectViewModel proj = new ProjectViewModel();
        //    proj.Name = "My New Project";
        //    proj.InitialChildren = new IProjectItemViewModel[]
        //    {
        //        new ProjectDirectoryViewModel()
        //        {
        //            Name = "Math",
        //            InitialChildren = new IProjectItemViewModel[]
        //            {
        //                new ProjectFileViewModel()
        //                {
        //                    Id = 13,
        //                    Name = "RollingAverage.cs",
        //                    Content = "/// <summary>\r\n///    Basic Template v0.1 :: Rolling Average\r\n/// </summary>    \r\nusing System;\r\nusing System.Collections;\r\nusing System.Collections.Generic;\r\n\r\nnamespace qc {\r\n\r\n    /// <summary>\r\n    /// Example Helper Class: Basic Math Routines.\r\n    /// Using the QCS you can create subfolders, classes. \r\n    /// All the code is compiled into your algorithm.\r\n    /// </summary>    \r\n    public partial class MathAverage {\r\n\r\n        public int iSamplePeriod = 10;\r\n        public decimal dCurrentAverage = 0;\r\n\r\n        /// <summary>\r\n        /// Example helper class: Add a new sample to a rolling average.\r\n        /// </summary>\r\n        /// <param name=\"dNewSample\">Decimal new sample value</param>\r\n        /// <returns>decimal current rolling average.</returns>\r\n        public static decimal RollingAverage(decimal dNewSample) {\r\n\r\n            Random cRand = new Random();\r\n            return dNewSample * ((decimal)cRand.NextDouble());\r\n        \r\n        }\r\n\r\n    }\r\n}"
        //                }
        //            }
        //        },
        //        new ProjectFileViewModel()
        //        {
        //            Id = 12,
        //            Name = "Main.cs",
        //            Content = "/// \r\n///    Basic Template v0.1\r\n///     \r\nusing System;\r\nusing System.Collections;\r\nusing System.Collections.Generic;\r\n\r\nnamespace qc {\r\n\r\n    ///\r\n    /// Your Algorithm Class Name:\r\n    /// This can be anything you like but it must extend the QCAlgorithm Class.\r\n    ///     \r\n    public partial class BasicTemplateAlgorithm : QCAlgorithm {\r\n    \r\n        /// \r\n        /// Initialize the data and resolution you require for your strategy:\r\n        /// -> AddSecurity(MarketType.Equity, string Symbol, Resolution)\r\n        ///    MarketType.Equity is the only type supported for now.\r\n        ///    Resolution can be: Resolution.Tick, .Second or .Minute\r\n        /// -> Use this section to set up your algorithm.\r\n        /// \r\n        public override void Initialize() {\r\n            AddSecurity(MarketType.Equity, \"AAPL\", Resolution.Second); \r\n        }\r\n\r\n        /// \r\n        /// Handle a New TradeBar Event\r\n        ///    -> This event is fired to recieve new data from Minute or Second events.\r\n        /// -> Data arrives in a Dictionary, indexed by the stock symbol.\r\n        /// -> The TradeBar Class contains the properties dHigh, dLow, dOpen, dClose (\"dPrice alias\"), iVolume and dtTime\r\n        //  -> You can only have one resolution per symbol, i.e. Apple Tick or Second, but not both.\r\n        /// \r\n        /// Dictionary of TradeBar Class Data Packets\r\n        public override void OnTradeBar(Dictionary<string, TradeBar> lData) {\r\n            if (Portfolio.bHoldStock == false) {\r\n                Order(\"AAPL\", 10, OrderType.Market);\r\n            }\r\n        }\r\n\r\n\r\n        /// \r\n        /// Handle a New Tick Event\r\n        ///    -> Tick events are almost identical to tradebars but arrive one by one, for all stocks, through this function.\r\n        /// -> You probably want to check cTick.sSymbol to see which symbol the tick belongs to.\r\n        /// -> Tick Data Class has properties: eTickType (TickType.Trade or .Quote), DateTime dtTime of tick\r\n        //    -> If a trade tick: has the properties: decimal dPrice, int iQuantity\r\n        //  -> If a quote tick\" has the properties: decimal dBidPrice, dAskPrice, int iAskQuantity, iBidQuantity, string sBidMarket, sAskMarket \r\n        /// \r\n        /// \r\n        public override void OnTick(Tick cTick) {\r\n            if (Portfolio.bHoldStock == false) {\r\n                Order(\"AAPL\", 10, OrderType.Market);\r\n            }\r\n        }\r\n    }\r\n}"
        //        }
        //    };
        //    return proj;
        //}

        private void AnalyzeProjectButton_Click(object sender, RoutedEventArgs e)
        {
            //var projectVm = (ProjectViewModel) ProjectView.DataContext;
            //var fileVm = projectVm.SelectedProjectItem as ProjectFileViewModel;
            //if (fileVm == null)
            //{
            //    Logger.AppendLine("A file must be selected");
            //    return;
            //}
            
            //var projectDto = ProjectMVVMConverters.FromViewModelToModel(projectVm);
            //var request = new StatelessProjectRequest()
            //{
            //    Project = projectDto,
            //    CodeCompletionParameters = new StatelessProjectCodeCompletionParameters()
            //    {
            //        FileId = fileVm.Id,
            //        Offset = fileVm.CaretIndex
            //    }
            //};

            //var response = MockWebServiceUtility.Server_HandleStatelessCodeCompletionRequest(request);


            //var jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
            //Logger.AppendLine(jsonResponse);
            //Logger.SetCodeCompletionOptions(response.CompletionOptions);
        }
    }
}
