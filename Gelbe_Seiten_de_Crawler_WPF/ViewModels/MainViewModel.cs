using Gelbe_Seiten_de_Crawler_WPF.Models;
using Gelbe_Seiten_de_Crawler_WPF.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Gelbe_Seiten_de_Crawler_WPF.ViewModels
{     
    class MainViewModel : BaseViewModel
    {
        #region InputPropertys
        private string range = "-1";
        public string Range
        {
            get => range;
            set
            {
                range = value;
                OnPropertyChanged();
            }
        }

        private string what = "Restaurant";
        public String What
        {
            get => what;
            set
            {
                what = value;
                OnPropertyChanged();
            }
        }

        private string where = "Bielefeld";
        public string Where
        {
            get => where;
            set
            {
                where = value;
                OnPropertyChanged();
            }
        }

        private string ammount = "100";
        public string Ammount
        {
            get => ammount;
            set
            {
                ammount = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region RelayCommands
        public RelayCommand SendRequestCommand
        {
            get
            {
                return new RelayCommand(parameter => StartCrawling(), parameter => CanStartCrawling());
            }
        }
        public RelayCommand KillSendRequestCommand
        {
            get
            {
                return new RelayCommand(parameter => KillSendRequestThread());
            }
        }
        public RelayCommand ClearListCommand
        {
            get
            {
                return new RelayCommand(parameter => ClearList());
            }
        }


        #endregion

        #region Export Stuff

        public ExportToCsvService ExportToCsvService
        {
            get
            {
                return new ExportToCsvService();
            }
        }

        public RelayCommand ExportToCsvCommandSmall
        {
            get
            {
                return new RelayCommand(parameter => ExportToCsvSmall());
            }
        }
        public RelayCommand ExportToCsvCommandContact
        {
            get
            {
                return new RelayCommand(parameter => ExportToCsvContact());
            }
        }
        public RelayCommand ExportToCsvCommandFull
        {
            get
            {
                return new RelayCommand(parameter => ExportToCsvFull());
            }
        }
        public RelayCommand ToggleExportToCsvCommand
        {
            get
            {
                return new RelayCommand(parameter => ToggleExportToCsvPopUp());
            }
        }

        private bool isExportPopUpVisible;
        public bool IsExportPopUpVisible
        {
            get { return isExportPopUpVisible; }
            set
            {
                if (isExportPopUpVisible == value)
                    return;
                isExportPopUpVisible = value;
                OnPropertyChanged();
            }
        }


        private void ExportToCsvSmall()
        {
            KillSendRequestThread();
            ExportToCsvService.Export(Contacts.ToList(), Mode.Small);
        }
        private void ExportToCsvContact()
        {
            KillSendRequestThread();
            ExportToCsvService.Export(Contacts.ToList(), Mode.Contact);
        }
        private void ExportToCsvFull()
        {
            KillSendRequestThread();
            ExportToCsvService.Export(Contacts.ToList(), Mode.Full);
        }
        private void ToggleExportToCsvPopUp()
        {
            IsExportPopUpVisible = !IsExportPopUpVisible;
        }
        #endregion

        #region other Properties
        public CrawlerService CrawlerService { get; }

        public ObservableCollection<Contact> Contacts { get; } = new ObservableCollection<Contact>();

        public CancellationTokenSource cancellationTokenSource { get; set; }
        #endregion

        public MainViewModel()
        {
            CrawlerService = new CrawlerService();
        }



        private void ClearList() => Contacts.Clear();



        private void KillSendRequestThread()
        {
            cancellationTokenSource.Cancel();
        }


        public static bool searchIsActive { get; set; }

        public void StartCrawling()
        {
            searchIsActive = true;
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
            cancellationTokenSource = new CancellationTokenSource();

            Task.Factory.StartNew(() =>
            {
                CrawlerService.SendRequest(cancellationTokenSource, dispatcher, Contacts, Range, What, Where, "0", Ammount);
            });
        }



        public bool CanStartCrawling()
        {
            if (Range != ""
                && What != ""
                && Where != ""
                && Ammount != ""
                && int.TryParse(Ammount, out _)
                && int.TryParse(Range, out _)
                && Convert.ToInt32(Range) >= -1
                && Convert.ToInt32(Ammount) >= 1
                && !searchIsActive)
                return true;

            return false;
        }
    }

}
