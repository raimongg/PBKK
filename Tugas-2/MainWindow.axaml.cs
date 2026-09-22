using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace KalkulatorApp
{
    // ==========================================
    // 2. GUI (Window, Button, TextBlock, Border)
    // ==========================================
    public partial class MainWindow : Window
    {
        // C# Basic: Variables
        private double firstValue = 0;
        private string currentOperator = "";
        private bool isOperatorClicked = false;
        private bool isResultShown = false;

        // OOP: Instance (Object) dari class CalculatorEngine
        private readonly CalculatorEngine engine = new CalculatorEngine();

        public MainWindow()
        {
            InitializeComponent();
        }

        // ==========================================
        // 3. EVENT HANDLER & LOGIKA INPUT
        // ==========================================

        // Handler saat tombol angka, operator, atau 'C' diklik
        public void Btn_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Content is string text)
            {
                ProcessInput(text);
            }
        }

        // Handler saat tombol sama dengan (=) diklik
        public void BtnEquals_Click(object? sender, RoutedEventArgs e)
        {
            CalculateResult();
        }

        // Memproses semua jenis input (bisa dipanggil oleh klik tombol atau keyboard)
        private void ProcessInput(string input)
        {
            // Jika Tombol Clear (C) diklik
            if (input == "C")
            {
                ClearAll();
                return;
            }

            // Jika Tombol Akar (√) diklik
            if (input == "√")
            {
                CalculateSquareRoot();
                return;
            }

            // Jika Tombol Operator (+, -, ×, ÷, ^) diklik
            if ("÷×-+^".Contains(input))
            {
                SetOperator(input);
                return;
            }

            // Jika Titik Desimal (.) diklik
            if (input == ".")
            {
                AddDecimal();
                return;
            }

            // Jika Angka (0-9) diklik
            AddDigit(input);
        }

        // Membersihkan layar & me-reset kalkulator
        private void ClearAll()
        {
            TxtDisplay.Text = "0";
            LblOperation.Text = "";
            firstValue = 0;
            currentOperator = "";
            isOperatorClicked = false;
            isResultShown = false;
        }

        // Menambahkan angka ke layar
        private void AddDigit(string digit)
        {
            if (TxtDisplay.Text == "0" || isOperatorClicked || isResultShown)
            {
                TxtDisplay.Text = digit;
                isOperatorClicked = false;
                isResultShown = false;
            }
            else
            {
                // Batasi panjang angka agar tampilan tetap rapi
                if (TxtDisplay.Text != null && TxtDisplay.Text.Length < 15)
                {
                    TxtDisplay.Text += digit;
                }
            }
        }

        // Menambahkan titik desimal (.)
        private void AddDecimal()
        {
            if (isOperatorClicked || isResultShown)
            {
                TxtDisplay.Text = "0.";
                isOperatorClicked = false;
                isResultShown = false;
                return;
            }

            if (TxtDisplay.Text != null && !TxtDisplay.Text.Contains("."))
            {
                TxtDisplay.Text += ".";
            }
        }

        // Menetapkan operator matematika (+, -, ×, ÷)
        private void SetOperator(string op)
        {
            // Error Handling: menangani kesalahan format saat parsing angka
            try
            {
                // Jika sudah ada operator dan user mengetik angka kedua, hitung dulu sebelum lanjut (chaining)
                if (!string.IsNullOrEmpty(currentOperator) && !isOperatorClicked && !isResultShown)
                {
                    double secondValue = ParseDisplay();
                    firstValue = engine.PerformOperation(firstValue, secondValue, currentOperator);
                    TxtDisplay.Text = FormatNumber(firstValue);
                }
                else
                {
                    firstValue = ParseDisplay();
                }

                currentOperator = op;
                LblOperation.Text = $"{FormatNumber(firstValue)} {currentOperator}";
                isOperatorClicked = true;
                isResultShown = false;
            }
            catch (DivideByZeroException ex)
            {
                HandleError(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                HandleError(ex.Message);
            }
            catch (OverflowException ex)
            {
                HandleError(ex.Message);
            }
            catch (ArgumentException ex)
            {
                HandleError(ex.Message);
            }
            catch (Exception ex)
            {
                HandleError("Format angka tidak valid: " + ex.Message);
            }
        }

        // Menghitung akar kuadrat (√) dari angka di display
        private void CalculateSquareRoot()
        {
            try
            {
                double currentValue = ParseDisplay();
                double result = engine.PerformUnaryOperation(currentValue, "√");

                if (string.IsNullOrEmpty(currentOperator))
                {
                    // Perhitungan akar tunggal: contoh √9 = 3
                    LblOperation.Text = $"√({FormatNumber(currentValue)}) =";
                    TxtDisplay.Text = FormatNumber(result);
                    firstValue = result;
                    isResultShown = true;
                    isOperatorClicked = false;
                }
                else
                {
                    // Akar diterapkan pada operand kedua dalam operasi yang sedang berjalan
                    // Contoh: 10 + √9 -> display menjadi 3, label "10 + √(9)"
                    LblOperation.Text = $"{FormatNumber(firstValue)} {currentOperator} √({FormatNumber(currentValue)})";
                    TxtDisplay.Text = FormatNumber(result);
                    isOperatorClicked = false;
                    isResultShown = false;
                }
            }
            catch (ArgumentException ex)
            {
                HandleError(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                HandleError(ex.Message);
            }
            catch (Exception ex)
            {
                HandleError("Terjadi kesalahan: " + ex.Message);
            }
        }

        // Menghitung hasil perhitungan saat tombol '=' ditekan
        private void CalculateResult()
        {
            // Error Handling: try-catch untuk mencegah crash aplikasi
            try
            {
                if (!string.IsNullOrEmpty(currentOperator))
                {
                    double secondValue = ParseDisplay();
                    LblOperation.Text = $"{FormatNumber(firstValue)} {currentOperator} {FormatNumber(secondValue)} =";

                    // OOP: Memanggil method dari instance CalculatorEngine
                    double result = engine.PerformOperation(firstValue, secondValue, currentOperator);

                    TxtDisplay.Text = FormatNumber(result);
                    firstValue = result;
                    currentOperator = "";
                    isResultShown = true;
                    isOperatorClicked = false;
                }
            }
            catch (DivideByZeroException ex)
            {
                HandleError(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                HandleError(ex.Message);
            }
            catch (OverflowException ex)
            {
                HandleError(ex.Message);
            }
            catch (ArgumentException ex)
            {
                HandleError(ex.Message);
            }
            catch (FormatException)
            {
                HandleError("Format angka tidak valid!");
            }
            catch (Exception ex)
            {
                HandleError("Terjadi kesalahan: " + ex.Message);
            }
        }

        // Menghapus karakter terakhir (Backspace)
        private void HandleBackspace()
        {
            if (isResultShown || isOperatorClicked)
            {
                return;
            }

            if (!string.IsNullOrEmpty(TxtDisplay.Text) && TxtDisplay.Text.Length > 1)
            {
                TxtDisplay.Text = TxtDisplay.Text.Substring(0, TxtDisplay.Text.Length - 1);
            }
            else
            {
                TxtDisplay.Text = "0";
            }
        }

        // Konversi teks display ke tipe double dengan aman
        private double ParseDisplay()
        {
            string text = TxtDisplay.Text?.Replace(",", ".") ?? "0";
            if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
            {
                return value;
            }
            return 0;
        }

        // Format angka hasil agar tidak memunculkan .0 berlebih atau desimal panjang tak terhingga
        private string FormatNumber(double number)
        {
            if (double.IsNaN(number) || double.IsInfinity(number))
            {
                return "Error";
            }

            // Gunakan format desimal bersih
            return number.ToString("G12", CultureInfo.InvariantCulture);
        }

        // Penanganan error tampilan
        private void HandleError(string message)
        {
            LblOperation.Text = message;
            TxtDisplay.Text = "0";
            firstValue = 0;
            currentOperator = "";
            isOperatorClicked = false;
            isResultShown = false;
        }

        // ==========================================
        // 4. KEYBOARD SUPPORT
        // ==========================================
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.Key)
            {
                case Key.D0:
                case Key.NumPad0:
                    ProcessInput("0");
                    break;
                case Key.D1:
                case Key.NumPad1:
                    ProcessInput("1");
                    break;
                case Key.D2:
                case Key.NumPad2:
                    ProcessInput("2");
                    break;
                case Key.D3:
                case Key.NumPad3:
                    ProcessInput("3");
                    break;
                case Key.D4:
                case Key.NumPad4:
                    ProcessInput("4");
                    break;
                case Key.D5:
                case Key.NumPad5:
                    ProcessInput("5");
                    break;
                case Key.D6:
                    if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                        ProcessInput("^");
                    else
                        ProcessInput("6");
                    break;
                case Key.NumPad6:
                    ProcessInput("6");
                    break;
                case Key.D7:
                case Key.NumPad7:
                    ProcessInput("7");
                    break;
                case Key.D8:
                    if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                        ProcessInput("×");
                    else
                        ProcessInput("8");
                    break;
                case Key.NumPad8:
                    ProcessInput("8");
                    break;
                case Key.D9:
                case Key.NumPad9:
                    ProcessInput("9");
                    break;
                case Key.OemPeriod:
                case Key.Decimal:
                    ProcessInput(".");
                    break;
                case Key.Add:
                case Key.OemPlus:
                    ProcessInput("+");
                    break;
                case Key.Subtract:
                case Key.OemMinus:
                    ProcessInput("-");
                    break;
                case Key.Multiply:
                    ProcessInput("×");
                    break;
                case Key.Divide:
                case Key.Oem2: // '/' key
                    ProcessInput("÷");
                    break;
                case Key.R:
                    ProcessInput("√");
                    break;
                case Key.Enter:
                    CalculateResult();
                    break;
                case Key.Escape:
                case Key.C:
                    ClearAll();
                    break;
                case Key.Back:
                    HandleBackspace();
                    break;
            }
        }
    }
}