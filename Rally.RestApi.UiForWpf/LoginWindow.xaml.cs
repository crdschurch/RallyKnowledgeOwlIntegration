﻿using Rally.RestApi.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Rally.RestApi.UiForWpf
{
	/// <summary>
	/// Interaction logic for LoginWindow.xaml
	/// </summary>
	public partial class LoginWindow : Window
	{
		#region Enum: TabType
		private enum TabType
		{
			Credentials,
			Rally,
			Proxy,
		}
		#endregion

		#region Enum: EditorControlType
		private enum EditorControlType
		{
			Username,
			Password,
			RallyServer,
			ProxyServer,
			ProxyUsername,
			ProxyPassword,
		}
		#endregion

		#region Static Values
		private static ImageSource LogoImage;
		private static string HeaderLabelText;
		private static string CredentialsTabText;
		private static string RallyServerTabText;
		private static string ProxyServerTabText;

		private static string UserNameLabelText;
		private static string PwdLabelText;

		private static string ServerLabelText;
		private static string ProxyServerLabelText;
		private static string ProxyUserNameLabelText;
		private static string ProxyPwdLabelText;

		private static string SsoInProgressText;
		private static string LoginText;
		private static string LogoutText;
		private static string CancelText;

		private static Uri DefaultServer;
		private static Uri DefaultProxyServer;
		#endregion

		Dictionary<EditorControlType, Control> controls;
		Dictionary<Control, Label> controlReadOnlyLabels;

		internal RestApiAuthMgrWpf AuthMgr { get; set; }
		internal event AuthenticationComplete AuthenticationComplete;
		Label ssoInProgressLabel;
		Button loginButton;
		Button logoutButton;
		Button cancelButton;

		#region Constructor
		/// <summary>
		/// Constructor
		/// </summary>
		public LoginWindow()
		{
			InitializeComponent();

			Logo.Source = LogoImage;
			headerLabel.Content = HeaderLabelText;
			controls = new Dictionary<EditorControlType, Control>();
			controlReadOnlyLabels = new Dictionary<Control, Label>();
		}
		#endregion

		#region Configure
		/// <summary>
		/// <para>Configure this control with the items that it needs to work.</para>
		/// <para>Nullable parameters have defaults that will be used if not provided.</para>
		/// </summary>
		internal static void Configure(ImageSource logo, string headerLabelText,
			Uri defaultServer, Uri defaultProxyServer,
			string credentialsTabText, string userNameLabelText, string pwdLabelText,
			string serverTabText, string serverLabelText,
			string proxyServerTabText, string proxyServerLabelText,
			string proxyUserNameLabelText, string proxyPwdLabelText, string ssoInProgressText,
			string loginText, string logoutText, string cancelText)
		{
			LogoImage = logo;
			HeaderLabelText = headerLabelText;
			CredentialsTabText = credentialsTabText;
			UserNameLabelText = userNameLabelText;
			PwdLabelText = pwdLabelText;

			DefaultServer = defaultServer;
			DefaultProxyServer = defaultProxyServer;

			RallyServerTabText = serverTabText;
			ServerLabelText = serverLabelText;

			ProxyServerTabText = proxyServerTabText;
			ProxyServerLabelText = proxyServerLabelText;
			ProxyUserNameLabelText = proxyUserNameLabelText;
			ProxyPwdLabelText = proxyPwdLabelText;

			SsoInProgressText = ssoInProgressText;
			LoginText = loginText;
			LogoutText = logoutText;
			CancelText = cancelText;

			#region Default Strings: Credentials
			if (String.IsNullOrWhiteSpace(CredentialsTabText))
				CredentialsTabText = "Credentials";

			if (String.IsNullOrWhiteSpace(UserNameLabelText))
				UserNameLabelText = "User Name";

			if (String.IsNullOrWhiteSpace(PwdLabelText))
				PwdLabelText = "Password";
			#endregion

			#region Default Strings: Rally
			if (String.IsNullOrWhiteSpace(RallyServerTabText))
				RallyServerTabText = "Rally";

			if (String.IsNullOrWhiteSpace(ServerLabelText))
				ServerLabelText = "Server";
			#endregion

			#region Default Strings: Proxy
			if (String.IsNullOrWhiteSpace(ProxyServerTabText))
				ProxyServerTabText = "Proxy";

			if (String.IsNullOrWhiteSpace(ProxyServerLabelText))
				ProxyServerLabelText = "Server";

			if (String.IsNullOrWhiteSpace(ProxyUserNameLabelText))
				ProxyUserNameLabelText = "User Name";

			if (String.IsNullOrWhiteSpace(ProxyPwdLabelText))
				ProxyPwdLabelText = "Password";
			#endregion

			#region Default Strings: Buttons
			if (String.IsNullOrWhiteSpace(SsoInProgressText))
				SsoInProgressText = "SSO in Progress";

			if (String.IsNullOrWhiteSpace(LoginText))
				LoginText = "Login";

			if (String.IsNullOrWhiteSpace(LogoutText))
				LogoutText = "Logout";

			if (String.IsNullOrWhiteSpace(CancelText))
				CancelText = "Cancel";
			#endregion
		}
		#endregion

		#region UpdateLoginState
		/// <summary>
		/// Updates the login state to show the correct buttons.
		/// </summary>
		internal void UpdateLoginState()
		{
			bool isReadOnly = true;
			switch (AuthMgr.Api.AuthenticationState)
			{
				case RallyRestApi.AuthenticationResult.Authenticated:
					loginButton.Visibility = Visibility.Hidden;
					logoutButton.Visibility = Visibility.Visible;
					ssoInProgressLabel.Visibility = Visibility.Hidden;
					break;
				case RallyRestApi.AuthenticationResult.PendingSSO:
					loginButton.Visibility = Visibility.Hidden;
					logoutButton.Visibility = Visibility.Hidden;
					ssoInProgressLabel.Visibility = Visibility.Visible;
					break;
				case RallyRestApi.AuthenticationResult.NotAuthorized:
					loginButton.Visibility = Visibility.Visible;
					logoutButton.Visibility = Visibility.Hidden;
					ssoInProgressLabel.Visibility = Visibility.Hidden;
					isReadOnly = false;
					break;
				default:
					throw new InvalidProgramException("Unknown authentication state.");
			}

			SetReadOnlyStateForEditors(isReadOnly);
		}
		#endregion

		#region BuildLayout
		internal void BuildLayout(RestApiAuthMgrWpf authMgr)
		{
			AuthMgr = authMgr;
			TabControl tabControl = new TabControl();
			tabControl.Margin = new Thickness(10);
			Grid.SetColumn(tabControl, 0);
			Grid.SetColumnSpan(tabControl, 2);
			Grid.SetRow(tabControl, 1);
			layoutGrid.Children.Add(tabControl);

			AddTab(tabControl, TabType.Credentials);
			AddTab(tabControl, TabType.Rally);
			AddTab(tabControl, TabType.Proxy);

			inputRow.Height = new GridLength(tabControl.Height + 35, GridUnitType.Pixel);
			inputRow.MinHeight = inputRow.Height.Value;

			this.Height = inputRow.Height.Value + (28 * 2) + 50 + 50;
			this.MinHeight = this.Height;
			this.MaxHeight = this.Height;

			AddButtons();
		}
		#endregion

		#region SetReadOnlyStateForEditor
		private void SetReadOnlyStateForEditors(bool isReadOnly)
		{
			Array controlTypes = Enum.GetValues(typeof(EditorControlType));
			foreach (EditorControlType editorControlType in controlTypes)
			{
				Control control = GetEditor(editorControlType);
				if (isReadOnly)
					control.Visibility = Visibility.Hidden;
				else
					control.Visibility = Visibility.Visible;

				if (controlReadOnlyLabels.ContainsKey(control))
				{
					Label label = controlReadOnlyLabels[control];
					TextBox textBox = control as TextBox;
					if (textBox != null)
						label.Content = textBox.Text;

					if (isReadOnly)
						label.Visibility = Visibility.Visible;
					else
						label.Visibility = Visibility.Hidden;
				}
			}
		}
		#endregion

		#region AddTab
		private void AddTab(TabControl tabControl, TabType tabType)
		{
			TabItem tab = new TabItem();
			tabControl.Items.Add(tab);

			Grid tabGrid = new Grid();
			tab.Content = tabGrid;
			tabGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
			tabGrid.VerticalAlignment = VerticalAlignment.Top;
			AddColumnDefinition(tabGrid, 120);
			AddColumnDefinition(tabGrid);

			if (tabType == TabType.Credentials)
			{
				tab.Header = CredentialsTabText;
				AddInputToTabGrid(tabGrid, UserNameLabelText, GetEditor(EditorControlType.Username));
				AddInputToTabGrid(tabGrid, PwdLabelText, GetEditor(EditorControlType.Password), true);
			}
			else if (tabType == TabType.Rally)
			{
				tab.Header = RallyServerTabText;
				AddInputToTabGrid(tabGrid, ServerLabelText, GetEditor(EditorControlType.RallyServer));
			}
			else if (tabType == TabType.Proxy)
			{
				tab.Header = ProxyServerTabText;
				AddInputToTabGrid(tabGrid, ProxyServerLabelText, GetEditor(EditorControlType.ProxyServer));
				AddInputToTabGrid(tabGrid, ProxyUserNameLabelText, GetEditor(EditorControlType.ProxyUsername));
				AddInputToTabGrid(tabGrid, ProxyPwdLabelText, GetEditor(EditorControlType.ProxyPassword), true);
			}
			else
				throw new NotImplementedException();

			if ((tabControl.Height.ToString().Equals("NaN", StringComparison.InvariantCultureIgnoreCase)) ||
				(tabControl.Height < tabGrid.Height + 20))
			{
				tabControl.Height = tabGrid.Height + 20;
				tabControl.MinHeight = tabControl.Height;
			}
		}
		#endregion

		#region AddInputToTabGrid
		private void AddInputToTabGrid(Grid tabGrid, string labelText, Control control, bool skipReadOnlyLabel = false)
		{
			int rowIndex = tabGrid.RowDefinitions.Count;
			AddRowDefinition(tabGrid, 28);
			Label label = new Label();
			label.Content = labelText;
			label.FontWeight = FontWeights.Bold;
			AddControlToGrid(tabGrid, label, rowIndex, 0);

			if (control != null)
			{
				AddControlToGrid(tabGrid, control, rowIndex, 1);
				if (!skipReadOnlyLabel)
				{
					Label readOnlyLabel = new Label();
					controlReadOnlyLabels.Add(control, readOnlyLabel);
					AddControlToGrid(tabGrid, readOnlyLabel, rowIndex, 1);
				}
			}
		}
		#endregion

		#region GetEditor
		private Control GetEditor(EditorControlType controlType, string defaultValue = null)
		{
			Control control = null;
			if (controls.ContainsKey(controlType))
				control = controls[controlType];
			else
			{
				switch (controlType)
				{
					case EditorControlType.Username:
					case EditorControlType.RallyServer:
					case EditorControlType.ProxyServer:
					case EditorControlType.ProxyUsername:
						TextBox textBox = new TextBox();
						switch (controlType)
						{
							case EditorControlType.Username:
								if (AuthMgr.Api.ConnectionInfo != null)
									textBox.Text = AuthMgr.Api.ConnectionInfo.UserName;
								break;
							case EditorControlType.RallyServer:
								if ((AuthMgr.Api.ConnectionInfo != null) &&
									(!String.IsNullOrWhiteSpace(AuthMgr.Api.ConnectionInfo.Server.ToString())))
								{
									textBox.Text = AuthMgr.Api.ConnectionInfo.Server.ToString();
								}
								else
									textBox.Text = DefaultServer.ToString();
								break;
							case EditorControlType.ProxyServer:
								if ((AuthMgr.Api.ConnectionInfo != null) &&
									(AuthMgr.Api.ConnectionInfo.Proxy != null))
								{
									textBox.Text = AuthMgr.Api.ConnectionInfo.Proxy.Address.ToString();
								}
								else if (DefaultProxyServer != null)
									textBox.Text = DefaultProxyServer.ToString();
								break;
							default:
								break;
						}
						control = textBox;
						break;
					case EditorControlType.Password:
					case EditorControlType.ProxyPassword:
						PasswordBox passwordBox = new PasswordBox();
						passwordBox.PasswordChar = '*';
						control = passwordBox;
						break;
					default:
						throw new NotImplementedException();
				}

				control.Margin = new Thickness(0, 0, 10, 0);
				control.HorizontalAlignment = HorizontalAlignment.Stretch;
				control.MinWidth = 150;
				control.Height = 20;
				controls.Add(controlType, control);
			}

			return control;
		}
		#endregion

		#region GetEditorValue
		private string GetEditorValue(EditorControlType controlType)
		{
			Control control = GetEditor(controlType);
			if (control == null)
				return null;

			TextBox textBox = control as TextBox;
			if (textBox != null)
				return textBox.Text;

			PasswordBox passwordBox = control as PasswordBox;
			if (passwordBox != null)
				return passwordBox.Password;

			return null;
		}
		#endregion

		#region AddButtons
		private void AddButtons()
		{
			Grid buttonGrid = new Grid();
			Grid.SetColumn(buttonGrid, 1);
			Grid.SetRow(buttonGrid, 4);
			layoutGrid.Children.Add(buttonGrid);

			AddColumnDefinition(buttonGrid, 70);
			AddColumnDefinition(buttonGrid, 70);
			AddColumnDefinition(buttonGrid);

			Thickness margin = new Thickness(5, 0, 5, 0);

			ssoInProgressLabel = new Label();
			ssoInProgressLabel.Content = SsoInProgressText;
			AddControlToGrid(buttonGrid, ssoInProgressLabel, 0, 0);

			loginButton = new Button();
			loginButton.Margin = margin;
			loginButton.IsDefault = true;
			loginButton.Content = LoginText;
			loginButton.Click += loginButton_Click;
			AddControlToGrid(buttonGrid, loginButton, 0, 0);

			logoutButton = new Button();
			logoutButton.Margin = margin;
			logoutButton.Content = LogoutText;
			logoutButton.Click += logoutButton_Click;
			AddControlToGrid(buttonGrid, logoutButton, 0, 0);

			cancelButton = new Button();
			cancelButton.Margin = margin;
			cancelButton.Content = CancelText;
			cancelButton.Click += cancelButton_Click;
			AddControlToGrid(buttonGrid, cancelButton, 0, 1);
		}
		#endregion

		#region AddControlToGrid
		private void AddControlToGrid(Grid grid, UIElement control, int row, int column, int rowSpan = 1, int colSpan = 1)
		{
			if (row >= 0)
				Grid.SetRow(control, row);
			if (rowSpan > 1)
				Grid.SetRowSpan(control, rowSpan);

			if (column >= 0)
				Grid.SetColumn(control, column);
			if (colSpan > 1)
				Grid.SetColumnSpan(control, colSpan);

			grid.Children.Add(control);
		}
		#endregion

		#region AddRowDefinition
		private void AddRowDefinition(Grid grid, int pixels = Int32.MaxValue)
		{
			RowDefinition rowDef = new RowDefinition();
			if (pixels == Int32.MaxValue)
				rowDef.Height = GridLength.Auto;
			else
				rowDef.Height = new GridLength(pixels, GridUnitType.Pixel);
			grid.RowDefinitions.Add(rowDef);

			if (pixels != Int32.MaxValue)
			{
				grid.MinHeight += pixels + 2;
				grid.Height = grid.MinHeight;
			}
			else
				grid.Height = double.NaN;
		}
		#endregion

		#region AddColumnDefinition
		private void AddColumnDefinition(Grid grid, int pixels = Int32.MaxValue)
		{
			ColumnDefinition colDef = new ColumnDefinition();
			if (pixels != Int32.MaxValue)
				colDef.Width = new GridLength(pixels, GridUnitType.Pixel);

			grid.ColumnDefinitions.Add(colDef);
		}
		#endregion

		#region loginButton_Click
		void loginButton_Click(object sender, RoutedEventArgs e)
		{
			WebProxy proxy = null;
			string proxyServer = GetEditorValue(EditorControlType.ProxyServer);
			if (!String.IsNullOrWhiteSpace(proxyServer))
			{
				proxy = new WebProxy(new Uri(proxyServer));
				string proxyUser = GetEditorValue(EditorControlType.ProxyUsername);
				string proxyPassword = GetEditorValue(EditorControlType.ProxyUsername);
				if (!String.IsNullOrWhiteSpace(proxyUser))
					proxy.Credentials = new NetworkCredential(proxyUser, proxyPassword);
				else
					proxy.UseDefaultCredentials = true;
			}

			AuthMgr.Api.Authenticate(GetEditorValue(EditorControlType.Username), GetEditorValue(EditorControlType.Password),
				GetEditorValue(EditorControlType.RallyServer), proxy);

			if (AuthenticationComplete != null)
			{
				switch (AuthMgr.Api.AuthenticationState)
				{
					case RallyRestApi.AuthenticationResult.Authenticated:
						AuthenticationComplete.Invoke(AuthMgr.Api.AuthenticationState, AuthMgr.Api);
						Close();
						break;
					case RallyRestApi.AuthenticationResult.PendingSSO:
					case RallyRestApi.AuthenticationResult.NotAuthorized:
						AuthenticationComplete.Invoke(AuthMgr.Api.AuthenticationState, null);
						break;
					default:
						throw new NotImplementedException();
				}
			}
		}
		#endregion

		#region logoutButton_Click
		void logoutButton_Click(object sender, RoutedEventArgs e)
		{
			AuthMgr.Api.Logout();
			AuthenticationComplete.Invoke(AuthMgr.Api.AuthenticationState, null);
		}
		#endregion

		#region cancelButton_Click
		void cancelButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
		#endregion

		#region SsoAuthenticationComplete
		internal void SsoAuthenticationComplete(RallyRestApi.AuthenticationResult authenticationResult, RallyRestApi api)
		{
			if (authenticationResult == RallyRestApi.AuthenticationResult.Authenticated)
				Close();
			else
			{
				UpdateLoginState();
			}
		}
		#endregion

		#region OnClosing
		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			AuthMgr.LoginWindowSsoAuthenticationComplete = null;
			base.OnClosing(e);
		}
		#endregion
	}
}
