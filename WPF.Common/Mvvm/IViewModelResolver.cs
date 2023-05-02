using WPF.Common.Infrastructure;
using Prism.Ioc;
using System;

/**
 * @Class Name : IViewModelResolver.cs
 * @Description : 뷰 모델 Resolver 인터페이스
 * @author 고형균
 * @since 2022.11.18
 * @version 1.0
 */
namespace WPF.Common.Mvvm
{
    public interface IViewModelResolver
    {
        object ResolveViewModelForView(object view, Type viewModelType);

        IViewModelResolver IfInheritsFrom<TView, TViewModel>(Action<TView, TViewModel, IContainerProvider> configuration);

        IViewModelResolver IfInheritsFrom<TView>(Type genericInterfaceType, Action<TView, object, IGenericInterface, IContainerProvider> configuration);
    }
}
