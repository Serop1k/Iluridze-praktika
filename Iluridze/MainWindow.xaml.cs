using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Windows;
using System.Windows.Controls;

namespace Iluridze
{
    public partial class MainWindow : Window
    {
        private string strokaPodklucheniya = "Server=localhost;Database=RestaurantDB;Uid=root;Pwd=root;";

        public MainWindow()
        {
            InitializeComponent();
            ZagruzitZakazi();
        }

        private void ZagruzitZakazi()
        {
            try
            {
                using (var podkluchenie = new MySqlConnection(strokaPodklucheniya))
                {
                    podkluchenie.Open();
                    string sqlZapros = "SELECT OrderID, CustomerName as imya, OrderDate, TotalAmount FROM Orders ORDER BY OrderDate DESC";
                    var adapter = new MySqlDataAdapter(sqlZapros, podkluchenie);
                    var tablicaDannih = new DataTable();
                    adapter.Fill(tablicaDannih);

                    TablicaZakazov.ItemsSource = tablicaDannih.DefaultView;
                }
            }
            catch (Exception oshibka)
            {
                MessageBox.Show("Ошибка загрузки: " + oshibka.Message);
            }
        }

        private void DobavitZakaz(object otpravitel, RoutedEventArgs e)
        {
            try
            {
                string imyaKlienta = PoleImeni.Text;
                string vibranoeBludo = (SpisokEdi.SelectedItem as ComboBoxItem)?.Content.ToString();

                if (string.IsNullOrWhiteSpace(imyaKlienta) || string.IsNullOrWhiteSpace(vibranoeBludo))
                {
                    MessageBox.Show("Заполните все поля!");
                    return;
                }

                using (var podkluchenie = new MySqlConnection(strokaPodklucheniya))
                {
                    podkluchenie.Open();
                    string sqlZapros = @"INSERT INTO Orders 
                                        (CustomerName, OrderDate, TotalAmount) 
                                        VALUES 
                                        (@imya, @data, @summa)";

                    var komanda = new MySqlCommand(sqlZapros, podkluchenie);
                    komanda.Parameters.AddWithValue("@imya", imyaKlienta);
                    komanda.Parameters.AddWithValue("@data", DateTime.Now);
                    komanda.Parameters.AddWithValue("@summa", 0);

                    komanda.ExecuteNonQuery();
                }

                MessageBox.Show("Заказ добавлен!");
                PoleImeni.Clear();
                SpisokEdi.SelectedIndex = -1;
                ZagruzitZakazi();
            }
            catch (Exception oshibka)
            {
                MessageBox.Show("Ошибка добавления: " + oshibka.Message);
            }
        }

        private void ObnovitZakaz(object otpravitel, RoutedEventArgs e)
        {
            try
            {
                if (!int.TryParse(PoleID.Text, out int idZakaza) ||
                    !decimal.TryParse(PoleKolichestva.Text, out decimal novayaSumma))
                {
                    MessageBox.Show("Проверьте введенные данные!");
                    return;
                }

                using (var podkluchenie = new MySqlConnection(strokaPodklucheniya))
                {
                    podkluchenie.Open();
                    string sqlZapros = @"UPDATE Orders 
                                        SET TotalAmount = @summa 
                                        WHERE OrderID = @id";

                    var komanda = new MySqlCommand(sqlZapros, podkluchenie);
                    komanda.Parameters.AddWithValue("@summa", novayaSumma);
                    komanda.Parameters.AddWithValue("@id", idZakaza);

                    int resultat = komanda.ExecuteNonQuery();

                    if (resultat > 0)
                    {
                        MessageBox.Show("Заказ обновлен!");
                        ZagruzitZakazi();
                    }
                    else
                    {
                        MessageBox.Show("Заказ не найден!");
                    }
                }
            }
            catch (Exception oshibka)
            {
                MessageBox.Show("Ошибка обновления: " + oshibka.Message);
            }
        }

        private void UdalitZakaz(object otpravitel, RoutedEventArgs e)
        {
            if (!int.TryParse(PoleID.Text, out int idZakaza))
            {
                MessageBox.Show("Введите корректный ID!");
                return;
            }

            try
            {
                var otvet = MessageBox.Show($"Удалить заказ #{idZakaza}?",
                                          "Подтверждение",
                                          MessageBoxButton.YesNo);

                if (otvet == MessageBoxResult.Yes)
                {
                    using (var podkluchenie = new MySqlConnection(strokaPodklucheniya))
                    {
                        podkluchenie.Open();
                        string sqlZapros = "DELETE FROM Orders WHERE OrderID = @id";
                        var komanda = new MySqlCommand(sqlZapros, podkluchenie);
                        komanda.Parameters.AddWithValue("@id", idZakaza);

                        int resultat = komanda.ExecuteNonQuery();

                        if (resultat > 0)
                        {
                            MessageBox.Show("Заказ удален!");
                            ZagruzitZakazi();
                            PoleID.Clear();
                            PoleKolichestva.Clear();
                        }
                        else
                        {
                            MessageBox.Show("Заказ не найден!");
                        }
                    }
                }
            }
            catch (Exception oshibka)
            {
                MessageBox.Show("Ошибка удаления: " + oshibka.Message);
            }
        }

        private void UdalitIzTablici(object otpravitel, RoutedEventArgs e)
        {
            if (TablicaZakazov.SelectedItem is DataRowView stroka)
            {
                PoleID.Text = stroka["OrderID"].ToString();
                UdalitZakaz(otpravitel, e);
            }
        }
    }
}