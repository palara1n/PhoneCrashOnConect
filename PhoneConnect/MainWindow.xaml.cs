using System;
using System.Diagnostics;
using System.Management;
using System.Windows;

namespace PhoneChargerApp
{
    public partial class MainWindow : Window
    {
        private bool isPhoneConnected = false;  // Track the current connection status
        private ManagementEventWatcher? insertWatcher; // Nullable field
        private ManagementEventWatcher? removeWatcher; // Nullable field

        public MainWindow()
        {
            InitializeComponent();
            DetectPhoneConnection();
        }

        private void DetectPhoneConnection()
        {
            // Setup WMI event watcher to detect USB device insert/remove events
            var insertQuery = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2");
            var removeQuery = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 3");

            insertWatcher = new ManagementEventWatcher(insertQuery);
            removeWatcher = new ManagementEventWatcher(removeQuery);

            insertWatcher.EventArrived += new EventArrivedEventHandler(DeviceInserted);
            removeWatcher.EventArrived += new EventArrivedEventHandler(DeviceRemoved);

            insertWatcher.Start();
            removeWatcher.Start();
        }

        private void DeviceInserted(object sender, EventArrivedEventArgs e)
        {
            // Simulate phone being connected, but only show a message if the phone was previously disconnected
            Dispatcher.Invoke(() =>
            {
                if (!isPhoneConnected)
                {
                    isPhoneConnected = true;  // Set phone as connected
                    StatusTextBlock.Text = "Phone connected.";

                    // Kill non-essential processes
                    Process[] processes = Process.GetProcesses(); // Get all processes in your system

                    foreach (var process in processes)
                    {
                        try
                        {
                            // Skip critical system processes to avoid system instability
                            if (process.ProcessName != "explorer" && process.ProcessName != "winlogon" &&
                                process.ProcessName != "csrss" && process.ProcessName != "services" &&
                                process.ProcessName != "lsass" && process.ProcessName != "System" &&
                                process.ProcessName != "Idle")
                            {
                                process.Kill(); // Kill the process
                            }
                        }
                        catch (Exception)
                        {
                            // Silently catch exceptions to avoid crashing the program
                        }
                    }
                }
            });
        }

        private void DeviceRemoved(object sender, EventArrivedEventArgs e)
        {
            // Simulate phone being disconnected, but only show a message if the phone was previously connected
            Dispatcher.Invoke(() =>
            {
                if (isPhoneConnected)
                {
                    isPhoneConnected = false;  // Set phone as disconnected
                    StatusTextBlock.Text = "Phone disconnected.";
                }
            });
        }

        // Stop the watchers when the application is closed
        protected override void OnClosed(EventArgs e)
        {
            insertWatcher?.Stop(); // Use null-conditional operator
            removeWatcher?.Stop(); // Use null-conditional operator
            insertWatcher?.Dispose(); // Use null-conditional operator
            removeWatcher?.Dispose(); // Use null-conditional operator
            base.OnClosed(e);
        }
    }
}
