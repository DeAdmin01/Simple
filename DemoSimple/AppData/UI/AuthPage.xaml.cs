using DemoSimple.AppData.Model;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace DemoSimple.AppData.UI
{
    public partial class AuthPage : Page
    {
        public AuthPage() => InitializeComponent();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string err = "";
            if (string.IsNullOrWhiteSpace(UserLoginbox.Text)) err += "Введите логин.\n";
            if (string.IsNullOrWhiteSpace(UserPasswordBox.Password)) err += "Введите пароль.";

            if (err != "")
            {
                MessageBox.Show(err);
                return;
            }

            var user = Connection.connection.Users.FirstOrDefault(x => x.Login == UserLoginbox.Text);

            if (user == null)
            {
                MessageBox.Show("Пользователь не найден.");
                return;
            }

            if (user.IsBlocked)
            {
                MessageBox.Show("Пользователь заблокирован.");
                return;
            }

            if (new Capcha().ShowDialog() != true)
            {
                Fail(user, "Капча не пройдена.");
                return;
            }

            if (user.Password != UserPasswordBox.Password)
            {
                Fail(user, "Неверный пароль.");
                return;
            }
            user.FailedAttempts = 0;
            Connection.connection.SaveChanges();

            if (user.Role.Name == "Администратор")
                NavigationService.Navigate(new AdminPage());
            else
                NavigationService.Navigate(new UserPage(user));
        }
        private void Fail(User u, string reason)
        {
            u.FailedAttempts++;
            if (u.FailedAttempts >= 3)
            {
                u.IsBlocked = true;
                MessageBox.Show("Пользователь заблокирован, превышено число попыток.");
            }
            else
            {
                MessageBox.Show($"{reason} Осталось попыток: {3 - u.FailedAttempts}");
            }
            Connection.connection.SaveChanges();
        }
    }
}