﻿using System;
using System.Collections.Generic;
using System.Linq;
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

namespace TopDeck
{
    public partial class ListCardsView : UserControl
    {
        public DatabaseManager DBMan
        {
            get;
            set;
        }

        public ListCardsView()
        {
            InitializeComponent();
            FilterFiltersPanel.MiddlePanel = FilterListPanel;
            FilterListPanel.RightPanel = CardPanel;
        }

        public void setDatabaseManager(DatabaseManager db){
            DBMan = db;
            FilterFiltersPanel.setDatabaseManager(db);
            FilterListPanel.setDatabaseManager(db);
        }
    }
}
