using ALCRWNS;
using System.IO;
using System.Windows;
using System;
using System.Globalization;
using System.Collections.Generic;

namespace WPFALC
{
    public class ALCReaderWriter : ALCRW
    {
        public ALCReaderWriter(OneFolder context, DirectoryInfo di) : base(di) { _context = context; }

        protected override bool AddOnePhotoOverride(
            string DateStr,
            string JustTheName,
            string Title,
            string People,
            string AlbumT,
            string Place,
            bool NoShow,
            bool Favorite,
            string FlickrId,
            string FlickrSecret,
            string FlickrOriginalSecret,
            string FlickrFarm,
            string FlickrServer,
            string Rectangles
            )
        {
            bool fChanged = false;

            Photo p = new Photo(_context);

            p.DateStr = DateStr;
            p.JustTheName = JustTheName;
            _context.Photos.Insert(_context.Photos.Count, p);
            p.FileName = Path + JustTheName;
            p.Title = Title;
            PeopleList peoplelist = OneFolder.StringToList(People);
            p.AlbumT = AlbumT;
            p.Place = Place;
            p.NoShow = NoShow;
            p.Favorite = Favorite;
            p.FlickrId = FlickrId;
            p.FlickrSecret = FlickrSecret;
            p.FlickrOriginalSecret = FlickrOriginalSecret;
            p.FlickrFarm = FlickrFarm;
            p.FlickrServer = FlickrServer;

            RectList rects = OneFolder.StringToRectList(Rectangles);

            // First fill in the rects for each name. 
            for (int i = 0; i < Math.Min(peoplelist.Count, rects.Count); i++)
            {
                peoplelist[i].Rect = rects[i];
            }

            // Next, if the ini file had data, it will override the rect data that we had in the .abm file.
            if (_context.FacesPerFile.ContainsKey(JustTheName))
            {
                PeopleList faces = new PeopleList();
                foreach (var face in _context.FacesPerFile[JustTheName])
                {
                    Contact c;
                    try
                    {
                        c = _context.Contacts[face.PId];
                    }
                    catch
                    {
                        continue;
                    }
                    if (!peoplelist.Contains(new Person(c.Alias)))
                    {
                        peoplelist.Add(new Person(c.Alias, face.Rect, face.PId));
                    }
                    else
                    {
                        foreach (var person in peoplelist)
                        {
                            if (person.Name == c.Alias && person.PId == 0)
                            {
                                person.PId = face.PId;

                                if (person.Rect != face.Rect)
                                {
                                    fChanged = true;
                                }
                                if ((person.Rect != 0) && (person.Rect != face.Rect)) MessageBox.Show("Rectangle mismatch");

                                person.Rect = face.Rect;
                                break;
                            }
                        }
                    }
                }
            }
            p.People = peoplelist;

            return fChanged;
        }

        protected override void AddOneAlbumOverride(string Name, string Month, string Year, string Photo, string Story)
        {
            Album a = new Album(_context);
            a.Name = Name;
            a.Month = Month;
            a.Year = Year;
            a.Photo = Photo;
            a.Story = Story;
            _context.Albums.Insert(_context.Albums.Count, a);
        }

        protected override bool GetOnePhotoOverride(int i, out string DateStr, out string JustTheName, out string Title,
            out string People, out string AlbumT, out string Place, out bool NoShow, out bool Favorite,
            out string FlickrId, out string FlickrSecret, out string FlickrOriginalSecret, out string FlickrFarm, out string FlickrServer,
            out string Rectangles)
        {
            if (i < _context.Photos.Count)
            {
                Photo px = _context.Photos[i];

                DateStr = px.DateStr;
                JustTheName = px.JustTheName;
                Title = px.Title;
                string ps =
                People = (string)(new PLVC()).Convert(px.People, typeof(string), null, CultureInfo.CurrentCulture);
                AlbumT = px.AlbumT;
                Place = px.Place;
                NoShow = px.NoShow;
                Favorite = px.Favorite;
                FlickrId = px.FlickrId;
                FlickrSecret = px.FlickrSecret;
                FlickrOriginalSecret = px.FlickrOriginalSecret;
                FlickrFarm = px.FlickrFarm;
                FlickrServer = px.FlickrServer;
                Rectangles = (string)(new RLVC()).Convert(px.People, typeof(string), null, CultureInfo.CurrentCulture);
                return true;
            }
            else
            {
                DateStr = JustTheName = Title = People = AlbumT = Place = FlickrId = FlickrSecret = FlickrOriginalSecret =
                    FlickrFarm = FlickrServer = Rectangles = null;
                NoShow = Favorite = false;
                return false;
            }
        }

        protected override bool GetOneAlbumOverride(int i, out string Name, out string Month, out string Year, out string Photo, out string Story)
        {
            if (i < _context.Albums.Count)
            {
                Album a = _context.Albums[i];
                Name = a.Name;
                Month = a.Month;
                Year = a.Year;
                Photo = a.Photo;
                Story = a.Story;
                return true;
            }
            else
            {
                Name = Month = Year = Photo = Story = null;
                return false;
            }
        }

        private OneFolder _context;
    }

    public class PicasaContactReader : PicasaRW
    {
        public PicasaContactReader() : base() { }

        new public Dictionary<ulong, Contact> ReadContacts()
        {
            _contacts = new Dictionary<ulong, Contact>();
            base.ReadContacts();
            return _contacts;
        }

        protected override void AddOneContact(ulong id, string name, string alias)
        {
            Contact c = new Contact();
            c.Id = id;
            c.Name = name;
            c.Alias = alias;
            _contacts.Add(id, c);
        }

        Dictionary<ulong, Contact> _contacts;
    }

    public class PicasaFaceReader : PicasaRW
    {
        public PicasaFaceReader(OneFolder context) : base() { _context = context;  }

        protected override void NewFile(string filename)
        {
            _faces = new List<Person>();
        }

        protected override void NewFace(ulong id, ulong rect)
        {
            if (_faces != null)
            {
                _faces.Add(new Person(null, rect, id));
            }
        }

        protected override void DoneFile(string filename)
        {
            if (_faces != null && _faces.Count > 0)
            {
                _context.FacesPerFile.Add(filename, _faces);
            }
            _faces = null;
        }

        new public void ReadFacesPerFile(DirectoryInfo di)
        {
            base.ReadFacesPerFile(di);
        }

        private List<Person> _faces;
        private OneFolder _context;
    }
}
