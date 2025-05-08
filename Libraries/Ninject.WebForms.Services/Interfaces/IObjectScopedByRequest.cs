using System;

namespace Ninject.WebForms.Services.Interfaces
{
    public interface IObjectScopedByRequest : IDisposable
    {
        Guid Id { get; }
    }
}