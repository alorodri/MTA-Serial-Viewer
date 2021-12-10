using System;
using System.Diagnostics;
using System.Management;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace SerialViewer
{
    public partial class MainWindow : Window
    {
        private const bool ONLY_DEBUG_FALSE = false;
        private const bool ONLY_DEBUG_TRUE = true;

        private const bool SINGLE_QUERY_FALSE = false;
        private const bool SINGLE_QUERY_TRUE = true;

        private const string MTASA_VERSION_FOLDER = @"\1.5";
        private const string MTASA_REGISTRY_FOLDER = @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Multi Theft Auto: San Andreas All";
        private const string MTASA_GENERAL_SETTINGS_FOLDER = @"\Settings\general";
        private const string MTASA_SERIAL_KEY = "serial";

        public MainWindow()
        {
            InitializeComponent();
            FillSerial();
            FillSystemSpecs();
        }

        private static T? QueryInfo<T>(in string keyValue, in string propertyValue, in bool onlyDebug, in bool singleQuery)
        {
            ManagementObjectSearcher systemSearcher = new("select * from " + keyValue);

            uint found = 0;
            T? result = default;
            foreach(ManagementObject share in systemSearcher.Get())
            {
                PropertyData? PC = SearchPropertyValue(share, propertyValue, onlyDebug);
                if (PC == null) continue;

                if (found == 0)
                {
                    result = RuntimeCast<T>(PC.Value ?? "not found");

                    if (singleQuery) return result;
                }
                else
                {
                    SumResult(ref result, PC.Value);
                }

                ++found;
            }

            return result;
        }

        private void FillSerial()
        {
            string? keyValue = (string?)Registry.GetValue(MTASA_REGISTRY_FOLDER
                + MTASA_VERSION_FOLDER
                + MTASA_GENERAL_SETTINGS_FOLDER,
                MTASA_SERIAL_KEY,
                "not found");

            serialTextBox.Text = keyValue;
        }

        private void FillSystemSpecs()
        {
            SystemText.Text = QueryInfo<string>("Win32_OperatingSystem", "Caption", ONLY_DEBUG_FALSE, SINGLE_QUERY_TRUE);
            SystemText.Text += " ";
            SystemText.Text += QueryInfo<string>("Win32_OperatingSystem", "OSArchitecture", ONLY_DEBUG_FALSE, SINGLE_QUERY_TRUE);

            ProcessorText.Text = QueryInfo<string>("Win32_Processor", "Name", ONLY_DEBUG_FALSE, SINGLE_QUERY_TRUE);

            MemoryText.Text = Math.Round(QueryInfo<ulong>("Win32_PhysicalMemory", "Capacity", ONLY_DEBUG_FALSE, SINGLE_QUERY_FALSE)
                * 9.31e-10, MidpointRounding.AwayFromZero) + "GB @"
                + QueryInfo<uint>("Win32_PhysicalMemory", "Speed", ONLY_DEBUG_FALSE, SINGLE_QUERY_TRUE) + "MHz";

            GraphicsText.Text = QueryInfo<string>("Win32_VideoController", "Name", ONLY_DEBUG_FALSE, SINGLE_QUERY_FALSE);
        }

        private static PropertyData? SearchPropertyValue(in ManagementObject share, in string propertyValue, in bool onlyDebug)
        {
            foreach (PropertyData PC in share.Properties)
            {
                if (onlyDebug)
                {
                    Trace.WriteLine(PC.Name + ": " + PC.Value);
                }
                else
                {
                    if (PC.Name == propertyValue)
                    {
                        return PC;
                    }
                }
            }
            return null;
        }

        private static void SumResult<T>(ref T? result, object? pcValue)
        {
            if (typeof(T) == typeof(uint))
            {
                result = RuntimeCast<T>(RuntimeCast<uint>(result) + RuntimeCast<uint>(pcValue ?? (uint)0));
            }
            else if (typeof(T) == typeof(ulong))
            {
                result = RuntimeCast<T>(RuntimeCast<ulong>(result) + RuntimeCast<ulong>(pcValue ?? (ulong)0));
            }
            else if (typeof(T) == typeof(string))
            {
                result = RuntimeCast<T>(RuntimeCast<string>(result) + " | " + RuntimeCast<string>(pcValue ?? "not found"));
            }
        }

        private static T? RuntimeCast<T>(object? obj)
        {
            if (obj == null) return default;
            return (T)obj;
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(serialTextBox.Text);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MinHeight = Height;
        }

        private void CopyComputerSpecs(object sender, RoutedEventArgs e)
        {
            string content = "Operating System: " + SystemText.Text + "\n"
                + "Processor: " + ProcessorText.Text + "\n"
                + "Memory: " + MemoryText.Text + "\n"
                + "Graphics card: " + GraphicsText.Text;
            
            Clipboard.SetText(content);
        }

        private void ShowComputerSpecs(object sender, RoutedEventArgs e)
        {
            TextBlock tb = (TextBlock)ShowSpecsButtonGrid.Children[0];
            Line lineLeft = (Line)ShowSpecsButtonGrid.Children[1];
            Line lineRight = (Line)ShowSpecsButtonGrid.Children[2];

            if (SpecsGrid.Visibility == Visibility.Visible)
            {
                SpecsGrid.Visibility = Visibility.Collapsed;
                tb.Text = "Show computer specs";

                lineLeft.Y1 = 7;
                lineLeft.Y2 = 12;

                lineRight.Y1 = 12;
                lineRight.Y2 = 7;
            } else
            {
                SpecsGrid.Visibility = Visibility.Visible;
                CheckNewMinSizeWindow();
                tb.Text = "Hide computer specs";

                lineLeft.Y1 = 12;
                lineLeft.Y2 = 7;

                lineRight.Y1 = 7;
                lineRight.Y2 = 12;
            }
        }

        private void CheckNewMinSizeWindow()
        {
            if (SizeToContent == SizeToContent.Manual)
            {
                SizeToContent = SizeToContent.Height;
            }
        }
    }
}
