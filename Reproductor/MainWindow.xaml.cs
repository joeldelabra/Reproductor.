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
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Win32;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Windows.Threading;

namespace Reproductor
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        AudioFileReader reader;
        // NUuestra comunicacion con la tarjeta de sonido.
        WaveOutEvent output;

        DispatcherTimer Timer;
        VolumeSampleProvider volume;
        FadeInOutSampleProvider fades;
        bool dragging = false;
        bool faddingOut = false;
    

        public MainWindow()
        {
            InitializeComponent();
            LlenarComboSalida();

            // Iniciar Timer.
            // Establecer tiempo de ejecuciones
            // Establecer lo que se va a ejecutar.
            Timer = new DispatcherTimer();
            Timer.Interval = TimeSpan.FromMilliseconds(500);
            Timer.Tick += Timer_Tick;



        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (reader != null)
            {
                lblTiempoActual.Text =
                    reader.CurrentTime.ToString().Substring(0, 8);
                sldReproduccion.Value = reader.CurrentTime.TotalSeconds;
                if (!dragging)
                {
                    sldReproduccion.Value = reader.CurrentTime.TotalSeconds;
                }


            }
        }

        private void LlenarComboSalida()
        {
            cbSalida.Items.Clear();
            for (int i=0; i < WaveOut.DeviceCount; i++)
            {
                WaveOutCapabilities Capacidades =
                     WaveOut.GetCapabilities(i);
                cbSalida.Items.Add(Capacidades.ProductName);
            }

            cbSalida.SelectedIndex = 0;
        }

        private void btnElegirArchivo_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = 
                new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                txtRutaArchivo.Text =
                     openFileDialog.FileName;
            }
        }

        private void btnReproducir_Click(object sender, RoutedEventArgs e)
        {
            
            if (output != null && output.PlaybackState == PlaybackState.Paused)
            {
                output.Play();

                btnDetener.IsEnabled = true;
                btnPausa.IsEnabled = true;
                btnReproducir.IsEnabled = false;

            }
            else
            {
                reader =
                    new AudioFileReader(txtRutaArchivo.Text);

                fades = 
                    new FadeInOutSampleProvider(reader,true);
                double milisegundosFadeIn = double.Parse(txtDuracionFadeIn.Text) * 1000.0;
                fades.BeginFadeIn(milisegundosFadeIn);

                output =
                    new WaveOutEvent();

                output.DeviceNumber = cbSalida.SelectedIndex;

                output.PlaybackStopped += Output_PlaybackStopped;
                volume = 
                    new VolumeSampleProvider(fades);
                volume.Volume = (float) sldVolumen.Value;



                output.Init(volume);
                output.Play();

                btnDetener.IsEnabled = true;
                btnPausa.IsEnabled = true;
                btnReproducir.IsEnabled = false;

                lblTiempoFinal.Text =
                    reader.TotalTime.ToString().Substring(0,8);
                lblTiempoActual.Text =
                    reader.CurrentTime.ToString().Substring(0, 8);
                sldReproduccion.Maximum = reader.TotalTime.TotalSeconds;
                sldReproduccion.Value = reader.CurrentTime.TotalSeconds;



                Timer.Start();

            }
        }

        private void Output_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            reader.Dispose();
            output.Dispose();
            Timer.Stop();
        }

        private void btnPausa_Click(object sender, RoutedEventArgs e)
        {
            if (output != null)
            {
                output.Pause();

                btnDetener.IsEnabled = true;
                btnPausa.IsEnabled = false;
                btnReproducir.IsEnabled = true;
            }

        }

        private void btnDetener_Click(object sender, RoutedEventArgs e)
        {
            if (output != null)
            {
                output.Stop();

                btnDetener.IsEnabled = false;
                btnPausa.IsEnabled = false;
                btnReproducir.IsEnabled = true;
            }
        }
        private void sdlReproduccion_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            dragging = true;
        }

        private void sdlReproduccion_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            dragging = false;
            if (reader != null && output != null && (output.PlaybackState != PlaybackState.Stopped))
            {
                reader.CurrentTime = TimeSpan.FromSeconds(sldReproduccion.Value);
            }
        }

        private void sldReproduccion_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (volume != null && output != null && output.PlaybackState != PlaybackState.Stopped)
            {
                volume.Volume = (float)sldVolumen.Value;
                lblPorcentajeVolumen.Text = ((int)(sldVolumen.Value * 100)).ToString() + " %";
            }
        }

        private void btnFadeOut_Click(object sender, RoutedEventArgs e)
        {
            if(!faddingOut && fades !=null && output != null)
            {
                faddingOut = true;
                double milisegundosFadeOut = Double.Parse(txtDuracionFadeOut.Text) * 1000.0;
                fades.BeginFadeOut(milisegundosFadeOut);
            }
        }
    }
}
