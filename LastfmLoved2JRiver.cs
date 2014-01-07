
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using CookComputing.XmlRpc;
using System.Windows.Forms;
using LastFm.APIMethods;
//css_reference MediaCenter.dll;


class Script : MarshalByRefObject
{
  public void Init(MediaCenter.MCAutomation mediaCenterInterface)
  {
    // Last.fm username
    string username = "INSERT YOUR LAST.FM USERNAME HERE";
    // Number of tracks to fetch (to get them all, just set this to a large enough value)
    int numTracks = 200;
	  // Name of playlist to create in JRiver Media Center (will be overwritten!)
	  string playlistName = "Last.fm Loved Tracks";

    LastFM lfm = new LastFM();
    Console.WriteLine("Fetching loved tracks for user '" + username + "' from Last.fm ...");
    LovedTrack[] lovedTracks = lfm.getLovedTracks(username, numTracks);

    MediaCenter.IMJPlaylistsAutomation localPlaylists = mediaCenterInterface.GetPlaylists();
    bool successDelete = localPlaylists.DeletePlaylist(null, playlistName);
    MediaCenter.IMJPlaylistAutomation lovedTracksPlaylist = localPlaylists.CreatePlaylist(null, playlistName);
      
    Console.WriteLine("Matching tracks from Last.fm to tracks in JRiver Media Center ...");
    foreach (LovedTrack track in lovedTracks)
    {
      DateTime lfmLoved = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
      lfmLoved = lfmLoved.AddSeconds(Int64.Parse(track.Date));
      lfmLoved = lfmLoved.ToLocalTime();
                        
      string searchString = "[Artist]=\"" + track.Artist + "\" [Name]=\"" + track.Name + "\"";
      MediaCenter.IMJFilesAutomation localTracks = mediaCenterInterface.Search(searchString);

      // Just use the first matching file (if any)
      if (localTracks.GetNumberFiles()>0)
      {
        MediaCenter.IMJFileAutomation localTrack = localTracks.GetFile(0);
        bool successAdd = lovedTracksPlaylist.AddFile(localTrack.Filename, -1);
        if (successAdd == false)
        {
          Console.WriteLine("Error: Failure adding " + track.Artist + " - " + track.Name + " to loved tracks playlist\t\t");
        }
        else
        {
          Console.WriteLine("Added " + track.Artist + " - " + track.Name + " to loved tracks playlist\t\t");
        }
      }
    }
    Console.WriteLine("Finished!");
  }
}


namespace LastFm.APIMethods
{

  public struct RecentTrack
  {
    private string _artist;
    private string _name;
    private string _album;
    private string _date;

    public string Artist
    {
      get {
        return _artist;
      }
      set {
        _artist = value;
      }
    }

    public string Name
    {
      get {
        return _name;
      }
      set {
        _name = value;
      }
    }

    public string Album
    {
      get {
        return _album;
      }
      set {
        _album = value;
      }
    }

    public string Date
    {
      get {
        return _date;
      }
      set {
        _date = value;
      }
    }

    public RecentTrack(string artist, string name, string album, string date)
    {
      this._artist = artist;
      this._name = name;
      this._album = album;
      this._date = date;
    }
  }
  
  public struct LibraryTrack
  {
    private string _artist;
    private string _name;
    private string _album;
    private int _playcount;

    public string Artist
    {
      get {
        return _artist;
      }
      set {
        _artist = value;
      }
    }

    public string Name
    {
      get {
        return _name;
      }
      set {
        _name = value;
      }
    }

    public string Album
    {
      get {
        return _album;
      }
      set {
        _album = value;
      }
    }

    public int PlayCount
    {
      get {
        return _playcount;
      }
      set {
        _playcount = value;
      }
    }

    public LibraryTrack(string artist, string name, string album, int playcount)
    {
      this._artist = artist;
      this._name = name;
      this._album = album;
      this._playcount = playcount;
    }
  }

  public struct LovedTrack
  {
    private string _artist;
    private string _name;
    private string _date;
    
    public string Artist
    {
      get {
        return _artist;
      }
      set {
        _artist = value;
      }
    }

    public string Name
    {
      get {
        return _name;
      }
      set {
        _name = value;
      }
    }
    
    public string Date
    {
      get {
        return _date;
      }
      set {
        _date = value;
      }
    }
    
    public LovedTrack(string artist, string name, string date)
    {
      this._artist = artist;
      this._name = name;
      this._date = date;
    }
  }
  
  
  public struct RecentTracksRequest
  {
    public string api_key;
    public string user;
    public int limit;
    public int page;

