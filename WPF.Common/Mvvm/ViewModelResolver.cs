﻿using System;
using System.Linq;
using System.Windows;
using WPF.Common.Infrastructure;
using Prism.Ioc;

/**
 * @Class Name : ViewModelResolver.cs
 * @Description : 뷰 모델 Resolver
 * @author 고형균
 * @since 2022.11.18
 * @version 1.0
 */
namespace WPF.Common.Mvvm
{
    public class ViewModelResolver : IViewModelResolver
    {
        private readonly Func<IContainerProvider> _containerFactory;
        private Action<object, object, IContainerProvider> _configureViewAndViewModel;
        private IContainerProvider _container;

        public IContainerProvider Container => _container ?? (_container = _containerFactory());

        public ViewModelResolver(Func<IContainerProvider> containerFactory)
        {
            Guards.ThrowIfNull(containerFactory);

            _containerFactory = containerFactory;
        }

        public object ResolveViewModelForView(object view, Type viewModelType)
        {
            var viewModel = Container.Resolve(viewModelType);
            _configureViewAndViewModel?.Invoke(view, viewModel, Container);

            return viewModel;
        }

        public IViewModelResolver IfInheritsFrom<TView, TViewModel>(Action<TView, TViewModel, IContainerProvider> configuration)
        {
            var previousAction = _configureViewAndViewModel;
            _configureViewAndViewModel = (view, viewModel, container) =>
            {
                previousAction?.Invoke(view, viewModel, container);
                if (view is TView tView && viewModel is TViewModel tViewModel)
                {
                    configuration?.Invoke(tView, tViewModel, container);
                }
            };
            return this;
        }

        public IViewModelResolver IfInheritsFrom<TView>(Type genericInterfaceType, Action<TView, object, IGenericInterface, IContainerProvider> configuration)
        {
            var previousAction = _configureViewAndViewModel;
            _configureViewAndViewModel = (view, viewModel, container) =>
            {
                previousAction?.Invoke(view, viewModel, container);
                var interfaceInstance = viewModel.AsGenericInterface(genericInterfaceType);
                if (view is TView tView && interfaceInstance != null)
                {
                    configuration?.Invoke(tView, viewModel, interfaceInstance, container);
                }
            };
            return this;
        }
    }

    public static class ViewModelResolverExtensions
    {
        public static IViewModelResolver UseDefaultConfigure(this IViewModelResolver @this) => @this
            .IfInheritsFrom<ViewModelBase>((view, viewModel) =>
            {
                viewModel.Dispatcher = view.Dispatcher;
            })
            .IfInheritsFrom<IViewLoadedAndUnloadedAware>((view, viewModel) =>
            {
                view.Loaded += (sender, e) => viewModel.OnLoaded();
                view.Unloaded += (sender, e) => viewModel.OnUnloaded();
            })
            .IfInheritsFrom(typeof(IViewLoadedAndUnloadedAware<>), (view, viewModel, interfaceInstance) =>
            {
                var viewType = view.GetType();
                if (interfaceInstance.GenericArguments.Single() != viewType)
                    throw new InvalidOperationException();

                var onLoadedMethod = interfaceInstance.GetMethod<Action<object>>("OnLoaded", viewType);
                var onUnloadedMethod = interfaceInstance.GetMethod<Action<object>>("OnUnloaded", viewType);

                view.Loaded += (sender, args) => onLoadedMethod(sender);
                view.Unloaded += (sender, args) => onUnloadedMethod(sender);
            });

        public static IViewModelResolver IfInheritsFrom<TViewModel>(this IViewModelResolver @this, Action<FrameworkElement, TViewModel> configuration)
        {
            Guards.ThrowIfNull(@this, configuration);

            return @this.IfInheritsFrom<FrameworkElement, TViewModel>((view, viewModel, container) => configuration(view, viewModel));
        }

        public static IViewModelResolver IfInheritsFrom(this IViewModelResolver @this, Type genericInterfaceType, Action<FrameworkElement, object, IGenericInterface> configuration)
        {
            Guards.ThrowIfNull(@this, configuration);

            return @this.IfInheritsFrom<FrameworkElement>(
                genericInterfaceType,
                (view, viewModel, interfaceInstance, container) => configuration(view, viewModel, interfaceInstance));
        }
    }
}
