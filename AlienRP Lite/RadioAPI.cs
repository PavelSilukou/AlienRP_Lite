using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Windows.Web.Http;
using System.Globalization;

namespace AlienRP_Lite
{
    class RadioAPI
    {
        static string quality = "premium";

        public async static Task<int> Login(string username, string password)
        {
            string jsonMessage;
            HttpResponseMessage response;
            try
            {
                Uri url = new Uri(String.Format("https://api.audioaddict.com/v1/di/members/authenticate?username={0}&password={1}", username, password));
                //Uri url = new Uri(String.Format("https://www.di.fm/login?username={0}&password={1}", username, password));
                HttpClient http = new HttpClient();

                response = await http.PostAsync(url, null);
                jsonMessage = await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                return 2;
            }

            if (jsonMessage.Equals("Invalid Username/Password"))
            {
                return 1;
            }

            if (response.IsSuccessStatusCode == false || response.StatusCode != HttpStatusCode.Ok)
            {
                return 2;
            }
            else
            {
                try
                {
                    JsonConvert.DeserializeObject<User>(jsonMessage);
                    JObject json = JObject.Parse(jsonMessage);

                    if (json["subscriptions"].HasValues)
                    {
                        string status = json["subscriptions"][0]["status"].ToString();
                        if (!status.Equals("active"))
                        {
                            return 3;
                        }
                    }
                    else
                    {
                        return 4;
                    }
                }
                catch (Exception)
                {
                    return 5;
                }
            }

            return 0;
        }

        public async static Task<int> GetAudioLinks()
        {
            string jsonMessage;
            HttpResponseMessage response;
            try
            {
                //Uri url = new Uri(String.Format("http://listen.di.fm/{0}/{1}?{2}", quality, PlayerSettings.currentChannelKey, User.listenKey));
                Uri url = new Uri(String.Format("http://www.di.fm/_papi/v1/di/listen/{0}/{1}?{2}", quality, PlayerSettings.currentChannelKey, User.listenKey));
                HttpClient http = new HttpClient();

                response = await http.GetAsync(url);
                jsonMessage = await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                return 1;
            }

            if (jsonMessage.Equals("Unable to find a matching streamlist"))
            {
                return 1;
            }
            else if (jsonMessage.Equals("Unable to find a channel by that key"))
            {
                return 1;
            }
            else if (jsonMessage.Equals("Invalid Listen Key"))
            {
                return 1;
            }
            else if (response.IsSuccessStatusCode == false || response.StatusCode != HttpStatusCode.Ok)
            {
                return 1;
            }
            else
            {
                try
                {
                    IList<string> audioStreams = JsonConvert.DeserializeObject<IList<string>>(jsonMessage);
                    Random rnd = new Random();
                    int randomStream = rnd.Next(audioStreams.Count);
                    PlayerSettings.audioStream = audioStreams[randomStream];

                    return 0;
                }
                catch (Exception)
                {
                    return 1;
                }
            } 
        }

        public async static Task<Track> GetNowPlayingTrack()
        {
            string jsonMessage;
            HttpResponseMessage response;
            try
            {
                Uri url = new Uri("http://www.di.fm/_papi/v1/di/track_history/channel/" + PlayerSettings.currentChannelId);
                HttpClient http = new HttpClient();

                response = await http.GetAsync(url);
                //response = await http.GetAsync(url);
                //response = await http.GetAsync(url);
                jsonMessage = await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                return null;
            }

            if (response.IsSuccessStatusCode == false || response.StatusCode != HttpStatusCode.Ok)
            {
                return null;
            }

            try
            {
                JArray json = JArray.Parse(jsonMessage);

                for (int i = 0; i < json.Count; i++)
                {
                    string type = json[i]["type"].ToString();
                    if (!type.Equals("advertisement"))
                    {
                        Track track = new Track();
                        track.artistName = json[i]["display_artist"].ToString();
                        track.trackName = json[i]["display_title"].ToString();
                        track.duration = Int32.Parse(json[i]["duration"].ToString());
                        track.startTime = Int32.Parse(json[i]["started"].ToString());
                        //track.id = json[i]["track_id"].ToString();

                        //track.upVote = Int32.Parse(json[i]["votes"]["up"].ToString());
                        //track.downVote = Int32.Parse(json[i]["votes"]["down"].ToString());

                        //if (json[i]["images"].HasValues)
                        //{
                        //    string coverUrl = json[i]["images"]["default"].ToString();
                        //    coverUrl = "http:" + coverUrl.Substring(0, coverUrl.IndexOf("jpg") + 3);
                        //    track.coverUrl = coverUrl + "?size=90x90";
                        //}
                        //else
                        //{
                        //    track.coverUrl = "";
                        //}

                        return track;
                    }
                }
            }
            catch (Exception)
            {
                
            }

            return null;
        }

