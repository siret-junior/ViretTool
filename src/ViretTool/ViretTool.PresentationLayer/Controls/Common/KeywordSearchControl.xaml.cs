using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Viret;
using ViretTool.BusinessLayer.Services;
using ViretTool.PresentationLayer.Controls.Common.KeywordSearch;

namespace ViretTool.PresentationLayer.Controls.Common
{
    public partial class KeywordSearchControl : UserControl
    {
        private readonly ViretCore _viretCore;
        public KeywordSearchControl()
        {
            InitializeComponent();
            textBox.KeyDown += OnKeyDown;
        }

        public static readonly DependencyProperty QueryResultProperty = DependencyProperty.Register(
            nameof(QueryResult),
            typeof(KeywordQueryResult),
            typeof(KeywordSearchControl),
            new FrameworkPropertyMetadata(
                (obj, args) =>
                {
                    if (args.NewValue == null)
                    {
                        ((KeywordSearchControl)obj).Clear();
                    }
                }) { BindsTwoWayByDefault = true });

        public KeywordQueryResult QueryResult
        {
            get => (KeywordQueryResult)GetValue(QueryResultProperty);
            set => SetValue(QueryResultProperty, value);
        }

        public static readonly DependencyProperty DatasetServicesManagerProperty = DependencyProperty.Register(
            nameof(DatasetServicesManager),
            typeof(IDatasetServicesManager),
            typeof(KeywordSearchControl),
            new FrameworkPropertyMetadata(
                (obj, args) =>
                {
                }));

        public IDatasetServicesManager DatasetServicesManager
        {
            get => (IDatasetServicesManager)GetValue(DatasetServicesManagerProperty);
            set => SetValue(DatasetServicesManagerProperty, value);
        }

        public void Clear()
        {
            textBox.Clear();
            QueryChangedEvent("");
        }

        


        private void QueryChangedEvent(string query)
        {
            QueryResult = new KeywordQueryResult(
                query.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries),
                query,
                "deprecated");
        }


        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                QueryChangedEvent(textBox.Text);
                e.Handled = true;
            }
            else if (e.Key == Key.Space)
            {
                // word completed
                // TODO ?
            }
            else if (e.Key == Key.OemSemicolon)
            {
                // sentence completed
                if (DatasetServicesManager.ViretCore.IsLoaded)
                {
                    DatasetServicesManager.ViretCore.RankingService.PreloadQuery(textBox.Text
                        .Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(sentence => sentence.Trim())
                        .ToArray());
                }
            }
        }

    }
}
