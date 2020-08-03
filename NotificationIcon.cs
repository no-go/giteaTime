using System;
using System.Timers;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using Newtonsoft.Json;

namespace giteaTime
{
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
		private StopWatch[] stopwatches;
		private const string APISTOPWATCHES  = "api/v1/user/stopwatches";
		
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

			aTimer = new System.Timers.Timer(30000);
			// Hook up the Elapsed event for the timer. 
			aTimer.Elapsed += OnTimedEvent;
			aTimer.AutoReset = true;
			aTimer.Enabled = true;
		}
		
		private void refreshMenu() {
			notificationMenu.MenuItems.Clear();
			
			try {
				using (var client = new WebClient()) {
					string response = client.DownloadString(String.Format("{0}{1}?token={2}", APIHOST, APISTOPWATCHES, APPTOKEN));
					stopwatches = JsonConvert.DeserializeObject<StopWatch[]>(response);
					foreach (StopWatch stopwatch in stopwatches) {
						notificationMenu.MenuItems.Add(String.Format(
							"{0} /{1}/{2}: Issue {3} '{4}'",
							stopwatch.created.ToString("ddd, dd.MM. HH:mm"),
							stopwatch.repo_owner_name,
							stopwatch.repo_name,
							stopwatch.issue_index,
							stopwatch.issue_title
						), (sender, args) => menuLinkClick(stopwatch));
					}
					notifyIcon.Icon = icon1;
					if (stopwatches.Length > 0) notifyIcon.Icon = icon2;
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
			using (Mutex mtx = new Mutex(true, "giteaTime", out isFirstInstance)) {
				if (isFirstInstance) {
					NotificationIcon notificationIcon = new NotificationIcon();
					notificationIcon.notifyIcon.Visible = true;
					Application.Run();
					notificationIcon.notifyIcon.Dispose();
				} else {
					// The application is already running
					// TODO: Display message box or change focus to existing application instance
				}
			} // releases the Mutex
		}

		private static void menuExitClick(object sender, EventArgs e) {
			Application.Exit();
		}
		
		private void IconDoubleClick(object sender, EventArgs e) {
			refreshMenu();
		}
		
		private void menuLinkClick(StopWatch stopwatch) {
			string url = String.Format(
				"{0}{1}/{2}/issues/{3}",
				APIHOST,
				stopwatch.repo_owner_name,
				stopwatch.repo_name,
				stopwatch.issue_index
			);
			try {
			Process.Start(url);
			} catch (Exception) {
				Console.Out.WriteLine("try to start with browser: " + url);
			}
		}
		
		private void OnTimedEvent(Object source, ElapsedEventArgs e) {
			refreshMenu();
		}

	}
	
}
