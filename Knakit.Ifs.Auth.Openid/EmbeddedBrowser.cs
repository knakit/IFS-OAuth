using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

namespace Knakit.Ifs.Auth.Openid
{
	public class EmbeddedBrowser
	{

		/// <summary>
		/// 
		/// </summary>
		public EmbeddedBrowser()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="startUrl"></param>
		/// /// <param name="endUrl"></param>
		/// <returns></returns>
		internal async Task<AuthResult> CreateAuthenticateWindowAsync(string startUrl, string endUrl)
		{
			using (Form form = new Form())
			using (var browser = new ExtendedWebBrowser()
			{
				Dock = DockStyle.Fill
			})
			{
				form.Name = "WebAuthentication";
				form.Text = "IFS Test Oauth Client by kank.it";
				//Handling the size according to the resolution and the scalling factor
				form.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
				Size beforeSize = new Size(1024, 768);
				//Size afterSize = FndDPIUtilities.Scale(beforeSize);
				Size afterSize = beforeSize;

				form.Width = afterSize.Width;
				form.Height = afterSize.Height;

				browser.IsWebBrowserContextMenuEnabled = false;
				form.ShowIcon = false;

				var signal = new SemaphoreSlim(0, 1);

				var result = new AuthResult
				{
					ResultType = AuthResultType.UserCancel
				};

				form.FormClosed += (o, e) =>
				{
					signal.Release();
				};

				browser.NavigateError += (o, e) =>
				{
					e.Cancel = true;
					result.ResultType = AuthResultType.Error;
					result.Response = e.StatusCode.ToString();
					signal.Release();
				};

				browser.NewWindow += browser_NewWindow;

				browser.BeforeNavigate2 += (o, e) =>
				{
					if (e.Url.StartsWith(endUrl))
					{
						Console.WriteLine("-e.Url " + e.Url);
						e.Cancel = true;
						result.ResultType = AuthResultType.Success;
						result.Response = Encoding.UTF8.GetString(e.PostData ?? new byte[] { });					
						string checkResponse = result.Response;
						string charLast = checkResponse.Substring(checkResponse.Length - 1);
						if (charLast == "\0")
						{
							result.Response = checkResponse.Substring(0, (checkResponse.Length) - 1);
						}
						if (result.Response.StartsWith("error"))
						{
							result.Response = result.Response.Replace("&error_description=", "\n\r");

							if (result.Response.Contains("error_subcode=cancel"))
							{
								result.ResultType = AuthResultType.UserCancel;
							}
							else
							{
								result.ResultType = AuthResultType.Error;
							}
						}

						signal.Release();
					}
				};

				form.Controls.Add(browser);
				browser.Show();

				form.Show();

				browser.Navigate(startUrl);

				await signal.WaitAsync();

				form.Hide();
				browser.Hide();

				return result;
			}
		}

		void browser_NewWindow(object sender, System.ComponentModel.CancelEventArgs e)
		{
			HtmlElement link = ((ExtendedWebBrowser)sender).Document.ActiveElement;
			String url = link.GetAttribute("href");

			if (url.Equals(""))
			{
				e.Cancel = false;
				return;
			}

			if (url.StartsWith("//"))
			{

			}
			else if (url.StartsWith("/"))
			{
				url = ((ExtendedWebBrowser)sender).Url.Host + url;
			}

			e.Cancel = true;

			System.Diagnostics.Process.Start(url);
		}

	}
}