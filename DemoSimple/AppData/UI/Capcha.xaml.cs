using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace DemoSimple.AppData.UI
{
    /// <summary>
    /// Логика взаимодействия для Capcha.xaml
    /// </summary>
    public partial class Capcha : Window
    {
        Image[] imgs;
        public Capcha()
        {
            InitializeComponent();
            MessageBox.Show("Поворачивайте фрагменты, чтобы собрать изображение.");
            imgs = new[] { PicOne, PicTwo, PicThree, PicFour };
            Random r = new Random();
            foreach (var img in imgs)
            {
                int a = new[] { 90, 180, 270 }[r.Next(3)];
                img.Tag = a;
                img.RenderTransform = new RotateTransform(a);
            }
        }

        private void Pic_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var img = (Image)sender;
            int a = (Convert.ToInt32(img.Tag) + 90) % 360;
            img.RenderTransform = new RotateTransform(a);
            img.Tag = a;
        }

        private void Check_Click(object sender, RoutedEventArgs e)
        {
            if (imgs.All(x => Convert.ToInt32(x.Tag) == 0))
                DialogResult = true; 
            else
                MessageBox.Show("Изображения расположены не верно");
        }
    }
}
