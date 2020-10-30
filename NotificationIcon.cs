using System;
using System.Timers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace giteaTaskicon
{
	public class TeaEntry {
		// TeaEntry represent a new Notification
		public DateTimeOffset	updated_at 		{ get; set; }
		public string			url				{ get; set; }
		public string			title			{ get; set; }
		public string			type			{ get; set; }
	}

	public class StopWatch {
		// StopWatch represent a running stopwatch
		public DateTimeOffset	created 		{ get; set; }
		public int				issue_index		{ get; set; }
		public string			issue_title		{ get; set; }
		public string			repo_name		{ get; set; }
		public string			repo_owner_name	{ get; set; }
	}

	public sealed class NotificationIcon {
		private System.Timers.Timer aTimer;
		
		public NotifyIcon notifyIcon;
		private Icon icon1;
		private Icon icon2;
		private Icon icon3;
		private ContextMenu notificationMenu;
		private const string APIWORD = "api/v1/notifications";
		private const string APIWORD2 = "api/v1/user/stopwatches";
		
		private static string APIHOST = "";
		private static string APPTOKEN = "";
		private static string OSPLAT = "";
		private static string TIMEFORMAT = "";
		
		public NotificationIcon() {
			notifyIcon = new NotifyIcon();
			notificationMenu = new ContextMenu();
			icon1 = new Icon("empty.ico");
			icon2 = new Icon("full.ico");
			icon3 = new Icon("ups.ico");
			ServicePointManager.ServerCertificateValidationCallback += (send, certificate, chain, sslPolicyErrors) => { return true; };
								
			refreshMenu();
			
			notifyIcon.DoubleClick += IconDoubleClick;
			notifyIcon.ContextMenu = notificationMenu;

			aTimer = new System.Timers.Timer(300000); //5min
			// Hook up the Elapsed event for the timer. 
			aTimer.Elapsed += OnTimedEvent;
			aTimer.AutoReset = true;
			aTimer.Enabled = true;
		}
		
		private void refreshMenu() {
			notificationMenu.MenuItems.Clear();
			JArray datas = new JArray();
			List<StopWatch> stopwatches = new List<StopWatch>();

			notifyIcon.Icon = icon1;

			try {
				using (var client = new WebClient()) {
					// fetch notifications
					string response = client.DownloadString(String.Format("{0}{1}?token={2}", APIHOST, APIWORD, APPTOKEN));					
					datas = JArray.Parse(response);					
				}
				if (datas.Count == 1) {
					notificationMenu.MenuItems.Add("NOTIFICATION", (sender, args) =>urlOpen(String.Format("{0}notifications", APIHOST)) );
				} else if (datas.Count > 1) {
					notificationMenu.MenuItems.Add("NOTIFICATIONS", (sender, args) =>urlOpen(String.Format("{0}notifications", APIHOST)) );
				}
				foreach (JObject data in datas) {
					TeaEntry te = new TeaEntry();
					Console.Out.WriteLine();
					te.title = data["subject"]["title"].ToString();
					te.updated_at = data["updated_at"].ToObject<DateTimeOffset>();
					te.url = data["repository"]["html_url"].ToString();
					te.type = data["subject"]["type"].ToString();
					
					notificationMenu.MenuItems.Add(String.Format(
						"{0} {1} ({2})",
						te.updated_at.ToString(TIMEFORMAT),
						te.title,
						te.type
					), (sender, args) => menuLinkClick(te));
					
				}

			} catch (Exception ex) {
				notificationMenu.MenuItems.Add(ex.Message);
				notificationMenu.MenuItems.Add("something went wrong on notification request");
				notifyIcon.Icon = icon3;
			}

			try {
				using (var client = new WebClient()) {
					// fetch stopwatch
					string response = client.DownloadString(String.Format("{0}{1}?token={2}", APIHOST, APIWORD2, APPTOKEN));
					stopwatches = JsonConvert.DeserializeObject<List<StopWatch>>(response);
				}
				if (stopwatches.Count == 1) {
					notificationMenu.MenuItems.Add("STOPWATCH");
				} else if (stopwatches.Count > 1) {
					notificationMenu.MenuItems.Add("STOPWATCHES");
				}
				foreach (StopWatch sw in stopwatches) {
					notificationMenu.MenuItems.Add(String.Format(
						"{0} {1}",
						sw.created.ToString(TIMEFORMAT),
						sw.issue_title
					), (sender, args) => menuLinkClick(sw));
				}
			} catch (Exception ex) {
				notificationMenu.MenuItems.Add(ex.Message);
				notificationMenu.MenuItems.Add("something went wrong on stopwatch request");
				notifyIcon.Icon = icon3;
			}
			if (datas.Count > 0) notifyIcon.Icon = icon2;
			if (stopwatches.Count > 0) notifyIcon.Icon = icon2;

			notificationMenu.MenuItems.Add(new MenuItem("Exit", menuExitClick));
		}
		
		public static void Main() {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			
			string[] arguments = Environment.GetCommandLineArgs();
			if (arguments.Length < 5) {
				MessageBox.Show(
					String.Format("{0} Win|Linux|Osx \"ddd, dd.MM. HH:mm\" https://domain:port/path/ token00api23key42etc999", arguments[0]),
					"Start me with four parameters!",
					MessageBoxButtons.OK,
					MessageBoxIcon.Exclamation,
					MessageBoxDefaultButton.Button1
				);
				return;
			} else {
				OSPLAT = arguments[1];
				TIMEFORMAT = arguments[2];
				APIHOST = arguments[3];
				APPTOKEN = arguments[4];
			}
			
			bool isFirstInstance;
			// Please use a unique name for the mutex to prevent conflicts with other programs
			using (Mutex mtx = new Mutex(true, "giteaTaskicon", out isFirstInstance)) {
				if (isFirstInstance) {
					NotificationIcon notificationIcon = new NotificationIcon();
					notificationIcon.notifyIcon.Visible = true;
					Application.Run();
					notificationIcon.notifyIcon.Dispose();
				}
			} // releases the Mutex
		}

		private static void menuExitClick(object sender, EventArgs e) {
			Application.Exit();
		}
		
		private void IconDoubleClick(object sender, EventArgs e) {
			refreshMenu();
		}
		
		private void urlOpen(string url) {
			try {
				Process.Start(url);
			} catch (Exception) {
				if (OSPLAT == "Win") {
					url = url.Replace("&", "^&");
					Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
				} else if (OSPLAT == "Linux") {
					Process.Start("xdg-open", url);
				} else if (OSPLAT == "Osx") {
					Process.Start("open", url);
				} else {
					Console.Out.WriteLine("Fail to start with browser: " + url);
				}
			}
		}

		private void menuLinkClick(TeaEntry te) {
			urlOpen(te.url);
		}

		private void menuLinkClick(StopWatch sw) {
			string url = String.Format("{0}{1}/{2}/issues/{3}", APIHOST, sw.repo_owner_name, sw.repo_name, sw.issue_index);
			urlOpen(url);
		}
		
		private void OnTimedEvent(Object source, ElapsedEventArgs e) {
			refreshMenu();
		}

	}
	
}
