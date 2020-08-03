using System;
using System.Timers;
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
		// StopWatch represent a running stopwatch
		public DateTimeOffset	updated_at 		{ get; set; }
		public string			url				{ get; set; }
		public string			title			{ get; set; }
		public string			type			{ get; set; }
	}
	
	public sealed class NotificationIcon {
		private System.Timers.Timer aTimer;
		
		public NotifyIcon notifyIcon;
		private Icon icon1;
		private Icon icon2;
		private Icon icon3;
		private ContextMenu notificationMenu;
		private const string APIWORD = "api/v1/notifications";
		
		private static string APIHOST = "";
		private static string APPTOKEN = "";
		
		public NotificationIcon() {
			notifyIcon = new NotifyIcon();
			notificationMenu = new ContextMenu();
			icon1 = new Icon("empty.ico");
			icon2 = new Icon("full.ico");
			icon3 = new Icon("ups.ico");
			
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
			
			try {
				using (var client = new WebClient()) {
					string response = client.DownloadString(String.Format("{0}{1}?token={2}", APIHOST, APIWORD, APPTOKEN));
					
					JArray datas = JArray.Parse(response);
					
					foreach (JObject data in datas) {
						TeaEntry te = new TeaEntry();
						Console.Out.WriteLine();
						te.title = data["subject"]["title"].ToString();
						te.updated_at = data["updated_at"].ToObject<DateTimeOffset>();
						te.url = data["repository"]["html_url"].ToString();
						te.type = data["subject"]["type"].ToString();
						
						notificationMenu.MenuItems.Add(String.Format(
							"{0} {1} ({2})",
							te.updated_at.ToString("ddd, dd.MM. HH:mm"),
							te.title,
							te.type
						), (sender, args) => menuLinkClick(te));
						
					}
					notifyIcon.Icon = icon1;
					if (datas.Count > 0) notifyIcon.Icon = icon2;
				}
			} catch (Exception ex) {
				notificationMenu.MenuItems.Add(ex.Message);
				notificationMenu.MenuItems.Add("something went wrong");
				notifyIcon.Icon = icon3;
			}
			notificationMenu.MenuItems.Add(new MenuItem("Exit", menuExitClick));
		}
		
		public static void Main() {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			
			string[] arguments = Environment.GetCommandLineArgs();
			if (arguments.Length < 3) {
				MessageBox.Show(
					String.Format("{0} http://domain:port/path/ token00api23key42etc999", arguments[0]),
					"Äh... start me with two parameters!",
					MessageBoxButtons.OK,
					MessageBoxIcon.Exclamation,
					MessageBoxDefaultButton.Button1
				);
				return;
			} else {
				APIHOST = arguments[1];
				APPTOKEN = arguments[2];
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
		
		private void menuLinkClick(TeaEntry te) {
			try {
			Process.Start(te.url);
			} catch (Exception) {
				Console.Out.WriteLine("try to start with browser: " + te.url);
			}
		}
		
		private void OnTimedEvent(Object source, ElapsedEventArgs e) {
			refreshMenu();
		}

	}
	
}