    public RecentTracksRequest(string _user, string _api_key, int _limit, int _page)
    {
      this.api_key = _api_key;
      this.user = _user;
      this.limit = _limit;  // Last.fm API: limit must be between 1 and 200. Default is 10.
      this.page = _page;
    }
  }
  
  public struct LibraryTracksRequest
  {
    public string api_key;
    public string user;
    public int limit;
    public int page;

    public LibraryTracksRequest(string _user, string _api_key, int _limit, int _page)
    {
      this.api_key = _api_key;
      this.user = _user;
      this.limit = _limit;
      this.page = _page;
    }
  }

  public struct LovedTracksRequest
  {
    public string api_key;
    public string user;
    public int limit;

    public LovedTracksRequest(string _user, string _api_key, int _limit)
    {
      this.api_key = _api_key;
      this.user = _user;
      /* Tracks are returned in paginated results, but since there is no
       * maximum imposed on the limit variable here (unlike user.getRecentTracks),
       * we simply ignore pagination and fetch only the first page.
       * Hence, set limit to as many tracks as you want to fetch, or
       * to a large (very large? depends on how many tracks you've loved)
       * number to get them all.
       * Default for limit when not specified is 50, we set it to 100 below.
       */
      this.limit = _limit;
    }  
  }
  
  
  [XmlRpcUrl("http://ws.audioscrobbler.com/2.0/")]
  public interface ILastFM : IXmlRpcProxy
  {
    // These map to the Last.fm API - see http://www.last.fm/api
    [XmlRpcMethod("user.getRecentTracks")]
    string getRecentTracks(RecentTracksRequest x);

    [XmlRpcMethod("library.getTracks")]
    string getLibraryTracks(LibraryTracksRequest x);
    
    [XmlRpcMethod("user.getLovedTracks")]
    string getLovedTracks(LovedTracksRequest x);
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
      return getRecentTracks(user, 100, 1);
    }