        public async static Task<List<Channel>> GetAllChannels()
        {
            string jsonMessage;
            HttpResponseMessage response;
            try
            {
                Uri url = new Uri("http://www.di.fm/_papi/v1/di/channel_filters/1");
                HttpClient http = new HttpClient();

                response = await http.GetAsync(url);
                jsonMessage = await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                return null;
            }

            if (response.IsSuccessStatusCode == false || response.StatusCode != HttpStatusCode.Ok)
            {
                return null;
            }
            else
            {
                try
                {
                    JObject json = JObject.Parse(jsonMessage);

                    List<Channel> channels = new List<Channel>();

                    for (int i = 0; i < json["channels"].Count(); i++)
                    {
                        try
                        {
                            Channel channel = new Channel();

                            channel.id = json["channels"][i]["id"].ToString();
                            channel.key = json["channels"][i]["key"].ToString();
                            channel.name = json["channels"][i]["name"].ToString();

                            string imageUrl = json["channels"][i]["images"]["horizontal_banner"].ToString();
                            int imageFormatIndex = imageUrl.IndexOf("jpg");
                            if (imageFormatIndex == -1)
                            {
                                imageFormatIndex = imageUrl.IndexOf("png");
                            }

                            imageUrl = "http:" + imageUrl.Substring(0, imageFormatIndex + 3);
                            channel.imageUrl = imageUrl;


                            string imageCompactUrl = json["channels"][i]["images"]["compact"].ToString();
                            imageFormatIndex = imageCompactUrl.IndexOf("jpg");
                            if (imageFormatIndex == -1)
                            {
                                imageFormatIndex = imageCompactUrl.IndexOf("png");
                            }
                            imageCompactUrl = "http:" + imageCompactUrl.Substring(0, imageFormatIndex + 3);
                            channel.imageCompactUrl = imageCompactUrl + "?size=90x90";

                            channels.Add(channel);
                        }
                        catch (Exception) { }
                    }

                    IList<FavoriteChannel> favoriteChannels = await GetFavoritesChannelsList();

                    if (favoriteChannels == null)
                    {
                        return channels;
                    }

                    foreach (FavoriteChannel favorite in favoriteChannels.Reverse())
                    {
                        for (int i = 0; i < channels.Count; i++)
                        {
                            if (channels[i].id == favorite.channelId)
                            {
                                Channel tempChannel = channels.ElementAt(i);
                                tempChannel.isFavorite = true;
                                channels.RemoveAt(i);
                                channels.Insert(0, tempChannel);
                            }
                        }
                    }

                    return channels;
                }
                catch(JsonException)
                {
                    return null;
                }
            }
        }

        public async static Task<IList<FavoriteChannel>> GetFavoritesChannelsList()
        {
            string jsonMessage;
            HttpResponseMessage response;
            try
            {
                //Uri url = new Uri(String.Format("https://api.audioaddict.com/v1/di/members/{0}/favorites/channels?api_key={1}", User.id, User.apiKey));
                Uri url = new Uri(String.Format("https://www.di.fm/_papi/v1/di/members/{0}/favorites/channels?api_key={1}", User.id, User.apiKey));
                HttpClient http = new HttpClient();

                response = await http.GetAsync(url);
                jsonMessage = await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                return null;
            }

            if (jsonMessage.Contains("Member Authentication required"))
            {
                return null;
            }
            else if (jsonMessage.Equals("Invalid API Key"))
            {
                return null;
            }

            if (response.IsSuccessStatusCode == false || response.StatusCode != HttpStatusCode.Ok)
            {
                return null;
            }
            else
            {
                try
                {
                    return JsonConvert.DeserializeObject<IList<FavoriteChannel>>(jsonMessage);
                }
                catch (JsonException)
                {
                    return null;
                }
            }
        }

        //public async static void ProcessLogin(string username, string password)
        //{
        //    Task<int> task = Login(username, password);
        //    int x = await task;

        //    Debug.WriteLine(x);
        //    Debug.WriteLine(User.apiKey);
        //}

        public async static Task<string> Ping()
        {
            string jsonMessage;
            HttpResponseMessage response;
            try
            {
                Uri url = new Uri("http://www.di.fm/_papi/v1/ping");
                HttpClient http = new HttpClient();
                response = await http.GetAsync(url);
                jsonMessage = await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                return "";
            }

            DateTime now = DateTime.Now;

            try
            {
                JObject json = JObject.Parse(jsonMessage.ToString());
                string time = json["time"].ToString();
                //return time;
                PlayerSettings.timeOffset = ConvertToUnixTimestamp(now) - ConvertToUnixTimestamp(DateTime.ParseExact(time, "ddd, dd MMM yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture));
                return "";
            }
            catch (Exception)
            {
                PlayerSettings.timeOffset = ConvertToUnixTimestamp(now);
                return "";
            }
        }

        public static double ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date - origin;
            return Math.Floor(diff.TotalSeconds);
        }

        //private async void ProcessDataAsync()
        //{
        //    Task<string> task = Ping();
        //    string x = await task;
        //}
    }

    public class User
    {
        [JsonProperty("api_key")]
        public static string apiKey { get; set; }
        [JsonProperty("listen_key")]
        public static string listenKey { get; set; }
        [JsonProperty("first_name")]
        public static string firstName { get; set; }
        [JsonProperty("last_name")]
        public static string lastName { get; set; }
        [JsonProperty("id")]
        public static string id { get; set; }
    }

    public class FavoriteChannel
    {
        [JsonProperty("channel_id")]
        public string channelId;
        public int position;
    }

    public static class PlayerSettings
    {
        public static int qualitylistId = 0;
        public static string currentChannelKey = "trance";
        public static string currentChannelId = "1";
        public static string audioStream;
        public static double timeOffset = 0;
    }

    public class Channel
    {
        public string id;
        public string key;
        public string name;
        public string imageUrl;
        public bool isFavorite = false;
        public string imageCompactUrl;

        public Channel(string id, string key, string name)
        {
            this.id = id;
            this.key = key;
            this.name = name;
        }

        public Channel()
        {
        }
    }

    public class Track
    {
        public string artistName = "";
        public string trackName = "";
        public int duration;
        public int startTime;
        //public string id;
        //public string coverUrl = "";
        //public int downVote;
        //public int upVote;
    }
}
