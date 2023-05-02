using Prism.Events;
using Prism.Ioc;
using Prism.Logging;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Windows.Threading;

/**
 * @Class Name : ViewModelBase.cs
 * @Description : 뷰 모델 공통
 * @author 고형균
 * @since 2022.11.18
 * @version 1.0
 */
namespace WPF.Common.Mvvm
{
    public abstract class ViewModelBase : BindableBase
    {
        private string title;
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        protected IContainerExtension Container { get; }

        private IEventAggregator eventAggregator;
        public IEventAggregator EventAggregator
        {
            get { return eventAggregator; }
            private set { this.SetProperty(ref this.eventAggregator, value); }
        }

        private IRegionManager regionManager;
        public IRegionManager RegionManager
        {
            get { return regionManager; }
            private set { this.SetProperty(ref this.regionManager, value); }
        }

        public Dispatcher Dispatcher { get; set; }
        protected virtual void Invoke(Action action) => Dispatcher.Invoke(action);

        public ILogger Logger { get; }

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="container"></param>
        public ViewModelBase(IContainerExtension container)
        {
            this.Container = container;
            EventAggregator = container.Resolve<IEventAggregator>();
            RegionManager = container.Resolve<IRegionManager>();

            Logger = container.Resolve<ILogger>();
        }
    }
}
