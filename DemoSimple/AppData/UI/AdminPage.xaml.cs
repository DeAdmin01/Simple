using DemoSimple.AppData.Model;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace DemoSimple.AppData.UI
{
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            UsersListBox.ItemsSource = Connection.connection.Users.ToList();
            RoleComboBox.ItemsSource = Connection.connection.Roles.ToList();
        }

        private void UsersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EditPanel.IsEnabled = true;
            EditPanel.DataContext = UsersListBox.SelectedItem as User;
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            UsersListBox.SelectedItem = null;
            EditPanel.IsEnabled = true;
            EditPanel.DataContext = new User { Login = "Имя пользователя", UserRole = 1, IsBlocked = false, FailedAttempts = 0 };
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (EditPanel.DataContext is User u && u.UserID != 0)
            {
                Connection.connection.Users.Remove(u);
                Connection.connection.SaveChanges();
                LoadData();
                MessageBox.Show("Пользователь удален.");
            }
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            if (EditPanel.DataContext is User u)
            {
                if (string.IsNullOrWhiteSpace(u.Login) || string.IsNullOrWhiteSpace(u.Password))
                {
                    MessageBox.Show("Заполните поля логина и пароля.");
                    return;
                }

                if (Connection.connection.Users.Any(x => x.UserID != u.UserID && x.Login == u.Login))
                {
                    MessageBox.Show("Имя пользователя уже используется.");
                    return;
                }
                if (u.UserID == 0)
                {
                    using (var connection = new DemoSimpleEntities())
                    {
                        Connection.connection.Users.Add(u);
                        Connection.connection.SaveChanges();
                    }
                }

                Connection.connection.SaveChanges();
                LoadData();
                MessageBox.Show("Изменения успешно сохранены.");
            }
        }
        private void Exit_Click(object sender, RoutedEventArgs e) => NavigationService.GoBack();
    }
}