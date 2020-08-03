using System;
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
		private NotifyIcon notifyIcon;
		private ContextMenu notificationMenu;
		private const string APISTOPWATCHES  = "api/v1/user/stopwatches";
		
		private static string APIHOST = "http://localhost:3000/";
		private static string APPTOKEN = "a5db90680933c7501e4ea9eb2d7185d24012e2c9";
		
		public NotificationIcon()
		{
			notifyIcon = new NotifyIcon();
			
			refreshMenu();
			
			notifyIcon.DoubleClick += IconDoubleClick;
			notifyIcon.Icon = new Icon("logo.ico");
			notifyIcon.ContextMenu = notificationMenu;
		}
		
		private void refreshMenu() {
			notificationMenu = new ContextMenu();
			
			using (var client = new WebClient()) {
				string response = client.DownloadString(String.Format("{0}{1}?token={2}", APIHOST, APISTOPWATCHES, APPTOKEN));
				StopWatch[] sw = JsonConvert.DeserializeObject<StopWatch[]>(response);
				foreach (StopWatch stopwatch in sw) {
					notificationMenu.MenuItems.Add(String.Format(
						"since {0}: {1}/{2}: Issue ({3}) {4}",
						stopwatch.created.ToString("g"),
						stopwatch.repo_owner_name,
						stopwatch.repo_name,
						stopwatch.issue_index,
						stopwatch.issue_title
					));
				}
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
		
		private void menuExitClick(object sender, EventArgs e)
		{
			Application.Exit();
		}
		
		private void IconDoubleClick(object sender, EventArgs e)
		{
			MessageBox.Show("The icon was double clicked");
		}
		#endregion
	}
}
