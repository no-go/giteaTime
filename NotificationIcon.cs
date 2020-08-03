/*
 * Created by SharpDevelop.
 * User: unknown
 * Date: 30.07.2020
 * Time: 21:06
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using Newtonsoft.Json;

namespace giteaTime
{
	public class StopWatch{
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
		//private const string APIURL  = "api/swagger/";
		private const string APIHOST = "http://localhost:3000/";
		private const string APISTOPWATCHES  = "api/v1/user/stopwatches";
		private const string APPTOKEN = "a5db90680933c7501e4ea9eb2d7185d24012e2c9";
		// get /repos/{owner}/{repo}/issues/{index}/times
		// delete /repos/{owner}/{repo}/issues/{index}/times/{id}
		// delete supstract the time 
		
		// curl -X DELETE "http://localhost:3000/api/v1/repos/meister/test/issues/2/times/4?token=a5db90680933c7501e4ea9eb2d7185d24012e2c9" -H  "accept: application/json"
		
		// get /user/times
		
		/*
		[
  {
    "id": 4,
    "created": "2020-07-30T23:17:54+02:00",
    "time": 3410,
    "user_id": 1,
    "user_name": "meister",
    "issue_id": 2,
    "issue": {
      "id": 2,
      "url": "http://localhost:3000/api/v1/repos/meister/test/issues/1",
      "html_url": "http://localhost:3000/meister/test/issues/1",
      "number": 1,
      "user": {
        "id": 1,
        "login": "meister",
        "full_name": "",
        "email": "mein@dummer.click",
        "avatar_url": "http://localhost:3000/user/avatar/meister/-1",
        "language": "en-US",
        "is_admin": true,
        "last_login": "2020-07-30T23:17:28+02:00",
        "created": "2020-07-28T22:08:07+02:00",
        "username": "meister"
      },
      "original_author": "",
      "original_author_id": 0,
      "title": "hallo",
      "body": "",
      "labels": [],
      "milestone": null,
      "assignee": {
        "id": 1,
        "login": "meister",
        "full_name": "",
        "email": "mein@dummer.click",
        "avatar_url": "http://localhost:3000/user/avatar/meister/-1",
        "language": "en-US",
        "is_admin": true,
        "last_login": "2020-07-30T23:17:28+02:00",
        "created": "2020-07-28T22:08:07+02:00",
        "username": "meister"
      },
      "assignees": [
        {
          "id": 1,
          "login": "meister",
          "full_name": "",
          "email": "mein@dummer.click",
          "avatar_url": "http://localhost:3000/user/avatar/meister/-1",
          "language": "en-US",
          "is_admin": true,
          "last_login": "2020-07-30T23:17:28+02:00",
          "created": "2020-07-28T22:08:07+02:00",
          "username": "meister"
        }
      ],
      "state": "open",
      "comments": 0,
      "created_at": "2020-07-28T22:30:42+02:00",
      "updated_at": "2020-07-30T23:17:54+02:00",
      "closed_at": null,
      "due_date": null,
      "pull_request": null,
      "repository": {
        "id": 1,
        "name": "test",
        "owner": "meister",
        "full_name": "meister/test"
      }
    }
  }
]
*/
		
		#region Initialize icon and menu
		public NotificationIcon()
		{
			notifyIcon = new NotifyIcon();
			notificationMenu = new ContextMenu(InitializeMenu());
			
			notifyIcon.DoubleClick += IconDoubleClick;
			//System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NotificationIcon));
			//notifyIcon.Icon = (Icon)resources.GetObject("$this.Icon");
			notifyIcon.Icon = new Icon("stimpy2.ico");
			notifyIcon.ContextMenu = notificationMenu;
		}
		
		private MenuItem[] InitializeMenu()
		{
			
			MenuItem[] menu = new MenuItem[] {
				new MenuItem("About", menuAboutClick),
				new MenuItem("Exit", menuExitClick)
			};
			return menu;
		}
		#endregion
		
		#region Main - Program entry point
		/// <summary>Program entry point.</summary>
		/// <param name="args">Command Line Arguments</param>
		[STAThread]
		public static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			
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
		private void menuAboutClick(object sender, EventArgs e)
		{
			MessageBox.Show("About This Application");
		}
		
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
