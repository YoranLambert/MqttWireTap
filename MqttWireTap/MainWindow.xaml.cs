using HiveMQtt.Client;
using HiveMQtt.Client.Events;
using HiveMQtt.Client.Options;
using Microsoft.Expression.Interactivity.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MqttWireTap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool mode = false;
        public static HiveMQClient Client { get; set; }
        public ICommand SubscribeCommand => new ActionCommand(() => Subscribe());
        public ICommand UnsubscribeCommand => new ActionCommand(() => Unsubscribe());
        public ICommand TopicCommand => new ActionCommand(() => Publish());
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            ModeLbl.Content = "Publisher mode";
            connect();
        }

        private async void connect()
        {
            var options = new HiveMQClientOptions();
            options.Host = "508b55b4d3794a40af7e9ade9d709bee.s2.eu.hivemq.cloud";
            options.Port = 8883;
            options.UserName = "Boars";
            options.Password = "BoarCrane1";
            Client = new HiveMQClient(options);
            await Client.ConnectAsync().ConfigureAwait(false);
            Client.OnMessageReceived +=
                (sender, e) => { OnMessageRecieved(sender, e); };
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            Client.DisconnectAsync();
            base.OnClosing(e);
        }
        private void OnMessageRecieved(object sender, OnMessageReceivedEventArgs e)
        {
            string message = $"{DateTime.Now.ToLongTimeString()} {e.PublishMessage.Topic}: {e.PublishMessage.PayloadAsString}";
            Dispatcher.BeginInvoke(new Action(delegate () { MessagesLts.Items.Insert(0, message); }));
            while (MessagesLts.Items.Count > 100) Dispatcher.Invoke(new Action(delegate () { MessagesLts.Items.RemoveAt(MessagesLts.Items.Count - 1); }));
        }

        private async void UnSubscribeBtn_Click(object sender, RoutedEventArgs e)
        {
            Unsubscribe();
        }
        private async void Unsubscribe()
        {
            string topic = SubcribedLts.SelectedItem as string;
            if (topic == null) return;
            await Client.UnsubscribeAsync(topic);
            SubcribedLts.Items.Remove(topic);
        }
        private async void SubscribeBtn_Click(object sender, RoutedEventArgs e)
        {
            Subscribe();
        }
        private async void Subscribe()
        {
            string topic = SubscribeInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(topic) || SubcribedLts.Items.Contains(topic)) return;
            await Client.SubscribeAsync(topic);
            SubcribedLts.Items.Add(topic);
            SubscribeInput.Clear();
            SubscribeInput.Focus();
        }
        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            MessagesLts.Items.Clear();
        }

        private void ModeBtn_Click(object sender, RoutedEventArgs e)
        {
            mode = !mode;
            ModeLbl.Content = mode ? "Publisher mode" : "Subscriber mode";
            SubscribeInput.Visibility = mode ? Visibility.Collapsed : Visibility.Visible;
            SubscribeBtn.Visibility = mode ? Visibility.Collapsed : Visibility.Visible;
            UnSubscribeBtn.Visibility = mode ? Visibility.Collapsed : Visibility.Visible;
            TopicInput.Visibility = mode ? Visibility.Visible : Visibility.Collapsed;
            MessageInput.Visibility = mode ? Visibility.Visible : Visibility.Collapsed;
            PublishBtn.Visibility = mode ? Visibility.Visible : Visibility.Collapsed;
        }
        private void PublishBtn_Click(object sender, RoutedEventArgs e)
        {
            Publish();
        }
        private void Publish()
        {
            string topic = TopicInput.Text.Trim();
            string message = MessageInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(topic) || string.IsNullOrWhiteSpace(message)) return;
            Client.PublishAsync(topic, message);
        }
    }
}