    public RecentTrack[] getRecentTracks(string user, int numberOfTracks, int maxPages)
    {
#if DEBUG
      //RequestResponseLogger dumper = new RequestResponseLogger();
      //dumper.Directory = "C:\\temp";
      //dumper.Attach(_proxy);
#endif
      int page = 1;
      int totalPages = 9999;
      if (maxPages>0)
      {
        Console.WriteLine("Number of pages that will be fetched is limited to " + maxPages);
        totalPages = maxPages;
      }
      else if (maxPages==0)
      {
        Console.WriteLine("All available pages will be fetched");
      }
      List<RecentTrack> recentTracks = new List<RecentTrack>();
      while (page<=totalPages)
      {
        if (page == 1) {
         Console.WriteLine("Fetching page " + page + " (" + numberOfTracks + " tracks per page) ...");
        } else {
         Console.WriteLine("Fetching page " + page + " (of " + totalPages + ") ...");
        }
        RecentTracksRequest request = new RecentTracksRequest(user, this._api_key, numberOfTracks, page);
        string reply = _proxy.getRecentTracks(request);

        //Last.fm magic quotes its replies for some reason
        reply = reply.Replace("\\'", "'");
        reply = reply.Replace("\\\"", "\"");

        XmlTextReader xml = new XmlTextReader(new System.IO.StringReader(reply));
        string artist = null;
        string name = null;
        string album = null;
        string date = null;

        while (xml.Read())
        {
          switch (xml.NodeType)
          {
          case XmlNodeType.Element:
            if (xml.Name.Equals("recenttracks") && maxPages==0)
            {
              totalPages = Convert.ToInt32(xml.GetAttribute("totalPages"));
              if (page==1) Console.WriteLine("Total number of pages to fetch: " + totalPages);
            }
            else if (xml.Name.Equals("artist"))
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
        page++;
      }
      return recentTracks.ToArray();
    }


    public LibraryTrack[] getLibraryTracks(string user)
    {
      return getLibraryTracks(user, 200);
    }

    public LibraryTrack[] getLibraryTracks(string user, int numberOfTracks)
    {
      int page = 1;
      int totalPages = 9999;
      List<LibraryTrack> libraryTracks = new List<LibraryTrack>();
      while (page<=totalPages)
      {
        if (page == 1) {
         Console.WriteLine("Fetching page " + page + " (" + numberOfTracks + " tracks per page) ...");
        } else {
         Console.WriteLine("Fetching page " + page + " (of " + totalPages + ") ...");
        }
        LibraryTracksRequest request = new LibraryTracksRequest(user, this._api_key, numberOfTracks, page);
        string reply = _proxy.getLibraryTracks(request);

        //Last.fm magic quotes its replies for some reason
        reply = reply.Replace("\\'", "'");
        reply = reply.Replace("\\\"", "\"");

        XmlTextReader xml = new XmlTextReader(new System.IO.StringReader(reply));
        string artist = null;
        string name = null;
        string album = null;
        int playcount = 0;

        bool insideArtistElement = false;
        bool insideAlbumElement = false;

        while (xml.Read())
        {
          switch (xml.NodeType)
          {
          case XmlNodeType.Element:
            if (xml.Name.Equals("tracks"))
            {
              totalPages = Convert.ToInt32(xml.GetAttribute("totalPages"));
              if (page==1) Console.WriteLine("Total number of pages to fetch: " + totalPages);
            }
            else if (xml.Name.Equals("name"))
            {
              if (insideArtistElement == true)
              {
                artist = xml.ReadElementContentAsString();
              }
              else if (insideAlbumElement == true)
              {
                album = xml.ReadElementContentAsString();
              }
              else
              {
                name = xml.ReadElementContentAsString();
              }
            }
            else if (xml.Name.Equals("artist"))
            {
              insideArtistElement = true;
            }
            else if (xml.Name.Equals("album"))
            {
              insideAlbumElement = true;
            }
            else if (xml.Name.Equals("playcount"))
            {
              playcount = xml.ReadElementContentAsInt();
            }
            break;
          case XmlNodeType.EndElement:
            if (xml.Name.Equals("track"))
            {
              if(artist != null && album != null && name != null && playcount > 0) libraryTracks.Add(new LibraryTrack(artist, name, album, playcount));
              artist = null;
              name = null;
              album = null;
              playcount = 0;
            }
            else if (xml.Name.Equals("artist"))
            {
              insideArtistElement = false;
            }
            else if (xml.Name.Equals("album"))
            {
              insideAlbumElement = false;;
            }
            break;
          }
        }
        page++;
      }
      return libraryTracks.ToArray();
    }

    
    public LovedTrack[] getLovedTracks(string user)
    {
      return getLovedTracks(user, 100);
    }
    
    public LovedTrack[] getLovedTracks(string user, int numberOfTracks)
    {
      Console.WriteLine("Fetching maximum " + numberOfTracks + " loved tracks ...");
      List<LovedTrack> lovedTracks = new List<LovedTrack>();
      LovedTracksRequest request = new LovedTracksRequest(user, this._api_key, numberOfTracks);
      string reply = _proxy.getLovedTracks(request);

      //Last.fm magic quotes its replies for some reason
      reply = reply.Replace("\\'", "'");
      reply = reply.Replace("\\\"", "\"");

      XmlTextReader xml = new XmlTextReader(new System.IO.StringReader(reply));
      string artist = null;
      string name = null;
      string date = null;

      bool insideArtistElement = false;

      while (xml.Read())
      {
        switch (xml.NodeType)
        {
        case XmlNodeType.Element:
          if (xml.Name.Equals("name"))
          {
            if (insideArtistElement == true) 
            {
              artist = xml.ReadElementContentAsString();
            } else {
              name = xml.ReadElementContentAsString();
            }                
          }
          else if (xml.Name.Equals("date"))
          {
            date = xml.GetAttribute("uts");
          }
          else if (xml.Name.Equals("artist"))
          {
            insideArtistElement = true;
          } 
          break;
        case XmlNodeType.EndElement:
          if (xml.Name.Equals("track"))
          {
            if(artist != null && name != null && date != null) lovedTracks.Add(new LovedTrack(artist, name, date));
            artist = null;
            name = null;
            date = null;
          }            
          else if (xml.Name.Equals("artist"))
          {
            insideArtistElement = false;
          }
          break;
        }
      }
      return lovedTracks.ToArray();
    }
    
    
    private void LogMessageToFile(string msg)
    {
      System.IO.StreamWriter sw = System.IO.File.AppendText("%TEMP%\\Ehead.LastFmAPI.txt");
      try
      {
        string logLine = System.String.Format("{0:G}: {1}.", System.DateTime.Now, msg);
        sw.WriteLine(logLine);
      }
      finally
      {
        sw.Close();
      }
    }
  }
}

