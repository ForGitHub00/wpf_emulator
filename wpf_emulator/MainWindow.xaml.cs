using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace wpf_emulator {
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            TCP_X = Canvas.GetLeft(tcp);
            TCP_Y = Canvas.GetTop(tcp);

           // MessageBox.Show($"{laser.Parent}");

            var descriptor = DependencyPropertyDescriptor.FromProperty(Canvas.LeftProperty, typeof(Ellipse));
            descriptor.AddValueChanged(tcp, OnValueChanged);

        }

        private void OnValueChanged(object sender, EventArgs e) {
            Canvas.SetLeft(laser, TCP_X + 40);
            Canvas.SetTop(laser, TCP_Y - 40);
        }

        public double TCP_X;
        public double TCP_Y;

        Thread thrd_move;
        Thread thrd_draw;
        Thread thrd_geom;


        private void bt_start_Click(object sender, RoutedEventArgs e) {
            thrd_move = new Thread(new ThreadStart(Move));
            thrd_draw = new Thread(new ThreadStart(Draw));
            thrd_geom = new Thread(new ThreadStart(Geom));
            thrd_move.Start();
            thrd_draw.Start();
            thrd_geom.Start();
        }
        public void Move() {
            while (true) {
                Dispatcher.Invoke(() => {
                    TCP_X += 1;
                    //TCP_Y += 1;
                });
                Thread.Sleep(20);
            }
        }
        public void Draw() {
            while (true) {
                Dispatcher.Invoke(() => {
                    cnv.Children.Clear();
                    Canvas.SetLeft(tcp, TCP_X);
                    Canvas.SetTop(tcp, TCP_Y);                   

                    cnv.Children.Add(laser);
                    cnv.Children.Add(tcp);
                    cnv.Children.Add(shw);

                    var g = RenderedIntersect(cnv, laser, shw);
                    var p = new Path() { Stroke = Brushes.Yellow, StrokeThickness = 2, Data = g };
                    if (!p.Data.Bounds.IsEmpty) {
                        cnv.Children.Add(p);
                        TCP_Y = p.Data.Bounds.Y;             
                    }
                    

                });
            }
        }
        public void Geom() {
            /*
            while (true) {
                Dispatcher.Invoke(() => {                  
                    cnv.Children.Add(p);
                });
                Thread.Sleep(40);
            }*/
        }

        static CombinedGeometry RenderedIntersect(Visual ctx, Shape s1, Shape s2) {
            var p = new Pen(Brushes.Transparent, 0.01);
            
            var t1 = s1.TransformToAncestor(ctx) as Transform;
            var t2 = s2.TransformToAncestor(ctx) as Transform;
            var g1 = s1.RenderedGeometry;
            var g2 = s2.RenderedGeometry;
            if (s1 is Line)
                g1 = g1.GetWidenedPathGeometry(p);
            if (s2 is Line)
                g2 = g2.GetWidenedPathGeometry(p);
            g1.Transform = t1;
            g2.Transform = t2;
            return new CombinedGeometry(GeometryCombineMode.Intersect, g1, g2);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            thrd_move.Abort();
            thrd_draw.Abort();
            Dispatcher.DisableProcessing();
        }


    }
}
