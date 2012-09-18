using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Collections.Specialized;
using System.Globalization;
using System.Threading;
using System.Windows.Threading;
using System.Net;
using ALCRWNS;
using MiscPhotoServices;
using System.Windows;

namespace WPFALC
{
    public class INPC : INotifyPropertyChanged
    {
        public INPC(INPC owner)
        {
            _owner = owner;
        }

        #region INotifyPropertyChanged Members
        protected void FPC(string name)
        {
            Changed = true;
            FPCNC(name);
        }

        protected void FPCNC(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        public bool Changed
        {
            set
            {
                if (_owner == null)
                {
                    Window1.Main.Title = "Album Creator" + (value ? " *" : "");
                    _changed = value;
                }
                else
                {
                    _owner.Changed = value;
                }
            }
            get
            {
                if (_owner == null)
                {
                    return _changed;
                }
                else
                {
                    return _owner.Changed;
                }
            }
        }
        private bool _changed;

        private INPC _owner;
    }

    public class LinkNode : INPC
    {
        public LinkNode(INPC owner) : base(owner) { }
        public LinkNode Next;
        public LinkNode Previous;
    }

    public class Album : LinkNode
    {
        public Album(OneFolder owner) : base(owner) { _oneFolder = owner; }

        public OneFolder Owner { get { return _oneFolder; } }

        private OneFolder _oneFolder;

        #region Name
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                FPC("Name");
            }
        }
        private string _name;
        #endregion

        #region Month
        public string Month
        {
            get { return _month; }
            set
            {
                _month = value;
                FPC("Month");
            }
        }
        private string _month;
        #endregion

        #region Year
        public string Year
        {
            get { return _year; }
            set
            {
                _year = value;
                FPC("Year");
            }
        }
        private string _year;
        #endregion

        #region Photo
        public string Photo
        {
            get { return _photo; }
            set
            {
                _photo = value;
                FPC("Photo");
                FPC("FileName");
            }
        }
        private string _photo;
        #endregion

        #region Count
        public int Count
        {
            get { return _count; }
            set
            {
                _count = value;
                FPC("Count");
            }
        }
        private int _count;
        #endregion

        #region
        public string Story
        {
            get { return _story; }
            set
            {
                _story = value;
                FPC("Story");
            }
        }
        private string _story;
        #endregion

