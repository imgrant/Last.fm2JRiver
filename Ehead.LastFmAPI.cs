
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using CookComputing.XmlRpc;
using System.Windows.Forms;

namespace Ehead.LastFmAPI
{
    public struct SimilarTrack
    {
        private string _name;
        private string _artist;
        private float _match;

        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        public string Artist
        {
            get { return _artist; }
            set { _artist = value; }
        }

        public float Match
        {
            get { return _match; }
            set { _match = value; }
        }
    }

    public struct SimilarArtist
    {
        private string _artist;
        private float _match;
        
        public string Artist
        {
            get { return _artist; }
            set { _artist = value; }
        }

        public SimilarArtist(string artist, float match)
        {
            this._artist = artist;
            this._match = match;
        }

        public float Match
        {
            get { return _match; }
            set { _match = value; }
        }
    }

    public struct RecentTrack
    {
        private string _artist;
        private string _name;
        private string _album;
        private string _date;

        public string Artist
        {
            get { return _artist; }
            set { _artist = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Album
        {
            get { return _album; }
            set { _album = value; }
        }

        public string Date
        {
            get { return _date; }
            set { _date = value; }
        }

        public RecentTrack(string artist, string name, string album, string date)
        {
            this._artist = artist;
            this._name = name;
            this._album = album;
            this._date = date;
        }
    }

    public struct SimilarTracksRequest
    {
        public string api_key;
        public string artist;
        public string track;

        public SimilarTracksRequest(string artist, string track, string api_key)
        {
            this.api_key = api_key;
            this.artist = artist;
            this.track = track;
        }
    }

    public struct SimilarArtistsRequest
    {
        public string api_key;
        public string artist;

        public SimilarArtistsRequest(string _artist, string _api_key)
        {
            this.api_key = _api_key;
            this.artist = _artist;
        }
    }

    public struct RecentTracksRequest
    {
        public string api_key;
        public string user;
        public string limit;

        public RecentTracksRequest(string _user, string _api_key)
        {
            this.api_key = _api_key;
            this.user = _user;
            this.limit = "50";
        }

        public RecentTracksRequest(string _user, string _api_key, string _limit)
        {
            this.api_key = _api_key;
            this.user = _user;
            this.limit = _limit;
        }
    }

    [XmlRpcUrl("http://ws.audioscrobbler.com/2.0/")]
    public interface ILastFM : IXmlRpcProxy
    {
        // These map to the Last.fm API
        [XmlRpcMethod("artist.getSimilar")]
        string getSimilarArtists(SimilarArtistsRequest x);

        [XmlRpcMethod("track.getSimilar")]
        string getSimilarTracks(SimilarTracksRequest x);

        [XmlRpcMethod("user.getRecentTracks")]
        string getRecentTracks(RecentTracksRequest x);
    }

    public class LastFM
    {
        private ILastFM _proxy;
        private string _api_key = "65af509b7e99d8895de0f087b173e54e";
        private int _timeout = 15000;

        public LastFM()
        {
            this._proxy = XmlRpcProxyGen.Create<ILastFM>();
            _proxy.Timeout = _timeout;
        }

        public LastFM(int timeout): this()
        {
            _proxy.Timeout = timeout;
            this._timeout = timeout;
        }


        public RecentTrack[] getRecentTracks(string user)
        {
            return getRecentTracks(user, "50");
        }

        public RecentTrack[] getRecentTracks(string user, string numberOfTracks)
        {
            #if DEBUG
            //RequestResponseLogger dumper = new RequestResponseLogger();
            //dumper.Directory = "C:\\temp";
            //dumper.Attach(_proxy);
            #endif

            List<RecentTrack> recentTracks = new List<RecentTrack>();
            RecentTracksRequest request = new RecentTracksRequest(user, this._api_key, numberOfTracks);
            string reply = _proxy.getRecentTracks(request);

            //Last.fm magic quotes it's replies for some reason
            reply = reply.Replace("\\'", "'");
            reply = reply.Replace("\\\"", "\"");

            XmlTextReader xml = new XmlTextReader(new System.IO.StringReader(reply));
            string artist = null;
            string name = null;
            string album = null;
            string date = null;

            //LogMessageToFile("User: " + user + "\n");
            RecentTrack currentTrack = new RecentTrack();
            bool insideArtistElement = false;

            while (xml.Read())
            {
                switch (xml.NodeType)
                {
                    case XmlNodeType.Element:
                        if (xml.Name.Equals("artist"))
                        {
                            artist = xml.ReadElementContentAsString();
                        }
                        else if (xml.Name.Equals("name"))
                        {
                            name = xml.ReadElementContentAsString();
                        }
                        else if (xml.Name.Equals("album"))
                        {
                            album = xml.ReadElementContentAsString();
                        }
                        else if (xml.Name.Equals("date"))
                        {
                            date = xml.GetAttribute("uts");
                        }
                        break;
                    case XmlNodeType.EndElement:
                        //LogMessageToFile(artistName + "  -  " + artistMatch);
                        if (xml.Name.Equals("track"))
                        {
                            if(artist != null && name != null && date != null) recentTracks.Add(new RecentTrack(artist, name, album, date));
                            artist = null;
                            name = null;
                            album = null;
                            date = null;
                        }
                        break;
                }
            }

            return recentTracks.ToArray();
        }

        public SimilarArtist[] getSimilarArtists(string artist)
        {
            return getSimilarArtists(artist, 0);
        }

        public SimilarArtist[] getSimilarArtists(string artist, int similarity)
        {
            #if DEBUG
            //RequestResponseLogger dumper = new RequestResponseLogger();
            //dumper.Directory = "C:\\temp";
            //dumper.Attach(_proxy);
            #endif

            List<SimilarArtist> similarArtists = new List<SimilarArtist>();
            SimilarArtistsRequest request = new SimilarArtistsRequest(artist, this._api_key);
            string reply = _proxy.getSimilarArtists(request);

            //Last.fm magic quotes it's replies for some reason
            reply = reply.Replace("\\'", "'");
            reply = reply.Replace("\\\"", "\"");

            XmlTextReader xml = new XmlTextReader(new System.IO.StringReader(reply));
            string artistName = null;
            float artistMatch = 0;
            //LogMessageToFile("Similarity: " + similarity + "\n");
            similarity = 0;
            while (xml.Read())
            {
                switch (xml.NodeType)
                {
                    case XmlNodeType.Element:
                        if (xml.Name.Equals("name"))
                        {
                            artistName = xml.ReadElementContentAsString();
                        } 
                        else if (xml.Name.Equals("match")) 
                        {
                            artistMatch = xml.ReadElementContentAsFloat();
                        }
                        break;
                    case XmlNodeType.EndElement:
                        //LogMessageToFile(artistName + "  -  " + artistMatch);
                        if (xml.Name.Equals("artist") && artist != null && artistMatch > similarity)
                        {
                            similarArtists.Add(new SimilarArtist(artistName,artistMatch));
                        }
                        break;
                }
            }
            foreach (SimilarArtist str in similarArtists)
            {
                //LogMessageToFile(str.Artist.ToString() + "  -  " + str.Match.ToString());
            }
            return similarArtists.ToArray();
        }

        public SimilarTrack[] getSimilarTracks(string artist, string track)
        {
            return getSimilarTracks(artist, track, 0);
        }

        public SimilarTrack[] getSimilarTracks(string artist, string track, float similarity)
        {
            #if DEBUG
            //RequestResponseLogger dumper = new RequestResponseLogger();
            //dumper.Directory = "C:\\temp";
            //dumper.Attach(_proxy);
            #endif

            List<SimilarTrack> similarTracks = new List<SimilarTrack>();
            SimilarTracksRequest request = new SimilarTracksRequest(artist, track, this._api_key);
            string reply = _proxy.getSimilarTracks(request);
            
            reply = reply.Replace("\\'", "'");
            reply = reply.Replace("\\\"", "\"");

            XmlTextReader xml = new XmlTextReader(new System.IO.StringReader(reply));
            SimilarTrack currentTrack = new SimilarTrack();
            bool insideArtistElement = false;
            while (xml.Read())
            {
                switch (xml.NodeType)
                {
                    case XmlNodeType.Element:
                        if (xml.Name.Equals("name"))
                        {
                            if (insideArtistElement == false) currentTrack.Name = xml.ReadElementContentAsString();
                            else
                            {
                                currentTrack.Artist = xml.ReadElementContentAsString();
                            }
                        }
                        else if (xml.Name.Equals("match"))
                        {
                            currentTrack.Match = xml.ReadElementContentAsFloat();
                        }
                        else if (xml.Name.Equals("artist"))
                            insideArtistElement = true;
                        break;
                    case XmlNodeType.EndElement:
                        if (xml.Name.Equals("artist"))
                            insideArtistElement = false;
                        else if (xml.Name.Equals("track"))
                        {
                            if(currentTrack.Match>similarity) similarTracks.Add(currentTrack);
                        }
                        break;
                }
            }
            
            return similarTracks.ToArray();
        }
        
        private void LogMessageToFile(string msg)
        {
            System.IO.StreamWriter sw = System.IO.File.AppendText("C:\\temp\\Ehead.LastFmAPI.txt");
            try
            {
                string logLine = System.String.Format(
                    "{0:G}: {1}.", System.DateTime.Now, msg);
                sw.WriteLine(logLine);
            }
            finally
            {
                sw.Close();
            }
        }
    }
}
