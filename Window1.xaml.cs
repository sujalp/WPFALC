using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Globalization;

namespace WPFALC
{
    public class Contact
    {
        public ulong Id;
        public string Name;
        public string Alias;
    }

    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Dictionary<ulong, Contact> Contacts;

        public Window1()
        {
            InitializeComponent();
            Main = this;
            Contacts = new PicasaContactReader().ReadContacts();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = false;
            MainPage mp = this.Content as MainPage;
            if (mp != null)
            {
                if (mp.IsChanged)
                {
                    MessageBoxResult mbr = MessageBox.Show("Save Your Data?", "Alert", MessageBoxButton.YesNoCancel);
                    e.Cancel = mbr == MessageBoxResult.Cancel;
                    if (mbr == MessageBoxResult.Yes)
                    {
                        mp.Save();
                    }
                }
                mp.Abandon();
            }
            base.OnClosing(e);
        }

        public static Window1 Main;
    }
}
