using HearthArenaUploader.Data;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Logging;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Threading.Tasks;
using System.Web;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace HearthArenaUploader
{
	public class HearthArenaUploaderLogic
	{
		const string prefix = "arena_run";
		readonly string rememberMe = "on";
		readonly string submit = "Login";
		readonly string connectionError = "connection error. Try again, whene heartharena.com is available.";

		string username;
		SecureString password;

		Uri uriAddArena = new Uri("https://www.heartharena.com/my-arenas/add");

		public HearthArenaUploaderLogic(string username, SecureString password)
		{
			this.username = username;
			this.password = password;
		}

		public async Task<Result<UploadResults>> LoginAndSubmitArenaRuns(IEnumerable<Deck> runs, Action<double> setProgress = null)
		{
			Result<CookieContainer, UploadResults> result = await LogInToHearthArena();
			CookieContainer cookieContainer = result.ResultData;
			if (result.Outcome == UploadResults.Success && cookieContainer != null)
			{
				Log.WriteLine(string.Format("Uploading {0} run(s)", runs.Count()), LogType.Info);
				int i = 1;
				int max = runs.Count();
				foreach (Deck run in runs)
				{
					Result<UploadResults> uploadResult = await SubmitArenaRun(run, cookieContainer);
					if (uploadResult.Outcome != UploadResults.Success)
					{
						return uploadResult;
					}
					if (setProgress != null)
						setProgress((double)i++ / max);
				}
				Log.WriteLine("Upload successful", LogType.Info);
				return new Result<UploadResults>(UploadResults.Success, string.Empty);
			}

			return new Result<UploadResults>(result.Outcome, result.ErrorMessage);
		}

		// returns null, if login failed
		private async Task<Result<CookieContainer, UploadResults>> LogInToHearthArena()
		{
			CookieContainer cookieContainer = null;
			Uri uriLogin = new Uri("https://www.heartharena.com/login");
			Uri uriLoginCheck = new Uri("https://www.heartharena.com/login_check");

			// get login token
			HttpWebRequest httpWebRequest = WebRequest.Create(uriLogin) as HttpWebRequest;
			httpWebRequest.Method = "GET";
			httpWebRequest.ProtocolVersion = new Version("1.1");
			httpWebRequest.KeepAlive = true;
			cookieContainer = new CookieContainer();
			httpWebRequest.CookieContainer = cookieContainer;
			HttpWebResponse webResponse;
			try
			{
				webResponse = await httpWebRequest.GetResponseAsync() as HttpWebResponse;
			}
			catch (Exception e)
			{
				string connectionErrorExtended = connectionError + Environment.NewLine + e;
				Log.WriteLine(connectionErrorExtended, LogType.Info);
				return new Result<CookieContainer, UploadResults>(null, UploadResults.ConnectionError, connectionErrorExtended);
			}

			string content = string.Empty;

			using (StreamReader sr = new StreamReader(webResponse.GetResponseStream()))
			{
				content = sr.ReadToEnd();
			}

			HtmlDocument doc = new HtmlDocument();
			doc.LoadHtml(content);
			HtmlNode node = doc.DocumentNode.SelectSingleNode(@"//input[@name='_csrf_token']");
			string csrfToken = node.Attributes["value"].Value;

			string body = "";
			body = CreatePostBodyString(GetLoginBodyArgs(csrfToken));

			// post login
			HttpWebRequest httpWebRequestLogin = WebRequest.Create(uriLoginCheck) as HttpWebRequest;
			httpWebRequestLogin.Method = "POST";
			httpWebRequestLogin.ProtocolVersion = new Version("1.1");
			httpWebRequestLogin.KeepAlive = true;
			httpWebRequestLogin.ContentType = "application/x-www-form-urlencoded";
			httpWebRequestLogin.Referer = uriLogin.ToString();
			httpWebRequestLogin.CookieContainer = cookieContainer;
			using (StreamWriter stOut = new StreamWriter(httpWebRequestLogin.GetRequestStream(), System.Text.Encoding.ASCII))
			{
				stOut.Write(body);
				stOut.Close();
			}

			HttpWebResponse webResponseLogin;

			try
			{
				webResponseLogin = await httpWebRequestLogin.GetResponseAsync() as HttpWebResponse;
			}
			catch (Exception e)
			{
				string connectionErrorExtended = connectionError + Environment.NewLine + e;
				Log.WriteLine(connectionErrorExtended, LogType.Info);
				return new Result<CookieContainer, UploadResults>(null, UploadResults.ConnectionError, connectionErrorExtended);
			}

			using (StreamReader sr = new StreamReader(webResponseLogin.GetResponseStream()))
			{
				content = sr.ReadToEnd();
			}

			bool success = webResponseLogin.ResponseUri.AbsoluteUri == @"https://www.heartharena.com/account";

			if (!success)
			{
				if (webResponseLogin.ResponseUri.AbsoluteUri == uriLogin.ToString())
				{
					Log.WriteLine("Login to Hearth Arena failed: wrong credentials", LogType.Info);
					return new Result<CookieContainer, UploadResults>(null, UploadResults.LoginFailedCredentialsWrong, "wrong credentials");
				}
				else
				{
					Log.WriteLine("Login to Hearth Arena failed: unknown error (Response url: " + webResponseLogin.ResponseUri.AbsoluteUri + ")", LogType.Info);
					return new Result<CookieContainer, UploadResults>(null, UploadResults.LoginFailedUnknownError, "unknown error (Response url: " + webResponseLogin.ResponseUri.AbsoluteUri + ").");
				}
			}

			Log.WriteLine("Login to Hearth Arena successful", LogType.Info);
			return new Result<CookieContainer, UploadResults>(cookieContainer, UploadResults.Success, string.Empty);
		}

		private async Task<Result<UploadResults>> SubmitArenaRun(Deck run, CookieContainer cookieContainer)
		{
			// get add arena token
			HttpWebRequest httpWebRequestAddArena = WebRequest.Create(uriAddArena) as HttpWebRequest;
			httpWebRequestAddArena.Method = "GET";
			httpWebRequestAddArena.ProtocolVersion = new Version("1.1");
			httpWebRequestAddArena.KeepAlive = true;
			httpWebRequestAddArena.CookieContainer = cookieContainer;
			HttpWebResponse webResponseAddArena = null;
			try
			{
				webResponseAddArena = await httpWebRequestAddArena.GetResponseAsync() as HttpWebResponse;
			}
			catch (Exception e)
			{
				string connectionErrorExtended = connectionError + Environment.NewLine + e;
				Log.WriteLine(connectionErrorExtended, LogType.Info);
				return new Result<UploadResults>(UploadResults.ConnectionError, connectionErrorExtended);
			}

			string content = string.Empty;
			using (StreamReader sr = new StreamReader(webResponseAddArena.GetResponseStream()))
			{
				content = sr.ReadToEnd();
			}

			// get token from html
			HtmlDocument doc = new HtmlDocument();
			doc.LoadHtml(content);
			HtmlNode node = doc.DocumentNode.SelectSingleNode($@"//input[@name='{prefix}[_token]']");
			string token = node?.GetAttributeValue("value", null);
			if (token == null)
			{
				return new Result<UploadResults>(UploadResults.HtmlParsingError, "Couldn't parse submit arena run csrf token.");
			}
			// post add arena request            
			string postReq = ConvertArenaRunToRequest(run, token);

			HttpWebRequest httpWebPostAddArena = WebRequest.Create(uriAddArena) as HttpWebRequest;
			httpWebPostAddArena.Method = "POST";
			httpWebPostAddArena.ProtocolVersion = new Version("1.1");
			httpWebPostAddArena.KeepAlive = true;
			httpWebPostAddArena.ContentType = "application/x-www-form-urlencoded";
			httpWebPostAddArena.Referer = uriAddArena.ToString();
			httpWebPostAddArena.CookieContainer = cookieContainer;
			using (StreamWriter stOut = new StreamWriter(httpWebPostAddArena.GetRequestStream(), System.Text.Encoding.ASCII))
			{
				stOut.Write(postReq);
				stOut.Close();
			}

			HttpWebResponse webResponsePostAddArena;
			try
			{
				webResponsePostAddArena = await httpWebPostAddArena.GetResponseAsync() as HttpWebResponse;
			}
			catch (Exception e)
			{
				string connectionErrorExtended = connectionError + Environment.NewLine + e;
				Log.WriteLine(connectionErrorExtended, LogType.Info);
				return new Result<UploadResults>(UploadResults.ConnectionError, connectionErrorExtended);
			}

			using (StreamReader sr = new StreamReader(webResponsePostAddArena.GetResponseStream()))
			{
				content = sr.ReadToEnd();
			}

			bool success = webResponsePostAddArena.ResponseUri.AbsoluteUri == @"https://www.heartharena.com/my-arenas";
			if (!success)
			{
				Log.WriteLine("Submitting arena run failed"
					+ Environment.NewLine + webResponsePostAddArena.StatusCode + " (" + webResponsePostAddArena.ResponseUri + ")", LogType.Info);
			}

			PluginSettings.Instance.UploadedDecks.Add(run.DeckId);
			return new Result<UploadResults>(UploadResults.Success, string.Empty);
		}

		private Dictionary<string, string> GetLoginBodyArgs(string csrf_token)
		{
			Dictionary<string, string> bodyArgs = new Dictionary<string, string>();
			bodyArgs["_csrf_token"] = csrf_token;
			bodyArgs["_username"] = username;
			bodyArgs["_password"] = Encryption.ToInsecureString(password);
			bodyArgs["_remember_me"] = rememberMe;
			bodyArgs["_submit"] = submit;
			return bodyArgs;
		}

		private string ConvertArenaRunToRequest(Deck arenaRun, string token)
		{
			GameStats firstGame = arenaRun.DeckStats.Games.FirstOrDefault();
			string date = firstGame != null ? firstGame.StartTime.ToString("MM/dd/yyyy", CultureInfo.GetCultureInfo("en-US")) : DateTime.Now.ToString("MM/dd/yyyy", CultureInfo.GetCultureInfo("en-US"));
			HearthArenaClass deckClass;
			bool valid = Enum.TryParse(arenaRun.Class, out deckClass);
			Dictionary<string, string> bodyArgs = new Dictionary<string, string>()     
            {
				{prefix + AddSquareBrackets("classification"), ((int) deckClass).ToString()},
                {prefix + AddSquareBrackets("wins"), arenaRun.DeckStats.Games.Where(stats => stats.Result == GameResult.Win).Count().ToString()},
                {prefix + AddSquareBrackets("losses"), arenaRun.DeckStats.Games.Where(stats => stats.Result == GameResult.Loss).Count().ToString()},
                {prefix + AddSquareBrackets("created_at"), date},
                {prefix + AddSquareBrackets("notes"), ""},
                {prefix + AddSquareBrackets("reward") + AddSquareBrackets("gold"), arenaRun.ArenaReward.Gold.ToString()},
                {prefix + AddSquareBrackets("reward") + AddSquareBrackets("dust"), arenaRun.ArenaReward.Dust.ToString()},
                {prefix + AddSquareBrackets("reward") + AddSquareBrackets("packs"), arenaRun.ArenaReward.Packs.Count(pack => pack != ArenaRewardPacks.None).ToString()},
                {prefix + AddSquareBrackets("reward") + AddSquareBrackets("plainCards"), arenaRun.ArenaReward.Cards.Count(card => card != null && !card.Golden).ToString()},
                {prefix + AddSquareBrackets("reward") + AddSquareBrackets("goldenCards"), arenaRun.ArenaReward.Cards.Count(card => card != null && card.Golden).ToString()},
				{prefix + AddSquareBrackets("_token"), token}    
           
            };

			string match_prefix = prefix + AddSquareBrackets("matches");
			string match_prefix_indexed;
			int matchNr = 1;
			foreach (GameStats match in arenaRun.DeckStats.Games)
			{
				string result;
				if (match.Result == GameResult.Win)
					result = "win";
				else if (match.Result == GameResult.Loss)
					result = "loss";
				else
					continue;
				match_prefix_indexed = match_prefix + AddSquareBrackets(matchNr.ToString());
				HearthArenaClass hearthArenaClass;
				bool success = Enum.TryParse<HearthArenaClass>(match.OpponentHero, out hearthArenaClass);
				bodyArgs[match_prefix_indexed + AddSquareBrackets("opponentClassification")] = ((int)hearthArenaClass).ToString();
				bodyArgs[match_prefix_indexed + AddSquareBrackets("result")] = result;
				bodyArgs[match_prefix_indexed + AddSquareBrackets("coin")] = match.Coin ? "coin" : "no-coin";
				matchNr++;
			}

			return CreatePostBodyString(bodyArgs);
		}

		private string AddSquareBrackets(string text)
		{
			return "[" + text + "]";
		}

		// create body for application/x-www-form-urlencoded
		private string CreatePostBodyString(Dictionary<string, string> bodyArgs)
		{
			string body = string.Empty;
			foreach (KeyValuePair<string, string> kvp in bodyArgs)
			{
				body += HttpUtility.UrlEncode(kvp.Key) + "=" + HttpUtility.UrlEncode(kvp.Value);
				body += "&";
			}
			if (body.Last() == '&')
				body = body.Remove(body.Length - 1);

			// return HttpUtility.UrlEncode(body);
			// return Uri.EscapeUriString(body);
			return body;
		}

	}
}
