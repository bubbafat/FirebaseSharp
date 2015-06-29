using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using FirebaseSharp.Portable;
using RestSharp;
using RestSharp.Deserializers;

namespace NestPinAuth
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly object _displayLock = new object();
        private IStreamingResponse _resp;
        private Firebase _fb;

        public MainWindow()
        {
            InitializeComponent();

            TextNestClientId.Text = "[YOUR CLIENT ID]";
            PwNestSecret.Password = "[YOUR CLIENT SECRET]";
            TextNextState.Text = "state data";
        }

        private void GetPin_Click(object sender, RoutedEventArgs e)
        {
            Uri pinUrl = new Uri(string.Format("https://home.nest.com/login/oauth2?client_id={0}&state={1}",
                TextNestClientId.Text,
                TextNextState.Text));

            Process.Start(pinUrl.AbsoluteUri);
        }

        private void GetToken_Click(object sender, RoutedEventArgs e)
        {
            var client = new RestClient("https://api.home.nest.com/oauth2/access_token");
            var request = new RestRequest(Method.POST);
            request.AddParameter("client_id", TextNestClientId.Text);
            request.AddParameter("code", TextNestPin.Text);
            request.AddParameter("client_secret", PwNestSecret.Password);
            request.AddParameter("grant_type", "authorization_code");
            var response = client.Execute(request);

            JsonDeserializer deserializer = new JsonDeserializer();

            var json = deserializer.Deserialize<Dictionary<string, string>>(response);

            if (json.ContainsKey("access_token"))
            {
                TextNestAccessToken.Text = json["access_token"];
                return;
            }

            if (json.ContainsKey("message"))
            {
                TextNestAccessToken.Text = json["message"];
                return;                
            }

            if (json.ContainsKey("error_description"))
            {
                TextNestAccessToken.Text = json["error_description"];
                return;
            }
        }

        private async void Authentiate_Click(object sender, RoutedEventArgs e)
        {
            // if there was a running response, dispose it so that
            // any background tasks are cancelled.
            using (_resp) { }
            using (_fb) { }

            TextStreamingResults.Text = string.Empty;

            _fb = new Firebase(new Uri("https://developer-api.nest.com"), TextNestAccessToken.Text);
            _resp = await _fb.GetStreamingAsync("");

            _resp.Added += Added;
            _resp.Changed += Changed;
            _resp.Removed += Removed;

            _resp.Listen();
        }

        private void Removed(object sender, ValueRemovedEventArgs args)
        {
            Dispatcher.Invoke(() =>
            {
                lock (_displayLock)
                {
                    TextStreamingResults.Text += Environment.NewLine;
                    TextStreamingResults.Text += "REMOVED: " + args.Path;
                }
            });
        }

        private void Changed(object sender, ValueChangedEventArgs args)
        {
            Dispatcher.Invoke(() =>
            {
                lock (_displayLock)
                {
                    TextStreamingResults.Text += Environment.NewLine;
                    TextStreamingResults.Text += "CHANGED: " + args.Path;
                    TextStreamingResults.Text += Environment.NewLine;
                    TextStreamingResults.Text += "DATA: " + args.Data;
                }
            });
        }

        private void Added(object sender, ValueAddedEventArgs args)
        {
            Dispatcher.Invoke(() =>
            {
                lock (_displayLock)
                {
                    TextStreamingResults.Text += Environment.NewLine;
                    TextStreamingResults.Text += "ADDED: " + args.Path;
                    TextStreamingResults.Text += Environment.NewLine;
                    TextStreamingResults.Text += "DATA: " + args.Data;
                }
            });
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            using (_resp) { }
            using (_fb) { }
        }
    }
}
