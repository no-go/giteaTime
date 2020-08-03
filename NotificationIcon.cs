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
	
	public sealed class NotificationIcon
	{
		private System.Timers.Timer aTimer;
		
		public NotifyIcon notifyIcon;
		private Icon icon1;
		private Icon icon2;
		private ContextMenu notificationMenu;
		private StopWatch[] stopwatches;
		private const string APISTOPWATCHES  = "api/v1/user/stopwatches";
		
		private static string APIHOST = ""; //"http://localhost:3000/";
		private static string APPTOKEN = ""; //"a5db90680933c7501e4ea9eb2d7185d24012e2c9";
		
		public NotificationIcon()
		{
			notifyIcon = new NotifyIcon();
			notificationMenu = new ContextMenu();
			icon1 = new Icon("empty.ico");
			icon2 = new Icon("full.ico");
			
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

			notificationMenu.MenuItems.Add(new MenuItem("Exit", menuExitClick));
		}
		
		#region Main - Program entry point
		/// <summary>Program entry point.</summary>
		/// <param name="args">Command Line Arguments</param>
		[STAThread]
		public static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			
			if (args.Length < 2) {
				Console.WriteLine(String.Format("{0} http[s]://domain:port/path/ tokenapikeyetc...", "giteaTime.exe"));
				return;
			} else {
				APIHOST = args[0];
				APPTOKEN = args[1];
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
		#endregion
		
		#region Event Handlers
		private static void menuExitClick(object sender, EventArgs e) {
			Application.Exit();
		}
		
		private void IconDoubleClick(object sender, EventArgs e) {
			refreshMenu();
			//MessageBox.Show("Refreshed!");
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
				Console.WriteLine("try to start with browser: " + url);
			}
		}
		
		private void OnTimedEvent(Object source, ElapsedEventArgs e) {
			refreshMenu();
		}
		#endregion
	}
	
}