        public string FileName { get { return _oneFolder.Path + "\\" + Photo; } }
    }

    public class SelectPhotoForAlbum
    {
        public Album A { get; set; }
        public PhotoList PL { get; set; }
    }

    public class Person
    {
        public Person(string s) : this(s, 0, 0) {}

        public Person(string s, ulong r, ulong pid)
        {
            Name = s;
            Rect = r;
            PId = pid;
        }

        public override bool Equals(object obj)
        {
            if (obj is Person)
                return Name.CompareTo(((Person)obj).Name) == 0;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public string Name { get; set; }
        public ulong  Rect { get; set; }
        public ulong  PId  { get; set; }
    }

    public class PeopleList : ObservableCollection<Person> 
    {
    }

    public class RectList : ObservableCollection<ulong>
    {
    }

    public class Photo : LinkNode
    {
        public Photo(OneFolder owner) : base(owner) 
        {
            _oneFolder = owner;
            owner.People.CollectionChanged += (sender, e) => UpdateGlobalPeopleList();
        }

        private void UpdateGlobalPeopleList()
        {
            var gpli = _oneFolder.People.Except(People);
            GlobalPeopleList.Clear();
            foreach (var s in gpli)
            {
                GlobalPeopleList.Add(s);
            }
        }

        public void AddPerson(string s)
        {
            People.Add(new Person(s));
            UpdateGlobalPeopleList();
            FPC("People");
        }

        public void UBTitle()
        {
            if (Next != null)
            {
                ((Photo)Next).Title = Title;
            }
        }

        public void UBATitle(bool fForce)
        {
            Photo p = (Photo)this.Next;
            while (p != null)
            {
                if (p.Title == "" || fForce)
                {
                    p.Title = Title;
                }
                p = (Photo)p.Next;
            }
        }

        public void UBPeople()
        {
            if (Next != null)
            {
                ((Photo)Next).People = People;
            }
        }

        public void UBAPeople(bool fForce)
        {
            Photo p = (Photo)this.Next;
            while (p != null)
            {
                if (p.People == null || fForce)
                {
                    p.People = People;
                }
                p = (Photo)p.Next;
            }
        }

        public void UBAlbumT()
        {
            if (Next != null)
            {
                ((Photo)Next).AlbumT = AlbumT;
            }
        }

        public void UBAAlbumT(bool fForce)
        {
            Photo p = (Photo)this.Next;
            while (p != null)
            {
                if (p.AlbumT == "" || fForce)
                {
                    p.AlbumT = AlbumT;
                }
                p = (Photo)p.Next;
            }
        }

        public void UBPlace()
        {
            if (Next != null)
            {
                ((Photo)Next).Place = Place;
            }
        }

        public void UBAPlace(bool fForce)
        {
            Photo p = (Photo)this.Next;
            while (p != null)
            {
                if (p.Place == "" || fForce)
                {
                    p.Place = Place;
                }
                p = (Photo)p.Next;
            }
        }

        #region DateStr
        public string DateStr
        {
            get  { return _dateStr; }
            set 
            {
                _dateStr = value;
                FPC("DateStr");
            }
        }
        private string _dateStr;
        #endregion

        #region FileName
        public string FileName
        {
            get  { return _fileName; }
            set 
            {
                _fileName = value;
                FPC("FileName");
            }
        }
        private string _fileName;
        #endregion

        #region SmallFileName
        public string SmallFileName
        {
            get { return _smallFileName; }
            set
            {
                _smallFileName = value;
                FPCNC("SmallFileName");
            }
        }
        private string _smallFileName;
        #endregion

        #region Title
        public string Title
        {
            get  { return _title; }
            set 
            {
                _title = value;
                FPC("Title");
            }
        }
        private string _title;
        #endregion

        #region People
        public PeopleList People
        {
            get { return _people; }
            set
            {
                _people = value;
                _oneFolder.UpdatePeopleList(_people);
                FPC("People");
                UpdateGlobalPeopleList();
            }
        }
        private PeopleList _people;
        #endregion

        #region AlbumT
        public string AlbumT
        {
            get { return _albumT; }
            set
            {
                _albumT = value;
                FPC("AlbumT");
                _oneFolder.UpdateAlbumList(_albumT, Month, Year);
            }
        }
        private string _albumT;
        #endregion

        private int Month
        {
            get
            {
                try
                {
                    DateTime dt = DateTime.ParseExact(_dateStr, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
                    return dt.Month;
                }
                catch
                {
                    return 1;
                }
            }
        }

        private int Year
        {
            get
            {
                try
                {
                    DateTime dt = DateTime.ParseExact(_dateStr, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
                    return dt.Year;
                }
                catch
                {
                    return 2000;
                }
            }
        }

        #region Place
        public string Place
        {
            get { return _place; }
            set
            {
                _place = value;
                FPC("Place");
            }
        }
        private string _place;
        #endregion

        #region NoShow
        public bool NoShow
        {
            get { return _noShow; }
            set
            {
                _noShow = value;
                FPC("NoShow");
            }
        }
        private bool _noShow;
        #endregion

        #region Favorite
        public bool Favorite
        {
            get { return _favorite; }
            set
            {
                _favorite = value;
                FPC("Favorite");
            }
        }
        private bool _favorite;
        #endregion

        #region FlickrId
        public string FlickrId
        {
            get { return _flickrId; }
            set
            {
                _flickrId = value;
                FPC("FlickrId");

                FlickrUploadPercent = (uint) ( (value == "" || value == null) ? 0 : 100 );
            }
        }
        private string _flickrId;
        #endregion

        #region FlickrSecret
        public string FlickrSecret
        {
            get { return _flickrSecret; }
            set
            {
                _flickrSecret = value;
                FPC("FlickrSecret");
            }
        }
        private string _flickrSecret;
        #endregion

        #region FlickrOriginalSecret
        public string FlickrOriginalSecret
        {
            get { return _flickrOriginalSecret; }
            set
            {
                _flickrOriginalSecret = value;
                FPC("FlickrOriginalSecret");
            }
        }
        private string _flickrOriginalSecret;
        #endregion

        #region FlickrFarm
        public string FlickrFarm
        {
            get { return _flickrFarm; }
            set
            {
                _flickrFarm = value;
                FPC("FlickrFarm");
            }
        }
        private string _flickrFarm;
        #endregion

        #region FlickrServer
        public string FlickrServer
        {
            get { return _flickrServer; }
            set
            {
                _flickrServer = value;
                FPC("FlickrServer");
            }
        }
        private string _flickrServer;
        #endregion

        #region FlickrUploadPercent
        public uint FlickrUploadPercent
        {
            get { return _flickrUploadPercent; }
            set
            {
                _flickrUploadPercent = value;
                FPC("FlickrUploadPercent");
            }
        }
        private uint _flickrUploadPercent;
        #endregion

        #region GlobalPeopleList
        public PeopleList GlobalPeopleList
        {
            get { return _globalPeopleList; }
            set
            {
                _globalPeopleList = value;
                FPC("GlobalPeopleList");
            }
        }
        private PeopleList _globalPeopleList = new PeopleList();
        #endregion

        public OneFolder Owner { get { return _oneFolder; } }

        public bool Abandon;
        public string JustTheName;
        private OneFolder _oneFolder;
    }

    public class MyList<T> : ObservableCollection<T> where T : LinkNode
    {
        protected override void InsertItem(int index, T item)
        {
            if (index > 0)
            {
                item.Next = null;
                item.Previous = this[index - 1];
                this[index - 1].Next = item;
            }
            base.InsertItem(index, item);
        }
    }

    public class AlbumList : MyList<Album> { }
    public class PhotoList : MyList<Photo> { }

    public class OneFolder : INPC
    {
        public Dictionary<ulong, Contact> Contacts;
        public Dictionary<string, List<Person>> FacesPerFile;

        public OneFolder(Dictionary<ulong, Contact> contacts)
            : base(null) 
        { 
            Contacts = contacts;
            FacesPerFile = new Dictionary<string, List<Person>>();
        }

        #region Albums
        public AlbumList Albums
        {
            get { return _albums; }
            set
            {
                _albums = value;
                FPC("Albums");
            }
        }
        private AlbumList _albums = new AlbumList();
        #endregion

        #region Photos
        public PhotoList Photos
        {
            get { return _photos; }
            set
            {
                _photos = value;
                FPC("Photos");
            }
        }
        private PhotoList _photos = new PhotoList();
        #endregion

        #region People
        public PeopleList People
        {
            get { return _people; }
            set
            {
                _people = value;
                FPC("People");
            }
        }
        private PeopleList _people = new PeopleList();
        #endregion

        public void UpdatePeopleList(PeopleList names)
        {
            foreach (var n in names)
            {
                if (!People.Contains(n))
                {
                    People.Add(n);
                }
            }
        }

        public void UpdateAlbumList(string albumname, int albummonth, int albumyear)
        {
            bool fFound = false;

            foreach(var a in Albums)
            {
                if (a.Name == albumname)
                {
                    fFound = true;
                    break;
                }
            }
            if (!fFound)
            {
                Album a = new Album(this);
                a.Name = albumname;
                a.Month = albummonth.ToString();
                a.Year = albumyear.ToString();
                Albums.Add(a);
            }

            // now go through the full albumlist to make sure that there is atleast one photo which has that album title
            AlbumList toremove = new AlbumList();
            foreach (var a in Albums)
            {
                var count = 0;
                foreach (var p in Photos)
                {
                    if (p.AlbumT == a.Name)
                    {
                        count++;
                    }
                }
                if (count == 0)
                {
                    toremove.Add(a);
                }
                else
                {
                    a.Count = count;
                }
            }
            if (!_fLoading)
            {
                foreach (var a in toremove)
                {
                    Albums.Remove(a);
                }
            }
        }

        public static PeopleList StringToList(string input)
        {
            PeopleList output = new PeopleList();
            if (input != null)
            {
                string[] ss = input.Split(',');
                foreach (string s in ss)
                {
                    if (s != "and" && s != "" && !output.Contains(new Person(s)))
                    {
                        var strimmed = s.Trim();
                        if (strimmed != null && strimmed != "")
                        {
                            output.Add(new Person(strimmed));
                        }
                    }
                }
            }
            return output;
        }

        public static RectList StringToRectList(string input)
        {
            RectList output = new RectList();

            if (input != null)
            {
                string[] ss = input.Split(',');
                foreach (string s in ss)
                {
                    ulong u = 0;
                    if (s != "")
                    {
                        var strimmed = s.Trim();
                        if (strimmed != null && strimmed != "")
                        {
                            ulong val;
                            bool f;
                            f = ulong.TryParse(strimmed, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out val);
                            if (f)
                            {
                                u = val;
                            }
                        }
                    }
                    output.Add(u);
                }
            }
            return output;
        }

        public void Deserialize(DirectoryInfo di)
        {
            new PicasaFaceReader(this).ReadFacesPerFile(di);
            UpdatePeopleList(StringToList("Sujal, Anjali, Swarali, Kinner"));
            _alcrw = new ALCReaderWriter(this, di);

            try
            {
                _fLoading = true;
                Changed = _alcrw.Read();
            }
            finally
            {
                _fLoading = false;
            }

            _fAbandon = false;
            _TNThread = new Thread(BuildThumbnails);
            _TNThread.Start();

            //_FlickrThread = new Thread(FlickrThread);
            //_FlickrThread.Start();
        }

        #region ThumbnailCreationThread
        private delegate void SetFileNameDelegate(Photo p, String fileName, String dt);
        private void SetFN(Photo p, String fileName, String dt)
        {
            p.SmallFileName = fileName;
            if (dt != "")
            {
                p.DateStr = dt;
            }
        }

        void BuildThumbnails()
        {
            string ThumbPath = _alcrw.Path.Replace("Photos", "Photos\\Thumbs");

            if (Photos.Count != 0)
            {
                DirectoryInfo tdi = new DirectoryInfo(ThumbPath);
                tdi.Create();
            }

            foreach (Photo p in Photos)
            {
                if (_fAbandon)
                {
                    break;
                }
                string smallFileName = ThumbPath + p.JustTheName;

                string dt = "";

                if (!File.Exists(smallFileName))
                {
                    MPS.CreateThumbnail(p.FileName, smallFileName);
                }

                if (p.DateStr == "")
                {
                    dt = MPS.GetDateTimeStamp(p.FileName);
                }

                Window1.Main.Dispatcher.BeginInvoke(new SetFileNameDelegate(SetFN), p, smallFileName, dt);

                GC.Collect(2, GCCollectionMode.Forced);
            }
        }
        #endregion

#if NEEDED
        #region FlickrThread
        private static FlickrNet.Flickr g_FR;
        private static FlickrNet.Auth g_Auth;
        private static string g_AuthToken = "72157612276635695-47b1eb63b137deb9";

        static OneFolder()
        {
            g_FR = new FlickrNet.Flickr("1d39fd2053d23729f64981fd2dcd3cdb", "3dd5b228858000ed", g_AuthToken);
            g_Auth = g_FR.AuthCheckToken(g_AuthToken);
            g_FR.OnUploadProgress += new FlickrNet.Flickr.UploadProgressHandler(g_FR_OnUploadProgress);
        }

        private delegate void UploadProgressInfo(FlickrNet.Photo p, String fileName);

        static bool g_FR_OnUploadProgress(object sender, FlickrNet.UploadProgressEventArgs e)
        {
            if (_photoCurrentlyBeingUploaded.Abandon)
            {
                return false;
            }
            else
            {
                uint progress = ((uint)e.Bytes * 100) / ((uint)e.TotalBytes);
                Window1.Main.Dispatcher.BeginInvoke(new OnUploadProgressDelegate(OnUploadProgress), _photoCurrentlyBeingUploaded, progress);
                return true;
            }
        }

        public delegate void OnUploadProgressDelegate(Photo p, uint progress);
        static void OnUploadProgress(Photo p, uint progress)
        {
            p.FlickrUploadPercent = progress;
        }

        public delegate void FillPhotoDataDelegate(Photo p, string id, string secret, string osecret, string farm, string server);
        void FillPhotoData(Photo p, string id, string secret, string osecret, string farm, string server)
        {
            p.FlickrId = id;
            p.FlickrSecret = secret;
            p.FlickrOriginalSecret = osecret;
            p.FlickrFarm = farm;
            p.FlickrServer = server;

            Save();
        }

        void FlickrThread()
        {
            bool abortme = false;

            //PhotoSearchOptions options = new PhotoSearchOptions(g_Auth.User.UserId);
            //options.SortOrder = PhotoSearchSortOrder.DatePostedDesc;
            //FlickrNet.Photos photos = g_FR.PhotosSearch(options);
            //Flickr.FlushCache(g_FR.LastRequest);

            foreach (var p in Photos)
            {
                if (_fAbandon)
                {
                    break;
                }

                if (p.NoShow == false && (p.FlickrId == null || p.FlickrId == ""))
                {
                    try
                    {
                        _photoCurrentlyBeingUploaded = p;
                        _photoCurrentlyBeingUploaded.Abandon = false;
                        string flickrId = g_FR.UploadPicture(p.FileName, p.Title == "" ? p.JustTheName : p.Title);
                        //string flickrId = IdFromList(photos, p.Title == "" ? p.JustTheName : p.Title);
                        if (flickrId != "") 
                        {
                            FlickrNet.PhotoInfo pi = g_FR.PhotosGetInfo(flickrId);
                            Window1.Main.Dispatcher.BeginInvoke(new FillPhotoDataDelegate(FillPhotoData), p, flickrId, pi.Secret, pi.OriginalSecret, pi.Farm, pi.Server);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is WebException)
                        {
                            WebException wex = ex as WebException;
                            if (wex.Status == WebExceptionStatus.RequestCanceled)
                            {
                                abortme = true;
                            }
                        }
                    }
                    finally
                    {
                        _photoCurrentlyBeingUploaded.Abandon = false;
                        _photoCurrentlyBeingUploaded = null;
                    }
                    if (abortme)
                    {
                        break;
                    }
                }
            }
        }

        private string IdFromList(FlickrNet.Photos photos, string title)
        {
            foreach(var p in photos.PhotoCollection)
            {
                if (p.Title == title)
                {
                    return p.PhotoId;
                }
            }
            return "";
        }
        #endregion
#endif

        public void Abandon()
        {
            _fAbandon = true;
            if (_TNThread != null)
            {
                _TNThread.Join();
                _TNThread = null;
            }

            if (_photoCurrentlyBeingUploaded != null)
            {
                _photoCurrentlyBeingUploaded.Abandon = true;
                if (_FlickrThread != null)
                {
                    _FlickrThread.Join();
                    _FlickrThread = null;
                }
                _photoCurrentlyBeingUploaded = null;
            }
        }

        internal void Save()
        {
            _alcrw.Write();
            Changed = false;
        }

        public string Path { get { return _alcrw == null ? "" : _alcrw.Path; } }

        private ALCReaderWriter _alcrw;
        private bool _fAbandon;
        private Thread _TNThread;
        private Thread _FlickrThread;
        static private Photo _photoCurrentlyBeingUploaded;
        bool _fLoading;
    }

}
