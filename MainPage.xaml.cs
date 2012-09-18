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

namespace WPFALC
{

    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            FolderPicker.DataContext = new DirectoryInfo[] { ((App)App.Current).StartPath };
        }

        public bool IsChanged
        {
            get { return _folderCurrent != null && _folderCurrent.Changed; }
        }

        public void Save()
        {
            if (_folderCurrent != null && _folderCurrent.Changed)
            {
                _folderCurrent.Save();
            }
        }

        public void Abandon()
        {
            if (_folderCurrent != null)
            {
                _folderCurrent.Abandon();
            }
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_folderCurrent != null)
            {
                Save();
                _folderCurrent.Abandon();
            }

            DirectoryInfo di = e.NewValue as DirectoryInfo;
            _folderCurrent = new OneFolder(Window1.Main.Contacts);
            _folderCurrent.Deserialize(di);
            TheGrid.DataContext = _folderCurrent;
            ShowBigImage.Content = null;
            AlbumImagePicker.Content = null;
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ShowBigImage.Content = ((Image)sender).DataContext;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    ShowBigImage.Content = null;
                    AlbumImagePicker.Content = null;
                    break;
                case Key.Right:
                    Next();
                    break;
                case Key.Left:
                    Previous();
                    break;
            }
            base.OnKeyDown(e);
        }

        private OneFolder _folderCurrent;

        private void GoPrevious(object sender, RoutedEventArgs e)
        {
            Previous();
        }

        private void Previous()
        {
            Photo p = (Photo)ShowBigImage.Content;
            if (p!= null && p.Previous != null)
            {
                ShowBigImage.Content = p.Previous;
            }
        }

        private void GoNext(object sender, RoutedEventArgs e)
        {
            Next();
        }

        private void Next()
        {
            Photo p = (Photo)ShowBigImage.Content;
            if (p != null && p.Next != null)
            {
                ShowBigImage.Content = p.Next;
            }
        }

        private void ShowAlbumPhotoSelection(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            Album a = b.DataContext as Album;

            SelectPhotoForAlbum spfa = new SelectPhotoForAlbum();

            spfa.A = a;
            spfa.PL = new PhotoList();

            var plist = from photo in a.Owner.Photos where ((photo.AlbumT == a.Name) && (photo.NoShow == false)) select photo;
            foreach (var p in plist)
            {
                spfa.PL.Add(p);
            }

            AlbumImagePicker.Content = spfa;
        }

        private void SelectPhoto(object sender, MouseButtonEventArgs e)
        {
            Image i = sender as Image;
            Photo p = i.DataContext as Photo;

            var al = from album in p.Owner.Albums where (album.Name == p.AlbumT) select album;
            foreach (var a in al)
            {
                a.Photo = p.JustTheName;
                break;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            SelectPhotoForAlbum spfa = tb.DataContext as SelectPhotoForAlbum;

            if (spfa != null)
            {
                var al = from album in spfa.A.Owner.Albums where (album.Name == spfa.A.Name) select album;
                foreach (var a in al)
                {
                    a.Story = tb.Text;
                    break;
                }
            }
        }
    }

    public class PhotoLineButton : Button
    {
        public Photo Owner
        {
            get
            {
                if (_owner == null)
                {
                    DependencyObject methis = this;
                    do
                    {
                        if (methis is FrameworkElement)
                        {
                            FrameworkElement fe = methis as FrameworkElement;
                            if (fe.DataContext is Photo)
                            {
                                _owner = (Photo)fe.DataContext;
                                break;
                            }
                        }
                        methis = VisualTreeHelper.GetParent(methis);
                    } while (methis != null);
                }
                return _owner;
            }
        }
        private Photo _owner = null;
    }

    public class NameButton : PhotoLineButton
    {
        protected override void OnClick()
        {
            Owner.AddPerson((string)Content);
            base.OnClick();
        }
    }

    public class PersonTextBlock : ContentControl
    {
        private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        private void Ensure()
        {
            if (m_r == null)
            {
                DependencyObject methis = this;
                do
                {
                    if (methis is ShowBigImage)
                    {
                        ShowBigImage sbi = (ShowBigImage)methis;
                        m_s = sbi;
                        m_r = FindVisualChild<Rectangle>(sbi);
                        m_i = FindVisualChild<Image>(sbi);
                        break;
                    }
                    methis = VisualTreeHelper.GetParent(methis);
                } while (methis != null);
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            Person P = Content as Person;

            Ensure();
            m_r.Visibility = Visibility.Visible;
            Rect64 r = new Rect64(P.Rect, m_i.ActualWidth, m_i.ActualHeight);

            double top = r.Top;
            double left = r.Left;
            double bottom = r.Bottom;
            double right = r.Right;

            m_r.Width = right - left;
            m_r.Height = bottom - top;

            GeneralTransform T = m_i.TransformToVisual(m_s);
            Point currentPoint = T.Transform(new Point(0, 0));

            m_r.Margin = new Thickness(left + currentPoint.X, top + currentPoint.Y, 0, 0);

            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            Ensure();
            m_r.Visibility = Visibility.Collapsed;
            base.OnMouseLeave(e);
        }

        private ShowBigImage m_s;
        private Rectangle m_r;
        private Image m_i;
    }
    public class Rect64
    {
        private ulong uu;
        private double ww, hh;

        public Rect64(ulong u, double w, double h)
        {
            uu = u;
            ww = w / 0xffff;
            hh = h / 0xffff;
        }

        public double Left    { get { return ((double)((uu & 0xffff000000000000) >> 48)) * ww; } }
        public double Top     { get { return ((double)((uu & 0x0000ffff00000000) >> 32)) * hh; } }
        public double Right   { get { return ((double)((uu & 0x00000000ffff0000) >> 16)) * ww; } }
        public double Bottom  { get { return ((double)((uu & 0x000000000000ffff) >> 0 )) * hh; } }
    }

    public abstract class UBButton : PhotoLineButton
    {
        public UBButton() { Content = "UOneB"; Background = Brushes.LavenderBlush; }
    }

    public abstract class UBAButton : PhotoLineButton
    {
        public UBAButton() { Content = "UAllB"; Background = Brushes.LavenderBlush; }

        protected abstract void OnMyClick(bool fForce);

        protected override void OnClick()
        {
            OnMyClick(Keyboard.IsKeyDown(Key.LeftCtrl));
            base.OnClick();
        }
    }

    public class UBTitleButton : UBButton
    {
        protected override void OnClick()
        {
            Owner.UBTitle();
        }
    }

    public class UBATitleButton : UBAButton
    {
        protected override void OnMyClick(bool fForce)
        {
            Owner.UBATitle(fForce);
        }
    }

    public class UBPeopleButton : UBButton
    {
        protected override void OnClick()
        {
            Owner.UBPeople();
        }
    }

    public class UBAPeopleButton : UBAButton
    {
        protected override void OnMyClick(bool fForce)
        {
            Owner.UBAPeople(fForce);
        }
    }

    public class UBAlbumTButton : UBButton
    {
        protected override void OnClick()
        {
            Owner.UBAlbumT();
        }
    }

    public class UBAAlbumTButton : UBAButton
    {
        protected override void OnMyClick(bool fForce)
        {
            Owner.UBAAlbumT(fForce);
        }
    }

    public class UBPlaceButton : UBButton
    {
        protected override void OnClick()
        {
            Owner.UBPlace();
        }
    }

    public class UBAPlaceButton : UBAButton
    {
        protected override void OnMyClick(bool fForce)
        {
            Owner.UBAPlace(fForce);
        }
    }

    public class ShowBigImage : ContentControl
    {
        public ShowBigImage() { Content = null; }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            Visibility = (newContent == null) ? Visibility.Collapsed : Visibility.Visible;
            base.OnContentChanged(oldContent, newContent);
        }
    }

    public class GFSIC : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is DirectoryInfo)
            {
                DirectoryInfo[] dil = ((DirectoryInfo)value).GetDirectories();
                List<DirectoryInfo> dilNew = new List<DirectoryInfo>();
                foreach (DirectoryInfo di in dil)
                {
                    if (di.Name != "small" && di.Name != "Originals")
                    {
                        dilNew.Add(di);
                    }
                }
                return dilNew;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class PLVC : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string s = "";
            PeopleList pl = value as PeopleList;
            if (pl != null)
            {
                foreach (var p in pl)
                {
                    if (s != "")
                    {
                        s = s + ", ";
                    }
                    s = s + p.Name;
                }
            }
            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return OneFolder.StringToList(value as string);
        }

        #endregion
    }

    public class RLVC : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string s = "";
            PeopleList pl = value as PeopleList;
            if (pl != null)
            {
                foreach (var p in pl)
                {
                    if (s != "")
                    {
                        s = s + ",";
                    }
                    s = s + string.Format("{0:x}", p.Rect);
                }
            }
            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class FUPToV1 : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is uint)
            {
                uint V = (uint)value;
                if (V > 0 && V < 100)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class FUPToV2 : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is uint)
            {
                uint V = (uint)value;
                if (V > 0 && V < 100)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class ShowColorConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Color c = Colors.Purple;
            bool NoShow = (bool)values[0];
            PeopleList p = values[1] as PeopleList;

            if (NoShow)
            {
                c = Colors.Red;
            }
            else if (p == null || p.Count == 0)
            {
                c = Colors.Green;
            }
            else
            {
                c = Colors.Yellow;
            }

            return new SolidColorBrush(c);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
