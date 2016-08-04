using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Debug = System.Diagnostics.Debug;

namespace XamarinUWPdataBindBug
{
    public class Page1ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        private void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public Page1ViewModel()
        {
            MainText = "initializing...";

            Stuff = new ObservableCollection<uint>();


            //this task/thread *could* be inside a model... for simplicity, I did not create
            //a seperate model object
            Task.Run(async () =>
            {
                await Task.Delay(2000).ConfigureAwait(false);
                uint i = 0;
                do
                {
                    try
                    {
                        await Task.Delay(1000).ConfigureAwait(false);

                        //this call will throw a null ref internally to Xamarin.Forms 
                        //on UWP/Win8.1 (desktop), not Android.
                        Stuff.Add(++i);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("error: {0}", ex.ToString());
                    }
                } while (true);

            });
        }


        string myText;
        public string MainText
        {
            get { return myText; }
            set { myText = value; OnPropertyChanged("MainText"); }
        }

        public ObservableCollection<uint> Stuff { get; set; } 
    }
}